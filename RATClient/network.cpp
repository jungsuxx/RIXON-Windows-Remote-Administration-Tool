#include "network.h"
#include "tls.h"
#include "config.h"
#include <ws2tcpip.h>
#include <cstring>

static TlsCtx* s_tls = nullptr; // set after successful TLS handshake

// ── connect / disconnect ──────────────────────────────────────────────────

SOCKET Net_Connect(const char* addr, int port)
{
    SOCKET sock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (sock == INVALID_SOCKET) return INVALID_SOCKET;

    sockaddr_in sa = {};
    sa.sin_family = AF_INET;
    sa.sin_port   = htons(static_cast<unsigned short>(port));
    inet_pton(AF_INET, addr, &sa.sin_addr);

    if (connect(sock, reinterpret_cast<sockaddr*>(&sa), sizeof(sa)) == SOCKET_ERROR)
    {
        closesocket(sock);
        return INVALID_SOCKET;
    }

    // Configure socket *before* TLS handshake
    DWORD rcvTimeout = RECV_TIMEOUT_MS;
    setsockopt(sock, SOL_SOCKET, SO_RCVTIMEO,
               reinterpret_cast<const char*>(&rcvTimeout), sizeof(rcvTimeout));

    int yes = 1;
    setsockopt(sock, IPPROTO_TCP, TCP_NODELAY,
               reinterpret_cast<const char*>(&yes), sizeof(yes));

    // TLS handshake — tears down & rebuilds s_tls each connection
    TLS_Free(s_tls);
    s_tls = nullptr;

    s_tls = TLS_Connect(sock, addr);
    if (!s_tls)
    {
        closesocket(sock);
        return INVALID_SOCKET;
    }

    return sock;
}

void Net_Disconnect(SOCKET sock)
{
    TLS_Free(s_tls);
    s_tls = nullptr;
    closesocket(sock);
}

// ── send / recv ───────────────────────────────────────────────────────────

bool Net_SendAll(SOCKET sock, const unsigned char* data, int len)
{
    if (s_tls) return TLS_Send(s_tls, data, len);

    // Plain TCP fallback (no certificate configured on server)
    int sent = 0;
    while (sent < len)
    {
        int n = send(sock, reinterpret_cast<const char*>(data + sent), len - sent, 0);
        if (n == SOCKET_ERROR) return false;
        sent += n;
    }
    return true;
}

bool Net_RecvAll(SOCKET sock, unsigned char* buf, int len)
{
    if (s_tls)
    {
        int off = 0;
        while (off < len)
        {
            int n = TLS_Recv(s_tls, buf + off, len - off);
            if (n <= 0) return false;
            off += n;
        }
        return true;
    }

    // Plain TCP fallback
    int received = 0;
    while (received < len)
    {
        int n = recv(sock, reinterpret_cast<char*>(buf + received), len - received, 0);
        if (n <= 0) return false;
        received += n;
    }
    return true;
}

#include "protocol.h"
#include <cstring>

// ── internal helpers ──────────────────────────────────────────────────────

// Write [2-byte LE len][utf8 bytes] at buf+off. Returns new offset, -1 on overflow.
static int WriteStr(unsigned char* buf, int cap, int off, const char* str)
{
    int slen = static_cast<int>(strlen(str));
    if (slen > 0xFFFF) slen = 0xFFFF;
    if (off + 2 + slen > cap) return -1;

    buf[off]     = static_cast<unsigned char>(slen & 0xFF);
    buf[off + 1] = static_cast<unsigned char>(slen >> 8);
    memcpy(buf + off + 2, str, slen);
    return off + 2 + slen;
}

// Write 5-byte header at buf[0..4]. Returns 5.
static int WriteHeader(unsigned char* buf, int cap, unsigned char type, int payloadLen)
{
    if (cap < 5) return -1;
    buf[0] = static_cast<unsigned char>( payloadLen        & 0xFF);
    buf[1] = static_cast<unsigned char>((payloadLen >>  8) & 0xFF);
    buf[2] = static_cast<unsigned char>((payloadLen >> 16) & 0xFF);
    buf[3] = static_cast<unsigned char>((payloadLen >> 24) & 0xFF);
    buf[4] = type;
    return 5;
}

// Build payload in a temporary stack buffer, then prepend header into buf.
static int BuildPacket(unsigned char* buf, int cap,
                       unsigned char type,
                       unsigned char* payload, int payLen)
{
    if (5 + payLen > cap) return -1;
    WriteHeader(buf, cap, type, payLen);
    if (payLen > 0) memcpy(buf + 5, payload, payLen);
    return 5 + payLen;
}

// ── public API ────────────────────────────────────────────────────────────

int Pkt_BuildHello(unsigned char* buf, int cap,
                   const char* os, const char* hostname, const char* activeWnd)
{
    unsigned char payload[1536];
    int off = 0;
    if ((off = WriteStr(payload, sizeof(payload), off, os))        < 0) return -1;
    if ((off = WriteStr(payload, sizeof(payload), off, hostname))  < 0) return -1;
    if ((off = WriteStr(payload, sizeof(payload), off, activeWnd)) < 0) return -1;
    return BuildPacket(buf, cap, PKT_HELLO, payload, off);
}

int Pkt_BuildUpdate(unsigned char* buf, int cap, const char* activeWnd)
{
    unsigned char payload[1024];
    int off = WriteStr(payload, sizeof(payload), 0, activeWnd);
    if (off < 0) return -1;
    return BuildPacket(buf, cap, PKT_UPDATE, payload, off);
}

int Pkt_BuildPong(unsigned char* buf, int cap)
{
    return BuildPacket(buf, cap, PKT_PONG, nullptr, 0);
}

int Pkt_BuildScreenFrameHeader(unsigned char* buf, int cap, int jpegLen)
{
    if (cap < 5) return -1;
    buf[0] = static_cast<unsigned char>( jpegLen        & 0xFF);
    buf[1] = static_cast<unsigned char>((jpegLen >>  8) & 0xFF);
    buf[2] = static_cast<unsigned char>((jpegLen >> 16) & 0xFF);
    buf[3] = static_cast<unsigned char>((jpegLen >> 24) & 0xFF);
    buf[4] = PKT_SCREEN_FRAME;
    return 5;
}

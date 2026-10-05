#pragma once
#include <cstdint>

// Packet layout: [4 bytes LE: payload_len][1 byte: type][payload...]
// String field:  [2 bytes LE: utf8_len][utf8 bytes]
//
// HELLO        (0x01) client->server : [OS][Hostname][ActiveWindow]
// UPDATE       (0x02) client->server : [ActiveWindow]
// PING         (0x03) server->client : empty
// PONG         (0x04) client->server : empty
// SCREEN_START (0x05) server->client : [quality:u8][fps:u8]
// SCREEN_STOP  (0x06) server->client : empty
// SCREEN_FRAME (0x07) client->server : [JPEG bytes]

enum PacketType : unsigned char
{
    PKT_HELLO        = 0x01,
    PKT_UPDATE       = 0x02,
    PKT_PING         = 0x03,
    PKT_PONG         = 0x04,
    PKT_SCREEN_START = 0x05,
    PKT_SCREEN_STOP  = 0x06,
    PKT_SCREEN_FRAME = 0x07,
};

// All Build* functions write a complete packet into buf[cap].
// Return value: total bytes written, or -1 if buf is too small.
int Pkt_BuildHello             (unsigned char* buf, int cap,
                                 const char* os, const char* hostname, const char* activeWnd);
int Pkt_BuildUpdate            (unsigned char* buf, int cap, const char* activeWnd);
int Pkt_BuildPong              (unsigned char* buf, int cap);
// Writes only the 5-byte header; caller sends the JPEG bytes separately.
int Pkt_BuildScreenFrameHeader (unsigned char* buf, int cap, int jpegLen);

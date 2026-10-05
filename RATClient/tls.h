#pragma once
#define WIN32_LEAN_AND_MEAN
#include <winsock2.h>
#include <cstdint>

// Opaque Schannel TLS 1.2 context.
// One static instance is used internally; never create on the heap.
struct TlsCtx;

// Perform TLS 1.2 handshake on an already-connected socket.
// hostname is used for SNI (set to the server IP or DNS name).
// Returns pointer to the internal static context, or nullptr on failure.
// On failure the socket is NOT closed by this function.
TlsCtx* TLS_Connect(SOCKET sock, const char* hostname);

// Encrypt and send data.  Returns false on error.
bool TLS_Send(TlsCtx* ctx, const uint8_t* data, int len);

// Receive and decrypt.  Returns bytes written to outBuf, or -1 on error.
// May return fewer bytes than maxLen if only one TLS record was available.
int  TLS_Recv(TlsCtx* ctx, uint8_t* outBuf, int maxLen);

// Release Schannel handles and zero the context.
void TLS_Free(TlsCtx* ctx);

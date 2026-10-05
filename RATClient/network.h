#pragma once
#define WIN32_LEAN_AND_MEAN
#include <winsock2.h>

// Returns a connected, TLS-upgraded socket, or INVALID_SOCKET on failure.
// TLS is set up transparently; subsequent Send/Recv calls encrypt/decrypt.
SOCKET Net_Connect   (const char* addr, int port);

// Close the TLS session and the underlying socket.
void   Net_Disconnect(SOCKET sock);

// Send exactly len bytes (encrypted if TLS is active).  Returns false on error.
bool   Net_SendAll   (SOCKET sock, const unsigned char* data, int len);

// Receive exactly len bytes (decrypted if TLS is active).  Returns false on error.
bool   Net_RecvAll   (SOCKET sock, unsigned char* buf,  int len);

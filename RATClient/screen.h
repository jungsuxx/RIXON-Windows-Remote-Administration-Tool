#pragma once
#include <windows.h>

// Call once at startup after CoInitializeEx — creates the WIC factory.
void Screen_Init();

// Release the WIC factory (call at shutdown).
void Screen_Cleanup();

// Capture the primary monitor and JPEG-encode into buf.
// quality : 1..100   maxLen : capacity of buf
// Returns bytes written, or -1 on failure.
int Screen_Capture(unsigned char* buf, int maxLen, int quality);

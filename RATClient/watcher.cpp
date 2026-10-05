#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include "watcher.h"

void GetActiveWindowTitle(char* buf, int cap)
{
    buf[0] = '\0';

    HWND hwnd = GetForegroundWindow();
    if (!hwnd) return;

    wchar_t wbuf[512];
    if (GetWindowTextW(hwnd, wbuf, 512) == 0) return;

    WideCharToMultiByte(CP_UTF8, 0, wbuf, -1, buf, cap, nullptr, nullptr);
    buf[cap - 1] = '\0';
}

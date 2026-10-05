#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <stdio.h>
#include "sysinfo.h"

// ── OS string ─────────────────────────────────────────────────────────────

// RtlGetVersion tells the real version even if the app has no compatibility
// manifest, unlike GetVersionEx which can be shimmed.
typedef LONG (WINAPI* PFN_RtlGetVersion)(OSVERSIONINFOEXW*);

static void FillOsVersion(OSVERSIONINFOEXW& out)
{
    out = {};
    out.dwOSVersionInfoSize = sizeof(out);

    HMODULE ntdll = GetModuleHandleW(L"ntdll.dll");
    if (ntdll)
    {
        auto fn = reinterpret_cast<PFN_RtlGetVersion>(
            GetProcAddress(ntdll, "RtlGetVersion"));
        if (fn) { fn(&out); return; }
    }
    // Fallback (may return wrong version without manifest on Win8.1+)
#pragma warning(suppress: 4996)
    GetVersionExW(reinterpret_cast<OSVERSIONINFOW*>(&out));
}

void GetOsString(char* buf, int cap)
{
    OSVERSIONINFOEXW vi;
    FillOsVersion(vi);

    // Determine base name
    const char* name = "Windows";
    if (vi.dwMajorVersion == 10)
        name = (vi.dwBuildNumber >= 22000) ? "Windows 11" : "Windows 10";
    else if (vi.dwMajorVersion == 6)
    {
        if      (vi.dwMinorVersion == 3) name = "Windows 8.1";
        else if (vi.dwMinorVersion == 2) name = "Windows 8";
        else if (vi.dwMinorVersion == 1) name = "Windows 7";
    }

    // Determine edition via GetProductInfo
    const char* edition = "";
    DWORD dwType = 0;
    if (GetProductInfo(vi.dwMajorVersion, vi.dwMinorVersion, 0, 0, &dwType))
    {
        switch (dwType)
        {
        case 0x30: /* PRODUCT_PROFESSIONAL       */ edition = " Pro";        break;
        case 0x04: /* PRODUCT_ENTERPRISE         */ edition = " Enterprise"; break;
        case 0x1B: /* PRODUCT_ENTERPRISE_N       */ edition = " Enterprise N"; break;
        case 0x79: /* PRODUCT_EDUCATION          */ edition = " Education";  break;
        case 0xA8: /* PRODUCT_CORE               */
        case 0x65: /* PRODUCT_HOME_BASIC         */
        case 0x03: /* PRODUCT_HOME_PREMIUM       */ edition = " Home";       break;
        default: break;
        }
    }

    // Server variants
    if (vi.wProductType == VER_NT_SERVER ||
        vi.wProductType == VER_NT_DOMAIN_CONTROLLER)
        edition = " Server";

    _snprintf_s(buf, cap, _TRUNCATE,
        "%s%s (Build %lu)", name, edition, vi.dwBuildNumber);
}

// ── Hostname ──────────────────────────────────────────────────────────────

void GetHostnameString(char* buf, int cap)
{
    wchar_t wbuf[MAX_COMPUTERNAME_LENGTH + 1] = {};
    DWORD sz = MAX_COMPUTERNAME_LENGTH + 1;

    if (GetComputerNameW(wbuf, &sz))
    {
        WideCharToMultiByte(CP_UTF8, 0, wbuf, -1, buf, cap, nullptr, nullptr);
        buf[cap - 1] = '\0';
    }
    else
    {
        strncpy_s(buf, cap, "Unknown", _TRUNCATE);
    }
}

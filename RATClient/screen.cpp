#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <wincodec.h>
#include <objbase.h>
#include "screen.h"

#pragma comment(lib, "windowscodecs.lib")
#pragma comment(lib, "ole32.lib")

static IWICImagingFactory* s_factory = nullptr;

void Screen_Init()
{
    CoCreateInstance(CLSID_WICImagingFactory, nullptr, CLSCTX_INPROC_SERVER,
                     IID_IWICImagingFactory, reinterpret_cast<void**>(&s_factory));
}

void Screen_Cleanup()
{
    if (s_factory) { s_factory->Release(); s_factory = nullptr; }
}

int Screen_Capture(unsigned char* buf, int maxLen, int quality)
{
    if (!s_factory || !buf || maxLen < 1024) return -1;

    int sw = GetSystemMetrics(SM_CXSCREEN);
    int sh = GetSystemMetrics(SM_CYSCREEN);

    HDC     hdc  = GetDC(nullptr);
    HDC     hmem = CreateCompatibleDC(hdc);
    HBITMAP hbmp = CreateCompatibleBitmap(hdc, sw, sh);
    HGDIOBJ old  = SelectObject(hmem, hbmp);
    BOOL    ok   = BitBlt(hmem, 0, 0, sw, sh, hdc, 0, 0, SRCCOPY | CAPTUREBLT);
    SelectObject(hmem, old);
    DeleteDC(hmem);
    ReleaseDC(nullptr, hdc);

    if (!ok) { DeleteObject(hbmp); return -1; }

    int result = -1;
    IWICBitmap*            pBmp = nullptr;
    IWICStream*            pStr = nullptr;
    IWICBitmapEncoder*     pEnc = nullptr;
    IWICBitmapFrameEncode* pFrm = nullptr;
    IPropertyBag2*         pBag = nullptr;

    if (FAILED(s_factory->CreateBitmapFromHBITMAP(hbmp, nullptr,
                   WICBitmapIgnoreAlpha, &pBmp))) goto done;

    if (FAILED(s_factory->CreateStream(&pStr))) goto done;
    if (FAILED(pStr->InitializeFromMemory(
                   reinterpret_cast<WICInProcPointer>(buf),
                   static_cast<DWORD>(maxLen)))) goto done;

    if (FAILED(s_factory->CreateEncoder(GUID_ContainerFormatJpeg, nullptr, &pEnc))) goto done;
    if (FAILED(pEnc->Initialize(pStr, WICBitmapEncoderNoCache))) goto done;

    if (FAILED(pEnc->CreateNewFrame(&pFrm, &pBag))) goto done;

    {
        PROPBAG2 opt   = {};
        wchar_t  name[]= L"ImageQuality";
        opt.pstrName   = name;
        VARIANT  var   = {};
        var.vt         = VT_R4;
        var.fltVal     = static_cast<float>(quality) / 100.0f;
        pBag->Write(1, &opt, &var);
    }

    if (FAILED(pFrm->Initialize(pBag))) goto done;

    {
        WICPixelFormatGUID fmt = GUID_WICPixelFormat24bppBGR;
        pFrm->SetSize(static_cast<UINT>(sw), static_cast<UINT>(sh));
        pFrm->SetPixelFormat(&fmt);
        if (FAILED(pFrm->WriteSource(pBmp, nullptr))) goto done;
        if (FAILED(pFrm->Commit()))                   goto done;
        if (FAILED(pEnc->Commit()))                   goto done;
    }

    {
        LARGE_INTEGER  zero = {};
        ULARGE_INTEGER pos  = {};
        if (SUCCEEDED(pStr->Seek(zero, STREAM_SEEK_CUR, &pos)))
            result = static_cast<int>(pos.QuadPart);
    }

done:
    if (pBag) pBag->Release();
    if (pFrm) pFrm->Release();
    if (pEnc) pEnc->Release();
    if (pStr) pStr->Release();
    if (pBmp) pBmp->Release();
    DeleteObject(hbmp);
    return result;
}

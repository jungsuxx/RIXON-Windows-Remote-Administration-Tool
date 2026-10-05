// Schannel TLS 1.2 client wrapper — zero heap allocation after startup.
//
// One static TlsCtx is used (single-connection architecture).
// All recv/send operations are transparent to the rest of the codebase
// through network.cpp which holds the s_tls pointer.

#define WIN32_LEAN_AND_MEAN
#define SECURITY_WIN32
#include <windows.h>
#include <winsock2.h>
#include <security.h>
#include <schannel.h>
#include <wincrypt.h>
#include <cstring>
#include "tls.h"
#include "config.h"

#pragma comment(lib, "secur32.lib")
#pragma comment(lib, "crypt32.lib")

// ── context ───────────────────────────────────────────────────────────────

#define RAW_BUF_SIZE   32768  // encrypted input accumulator
#define PLAIN_BUF_SIZE 16384  // decrypted data waiting to be consumed

struct TlsCtx
{
    CredHandle hCred;
    CtxtHandle hCtxt;
    SOCKET     sock;
    bool       credInit;
    bool       ctxtInit;

    SecPkgContext_StreamSizes sizes;

    uint8_t rawBuf[RAW_BUF_SIZE];
    DWORD   rawLen;

    uint8_t plainBuf[PLAIN_BUF_SIZE];
    DWORD   plainLen;
    DWORD   plainOff;
};

static TlsCtx s_ctx; // single static context (one connection at a time)

// ── internal helpers ──────────────────────────────────────────────────────

static bool SendRaw(SOCKET sock, const void* data, DWORD len)
{
    const char* p = static_cast<const char*>(data);
    DWORD sent = 0;
    while (sent < len)
    {
        int n = send(sock, p + sent, static_cast<int>(len - sent), 0);
        if (n == SOCKET_ERROR) return false;
        sent += static_cast<DWORD>(n);
    }
    return true;
}

// Ensure ctx->rawLen >= need by reading from the socket (blocking with SO_RCVTIMEO).
static bool FillRaw(TlsCtx* ctx, int need)
{
    while (static_cast<int>(ctx->rawLen) < need)
    {
        int space = RAW_BUF_SIZE - static_cast<int>(ctx->rawLen);
        if (space <= 0) return false;

        int n = recv(ctx->sock,
                     reinterpret_cast<char*>(ctx->rawBuf + ctx->rawLen),
                     space, 0);
        if (n <= 0) return false;
        ctx->rawLen += static_cast<DWORD>(n);
    }
    return true;
}

// ── certificate verification ──────────────────────────────────────────────
// Check the server cert's CN against EXPECTED_CN.
// Empty EXPECTED_CN = skip (dev mode).

static bool VerifyCert(TlsCtx* ctx)
{
    if (EXPECTED_CN[0] == '\0') return true;

    PCCERT_CONTEXT pCert = nullptr;
    if (QueryContextAttributes(&ctx->hCtxt,
                               SECPKG_ATTR_REMOTE_CERT_CONTEXT,
                               &pCert) != SEC_E_OK || !pCert)
        return false;

    char subj[512] = {};
    CertNameToStrA(X509_ASN_ENCODING, &pCert->pCertInfo->Subject,
                   CERT_X500_NAME_STR, subj, sizeof(subj));
    CertFreeCertificateContext(pCert);

    for (const char* p = subj; *p; ++p)
    {
        if (p[0]=='C' && p[1]=='N' && p[2]=='=')
        {
            p += 3;
            char cn[256] = {}; int i = 0;
            while (*p && *p != ',' && i < 255) cn[i++] = *p++;
            return lstrcmpA(cn, EXPECTED_CN) == 0;
        }
    }
    return false;
}

// ── TLS handshake ─────────────────────────────────────────────────────────

static bool Handshake(TlsCtx* ctx, const char* host)
{
    // TLS 1.2 only; manual certificate validation so self-signed certs are accepted.
    SCHANNEL_CRED cred            = {};
    cred.dwVersion                = SCHANNEL_CRED_VERSION;
    cred.grbitEnabledProtocols    = SP_PROT_TLS1_2_CLIENT;
    cred.dwFlags                  = SCH_CRED_NO_DEFAULT_CREDS |
                                     SCH_CRED_MANUAL_CRED_VALIDATION;

    TimeStamp ts;
    SECURITY_STATUS ss = AcquireCredentialsHandleA(
        nullptr, const_cast<LPSTR>(UNISP_NAME_A), SECPKG_CRED_OUTBOUND,
        nullptr, &cred, nullptr, nullptr, &ctx->hCred, &ts);
    if (ss != SEC_E_OK) return false;
    ctx->credInit = true;

    wchar_t whost[256] = {};
    MultiByteToWideChar(CP_ACP, 0, host, -1, whost, 256);

    const DWORD reqFlags = ISC_REQ_SEQUENCE_DETECT | ISC_REQ_REPLAY_DETECT |
                            ISC_REQ_CONFIDENTIALITY | ISC_REQ_STREAM        |
                            ISC_REQ_ALLOCATE_MEMORY;
    DWORD retFlags = 0;

    // First call — no input, produces the TLS ClientHello output token.
    SecBuffer    outBuf  = { 0, SECBUFFER_TOKEN, nullptr };
    SecBufferDesc outDesc = { SECBUFFER_VERSION, 1, &outBuf };

    ss = InitializeSecurityContextW(
        &ctx->hCred, nullptr, whost,
        reqFlags, 0, SECURITY_NATIVE_DREP,
        nullptr, 0,
        &ctx->hCtxt, &outDesc, &retFlags, &ts);

    if (ss != SEC_I_CONTINUE_NEEDED) return false;
    ctx->ctxtInit = true;

    if (outBuf.cbBuffer && outBuf.pvBuffer)
    {
        bool ok = SendRaw(ctx->sock, outBuf.pvBuffer, outBuf.cbBuffer);
        FreeContextBuffer(outBuf.pvBuffer);
        outBuf = { 0, SECBUFFER_TOKEN, nullptr };
        if (!ok) return false;
    }

    // Handshake loop: read server messages, respond until SS == SEC_E_OK.
    while (ss == SEC_I_CONTINUE_NEEDED || ss == SEC_E_INCOMPLETE_MESSAGE)
    {
        if (!FillRaw(ctx, static_cast<int>(ctx->rawLen) + 1)) return false;

        SecBuffer inBufs[2] = {
            { ctx->rawLen, SECBUFFER_TOKEN, ctx->rawBuf },
            { 0,           SECBUFFER_EMPTY, nullptr     }
        };
        SecBufferDesc inDesc = { SECBUFFER_VERSION, 2, inBufs };

        outBuf  = { 0, SECBUFFER_TOKEN, nullptr };
        outDesc = { SECBUFFER_VERSION, 1, &outBuf };

        ss = InitializeSecurityContextW(
            &ctx->hCred, &ctx->hCtxt, nullptr,
            reqFlags, 0, SECURITY_NATIVE_DREP,
            &inDesc, 0,
            nullptr, &outDesc, &retFlags, &ts);

        // Preserve any unprocessed bytes (SECBUFFER_EXTRA) for the next iteration.
        if (inBufs[1].BufferType == SECBUFFER_EXTRA && inBufs[1].cbBuffer > 0)
        {
            DWORD extra = inBufs[1].cbBuffer;
            memmove(ctx->rawBuf, ctx->rawBuf + ctx->rawLen - extra, extra);
            ctx->rawLen = extra;
        }
        else if (ss != SEC_E_INCOMPLETE_MESSAGE)
        {
            ctx->rawLen = 0;
        }

        if (outBuf.cbBuffer && outBuf.pvBuffer)
        {
            bool ok = SendRaw(ctx->sock, outBuf.pvBuffer, outBuf.cbBuffer);
            FreeContextBuffer(outBuf.pvBuffer);
            outBuf = { 0, SECBUFFER_TOKEN, nullptr };
            if (!ok) return false;
        }
    }

    if (ss != SEC_E_OK) return false;

    QueryContextAttributes(&ctx->hCtxt, SECPKG_ATTR_STREAM_SIZES, &ctx->sizes);
    return true;
}

// ── public API ────────────────────────────────────────────────────────────

TlsCtx* TLS_Connect(SOCKET sock, const char* hostname)
{
    TLS_Free(&s_ctx); // reset any previous context
    s_ctx.sock = sock;

    if (!Handshake(&s_ctx, hostname) || !VerifyCert(&s_ctx))
    {
        TLS_Free(&s_ctx);
        return nullptr;
    }
    return &s_ctx;
}

bool TLS_Send(TlsCtx* ctx, const uint8_t* data, int len)
{
    // Static send buffer: header + max message + trailer.
    // cbHeader (5) + cbMaximumMessage (16384) + cbTrailer (256) < 18000
    static uint8_t buf[18000];

    int off = 0;
    while (off < len)
    {
        DWORD chunk = static_cast<DWORD>(len - off);
        if (chunk > ctx->sizes.cbMaximumMessage)
            chunk = ctx->sizes.cbMaximumMessage;

        memcpy(buf + ctx->sizes.cbHeader, data + off, chunk);

        SecBuffer bufs[3] = {
            { ctx->sizes.cbHeader,  SECBUFFER_STREAM_HEADER,  buf },
            { chunk,                SECBUFFER_DATA,            buf + ctx->sizes.cbHeader },
            { ctx->sizes.cbTrailer, SECBUFFER_STREAM_TRAILER,  buf + ctx->sizes.cbHeader + chunk }
        };
        SecBufferDesc desc = { SECBUFFER_VERSION, 3, bufs };

        if (EncryptMessage(&ctx->hCtxt, 0, &desc, 0) != SEC_E_OK) return false;

        DWORD total = bufs[0].cbBuffer + bufs[1].cbBuffer + bufs[2].cbBuffer;
        if (!SendRaw(ctx->sock, buf, total)) return false;

        off += static_cast<int>(chunk);
    }
    return true;
}

int TLS_Recv(TlsCtx* ctx, uint8_t* outBuf, int maxLen)
{
    // Return already-decrypted bytes first.
    if (ctx->plainLen > 0)
    {
        DWORD copy = ctx->plainLen < static_cast<DWORD>(maxLen)
                   ? ctx->plainLen : static_cast<DWORD>(maxLen);
        memcpy(outBuf, ctx->plainBuf + ctx->plainOff, copy);
        ctx->plainOff += copy;
        ctx->plainLen -= copy;
        if (ctx->plainLen == 0) ctx->plainOff = 0;
        return static_cast<int>(copy);
    }

    // Decrypt loop — may need multiple socket reads to complete one TLS record.
    for (;;)
    {
        if (ctx->rawLen == 0)
        {
            if (!FillRaw(ctx, 1)) return -1;
        }

        SecBuffer bufs[4] = {
            { ctx->rawLen, SECBUFFER_DATA, ctx->rawBuf },
            { 0, SECBUFFER_EMPTY, nullptr },
            { 0, SECBUFFER_EMPTY, nullptr },
            { 0, SECBUFFER_EMPTY, nullptr }
        };
        SecBufferDesc desc = { SECBUFFER_VERSION, 4, bufs };

        SECURITY_STATUS ss = DecryptMessage(&ctx->hCtxt, &desc, 0, nullptr);

        if (ss == SEC_E_INCOMPLETE_MESSAGE)
        {
            // Need more bytes to complete this TLS record.
            if (!FillRaw(ctx, static_cast<int>(ctx->rawLen) + 1)) return -1;
            continue;
        }

        if (ss == SEC_I_CONTEXT_EXPIRED || ss != SEC_E_OK) return -1;

        // Locate the decrypted data and any extra (leftover start of next record).
        uint8_t* plain    = nullptr;
        DWORD    plainLen = 0;
        DWORD    extraLen = 0;

        for (int i = 0; i < 4; ++i)
        {
            if (bufs[i].BufferType == SECBUFFER_DATA && bufs[i].pvBuffer)
            {
                plain    = static_cast<uint8_t*>(bufs[i].pvBuffer);
                plainLen = bufs[i].cbBuffer;
            }
            else if (bufs[i].BufferType == SECBUFFER_EXTRA && bufs[i].pvBuffer)
            {
                extraLen = bufs[i].cbBuffer;
            }
        }

        // Move extra bytes to the front of rawBuf for the next DecryptMessage call.
        if (extraLen > 0)
        {
            memmove(ctx->rawBuf, ctx->rawBuf + ctx->rawLen - extraLen, extraLen);
            ctx->rawLen = extraLen;
        }
        else
        {
            ctx->rawLen = 0;
        }

        if (!plain || plainLen == 0) continue; // Empty TLS record (keepalive, alert)

        DWORD ret = plainLen < static_cast<DWORD>(maxLen)
                  ? plainLen : static_cast<DWORD>(maxLen);
        memcpy(outBuf, plain, ret);

        // Stash any overflow beyond what the caller requested.
        if (plainLen > ret)
        {
            DWORD rem = plainLen - ret;
            if (rem > PLAIN_BUF_SIZE) rem = PLAIN_BUF_SIZE;
            memcpy(ctx->plainBuf, plain + ret, rem);
            ctx->plainLen = rem;
            ctx->plainOff = 0;
        }

        return static_cast<int>(ret);
    }
}

void TLS_Free(TlsCtx* ctx)
{
    if (!ctx) return;
    if (ctx->ctxtInit) { DeleteSecurityContext(&ctx->hCtxt); }
    if (ctx->credInit) { FreeCredentialsHandle(&ctx->hCred); }
    memset(ctx, 0, sizeof(*ctx));
}

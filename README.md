# RIXON

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows-blue.svg)]()
[![Language](https://img.shields.io/badge/language-C%23%20%2F%20C%2B%2B-informational.svg)]()

**Windows Remote Administration Tool — Built for Red Team Operations**

RIXON is a lightweight, fast remote administration tool for Windows, designed exclusively for **authorized red team engagements, penetration testing, and security research**.

> **Features are limited at the moment — I'll be adding more as time allows. Stay tuned for updates.**

---

## ⚠️ Legal Disclaimer

**RIXON is intended for authorized security professionals only.**

By downloading, compiling, or using this software, you agree that:

- You will only use RIXON on systems you **own** or have **explicit written permission** to test.
- This tool is strictly for **red team operations, penetration testing, and security research** in authorized environments.
- The author(s) take **absolutely no responsibility** for any illegal use, damage, or consequences arising from misuse of this tool.
- Unauthorized use against systems you do not own is **illegal** and may result in criminal prosecution.

**If you are not a security professional operating within a legal engagement — do not use this tool.**

---

## Features

- **TLS 1.2 Encrypted Communication** (Schannel on client, SslStream on server)
- **CN-based Certificate Pinning** — configurable TLS key shared between server and client
- **Live Client Management** — IP, OS, Hostname, Active Window tracking
- **Real-time Screen Viewing** — JPEG-compressed remote desktop stream with quality/FPS control
- **Auto-Reconnect with Exponential Backoff**
- **Single-instance Client Guard**
- **Zero Heap Allocation Client** — static buffers, minimal footprint
- **Pure Binary Protocol** — no JSON, no overhead
- Multi-port listener with per-port TLS status

> More features will be added over time.

---

## Architecture

| Component | Stack |
|---|---|
| Server | C# .NET 8, WinForms |
| Client | C++ Win32 API, Schannel TLS |
| Protocol | Binary length-prefixed (`[4 bytes LE: len][1 byte: type][payload...]`) |
| Transport | TCP + TLS 1.2 |
| Screen Capture | GDI BitBlt + WIC JPEG encoding |

---

## Screenshots

![Server UI](https://i.ibb.co/9kv8p4kp/image.png)

---

## Getting Started

### Requirements

**Server:**
- Windows 10 / 11
- .NET 8 Runtime

**Client (build):**
- Visual Studio 2022 with C++ Desktop Development workload
- Windows SDK 10.0

### Building

**Server:**
```
Open RATServer/RATServer.sln in Visual Studio or build with:
dotnet build RATServer/RATServer.csproj -c Release
```

**Client:**
1. Open `RATClient/RATClient.vcxproj` in Visual Studio 2022
2. Edit `config.h` — set `SERVER_ADDR`, `SERVER_PORT`, and `EXPECTED_CN` to match your server's TLS Key
3. Build in Release x64

### Configuration

Edit `RATClient/config.h` before compiling:

```c
#define SERVER_ADDR       "192.168.1.100"  // Your server IP
#define SERVER_PORT       4444
#define EXPECTED_CN       "RATServer"      // Must match TLS Key set in server UI
#define RECONNECT_MIN_SEC 1
#define RECONNECT_MAX_SEC 30
```

On the server, set the **TLS Key** in the Listening tab before starting a listener. The key must match `EXPECTED_CN` in the client config exactly.

---

## Roadmap

> The following features are **planned but not yet implemented**. I'll be adding them gradually as time allows.

- [ ] Keylogger
- [ ] File Manager
- [ ] Reverse Shell
- [ ] Process Manager
- [ ] Clipboard Sync
- [ ] Mouse / Keyboard Remote Input
- [ ] Multi-monitor support
- [ ] Persistence module

---

## License

RIXON is distributed under the [MIT License](LICENSE).

You are free to use, modify, and distribute this software. However, the MIT License does **not** waive your legal obligations — using this tool without authorization is still illegal regardless of the license.

---

## Contributing

Pull requests and issues are welcome. Please use this project responsibly.

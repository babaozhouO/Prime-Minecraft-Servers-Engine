## Prerequisites

Before you begin, you'll need:

- **A server** (Windows or Linux)
- **.NET 10 Runtime installed**

  - Windows: Download and install [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) (choose the **Run desktop apps** version)
  - Linux: Follow the [official Microsoft documentation](https://learn.microsoft.com/dotnet/core/install/linux) for your distribution

- Download the PMCSsE backend:

  - **GitHub**: [Releases page](https://github.com/babaozhouO/Prime-Minecraft-Servers-Engine/releases) → latest version → download `PMCSsE_Backend_AllPlatforms.zip`
  - **QQ Group**: 515093683 → Group files → `发行版` folder

> ⚠️ **Do not** extract the program to high-permission folders like `C:\Program Files` or `C:\Windows`, or it won't have permission to write configuration files and logs.

After extraction, you'll find a `PMCSsE_Backend` folder containing the backend program along with some one-click startup scripts:

| Script | Purpose |
|---|---|
| `start_foreground_win.bat` | Windows — foreground mode (with console window) |
| `start_background_win.bat` | Windows — background mode (no window) |
| `start_foreground_unix.sh` | Linux — foreground mode |
| `start_background_unix.sh` | Linux — background mode |
| `start_set_win.bat` | Windows — first-time setup (with `first` parameter) |
| `start_set_unix.sh` | Linux — first-time setup |

You can double-click or run these scripts directly. If you need custom parameters, you can also run the program manually from the command line.

---

## First Launch — Setup Wizard

The first time you run the backend, it needs initial configuration: address, port, and access key.

### Step 1: Launch with Setup Mode

The easiest way is to run the setup script:

- **Windows** → Double-click `start_set_win.bat`
- **Linux** → Run `./start_set_unix.sh`

Or manually with the `first` parameter:

**Windows:**
```
PMCSsE_Backend.exe first
```

**Linux:**
```
dotnet PMCSsE_Backend.dll first
```

### Step 2: Follow the Prompts

The program will ask you a few questions:

**① Choose a listening address**

```
Please select the IP address to listen on (enter the number):
[0] All (all IPv4 and IPv6 addresses) ← choose this in most cases
[1] 0.0.0.0 (all IPv4 addresses)
[2] :: (all IPv6 addresses)
...
```

Usually, choosing `0` (all addresses) is fine.

**② Set the listening port**

Enter a port number (1~65535). The backend will wait for frontend connections on this port.

```
Please set the frontend connection port, a number (1~65535)
Avoid commonly used ports like Web(80), SSH(22)
```

Default is `20000`, but you can choose any available port. The program will check if the port is already in use.

> 💡 On Linux, ports below 1024 require root privileges.

**③ Set an access key**

```
For security, you must set an access key
Do you want the program to generate one? (Y/N)
```

You can choose:
- **Y** (auto-generate) — The program creates a complex key and displays it on screen. **Write it down immediately.**
- **N** (manual) — Enter a key of at least 8 characters yourself

> ⚠️ **CRITICAL**: The access key is your only credential for connecting. **There is no way to recover it if lost.** Keep it safe!

### Step 3: Setup Complete

After configuration, the program will show:
```
Setup complete. Remove the "first" parameter and restart.
```

You can now close the window. The configuration files have been generated.

---

## Normal Startup

After the initial setup, you **do not** need the `first` parameter anymore.

Run the appropriate one-click script:

| Mode | Windows | Linux |
|---|---|---|
| **Foreground** (with console window) | Double-click `start_foreground_win.bat` | Run `./start_foreground_unix.sh` |
| **Background** (silent, no window) | Double-click `start_background_win.bat` | Run `./start_background_unix.sh` |

Or manually:

**Windows foreground:**
```
PMCSsE_Backend.exe
```

**Windows background:**
```
PMCSsE_Backend.exe background
```

**Linux foreground:**
```
dotnet PMCSsE_Backend.dll
```

**Linux background:**
```
dotnet PMCSsE_Backend.dll background
```

### Debug Mode

For more detailed logs when troubleshooting, add the `debug` parameter:

**Windows:**
```
PMCSsE_Backend.exe debug
```

**Linux:**
```
dotnet PMCSsE_Backend.dll debug
```

Combine with background mode:

**Windows:**
```
PMCSsE_Backend.exe background debug
```

**Linux:**
```
dotnet PMCSsE_Backend.dll background debug
```

### Tip: Create a Shortcut

**Windows:**
- Right-click `start_background_win.bat` → **Send to** → **Desktop (create shortcut)**
- Or right-click → **Pin to Start**
- Double-click the desktop icon to start the backend instantly

**Linux:**
- Create a `.desktop` shortcut for `start_background_unix.sh`
- Or add it to startup (crontab / systemd)

---

## How to Verify the Backend is Running

When you see output like this, the backend is ready:

```
PMCSsE starting
Project URL: https://github.com/babaozhouO/Prime-Minecraft-Servers-Engine
Listening for frontend connections on 0.0.0.0:20000
```

No error messages means everything is working. The backend is now waiting for the frontend to connect.

---

## How to Safely Shut Down the Backend

| Method | Description |
|---|---|
| **Press Ctrl+C** | In the backend console window, press `Ctrl+C`. The backend will safely close all Minecraft servers before stopping itself |
| **Send SIGTERM (Linux)** | If using systemd or Docker, send `SIGTERM` (systemd sends this by default) |
| **Through frontend (future)** | A future version will add a "Shutdown backend" button in the frontend settings ⏳ *Not yet implemented* |

> ⚠️ **Never use `kill -9` (SIGKILL) or Task Manager to force-kill the backend.** This can corrupt configuration files that are being written. Corrupted config files **cannot be recovered** and all manager settings will be lost. Always use Ctrl+C or SIGTERM for safe shutdown.

When the backend shuts down properly, you'll see:
```
Closing all MC servers
Shutting down native server
Stopping log recording
Program stopped and resources released
```

---

## Configuration Files

After startup, a `Configs` folder is created in the backend directory with two files:

| File | Contents | Notes |
|---|---|---|
| `Config_Plaintext.dat` | Listening address, port | Not encrypted, but only contains non-sensitive info |
| `Config_Ciphertext.dat` | Access key verification data, all server manager configs | **Encrypted** with your access key |

> ❌ **Do not manually modify or delete these files.** If deleted, you'll need to re-run the setup wizard with the `first` parameter. If the encrypted config is corrupted, all your server manager settings will be lost.

---

## Log Files

Logs are stored in `Logs/MainProgramLog/`, organized by date:

```
Logs/
└── MainProgramLog/
    ├── 2026-07-01.log
    ├── 2026-07-02.log
    └── 2026-07-03.log    ← today's log
```

Check these log files if you need to troubleshoot any issues.

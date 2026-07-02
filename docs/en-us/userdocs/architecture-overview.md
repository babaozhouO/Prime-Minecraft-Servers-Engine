## Overall Structure

PMCSsE is made up of **three core components** that work together:

```
┌─────────────────┐   Encrypted     ┌──────────────────────┐    Manage    ┌──────────────────┐
│                 │   ◄────────►    │                      │─────────────►│                  │
│   Frontend      │                 │   Backend             │              │  Minecraft       │
│   (Your PC)     │                 │   (Your Server)      │              │  Server(s)       │
│                 │                 │                      │              │                  │
└─────────────────┘                 └──────────────────────┘              └──────────────────┘
```

In one sentence:
> The **frontend** is your remote control, the **backend** is the butler on your server, and the butler takes care of one or more **Minecraft server processes**.

---

## The Three Components

### 1. Backend (PMCSsE_Backend)

This is the core of the software — a background process that runs on your server.

What the backend does:
- Waits for the frontend to connect
- Starts/stops Minecraft server processes based on frontend commands
- Forwards server logs to the frontend
- Sends commands to the Minecraft server
- Automatically loads plugins from the `plugins` folder

The backend has no graphical interface — just a console window (or runs silently in the background). All operations are done remotely through the frontend.

### 2. Frontend (PMCSsE_Frontend_AvaloniaUI)

This is the graphical interface you interact with directly. All platforms have the same operation logic and experience, only performance differs:

| Platform | Notes |
|---|---|
| **Windows / Linux / macOS** | Desktop version |
| **Android** | Phone or tablet |
| **iOS** | ⏳ Project includes iOS target, but cannot be published without an Apple Developer account |

What the frontend does:
- **Connection screen**: Enter server IP, port and access key to connect
- **Management screen**: Add/delete Minecraft server manager configs
- **Console screen**: Start/stop servers, send commands, view logs
- **Settings screen**: Edit Java path, startup arguments, etc.

### 3. Communicator Library (PMCSsE_Communicator)

You can't see this part, but it's the most important middleman. It's a code library used by both frontend and backend, responsible for:

- **Secure connection**: Only someone with the access key can connect
- **Encrypted communication**: All data between frontend and backend is encrypted
- **Protocol definition**: Defines the "language" frontend and backend use to talk to each other

---

## Communication Security

All communication between frontend and backend is encrypted. Simplified:

1. **Identity verification**: The backend shows its "ID card" (a fingerprint), you verify it's the right server
2. **Key exchange**: Both sides negotiate a temporary key known only to them
3. **Locked communication**: All subsequent conversations are encrypted — anyone eavesdropping sees only gibberish

---

## What About Plugins?

PMCSsE supports **plugin extensions**. Think of plugins like **game cartridges**:

- The backend is like a game console with basic functions
- Plugins are like game cartridges — add one to get new features
- Plugins are `.dll` files, just drop them into the `plugins` folder

The official example plugin shows how to automatically get the server version when starting a Vanilla server.

---

## Summary

| Component | Where it runs | What it does |
|---|---|---|
| **Backend** | Your server | Manages Minecraft server processes |
| **Frontend** | Your computer/phone | Graphical interface for remote control |
| **Communicator** | Built into both | Handles encryption and communication protocol |

> **Next steps**:
> 1. Set up the backend on your server → **Backend Setup**
> 2. Make it accessible from the internet → **Public Access**
> 3. Install the frontend and connect → **Frontend Installation**
> 4. Add and manage Minecraft servers → **Server Manager Usage**

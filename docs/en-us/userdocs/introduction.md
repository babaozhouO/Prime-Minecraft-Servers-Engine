## What is this?

**PMCSsE** (Prime Minecraft Servers Engine) is a tool that helps you remotely manage Minecraft servers.

Imagine: your Minecraft server is running on a remote machine, and every time you need to start/stop it or type a command, you have to SSH in or use remote desktop. PMCSsE solves this by giving you a **graphical remote control** — you can do everything from your own computer.

---

## What does "frontend/backend separation" mean?

PMCSsE consists of **two independent parts**:

- **Backend** → Runs on your server, silently managing Minecraft server processes
- **Frontend** → Runs on your computer or phone, providing the graphical interface

Think of it this way:
> The backend is the **stage actor**, the frontend is the **director's monitor**. The director doesn't need to be on stage to know what's happening and give instructions.

Benefits:
- The server only focuses on running Minecraft, no resources wasted on rendering a web UI
- All communication between frontend and backend is encrypted
- You can manage **multiple** Minecraft server instances at once
- Extensible with plugins for additional functionality

---

## Current Version (v1.0.4) Features

| Feature | Description |
|---|---|
| 🔗 **Remote Connection** | Connect to your backend using IP and access key |
| 📋 **Manage Multiple Servers** | Add, load, and delete Minecraft server instances |
| ▶️ **Start/Stop Servers** | Start, gracefully stop (send stop command), or force kill |
| ⌨️ **Send Commands** | Send commands to your server remotely |
| 📜 **View Logs** | Real-time server log viewing with history scrolling |
| ⚙️ **Edit Configuration** | Change server directory, Java path, startup arguments |
| 🔌 **Plugin Support** | Drop DLL files into the plugins folder, loaded automatically |

> These are the features currently available in v1.0.4. More features (auto-deployment, config file editing, command autocomplete, backup management, etc.) are under development.

---

## How to Get Started

Follow the documentation in this order:

1. **Architecture Overview** — Understand the big picture
2. **Backend Setup** — Get the backend running on your server
3. **Public Access** — Make it accessible from the internet
4. **Frontend Installation** — Install the frontend and connect
5. **Server Manager Usage** — Add and manage your Minecraft servers

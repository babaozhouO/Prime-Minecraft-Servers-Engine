## Overview

After connecting to the backend, managing Minecraft servers is done through **two main tabs**:

| Tab | Purpose |
|---|---|
| 📦 **All Managers** | View all saved manager configs — create, load, delete |
| ⚡ **Running Managers** | View loaded instances and their running status — open console |

Simply put:
> **All Managers** is like your **save file list** — it holds the configuration for each server.
> **Running Managers** is like **currently opened saves** — only loaded managers can be operated.

---

## The "All Managers" Tab

After connecting, click the **All Managers** tab. You'll see:

- **Left list**: All saved manager configs (each showing: server name, ID, type)
- **Right action buttons**:

| Button | Action |
|---|---|
| **Refresh** | Reload the config list from the backend |
| **New** | Create a new empty config (backend auto-assigns an ID) |
| **Start** | Load the selected config into "Running Managers" |
| **Stop** | Unload the selected instance from "Running Managers" |
| **Delete** | Remove the selected config from the list (**only persists after clicking "Save"**) |
| **Save** | Persist all current configs to the backend |

### Creating a New Manager

1. Click **New**
2. The backend auto-assigns a 3-digit ID (e.g., `001`, `002`)
3. The new config appears in the list with empty fields
4. You'll need to fill in the details in the **Settings** page later

### Loading a Manager

1. Select a config in the list (click to highlight)
2. Click **Start**
3. The manager appears in the **Running Managers** tab

> Once loaded, you can open its console from the Running Managers tab.

---

## The "Running Managers" Tab

Click the **Running Managers** tab. You'll see:

- **Left list**: All loaded managers (each showing: name, ID, type, **running status**)
  - Status shows **"Running"** or **"Stopped"**
- **Right action buttons**:

| Button | Action |
|---|---|
| **Refresh** | Reload the latest status from the backend |
| **Open** | Open the selected manager's detail panel (console/settings) |
| **Save** | Persist configs to the backend |

---

## Opening a Manager — Detail Panel

Select a manager in **Running Managers** and click **Open**. A detail panel overlays the interface.

The panel has a **drawer navigation** on the right side with these pages:

| Page | Purpose |
|---|---|
| 🖥️ **Console** | **Core operations** — start/stop server, send commands, view logs |
| 💾 **Backup** | Backup feature (entry exists, functionality pending) |
| 🔗 **Online Chat** | (Planned) |
| 📁 **File Manager** | (Planned) |
| ⚙️ **Settings** | Edit manager configuration |

> Click **Back** at the bottom of the drawer to close the detail panel.

---

### ① Console Page

The console is the **heart** of server management.

#### Layout

```
┌─────────────────────────────────────────────────────┐
│  Server Name                    [Log Polling Slider]│
├─────────────────────────────────────────────────────┤
│                                                     │
│             Log Display (with line numbers)          │
│                                                     │
├─────────────────────────────────────────────────────┤
│  [Command Input...]  [Send] [Older] [Start] [Stop] [Kill]
└─────────────────────────────────────────────────────┘
```

#### Start/Stop Server

Four buttons at the bottom, left to right:

| Button | Action |
|---|---|
| **Start** | Start the Minecraft server (requires Java path and startup args configured) |
| **Stop** | Graceful shutdown — sends `stop` command to the server, letting it save data |
| **Kill** | Force kill — terminates the process immediately, **may corrupt data**, only use when frozen |
| **Older** | Load older historical logs |

> Always use **Stop** for normal shutdowns to safely save your world data.

#### Sending Commands

- Type your command in the input box (**no** slash `/` needed)
- Press Enter or click Send to execute
- Autocomplete support is planned but not yet implemented ⏳

#### Viewing Logs

- Server output appears **in real-time** in the log display
- Line numbers auto-increment
- Click **Older** to browse history

---

### ② Settings Page

Click **Settings** in the drawer navigation to modify the current manager's configuration.

#### Configurable Items

| Item | Description |
|---|---|
| **Server Name** | A name for your reference, to distinguish between servers |
| **Server Type** | Select your Minecraft server type. If not listed, choose its **upstream** (e.g., Leaves → Paper). If still unsure, pick **Vanilla**. Different types only affect feature support, not normal operation |
| **Server Directory** | Full path to the Minecraft server folder |
| **Java Path** | Path to the Java executable (e.g., `/usr/bin/java` or `C:\Program Files\Java\bin\java.exe`) |
| **Startup Arguments** | JVM startup arguments (memory allocation, core jar, etc.). A generator exists but has no UI entry yet ⏳ |

#### How to Edit

1. Click **Edit Config** — input fields become editable
2. Make your changes
3. Click **Commit Config** — changes are sent to the backend and saved

> **Note**: If startup arguments contain dangerous keywords, the backend will automatically reject them and show an error.

---

## Stop vs Delete

| Action | Where | Effect |
|---|---|---|
| **Stop** | All Managers → select → click Stop | Removes from "Running Managers" list, but config remains in "All Managers" for future use |
| **Delete** | All Managers → select → click Delete | Removes the config from the current list. If you haven't clicked **Save**, the deletion isn't persisted — just **restart the backend** to reload the original config from disk |

> Use **Stop** for managers you don't currently need. Only use **Delete** when you're sure, and remember it won't be saved unless you also click **Save**.

## Download the Frontend

The frontend and backend are distributed separately. Choose the file for your platform:

| Platform | File |
|---|---|
| **Windows** | `PMCSsE_Frontend_win-x64.zip` |
| **Linux** | `PMCSsE_Frontend_linux-x64.zip` |
| **macOS** | `PMCSsE_Frontend_osx-x64.zip` |
| **Android** | `PMCSsE_Frontend_android-arm64.apk` |
| **iOS** | ⏳ Project includes iOS target, but cannot be published without an Apple Developer account |

Download from:
- **GitHub**: [Releases page](https://github.com/babaozhouO/Prime-Minecraft-Servers-Engine/releases)
- **QQ Group**: 515093683 → Group files → `发行版` folder

---

## System Requirements

- **Windows**: .NET 10 Desktop Runtime (same as the backend — if you already installed it, you're good)
  - [Download .NET 10 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- **Linux Desktop**: .NET 10 Runtime + a desktop environment (GNOME, KDE, etc.)
- **Android**: Install the APK directly, no extra setup needed

> 💡 If nothing happens when you double-click, check that .NET is installed. Run `dotnet --info` in a terminal to see installed versions.

---

## Launching the Frontend

### Windows

Go to the `PMCSsE_Frontend_AvaloniaUI.Desktop` folder and **double-click** `PMCSsE_Frontend_AvaloniaUI.Desktop.exe`.

You can also create a shortcut: right-click the `.exe` → **Send to** → **Desktop (create shortcut)**, or right-click → **Pin to Start**.

### Linux

Run in a terminal:
```bash
cd PMCSsE_Frontend_AvaloniaUI.Desktop
./PMCSsE_Frontend_AvaloniaUI.Desktop
```
You can also create a `.desktop` shortcut for easier access.

### Android

Install the APK file and open from your app list.

> iOS: ⏳ Waiting for an Apple Developer account before it can be published.

---

## Connecting to the Backend

After launching the frontend, you'll see the connection screen.

### Step 1: Add a Backend

In the **Connection** tab, select the **Add** subtab and fill in:

| Field | Description |
|---|---|
| **Host/IP** | Your server's IP address or domain (e.g., `192.168.1.100` or `mc.example.com`) |
| **Port** | The port you set during backend setup (default `20000`) |
| **Access Key** | The access key you set during backend setup |

Click **Add** when done.

> ⚠️ **If you fill in the access key, it will be saved to the local config file in plain text.** Anyone with access to your computer can see it. If left blank, you'll be asked to enter it when connecting — more secure. Choose based on your balance of convenience vs. security.

### Step 2: Connect from the List

Switch to the **Backend List** subtab. You'll see your newly added backend with three buttons:

| Button | Action |
|---|---|
| 🔗 **Connect** | Connect to this backend |
| ✏️ **Change Key** | Modify the saved access key |
| 🗑️ **Delete** | Remove this entry from the list |

Click **Connect**.

### Step 3: Verify Server Identity (First Connection Only)

The first time you connect to a backend, the frontend will show an RSA public key fingerprint:

```
Verifying server identity...
Please confirm the fingerprint:
e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
```

This prevents man-in-the-middle attacks — it ensures you're connecting to your real server.

What to do:
1. Check the backend console — it prints its fingerprint when starting. Compare the two.
2. If they match, click **"RSA Key Matches"**
3. If they **don't match, do NOT click confirm! This could be a man-in-the-middle attack** — someone might be impersonating your server. Check your network immediately.

> The fingerprint is only verified once per backend; it will be remembered afterward.

### Step 4: Enter Access Key

After verification, a password prompt appears. Enter your backend access key and click **Confirm**.

If the key is correct, the frontend connects successfully — the connection status icon changes to **Connected**.

---

## After Connecting

Once connected, you'll see six top-level tabs:

| Tab | Purpose |
|---|---|
| 🔗 **Connection** | Manage backend connection (add/disconnect/view logs) |
| 📦 **All Managers** | View all saved manager configs, create/load/delete |
| ⚡ **Running Managers** | View loaded instances and their status, open console |
| 🧰 **Toolbox** | (Not yet available) |
| ⚙️ **Settings** | (Not yet available) |
| ℹ️ **About** | Software info, changelog, credits |

> Most operations happen in **All Managers** and **Running Managers**. You'll learn how to use them in **Server Manager Usage**.

---

## How to Disconnect

Go to the **Connection** tab and click **Disconnect**. All previously loaded data will be cleared. Reconnecting will reload everything from the backend.

## Why Public Access?

The backend usually runs on your server, and the frontend runs on your personal computer. If they're on the same local network, just use the local IP address.

But if your server is at home and you're outside (or the server is in a data center/cloud), you need to make the backend accessible from the internet.

Here are several common solutions, from simplest to most complex.

---

## Option 1: Port Forwarding (if you have a public IP)

If your ISP gives you a public IP address (static or dynamic), this is the most direct way.

### Steps

1. **Log into your router's admin interface** (usually `192.168.1.1` or `192.168.0.1`)
2. Find **Port Forwarding** or **Virtual Server**
3. Add a rule:
   - External port: `20000` (or whatever port you set)
   - Internal IP: your server's local IP (e.g., `192.168.1.100`)
   - Internal port: `20000`
   - Protocol: `TCP`
4. Save and apply

Then enter your router's public IP in the frontend to connect.

> If your public IP is dynamic (changes on router reboot), use **DDNS (Dynamic DNS)** to get a domain name instead.

### Notes

- Some ISPs (like certain mobile/community broadband) don't provide public IPs — you may need to call and request one
- Opening a port to the internet means anyone can try to connect — **use a strong password**

---

## Option 2: NAT/Tunnel Services (if you don't have a public IP)

If you don't have a public IP or don't want to configure your router, use a tunnel service.

How it works: your server connects to a relay server (with a public IP), the frontend connects to the same relay, and data is forwarded between them.

### Recommended Tunnel Tools

| Tool | Features | Price |
|---|---|---|
| **SakuraFrp** | Many Chinese nodes, fast, free tunnels available, easy setup | Free/Paid |
| **OpenFrp** | Chinese FRP provider, free nodes available, convenient setup | Free/Paid |
| **Tailscale** | Based on WireGuard, extremely simple setup, direct peer-to-peer connection | Free for personal use |
| **ZeroTier** | Similar to Tailscale, powerful, supports custom networks | Free for personal use |
| **FRP** | Open-source, battle-tested, requires your own public server | Needs your own server |
| **Ngrok** | Well-known globally, free tier has bandwidth/connection limits | Free/Paid |

### Quick Example: Using SakuraFrp

1. Register at [SakuraFrp](https://www.natfrp.com/)
2. Download the client to your server
3. Create a tunnel:
   - Local address: `127.0.0.1`
   - Local port: `20000` (your backend port)
   - Remote port: pick one (e.g., `20000`)
4. Start the tunnel — you'll get a public address like `xxx.natfrp.com:20000`
5. Enter this address in the frontend

### Quick Example: Using Tailscale

1. Install [Tailscale](https://tailscale.com/) on both your server and your computer
2. Log into the same account on both devices
3. They'll automatically form a virtual network, each getting a `100.x.x.x` virtual IP
4. Enter the server's Tailscale IP and backend port in the frontend

Tailscale's advantage: **no port forwarding needed**, end-to-end encryption, and data doesn't go through third-party servers (direct connection in most cases).

---

## Option 3: Cloud Server

If your backend runs on a cloud server (Alibaba Cloud, Tencent Cloud, AWS, etc.), it usually has a public IP already. You just need to:

1. **Open the backend port** in the cloud server's firewall/security group (default `20000`, TCP)
2. Connect using the cloud server's public IP or domain

> This is the most hassle-free option if you already have a cloud server.

---

## Security Reminders

No matter which method you use to expose the backend to the internet:

- ⚠️ **Use a strong access key** (at least 16 characters, mix of upper/lowercase letters and numbers)
- ⚠️ **Don't use the default port `20000`** — choose an uncommon high port
- ⚠️ **Always verify the RSA public key fingerprint** on first connection to prevent man-in-the-middle attacks
- ⚠️ If using port forwarding, consider a **firewall** to restrict which IPs can connect

> If it's just for your own use, **Tailscale or ZeroTier** are the most recommended options — no open ports needed, maximum security.

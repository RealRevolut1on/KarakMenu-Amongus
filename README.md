<p align="center">
  <b>Status: 🟢</b>
</p>

<p align="center">
  <b>A custom Among Us mod menu with a hand-built GUI, deep game hooks and a built-in anticheat for all lobbies.</b>
</p>

<!-- omit in toc -->
# 🧭 Quick Navigation

- [📦 Download Versions](#-download-versions)
- [🔧 Installation Guide](#-installation-guide)
  - [🪟 Windows Setup](#-windows-setup)
  - [🐧 Linux / SteamOS](#-linux--steamos)
- [🔥 Menu Features](#-menu-features)
- [🎮 Using the Menu](#-using-the-menu)
- [🛠️ Building From Source](#%EF%B8%8F-building-from-source)
- [💬 Troubleshooting & FAQ](#-troubleshooting--faq)
- [📜 Credits](#-credits)
- [⚠️ Disclaimer](#%EF%B8%8F-disclaimer)

---

# 📦 Download Versions

| Mod Version | Among Us - Version | Link |
|-------------|--------------------|------|
| Beta V1.6 | 2026.9.29 | [Download](https://github.com/RealRevolut1on/KarakMenu-Amongus/releases/latest) |

> **Note:** the release is a single `KarakMenu.dll`. You still need BepInEx installed — see below.

# 🔧 Installation Guide

## 🪟 Windows Setup

1. Run among us normally and go into task manager, look at among us, if it says "Among Us (32-bit)" then download and extract 

Your folder should end up looking like this:

```
Among Us/
├─ Among Us.exe
├─ winhttp.dll
├─ doorstop_config.ini
└─ BepInEx/
   ├─ core/
   ├─ config/
   │  └─ KarakMenu.cfg
   └─ plugins/
      └─ KarakMenu.dll
```

## 🐧 Linux / SteamOS

1. Run Among Us through **Proton** (or Wine).
   - **Steam:** right-click Among Us → `Properties` → `Compatibility` → enable `Force the use of a specific Steam Play compatibility tool`.

2. Set up BepInEx following the official [Proton / Wine guide](https://docs.bepinex.dev/articles/advanced/proton_wine.html).

3. Add the DLL override to your launch options:

   ```
   WINEDLLOVERRIDES="winhttp.dll=n,b" %command%
   ```

4. Continue with the Windows steps above — the folder layout is identical inside the Proton prefix.

5. If you get `Unable to execute IL2CPP chainloader` or instant crashes, try:

   ```
   PROTON_NO_ESYNC=1 PROTON_USE_WINED3D=1 WINEDLLOVERRIDES="winhttp.dll=n,b" %command%
   ```

# 🔥 Menu Features

- **Combat** — no-clip, invert controls, immortality, custom speed, teleport to cursor or to any player, vent teleporting and spam-teleport
- **Visuals** — see roles, ghosts, player info and lobby info; live kill-cooldown estimates; vent highlighting; tracers and minimap tracking by team; freecam, zoom-out and spectate
- **Roles** — kill reach, auto-kill, shapeshifter/phantom/tracker/engineer/scientist tweaks, task menu, role swapping
- **Players** — per-player rows with speed, shapeshift, disco, jail, kick and vote-kick actions
- **Ship** — sabotage control, door and vent tools, auto-report, spawn spore clouds, unlock vents
- **System** — search bar, game modes, passive unlocks, config editor, rebindable keys, console and memory tools
- **AC** — a full server-side style anticheat for your own lobby: RPC rate limiting, level and name checks, platform validation, ban lists and configurable punishments

For the complete, up-to-date list, see [FEATURES.md](FEATURES.md).

# 🎮 Using the Menu

- **DELETE** opens and closes the menu — rebind it in the game at `System --> Keyinds`
- The menu is split into categories on the left and tabs on the top of each category.
- Every toggle can be bound to a hotkey from the **Keybinds** tab, and the whole profile can be saved/loaded.
- **Passive** features stay on by default on purpose — turning them off is what usually causes disconnects and missing cosmetics.
- **AC** only ever acts on clients connected to **your** lobby. It is not a client-side shield.

# 🛠️ Building From Source

Requirements: .NET SDK 6.0 or newer and a copy of the game (for the reference assemblies).

```bash
git clone https://github.com/RealRevolut1on/KarakMenu-Amongus.git
cd KarakMenu-Amongus
dotnet build src/KarakMenu.csproj -c Debug
```

- The build output lands in `src/bin/Debug/net6.0/KarakMenu.dll`.
- Building with `-c Release` runs the obfuscation pipeline — don't ship that unless you know what you're doing.
- NuGet packages (`BepInEx.Unity.IL2CPP`, `AmongUs.GameLibs.Steam`) restore automatically.

# 💬 Troubleshooting & FAQ

Click to expand each topic:

<details>

<summary><h2>❗ The menu doesn't show up</h2></summary>

Check, in order:

1. **Is BepInEx working at all?** You should see a console window when the game starts. If not, BepInEx is not installed correctly (or your launcher is bypassing `winhttp.dll`).
2. **Is the DLL in the right place?** It must be `BepInEx\plugins\KarakMenu.dll`, not in `BepInEx\core` and not next to `Among Us.exe`.
3. **Check the console** for a red `KarakMenu` line at startup. A load error there usually means a version mismatch.
4. **Is another mod clashing?** Remove other plugins from `BepInEx\plugins` and try again.
5. **Keybind** — the default is `DELETE`. If you changed `GUI.Keybind`, that's the key you need.

</details>

<details>

<summary><h2>❗ My game version is newer than the supported one</h2></summary>

Among Us updates break IL2CPP offsets, so KarakMenu only supports the build listed in [Download Versions](#-download-versions). If the game updated, either:

- wait for a KarakMenu release for the new build, or
- downgrade Among Us to a supported build.

Running a mismatched build usually starts fine and then misbehaves (dead buttons, broken tabs, odd crashes) instead of failing cleanly.

</details>

<details>

<summary><h2>👾 I found a bug or want a feature</h2></summary>

Open a new [GitHub issue](https://github.com/RealRevolut1on/KarakMenu-Amongus/issues/new) and include:

- your Among Us build (`2026.9.29`, etc.)
- your KarakMenu version
- the contents of the BepInEx console (or `BepInEx\LogOutput.log`)
- what you did and what happened

Reproducible steps get fixed; "it doesn't work" usually doesn't.

</details>

<details>

<summary><h2>👨‍💻 I want to contribute</h2></summary>

You'll want some C# and basic Unity knowledge — Among Us is a Unity IL2CPP game, and KarakMenu hooks it with BepInEx 6 and Harmony.

Useful resources:

- [BepInEx docs](https://docs.bepinex.dev) — plugin structure and IL2CPP chainloader
- [Harmony docs](https://harmony.pardeike.net) — the patching library used everywhere in `src\Patches`
- [sus.wiki / among-us-protocol](https://github.com/roobscoob/among-us-protocol) — the network protocol reference
- The game's own `dump.cs` (from an IL2CPP dumper) — the source of truth for every class and method signature

Fork the repo, make your changes in a branch, and open a pull request with a short description of what changed and why.

</details>

# 📜 Credits

- **[BepInEx](https://github.com/BepInEx/BepInEx)** — IL2CPP modding framework and runtime patching
- **[Harmony](https://github.com/pardeike/Harmony)** — method patching
- **[Reactor docs](https://docs.reactor.gg)** and the broader Among Us modding community — protocol notes, tooling and offsets
- **[sus.wiki](https://github.com/roobscoob/among-us-protocol)** — network protocol reference
- **Everyone who reported bugs, sent offsets and tested builds** — this menu is the result of that feedback loop

---

# ⚠️ Disclaimer

This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.

This mod is not intended to be used in any manner that interferes with Innersloth's services, Innersloth's operation of Among Us, the integrity or availability of the game, or the normal gameplay experience of other players. The creator does not endorse, encourage, or condone using this mod to disrupt games, negatively affect other users, bypass rules or protections, or gain an unfair advantage in any setting where such use is prohibited. Any misuse is solely the responsibility of the user. Usage of this mod can violate the terms of service of Among Us, which may lead to punitive action. The creator is not responsible for any consequences you may face due to usage. Use at your own risk.

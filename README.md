<p align="center">
  <b>Status: 🟢</b>
</p>

<p align="center">
  <b>A custom Among Us mod menu with a GUI, deep game hooks and a built-in anticheat for all lobbies.</b>
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

<a id="installation-guide"></a>
# 🔧 Installation Guide
Btw this most likely doesn't work on cracked Among Us, you can try if you want. (Just pay the devs $5 man)
## 🪟 Windows Setup

1. **What to download and extract**
   - Run Among Us normally and open Task Manager (`Ctrl + Shift + Esc`).
   - Find Among Us in the process list:
     - If it says `Among Us (32-bit)`, download the **x86** version of BepInEx.
     - If it says just `Among Us`, download the **x64** version of BepInEx.
   - Close Among Us completely before continuing.

2. **How to find your Among Us folder:**

   - **Steam:**
     1. Go to your Steam Library.
     2. Right-click Among Us in the left sidebar.
     3. Go to `Manage` → `Browse local files`.

   - **Epic Games:**
     1. Go to your Library.
     2. Click the three dots on Among Us and select `Manage`.
     3. Click the small folder icon next to `Installation`.

   - **itch.io:**
     1. Open the itch.io app.
     2. Right-click Among Us in your Library.
     3. Select `Manage` → `Open folder in Explorer`.
     4. Open the `AmongUsItch/Itch` folder.

   - **Xbox App / Microsoft Store:**
     1. Open the Xbox app.
     2. Find Among Us in your Library.
     3. Right-click it and select `Manage`.
     4. Go to the `Files` tab.
     5. Click `Browse...`.
     6. Open the `Among Us/Content` folder.

3. **Installing BepInEx:**

   Extract the contents of the downloaded BepInEx ZIP directly into the folder containing `Among Us.exe`.

   Your folder should end up looking like this:

   ( `KarakMenu.cfg` may or may not exist before the first launch.)

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

4. Go to [🎮 Using the Menu](#-using-the-menu).

5. **(Optional but recommended)** Install [HydraMenu](https://github.com/MrDiamond64/Hydra). Download the `.dll` file from its latest release and place it inside the `plugins` folder (alongside `KarakMenu.dll`). You can then use the `INS` key to open HydraMenu.

6. Have fun! :)

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

Among Us updates break IL2CPP offsets, so KarakMenu only supports the build listed in [📦 Download KarakMenu](https://github.com/RealRevolut1on/KarakMenu/releases). If the game updated, either:

- wait for a KarakMenu release for the new build, or:
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

## ⚠️ Disclaimer & Responsible Use

**KarakMenu is an unofficial, community-made Among Us modification.**

This project is intended to help players organize, configure, and manage private Among Us events.

### Responsible Use

- Only use gameplay-changing features in lobbies where all participants have been informed and consent to their use.
- Do not use this mod to cheat, exploit, grief, harass, or intentionally disrupt other players' experiences.
- Do not use features to gain an unfair advantage in public matchmaking or interfere with unsuspecting players.
- Follow the official [Among Us Code of Conduct](https://www.innersloth.com/code-of-conduct/) and [Mod Policy](https://www.innersloth.com/among-us-mod-policy/).
- Any gameplay-changing functionality used on official servers must comply with Innersloth's mod registration requirements.

**Unofficial Project Notice**

This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.

Users are responsible for how they use this software. This disclaimer does not grant permission to violate Innersloth's policies or override any applicable rules.

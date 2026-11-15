# 📋 Features

## 👱 Player

| Feature | Description | Type | Default|
|------------|-------------|------|--------|
| NoClip     | Allows you to walk through walls like a ghost | Toggle | Off
| Boost Speed | Doubles your player's speed | Toggle | Off |

#### Murder

| Feature | Description | Type | Default|
|------------|-------------|------|--------|
| Kill Player | Select a player to kill them immediatly | Menu |
| Kill All Crewmates | Kill all crewmates immediatly | Button |
| Kill All Impostors | Kill all impostors immediatly | Button |
| Kill All | Kill all players immediatly | Button |

#### Teleport

| Feature | Description | Type | Default|
|------------|-------------|------|--------|
| to Cursor | Teleport by right-clicking with your cursor. Works best with the ZoomOut feature | Toggle | Off |
| to Player | Teleport to a player's position by selecting them | Menu |

## 👁️ ESP

KarakMenu's ESP features are completely client-side, and thus undetectable by anticheat

| Feature | Description | Type | Default|
|------------|-------------|------|--------|
| See Roles | See every player's role through their nametag | Toggle | Off |
| See Ghosts | Allows you to see ghosts, protections, and ghost chat even if you are alive | Toggle | Off
| No Shadows | Removes all shadows, allowing you to see during blackouts and even through walls<br>Also, lets you see through spore clouds in the Fungle Jungle | Toggle | Off |
| Reveal Votes | Reveals votes as they are being cast rather than at the end of the meeting<br>Also, lets you see colored votes even if votes are set to anonymous | Toggle | Off |
| Always Chat | Keeps the chat icon always enabled, allowing you to chat at any time (even while not in a meeting or the lobby) | Toggle | Off |

#### Camera
    
| Feature | Description | Type | Default|
|------------|-------------|------|--------|
| Zoom Out | Allows you to zoom-out the player's camera using your mouse's scrollwheel | Toggle | Off
| Spectate | Allows you to pick a player to spectate with your camera | Menu |
| Freecam | Allows you to freely move your camera around without also moving your player | Toggle | Off |

#### Tracers

| Feature | Description | Type | Default|
|------------|-------------|------|--------|
| Crewmates | Shows tracer lines for alive crewmates (color: cyan) | Toggle | Off |
| Impostors | Shows tracer lines for alive impostors (color: red) | Toggle | Off
| Ghosts | Shows tracer lines for ghosts (color: white) | Toggle | Off |
| Dead Bodies | Shows tracer lines for dead bodies on the ground (color: yellow) | Toggle | Off |
| Color-based | Changes the color of tracer lines to the color of their players| Toggle | Off |

#### Minimap

| Feature | Description | Type | Default|
|------------|-------------|------|--------|
| Crewmates | Changes the map so that it shows the position of every alive crewmate (color: cyan) | Toggle | Off |
| Impostors | Changes the map so that it shows the position of every alive impostor (color: red) | Toggle | Off
| Ghosts | Changes the map so that it shows the position of every ghost (color: white) | Toggle | Off |
| Color-based | Changes the color of map icons to the color of their players | Toggle | Off |

## 🎭 Roles

| Feature | Description | Type | Default |
|------------|-------------|------|----|
| Set Fake Role | Change your current role to any role you want<br>(Shapeshifter & Phantom are disabled by default to prevent getting detected by the anticheat) | Menu |

#### Impostor

| Feature | Description | Type | Default |
|------------|-------------|------|----|
| Kill Anyone | Allows you to kill anyone, regardless if they are protected, impostors, crawling in a vent, or a ghost | Toggle | Off |
| No Kill Cooldown | Removes the cooldown period after kills, allowing you to spam-kill as much as you please | Toggle | Off |
| Kill Reach | Allows you to kill players regardless of how far they are on the map | Toggle | Off |

#### Phantom

| Feature | Description | Type | Default |
|------------|-------------|------|----|
| Kill While Vanished | Allows you to kill while invisible | Toggle | Off |

#### Shapeshifter

| Feature | Description | Type | Default |
|------------|-------------|------|----|
| No Ss Animation | Removes the shapeshift animation, making shapeshifting much quicker | Toggle | Off |
| Endless Ss Duration | Allows you to remain shapeshifted forever | Toggle | Off |

#### Crewmate

| Feature | Description | Type | Default |
|------------|-------------|------|----|
| Complete My Tasks | Complete all of your crewmate tasks immediatly | Button |

#### Tracker

| Feature | Description | Type | Default |
|------------|-------------|------|----|
| Endless Tracking | Allows you to track another player forever | Toggle | Off |
| No Track Delay | Removes the short delay between the tracked player and their icon on your tracker map | Toggle | Off |
| No Track Cooldown | Removes the cooldown period after tracking someone | Toggle | Off |

#### Engineer

| Feature | Description | Type | Default |
|------------|-------------|------|----|
| Endless Vent Time | Allows you to remain inside a vent forever despite being an engineer | Toggle | Off |
| No Vent Cooldown | Removes the cooldown period after coming out of a vent | Toggle | Off |

#### Scientist

| Feature | Description | Type | Default |
|------------|-------------|------|----|
| Endless Battery | The battery on your vitals panel will never run out | Toggle | Off |
| No Vitals Cooldown | Removes the cooldown period after closing vitals panel  | Toggle | Off |
    
## 🚀 Ship

| Feature | Description | Type | Default |
|------------|-------------|------|----|
| Unfixable Lights | Disables lights completely (they cannot be fixed manually by players)<br>You can enable them again by clicking the button | Toggle | Off |
| Report Body | Report any player as a dead body to start a meeting | Button |
| Close Meeting | Forcefully closes the meeting window (only for you), allowing you to move and interact with the game during meetings | Button |

#### Sabotage

KarakMenu's Sabotage features work even if you aren't impostor and are subject to no cooldown.

Moreover, different sabotages can be enabled at the same time, and they even work during meetings.

| Feature | Description | Type | 
|------------|-------------|------|
| Reactor | Allows you to enable/disable Reactor sabotage | Toggle | Off |
| Oxygen | Allows you to enable/disable Oxygen sabotage | Toggle | Off |
| Lights | Allows you to enable/disable Lights sabotage | Toggle | Off |
| Comms | Allows you to enable/disable Communications sabotage | Toggle | Off |
| Doors | Immediatly locks all doors on the ship | Button |
| MushroomMixup | Induces Mushroom Mixup sabotage on Fungle map | Button |

#### Vents

| Feature | Description | Type | Default|
|------------|-------------|------|--------|
| UseVents | Allows you to use vents even if you are not an impostor or an engineer | Toggle | Off
| KickVents | Forcefully kicks all players from vents | Button |
| WalkInVents | Allows you to move and interact with the game even though you are inside of a vent<br>This gives you a sort of invisibility until you disable the setting and leave the vent<br>(*Some activites such as killing will forcefully make you visible again*) | Toggle | Off

## 💤 Passive

These features are constantly running in the background and **cannot be disabled to avoid problems.**

| Feature | Description | Type | Default|
|------------|-------------|------|--------|
| Cosmetic Access | Uses the cosmetic assets already present in your local game files — hats, visors, skins, pets, nameplates and bundles. Purely client-side; nothing is uploaded or unlocked on your account | Toggle | On |
| Disconnect Handling | Skips the local "left early" timer after a disconnect | Toggle | On |
| Extra UI Features | Enables local UI features such as free chat, the friend list and custom names | Toggle | On |

## 📃 Config

Most settings live in `BepInEx/config/KarakMenu.cfg` and can also be edited from the **System → Config** tab.

| Config | Description | Type | Default |
|---|---|---|---|
| Config.ConfigEditor | The executable used by "Open Config". Any program works, a text editor is recommended | String | notepad.exe |
| GUI.Keybind | Keyboard key that toggles the GUI. Keycodes from this [list](https://docs.unity3d.com/Packages/com.unity.tiny@0.16/api/Unity.Tiny.Input.KeyCode.html) | String | Delete |
| GUI.Color | Custom GUI color, as an HTML color code | String | |
| GUI.OpenOnMouse | Opens the GUI at the current mouse position | Boolean | false |
| GUI.KeepSubwindowsOpen | Closing the GUI keeps its subwindows open | Boolean | false |
| GUI.Opacity | GUI opacity (0.15 = nearly invisible, 1 = opaque) | Single | 0.6 |
| Privacy.HideDeviceId | Hides your unique deviceId from Among Us | Boolean | true |
| Privacy.NoTelemetry | Stops Among Us from sending Unity Analytics data to Innersloth | Boolean | true |
| Profile.AutoLoadProfile | Automatically loads your saved toggle/keybind profile at startup | Boolean | false |
| Spoofing.Level | Level shown to other players in online games. Integers between 1 and 100001 | String | 76 |
| Spoofing.Platform | Platform shown to other players in online lobbies. Values from this [list](https://skeld.js.org/enums/constant.Platform.html) | String | |

## Other relevant features of KarakMenu

- A custom GUI that opens with **DELETE** by default, fully rebindable
- Keybind profiles: bind any toggle to a key, then save and load the whole set
- A built-in **AC** tab — per-RPC checks, rate limiting, ban lists and configurable punishments, applied to clients in **your** lobby
- Memory tools for searching and editing live game values
using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine.SceneManagement;
using System;
using UnityEngine;
using UnityEngine.Analytics;
using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace KarakMenu;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class KarakMenu : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static KarakMenu Plugin;
    public new static ManualLogSource Log;
    public static readonly string ProfilePath = Path.Combine(Paths.ConfigPath, "KarakProfile.txt");

    public static MenuUI menuUI;
    public static ConsoleUI consoleUI;
    public static RolesUI rolesUI;
    public static OverloadUI overloadUI;
    public static DoorsUI doorsUI;
    public static TasksUI tasksUI;
    public static ProtectUI protectUI;
    public static KeybindListener keybindListener;

    public static string karakVersion = "3.2.0";
    public static List<string> supportedAU = new List<string> { "2026.9.29", "2026.6.5", "2026.3.31" };
    public static bool isPanicked = false;
    public static bool inStealthMode = false;

    public static ConfigEntry<string> menuKeybind;
    public static ConfigEntry<string> menuHtmlColor;
    public static ConfigEntry<bool> menuOpenOnMouse;
    public static ConfigEntry<bool> menuKeepSubwindowsOpen;
    public static ConfigEntry<float> menuOpacity;
    public static ConfigEntry<string> spoofLevel;
    public static ConfigEntry<string> spoofPlatform;
    public static ConfigEntry<bool> spoofDeviceId;
    public static ConfigEntry<bool> noTelemetry;
    public static ConfigEntry<string> guestFriendCode;
    public static ConfigEntry<bool> guestMode;
    public static ConfigEntry<bool> autoLoadProfile;
    public static ConfigEntry<string> configEditor;
    public static ConfigEntry<int> adaptMaxStrength;
    public static ConfigEntry<float> adaptMaxCooldown;
    public static ConfigEntry<float> attackLogDelay;
    public static ConfigEntry<int> defaultStrength;
    public static ConfigEntry<float> defaultCooldown;
    public static ConfigEntry<int> killSwitchLvl;

    public override void Load()
    {
        Log = base.Log;
        Plugin = this;

        // Creates the anti-cheat ban/word lists on disk before anything reads them
        Anticheat.Initialize();

        // Loads config settings
        menuKeybind = Config.Bind("KarakMenu.GUI",
                                "Keybind",
                                "Delete",
                                "The keyboard key used to toggle the GUI on and off. List of supported keycodes: https://docs.unity3d.com/Packages/com.unity.tiny@0.16/api/Unity.Tiny.Input.KeyCode.html");

        menuHtmlColor = Config.Bind("KarakMenu.GUI",
                                "Color",
                                "",
                                "A custom color for your KarakMenu GUI. Supports html color codes");

        menuOpenOnMouse = Config.Bind("KarakMenu.GUI",
                                "OpenOnMouse",
                                false,
                                "When enabled, the KarakMenu GUI will always be opened at the current mouse position");

        menuKeepSubwindowsOpen = Config.Bind("KarakMenu.GUI",
                                "KeepSubwindowsOpen",
                                false,
                                "When enabled, closing the KarakMenu GUI will not automatically close its subwindows");

        menuOpacity = Config.Bind("KarakMenu.GUI",
                                "Opacity",
                                0.6f,
                                new ConfigDescription(
                                    "The opacity of the KarakMenu GUI (0.15 = nearly invisible, 1 = opaque)",
                                    new AcceptableValueRange<float>(0.15f, 1f)
                                ));

        autoLoadProfile = Config.Bind("KarakMenu.Profile",
                                "AutoLoadProfile",
                                false,
                                "When enabled, your saved keybind and toggle profile will be automatically loaded at game startup");

        configEditor = Config.Bind("KarakMenu.Config",
                                "ConfigEditor",
                                "notepad.exe",
                                "The program used to open the config file when using the Open Config toggle. Can be any executable, but using a text editor is recommended");

        // GuestMode config settings are commented out as the cheats are broken in latest updates

        // guestMode = Config.Bind("KarakMenu.GuestMode",
        //                         "GuestMode",
        //                         false,
        //                         "When enabled, a new guest account will generate every time you start the game, allowing you to bypass account bans and PUID detection");

        // guestFriendCode = Config.Bind("KarakMenu.GuestMode",
        //                         "FriendName",
        //                         "",
        //                         "The username that will be used when setting a friend code for your guest account. IMPORTANT: Can only be used with GuestMode, needs to be ≤ 10 characters, and cannot include special characters/discriminator (#1234)");

        spoofLevel = Config.Bind("KarakMenu.Spoofing",
                                "Level",
                                "",
                                "A custom player level to display to others in online games to hide your actual platform. IMPORTANT: Custom levels can only be within 1 and 100001. Decimal numbers will not work");

        spoofPlatform = Config.Bind("KarakMenu.Spoofing",
                                "Platform",
                                "",
                                "A custom gaming platform to display to others in online lobbies to hide your actual platform. List of supported platforms: https://skeld.js.org/enums/_skeldjs_constant.Platform.html");

        spoofDeviceId = Config.Bind("KarakMenu.Privacy",
                                "HideDeviceId",
                                true,
                                "When enabled, it will hide your unique deviceId from Among Us, which could potentially help bypass hardware bans in the future");

        noTelemetry = Config.Bind("KarakMenu.Privacy",
                                "NoTelemetry",
                                true,
                                "When enabled, it will stop Among Us from collecting analytics of your games and sending them to Innersloth using Unity Analytics");

        // adaptMaxStrength = Config.Bind("KarakMenu.Overload",
        //                         "AdaptMaxStrength",
        //                         18000,
        //                         new ConfigDescription(
        //                             "Maximum total number of RPCs sent during one overload cycle in AutoAdapt mode. Automatically divided between targets and reduced based on ping. IMPORTANT: Only goes from 1 to 100K RPCs",
        //                             new AcceptableValueRange<int>(1, 100000)
        //                         ));

        // adaptMaxCooldown = Config.Bind("KarakMenu.Overload",
        //                         "AdaptMaxCooldown",
        //                         1f,
        //                         new ConfigDescription(
        //                             "Maximum time (in seconds) for one full overload cycle to complete in AutoAdapt mode. Automatically distributed across targets (more targets = shorter delay per target). IMPORTANT: Only goes from 0s to 10s",
        //                             new AcceptableValueRange<float>(0f, 10f)
        //                         ));

        // attackLogDelay = Config.Bind("KarakMenu.Overload",
        //                         "AttackLogDelay",
        //                         2f,
        //                         "Minimum time (in seconds) between attack logs in normal (non-verbose) mode");

        // defaultStrength = Config.Bind("KarakMenu.Overload",
        //                         "DefaultStrength",
        //                         18000,
        //                         new ConfigDescription(
        //                             "Default number of malformed RPCs sent to each target during an overload cycle. Overridden if AutoAdapt mode is enabled. IMPORTANT: Only goes from 1 to 100K RPCs",
        //                             new AcceptableValueRange<int>(1, 100000)
        //                         ));

        // defaultCooldown = Config.Bind("KarakMenu.Overload",
        //                         "DefaultCooldown",
        //                         1f,
        //                         new ConfigDescription(
        //                             "Default cooldown (in seconds) between each target during an overload cycle. Overridden if AutoAdapt mode is enabled. IMPORTANT: Only goes from 0s to 10s",
        //                             new AcceptableValueRange<float>(0f, 10f)
        //                         ));

        // killSwitchLvl = Config.Bind("KarakMenu.Overload",
        //                         "DefaultKillSwitchLevel",
        //                         1,
        //                         new ConfigDescription(
        //                             "Default level used by kill switch. Each level adds 500 ms to the max allowed ping before overload stops. Helps avoid lagging / disconnects. IMPORTANT: Only goes from level 1 (500 ms) to 6 (3000 ms)",
        //                             new AcceptableValueRange<int>(1, 6)
        //                         ));

        // Enabled by default
        CheatToggles.unlockFeatures = true;
        CheatToggles.freeCosmetics = true;
        CheatToggles.avoidPenalties = true;

        // Enabled by default
        CheatToggles.rgbMode = true;
        CheatToggles.stealthMode = false;

        // Enabled by default
        CheatToggles.olAutoAdapt = true;
        CheatToggles.olKillSwitch = true;
        CheatToggles.olAutoStop = true;
        CheatToggles.olAutoClear = true;
        CheatToggles.olLogStartStop = true;
        CheatToggles.olLogAttack = true;
        CheatToggles.olLogAddRemove = true;
        CheatToggles.olLogDisconnect = true;

        Harmony.PatchAll();

        // UI
        menuUI = AddComponent<MenuUI>();
        consoleUI = AddComponent<ConsoleUI>();
        doorsUI = AddComponent<DoorsUI>();
        tasksUI = AddComponent<TasksUI>();
        protectUI = AddComponent<ProtectUI>();
        rolesUI = AddComponent<RolesUI>();
        AddComponent<DetectUI>();
        // overloadUI = AddComponent<OverloadUI>();

        // Components
        keybindListener = AddComponent<KeybindListener>();

        KarakTheme.Opacity = menuOpacity.Value;

        // Disables Telemetry (haven't fully tested if it works, but according to Unity docs it should)
        if (noTelemetry.Value)
        {
            Analytics.enabled = false;
            Analytics.deviceStatsEnabled = false;
            PerformanceReporting.enabled = false;
        }

        // Create profile file if it is missing
        if (!File.Exists(ProfilePath))
        {
            CheatToggles.SaveTogglesToProfile();
        }

        // Auto load profile on start if needed
        if (autoLoadProfile.Value)
        {
            CheatToggles.LoadTogglesFromProfile();
        }

        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((scene, _) =>
        {
            if (scene.name == "MainMenu")
            {
                // Clears any out-of-range NumImpostors/PlayerSpeedMod (and the
                // noOptionsLimits flag) left over from a previous match, before
                // it can poison the next "Create Game" — see KarakCheats.ResetOptionLimits.
                KarakCheats.ResetOptionLimits();
            }

            if (scene.name == "MainMenu" && !(inStealthMode || isPanicked))
            {
                // Warns about unsupported AU versions
                if (!supportedAU.Contains(Application.version))
                {
                    Utils.ShowPopup("\nThis version of KarakMenu and this version of Among Us are incompatible\n\nInstall the right version to avoid problems");
                }
            }
        }));
    }
}

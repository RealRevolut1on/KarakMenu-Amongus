using HarmonyLib;
using InnerNet;

namespace KarakMenu.AC;

// ---------------------------------------------------------------------------
// Platform validation.
// Replaces KarakMenu's old reflection-based IsPlatformValid check, which only
// looked at the platform id/name and never validated the XUID/PSID pairing.
// ---------------------------------------------------------------------------

internal class PlatformSpoofer
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
    private static class PlatformSpoof
    {
        private static void Postfix(PlayerControl __instance)
        {
            if (!Anticheat.IsEnabled || !Anticheat.CheckSpoofedPlatforms) return;
            if (__instance == null || __instance.Data == null) return;
            if (!Utils.isLobby) return;

            ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(__instance);
            if (clientData == null) return;

            PlatformSpecificData platformData = clientData.PlatformData;
            if (platformData == null) return;

            if (!IsValidPlatform(platformData))
            {
                Anticheat.Flag(__instance, $"{clientData.PlayerName} was detected with spoofed platform information. Platform: {platformData.Platform}, Platform name: {platformData.PlatformName}, XUID: {platformData.XboxPlatformId}, PSID: {platformData.PsnPlatformId}.");
            }
        }
    }

    public static bool IsValidPlatform(PlatformSpecificData platform)
    {
        if (platform == null) return true;

        string platformName = platform.PlatformName;
        ulong xuid = platform.XboxPlatformId;
        ulong psid = platform.PsnPlatformId;

        switch (platform.Platform)
        {
            case Platforms.StandaloneEpicPC:
            case Platforms.StandaloneSteamPC:
            case Platforms.StandaloneMac:
            case Platforms.StandaloneItch:
            case Platforms.IPhone:
            case Platforms.Android:
            // Platform ID 112 is used by Starlight (Android BepInEx)
            case (Platforms)112:
                if (IsGenericPlatformName(platformName) && xuid == 0 && psid == 0) return true;
                break;

            case Platforms.StandaloneWin10:
                if (IsGenericPlatformName(platformName) && xuid != 0 && psid == 0) return true;
                break;

            case Platforms.Xbox:
                // Xbox gamertags are 3-16 characters
                if (!IsGenericPlatformName(platformName) && platformName != null
                    && platformName.Length >= 3 && platformName.Length <= 16 && xuid != 0 && psid == 0) return true;
                break;

            case Platforms.Playstation:
                if (!IsGenericPlatformName(platformName) && xuid == 0 && psid != 0) return true;
                break;

            case Platforms.Switch:
                if (!IsGenericPlatformName(platformName) && xuid == 0 && psid == 0) return true;
                break;

            // On local lobbies every player has a platform id of 255
            case (Platforms)255:
                if (AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame) return true;
                break;
        }

        return false;
    }

    public static bool IsGenericPlatformName(string platformName) => platformName == "TESTNAME";
}

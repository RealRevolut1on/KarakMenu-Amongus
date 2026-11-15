using HarmonyLib;

namespace KarakMenu;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
public static class AmongUsClient_Update
{
    public static void Postfix()
    {
        KarakSpoof.SpoofLevel();

        // GuestMode cheats are commented out as they are broken in latest updates

        // Code to treat temp accounts the same as full accounts, including access to friend codes
        // if (!EOSManager.Instance.loginFlowFinished || !KarakMenu.guestMode.Value) return;
        // DataManager.Player.Account.LoginStatus = EOSManager.AccountLoginStatus.LoggedIn;

        // if (!string.IsNullOrWhiteSpace(EOSManager.Instance.FriendCode)) return;
        // var friendCode = KarakSpoof.spoofFriendCode();
        // var editUsername = EOSManager.Instance.editAccountUsername;
        // editUsername.UsernameText.SetText(friendCode);
        // editUsername.SaveUsername();
        // EOSManager.Instance.FriendCode = friendCode;
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
public static class AmongUsClient_OnGameJoined
{
    // Postfix patch of AmongUsClient.OnGameJoined to store the last joined game ID string
    public static string lastGameIdString = "";

    public static void Postfix(string gameIdString)
    {
        lastGameIdString = gameIdString;
        MoonwalkSync.Reset();
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
public static class AmongUsClient_CoStartGame
{
    public static void Postfix()
    {
        if (CheatToggles.logGameState)
            ConsoleUI.Log("Game started");
        Anticheat.ResetRoundState();
        KarakESP.ResetKillCooldownTracking();
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public static class AmongUsClient_OnGameEnd
{
    public static void Postfix(EndGameResult endGameResult)
    {
        Anticheat.ResetRoundState();
        if (CheatToggles.logGameState)
            ConsoleUI.Log($"Game ended with reason {endGameResult.GameOverReason}");
    }
}

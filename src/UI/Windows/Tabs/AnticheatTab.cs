using UnityEngine;

namespace KarakMenu;

public class AnticheatTab : ITab
{
    public string name => "General";

    // Error Kick fakes a client timeout: everyone else sees the player leave
    // "due to an error" instead of being kicked. the third punishment mode.
    private static readonly string[] ResponseOptions = { "Notify", "Kick", "Error Kick", "Ban" };

    public void Draw()
    {
        float colW = (MenuUI.ContentWidth - 12f) / 2f;

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawGeneral();
        GUILayout.EndVertical();

        GUILayout.Space(12);

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawLists();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        KarakUI.BeginSection("General");
        Anticheat.IsEnabled = KarakUI.Toggle(Anticheat.IsEnabled, "Enable Anti-Cheat", "ac-enabled");

        int mode = (int)Anticheat.DetectionResponseMode;
        mode = KarakUI.Combo("Response", mode, ResponseOptions);
        mode = Mathf.Clamp(mode, 0, ResponseOptions.Length - 1);
        Anticheat.DetectionResponseMode = (Anticheat.ResponseMode)mode;

        Anticheat.SendNotifications = KarakUI.Toggle(Anticheat.SendNotifications, "Send Notifications", "ac-notify");
        Anticheat.DiscardInvalidRpc = KarakUI.Toggle(Anticheat.DiscardInvalidRpc, "Discard Invalid RPCs", "ac-discard");
        Anticheat.CheckSpoofedPlatforms = KarakUI.Toggle(Anticheat.CheckSpoofedPlatforms, "Check Spoofed Platforms", "ac-platform");
        Anticheat.HideKickReason = KarakUI.Toggle(Anticheat.HideKickReason, "Hide Kick Reason", "ac-hidekick");
        Anticheat.KickCooldownSeconds = KarakUI.LabeledSlider("Kick Cooldown",
            Anticheat.KickCooldownSeconds, 0f, 10f, "0.0");

        if (KarakUI.Action("Reset Round State", "Reset"))
            Anticheat.ResetRoundState();
        KarakUI.EndSection();
    }

    private void DrawLists()
    {
        KarakUI.BeginSection("Identity & Lists");
        Anticheat.DetectInvalidFriendCodes = KarakUI.Toggle(Anticheat.DetectInvalidFriendCodes,
            "Detect Invalid Friend Codes", "ac-friendcode");
        Anticheat.UsePlayerBanList = KarakUI.Toggle(Anticheat.UsePlayerBanList,
            "Use Player Ban List", "ac-playerlist");
        Anticheat.UseNameBanList = KarakUI.Toggle(Anticheat.UseNameBanList,
            "Use Name Ban List", "ac-namelist");
        Anticheat.UseWordBanList = KarakUI.Toggle(Anticheat.UseWordBanList,
            "Use Word Ban List", "ac-wordlist");
        Anticheat.BanWordsLobbyOnly = KarakUI.Toggle(Anticheat.BanWordsLobbyOnly,
            "Banned Words — Lobby Only", "ac-lobbyonly");
        KarakUI.EndSection();
    }
}

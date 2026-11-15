using UnityEngine;

namespace KarakMenu;

public class AnticheatLimitsTab : ITab
{
    public string name => "Limits";

    public void Draw()
    {
        float colW = (MenuUI.ContentWidth - 12f) / 2f;

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawRateLimiting();
        GUILayout.EndVertical();

        GUILayout.Space(12);

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawLevelChecks();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawRateLimiting()
    {
        KarakUI.BeginSection("Rate Limiting");
        Anticheat.DetectInvalidRpcs = KarakUI.Toggle(Anticheat.DetectInvalidRpcs,
            "Extra KarakMenu Rules", "ac-rpcs");
        Anticheat.LimitRpcRate = KarakUI.Toggle(Anticheat.LimitRpcRate,
            "Limit RPC Rate", "ac-ratelimit");
        Anticheat.PacketRateLimit = Mathf.RoundToInt(KarakUI.LabeledSlider("Packet Rate Limit",
            Anticheat.PacketRateLimit, 25f, 1000f, "0"));
        KarakUI.EndSection();
    }

    private void DrawLevelChecks()
    {
        KarakUI.BeginSection("Level Checks");
        Anticheat.DetectPlayerLevels = KarakUI.Toggle(Anticheat.DetectPlayerLevels,
            "Detect Player Levels", "ac-detectlevels");
        Anticheat.MaxLevelThreshold = Mathf.RoundToInt(KarakUI.LabeledSlider("Max Level",
            Anticheat.MaxLevelThreshold, 100f, 10000f, "0"));

        GUILayout.Space(6);
        Anticheat.AutoKickLevels = KarakUI.Toggle(Anticheat.AutoKickLevels,
            "Auto Kick Levels", "ac-autokick");
        Anticheat.MinLevelThreshold = Mathf.RoundToInt(KarakUI.LabeledSlider("Min Level",
            Anticheat.MinLevelThreshold, 0f, 100f, "0"));
        KarakUI.EndSection();
    }
}

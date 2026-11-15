using UnityEngine;

namespace KarakMenu;

public class RolesTab : ITab
{
    public string name => "Roles";

    public void Draw()
    {
        float colW = (MenuUI.ContentWidth - 12f) / 2f;

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawImpostor();
        GUILayout.Space(10);
        DrawShapeshifter();
        GUILayout.Space(10);
        DrawTracker();
        GUILayout.EndVertical();

        GUILayout.Space(12);

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawEngineer();
        GUILayout.Space(10);
        DrawScientist();
        GUILayout.Space(10);
        DrawDetective();
        GUILayout.Space(10);
        DrawCrewmate();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawImpostor()
    {
        KarakUI.BeginSection("Impostor");
        CheatToggles.infiniteKillReach = KarakUI.Toggle(CheatToggles.infiniteKillReach, "Infinite Kill Reach");
        if (CheatToggles.infiniteKillReach)
            GUILayout.Label("<color=#ffaa00>Makes the kill button activate on any valid target regardless of distance. Host-only; on official servers the kill still requires host authority.</color>", GUIStylePreset.Label);
        if (KarakUI.Action("Flash Kill", "Kill")) CheatToggles.flashKill = true;
        GUILayout.Label("<color=#ff5555>Flash Kill and Auto Kill can cause instant anticheat bans on official servers.</color>", GUIStylePreset.Label);
        CheatToggles.autoKill = KarakUI.Toggle(CheatToggles.autoKill, "Auto Kill (nearest, every 0.25s)");
        KarakUI.EndSection();
    }

    private void DrawShapeshifter()
    {
        KarakUI.BeginSection("Shapeshifter");
        CheatToggles.noShapeshiftAnim = KarakUI.Toggle(CheatToggles.noShapeshiftAnim, "No Ss Animation");
        CheatToggles.endlessSsDuration = KarakUI.Toggle(CheatToggles.endlessSsDuration, "Endless Ss Duration");
        KarakUI.EndSection();
    }

    private void DrawCrewmate()
    {
        KarakUI.BeginSection("Crewmate");
        CheatToggles.showTasksMenu = KarakUI.Toggle(CheatToggles.showTasksMenu, "Show Tasks Menu");
        KarakUI.EndSection();
    }

    private void DrawTracker()
    {
        KarakUI.BeginSection("Tracker");
        CheatToggles.endlessTracking = KarakUI.Toggle(CheatToggles.endlessTracking, "Endless Tracking");
        CheatToggles.noTrackingDelay = KarakUI.Toggle(CheatToggles.noTrackingDelay, "No Track Delay");
        CheatToggles.noTrackingCooldown = KarakUI.Toggle(CheatToggles.noTrackingCooldown, "No Track Cooldown");
        CheatToggles.trackReach = KarakUI.Toggle(CheatToggles.trackReach, "Track Reach");
        KarakUI.EndSection();
    }

    private void DrawEngineer()
    {
        KarakUI.BeginSection("Engineer");
        CheatToggles.endlessVentTime = KarakUI.Toggle(CheatToggles.endlessVentTime, "Endless Vent Time");
        CheatToggles.noVentCooldown = KarakUI.Toggle(CheatToggles.noVentCooldown, "No Vent Cooldown");
        KarakUI.EndSection();
    }

    private void DrawScientist()
    {
        KarakUI.BeginSection("Scientist");
        CheatToggles.endlessBattery = KarakUI.Toggle(CheatToggles.endlessBattery, "Endless Battery");
        CheatToggles.noVitalsCooldown = KarakUI.Toggle(CheatToggles.noVitalsCooldown, "No Vitals Cooldown");
        KarakUI.EndSection();
    }

    private void DrawDetective()
    {
        KarakUI.BeginSection("Detective");
        CheatToggles.interrogateReach = KarakUI.Toggle(CheatToggles.interrogateReach, "Interrogate Reach");
        KarakUI.EndSection();
    }
}

using UnityEngine;

namespace KarakMenu;

public class ShipTab : ITab
{
    public string name => "Ship";

    public void Draw()
    {
        float colW = (MenuUI.ContentWidth - 12f) / 2f;

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawGeneral();
        GUILayout.Space(10);
        DrawSabotage();
        GUILayout.EndVertical();

        GUILayout.Space(12);

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawVents();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        KarakUI.BeginSection("General");
        CheatToggles.unfixableLights = KarakUI.Toggle(CheatToggles.unfixableLights, "Unfixable Lights");
        if (KarakUI.Action("Call Meeting", "Call"))         CheatToggles.callMeeting = true;
        if (KarakUI.Action("Close Meeting", "Close"))       CheatToggles.closeMeeting = true;
        CheatToggles.autoReportBodies = KarakUI.Toggle(CheatToggles.autoReportBodies, "Auto-Report Bodies");
        CheatToggles.autoOpenDoorsOnUse = KarakUI.Toggle(CheatToggles.autoOpenDoorsOnUse, "Auto-Open Doors On Use");
        KarakUI.EndSection();
    }

    private void DrawSabotage()
    {
        KarakUI.BeginSection("Sabotage");
        CheatToggles.reactorSab = KarakUI.Toggle(CheatToggles.reactorSab, "Reactor");
        CheatToggles.oxygenSab = KarakUI.Toggle(CheatToggles.oxygenSab, "Oxygen");
        CheatToggles.elecSab = KarakUI.Toggle(CheatToggles.elecSab, "Lights");
        CheatToggles.commsSab = KarakUI.Toggle(CheatToggles.commsSab, "Comms");
        CheatToggles.showDoorsMenu = KarakUI.Toggle(CheatToggles.showDoorsMenu, "Show Doors Menu");
        CheatToggles.mushSab = KarakUI.Toggle(CheatToggles.mushSab, "Mushroom Mixup");
        CheatToggles.mushSpore = KarakUI.Toggle(CheatToggles.mushSpore, "Trigger Spores");
        CheatToggles.sabotageMap = KarakUI.Toggle(CheatToggles.sabotageMap, "Open Sabotage Map");
        KarakUI.EndSection();
    }

    private void DrawVents()
    {
        KarakUI.BeginSection("Vents");
        CheatToggles.unlockVents = KarakUI.Toggle(CheatToggles.unlockVents, "Unlock Vents");
        if (KarakUI.Action("Kick All From Vents", "Kick")) CheatToggles.kickVents = true;
        CheatToggles.walkInVents = KarakUI.Toggle(CheatToggles.walkInVents, "Walk In Vents");
        GUILayout.Label("Disable Vents for Others / Block Sabotages moved to Combat → Host-Only.", GUIStylePreset.Label);
        KarakUI.EndSection();

        GUILayout.Space(10);
        KarakUI.BeginSection("Troll");
        CheatToggles.doorHallucination    = KarakUI.Toggle(CheatToggles.doorHallucination,    "Door Hallucination…");
        CheatToggles.doorHallucinationAll = KarakUI.Toggle(CheatToggles.doorHallucinationAll, "Door Hallucination (everyone)");
        KarakUI.EndSection();
    }
}

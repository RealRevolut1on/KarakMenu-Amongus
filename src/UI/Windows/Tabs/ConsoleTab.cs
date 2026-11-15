using UnityEngine;

namespace KarakMenu;

public class ConsoleTab : ITab
{
    public string name => "Console";

    public void Draw()
    {
        float colW = MenuUI.ContentWidth;

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawGeneral();
        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        KarakUI.BeginSection("General");
        CheatToggles.showConsole = KarakUI.Toggle(CheatToggles.showConsole, "Show Console");
        CheatToggles.logDeaths = KarakUI.Toggle(CheatToggles.logDeaths, "Log Deaths");
        CheatToggles.logShapeshifts = KarakUI.Toggle(CheatToggles.logShapeshifts, "Log Shapeshifts");
        CheatToggles.logVents = KarakUI.Toggle(CheatToggles.logVents, "Log Vents");
        KarakUI.EndSection();
    }
}

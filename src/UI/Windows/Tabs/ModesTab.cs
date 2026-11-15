using UnityEngine;

namespace KarakMenu;

public class ModesTab : ITab
{
    public string name => "Modes";

    public void Draw()
    {
        float colW = (MenuUI.ContentWidth - 12f) / 2f;

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawGeneral();
        GUILayout.EndVertical();

        GUILayout.Space(12);

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawOutfits();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        KarakUI.BeginSection("General");
        CheatToggles.rgbMode = KarakUI.Toggle(CheatToggles.rgbMode, "RGB Mode");
        CheatToggles.stealthMode = KarakUI.Toggle(CheatToggles.stealthMode, "Stealth Mode");
        CheatToggles.panicMode = KarakUI.Toggle(CheatToggles.panicMode, "Panic Mode");
        CheatToggles.streamProof = KarakUI.Toggle(CheatToggles.streamProof, "Stream-Proof");
        if (CheatToggles.streamProof)
        {
            GUILayout.Label(
                "Excludes the game window from screen-capture (OBS, Discord, Zoom) at the OS level — " +
                "you still see everything locally. Requires Windows 10 2004+.",
                GUIStylePreset.Label);
        }
        KarakUI.EndSection();
    }

    private void DrawOutfits()
    {
        KarakUI.BeginSection("Outfits");
        CheatToggles.copyOutfit = KarakUI.Toggle(CheatToggles.copyOutfit, "Copy Outfit from Player");
        if (KarakUI.Action("Confuse Now (random cosmetics)", "Do it")) CheatToggles.confuseNow = true;
        KarakUI.EndSection();
    }
}

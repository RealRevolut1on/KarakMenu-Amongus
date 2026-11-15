using UnityEngine;

namespace KarakMenu;

public class PassiveTab : ITab
{
    public string name => "Passive";

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
        CheatToggles.freeCosmetics = KarakUI.Toggle(CheatToggles.freeCosmetics, "Free Cosmetics");
        CheatToggles.avoidPenalties = KarakUI.Toggle(CheatToggles.avoidPenalties, "Avoid Penalties");
        CheatToggles.unlockFeatures = KarakUI.Toggle(CheatToggles.unlockFeatures, "Unlock Extra Features");
        CheatToggles.copyLobbyCodeOnDisconnect = KarakUI.Toggle(CheatToggles.copyLobbyCodeOnDisconnect, "Copy Lobby Code on Disconnect");
        CheatToggles.spoofAprilFoolsDate = KarakUI.Toggle(CheatToggles.spoofAprilFoolsDate, "Spoof Date to April 1st");
        KarakUI.EndSection();
    }
}

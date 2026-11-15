using UnityEngine;

namespace KarakMenu;

public class ChatTab : ITab
{
    public string name => "Chat";

    public void Draw()
    {
        float colW = (MenuUI.ContentWidth - 12f) / 2f;

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawGeneral();
        GUILayout.EndVertical();

        GUILayout.Space(12);

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawTextbox();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        KarakUI.BeginSection("General");
        CheatToggles.enableChat = KarakUI.Toggle(CheatToggles.enableChat, "Enable Chat");
        CheatToggles.bypassUrlBlock = KarakUI.Toggle(CheatToggles.bypassUrlBlock, "Bypass URL Block");
        CheatToggles.lowerRateLimits = KarakUI.Toggle(CheatToggles.lowerRateLimits, "Lower Rate Limits");
        KarakUI.EndSection();
    }

    private void DrawTextbox()
    {
        KarakUI.BeginSection("Textbox");
        CheatToggles.unlockCharacters = KarakUI.Toggle(CheatToggles.unlockCharacters, "Unlock Extra Characters");
        CheatToggles.longerMessages = KarakUI.Toggle(CheatToggles.longerMessages, "Allow Longer Messages");
        CheatToggles.unlockClipboard = KarakUI.Toggle(CheatToggles.unlockClipboard, "Unlock Clipboard");
        KarakUI.EndSection();
    }
}

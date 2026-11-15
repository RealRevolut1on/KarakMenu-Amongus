using System.Collections.Generic;
using UnityEngine;

namespace KarakMenu;

// Bottom-right notification listing other KarakMenu users in the current game
public class DetectUI : MonoBehaviour
{
    private GUIStyle _style;

    public void OnGUI()
    {
        if (KarakMenu.isPanicked) return;
        if (KarakDetection.DetectedIds.Count == 0) return;

        _style ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            richText = true,
            alignment = TextAnchor.MiddleRight
        };

        var names = new List<string>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null) continue;
            if (!KarakDetection.IsKarakUser(player.PlayerId)) continue;

            var colorHex = ColorUtility.ToHtmlStringRGB(player.Data.Color);
            names.Add($"<color=#{colorHex}>{player.Data.PlayerName}</color>");
        }

        if (names.Count == 0) return;

        string text = $"<color=#D1EB67><b>KARAKMENU USERS</b></color>  {string.Join(", ", names)}";

        _style.normal.textColor = KarakTheme.TextActive;
        GUI.Label(new Rect(Screen.width - 560, Screen.height - 36, 548, 24), text, _style);
    }
}

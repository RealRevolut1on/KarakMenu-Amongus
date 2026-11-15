using AmongUs.Data;
using UnityEngine;

namespace KarakMenu;

public class ConfigTab : ITab
{
    public string name => "Config";

    private static string _spoofLevelText;

    public void Draw()
    {
        float colW = (MenuUI.ContentWidth - 12f) / 2f;

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawGeneral();
        GUILayout.EndVertical();

        GUILayout.Space(12);

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawSettings();
        GUILayout.Space(10);
        DrawSpoofLevel();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        KarakUI.BeginSection("General");
        if (KarakUI.Action("Open Config", "Open"))   CheatToggles.openConfig = true;
        if (KarakUI.Action("Reload Config", "Reload")) CheatToggles.reloadConfig = true;
        if (KarakUI.Action("Save to Profile", "Save")) CheatToggles.saveProfile = true;
        if (KarakUI.Action("Load from Profile", "Load")) CheatToggles.loadProfile = true;
        KarakUI.EndSection();
    }

    private void DrawSettings()
    {
        KarakUI.BeginSection("Menu Settings");
        float opacity = KarakUI.LabeledSlider("Opacity", KarakMenu.menuOpacity.Value, 0.15f, 1f, "0.00");
        if (Mathf.Abs(opacity - KarakMenu.menuOpacity.Value) > 0.001f)
        {
            KarakMenu.menuOpacity.Value = opacity;
            KarakTheme.Opacity = opacity;
        }
        KarakUI.EndSection();
    }

    private void DrawSpoofLevel()
    {
        KarakUI.BeginSection("Spoof Level");

        if (_spoofLevelText == null)
        {
            _spoofLevelText = int.TryParse(KarakMenu.spoofLevel.Value, out var saved) && saved > 0
                ? saved.ToString()
                : "";
        }

        GUILayout.Label($"Currently showing: <b>{KarakMenu.spoofLevel.Value}</b>",
            GUIStylePreset.Label);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Level", GUIStylePreset.Label, GUILayout.Width(40f));
        string before = _spoofLevelText;
        _spoofLevelText = KarakUI.TextInput(_spoofLevelText, "spoof-level", GUILayout.ExpandWidth(true));
        if (KarakUI.Button("Apply", GUILayout.Width(70f), GUILayout.Height(26f)))
        {
            ApplySpoofLevel();
        }
        else if (_spoofLevelText != before)
        {
            // Apply as the user types so no separate click is ever required.
            ApplySpoofLevel();
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Type any level from 1 to 100001 (no decimals), then Apply.",
            GUIStylePreset.Label);
        KarakUI.EndSection();
    }

    private void ApplySpoofLevel()
    {
        if (string.IsNullOrWhiteSpace(_spoofLevelText)) return;

        if (!int.TryParse(_spoofLevelText.Trim(), out int parsed))
        {
            _spoofLevelText = "";
            return;
        }

        int clamped = Mathf.Clamp(parsed, 1, 100001);
        _spoofLevelText = clamped.ToString();
        KarakMenu.spoofLevel.Value = _spoofLevelText;
        KarakSpoof.SpoofLevel();
    }
}

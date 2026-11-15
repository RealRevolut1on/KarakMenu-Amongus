using System.Linq;
using System.Text;
using UnityEngine;

namespace KarakMenu;

// Cross-tab settings search — reuses the same CheatToggles.ToggleFields
// reflection registry KeybindsTab already lists every toggle from, but
// scoped as a live filter + direct flip rather than a jump-to-tab navigator:
// IMGUI has no cheap way to scroll a *different* tab's layout to a specific
// row before it's even drawn, so instead of a half-working "jump" button,
// typing a query here shows the matches themselves, each independently
// toggleable right in the results — no tab switch needed to act on it.
public class SearchTab : ITab
{
    public string name => "Search";

    private static string _filter = "";

    public void Draw()
    {
        float colW = MenuUI.ContentWidth;

        GUILayout.BeginVertical(GUILayout.Width(colW));

        KarakUI.BeginSection("Search Settings");
        GUILayout.Label(
            "Type to find any toggle across every tab. Results are live — flip them right here.",
            GUIStylePreset.Label);
        GUILayout.Space(6);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Search", GUIStylePreset.Label, GUILayout.Width(50f));
        _filter = KarakUI.TextInput(_filter ?? "", "search-filter", GUILayout.Width(240f));
        GUILayout.EndHorizontal();
        GUILayout.Space(6);

        if (string.IsNullOrEmpty(_filter))
        {
            GUILayout.Label("Start typing to search (e.g. \"kill\", \"vent\", \"speed\").", GUIStylePreset.Label);
        }
        else
        {
            int shown = 0;
            foreach (var name in CheatToggles.ToggleFields.Keys.OrderBy(n => n))
            {
                string label = Humanize(name);
                bool matches = name.IndexOf(_filter, System.StringComparison.OrdinalIgnoreCase) >= 0
                    || label.IndexOf(_filter, System.StringComparison.OrdinalIgnoreCase) >= 0;
                if (!matches) continue;

                DrawResult(name, label);
                shown++;
            }

            if (shown == 0) GUILayout.Label("No matching settings.", GUIStylePreset.Label);
        }

        KarakUI.EndSection();

        GUILayout.EndVertical();
    }

    private void DrawResult(string fieldName, string label)
    {
        var field = CheatToggles.ToggleFields[fieldName];
        bool value = (bool)field.GetValue(null);
        bool newValue = KarakUI.Toggle(value, label, $"search-{fieldName}");
        if (newValue != value) field.SetValue(null, newValue);
    }

    // "killAnyone" -> "Kill Anyone", "noKillCd" -> "No Kill Cd"
    private static string Humanize(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName)) return fieldName;
        var sb = new StringBuilder(fieldName.Length + 8);
        for (int i = 0; i < fieldName.Length; i++)
        {
            char c = fieldName[i];
            if (i == 0) { sb.Append(char.ToUpper(c)); continue; }
            if (char.IsUpper(c) && !char.IsUpper(fieldName[i - 1])) sb.Append(' ');
            sb.Append(c);
        }
        return sb.ToString();
    }
}

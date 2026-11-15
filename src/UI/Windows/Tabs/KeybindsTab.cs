using System.Linq;
using UnityEngine;

namespace KarakMenu;

// Lets the user assign a keyboard key to any toggle-able cheat, backed by the
// existing (previously UI-less) CheatToggles.Keybinds infrastructure that
// KeybindListener.Update() already reads from every frame. Click Bind, then
// press any key — the very next key event gets captured, Escape clears
// instead of assigning one.
public class KeybindsTab : ITab
{
    public string name => "Keybinds";

    private static string _awaitingBindFor;
    private static string _filter = "";

    public void Draw()
    {
        float colW = MenuUI.ContentWidth;

        GUILayout.BeginVertical(GUILayout.Width(colW));

        KarakUI.BeginSection("Keybinds");
        GUILayout.Label(
            "Click Bind, then press any key to assign it to that toggle. Press Escape while binding to clear it instead.",
            GUIStylePreset.Label);
        GUILayout.Space(6);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Search", GUIStylePreset.Label, GUILayout.Width(50f));
        _filter = KarakUI.TextInput(_filter ?? "", "keybind-filter", GUILayout.Width(200f));
        if (!string.IsNullOrEmpty(_filter) && KarakUI.Button("Clear", GUILayout.Width(60f), GUILayout.Height(22f)))
            _filter = "";
        GUILayout.EndHorizontal();
        GUILayout.Space(6);

        // Capture the very next key event while a bind request is pending.
        // Must run every Draw() call (not just when a button is clicked) since
        // the key press arrives as its own separate OnGUI event.
        if (_awaitingBindFor != null && !KarakUI.IsTextInputFocused && Event.current.type == EventType.KeyDown)
        {
            var pressed = Event.current.keyCode;
            if (pressed == KeyCode.Escape)
                CheatToggles.Keybinds[_awaitingBindFor] = KeyCode.None;
            else if (pressed != KeyCode.None)
                CheatToggles.Keybinds[_awaitingBindFor] = pressed;

            _awaitingBindFor = null;
            Event.current.Use();
        }

        int shown = 0;
        foreach (var toggleName in CheatToggles.ToggleFields.Keys.OrderBy(n => n))
        {
            if (!string.IsNullOrEmpty(_filter) && toggleName.IndexOf(_filter, System.StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            DrawRow(toggleName);
            shown++;
        }

        // Without this, a stray/leftover search filter that matches nothing
        // makes the whole list silently vanish with no indication why.
        if (shown == 0) GUILayout.Label("No matching keybinds.", GUIStylePreset.Label);

        KarakUI.EndSection();

        GUILayout.EndVertical();
    }

    private void DrawRow(string toggleName)
    {
        bool isAwaiting = _awaitingBindFor == toggleName;
        CheatToggles.Keybinds.TryGetValue(toggleName, out var key);

        // The row has to be at least as tall as the tallest button in it —
        // otherwise the 20px Bind/Clear buttons spill into the row below and
        // stack on top of each other down the whole list.
        GUILayout.BeginHorizontal(GUILayout.Height(24f));

        GUILayout.Label(toggleName, GUIStylePreset.Label, GUILayout.Width(220f));

        string keyLabel = isAwaiting ? "Press a key…" : (key == KeyCode.None ? "Unbound" : key.ToString());
        GUILayout.Label(keyLabel, GUIStylePreset.Label, GUILayout.Width(110f));

        if (KarakUI.Button(isAwaiting ? "Cancel" : "Bind", GUILayout.Width(70f), GUILayout.Height(20f)))
        {
            _awaitingBindFor = isAwaiting ? null : toggleName;
        }

        if (key != KeyCode.None)
        {
            if (KarakUI.Button("Clear", GUILayout.Width(60f), GUILayout.Height(20f)))
            {
                CheatToggles.Keybinds[toggleName] = KeyCode.None;
                if (isAwaiting) _awaitingBindFor = null;
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(3f);
    }
}

using UnityEngine;

namespace KarakMenu;

public class KeybindListener : MonoBehaviour
{
    public void Update()
    {
        if (KarakMenu.isPanicked) return;

        // Don't fire keybinds while the user is typing in a menu text field
        if (KarakUI.IsTextInputFocused) return;

        // Keybinds aren't triggered from typing in the chat
        if (HudManager.InstanceExists && HudManager.Instance.Chat && HudManager.Instance.Chat.IsOpenOrOpening) return;

        // Check each keybind to see if the user pressed it and toggle the corresponding cheat
        foreach (var (name, key) in CheatToggles.Keybinds)
        {
            if (key == KeyCode.None) continue;
            if (!Input.GetKeyDown(key)) continue;

            if (!CheatToggles.ToggleFields.TryGetValue(name, out var field)) continue;

            var current = (bool)field.GetValue(null);
            field.SetValue(null, !current);
        }
    }
}

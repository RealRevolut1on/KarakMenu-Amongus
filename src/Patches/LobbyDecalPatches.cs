using HarmonyLib;
using UnityEngine;

namespace KarakMenu;

// Shows the KarakMenu graffiti logo in the empty space below the lobby ship.
// Rendered via IMGUI (OnGUI) rather than a camera-parented SpriteRenderer so
// that positioning is resolution-independent — no world-space calibration needed.
// Disappears automatically when stealth mode or panic mode is active (same gate
// as ModManager's vanilla ModStamp in MenuUI.cs).
public static class KarakLobbyDecal
{
    private static GraffitiHUD _hud;

    // Exposed as MonoBehaviour so MenuUI.cs can read .enabled and the Unity
    // operator-bool null check — both work identically on MonoBehaviour.
    public static MonoBehaviour Instance => _hud;

    public static void Show()
    {
        if (_hud != null || Camera.main == null) return;
        var go = new GameObject("KarakMenuLobbyDecal");
        go.transform.SetParent(Camera.main.transform, false);
        _hud = go.AddComponent<GraffitiHUD>();
    }

    public static void Hide()
    {
        if (_hud == null) return;
        Object.Destroy(_hud.gameObject);
        _hud = null;
    }
}

// Draws the embedded karakmenu_logo.png directly in screen space.
// Position: centered horizontally, placed at ~72% of screen height —
// the empty band below the ship in the lobby scene.
// Alpha 88% so it reads as a painted-on mark rather than a UI overlay.
internal class GraffitiHUD : MonoBehaviour
{
    private Texture2D _tex;
    private const float AspectRatio = 1024f / 345f; // logo image native dimensions

    private void Start()
    {
        var sprite = Utils.LoadSprite("KarakMenu.Assets.karakmenu_logo.png", 100f);
        if (sprite != null) _tex = sprite.texture;
    }

    private void OnGUI()
    {
        if (!enabled) return;
        if (KarakMenu.inStealthMode || KarakMenu.isPanicked) return;
        if (_tex == null) return;

        float w = Screen.width * 0.195f;   // ~280 px at 1440-wide
        float h = w / AspectRatio;          // maintains native aspect ratio
        float x = (Screen.width - w) * 0.5f;
        float y = Screen.height * 0.715f;   // centers on the red zone (~580 px at 810 tall)

        GUI.color = new Color(1f, 1f, 1f, 0.88f);
        GUI.DrawTexture(new Rect(x, y, w, h), _tex, ScaleMode.ScaleToFit, true);
        GUI.color = Color.white;
    }
}

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class LobbyBehaviour_Start_Decal
{
    public static void Postfix() => KarakLobbyDecal.Show();
}

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.OnDestroy))]
public static class LobbyBehaviour_OnDestroy_Decal
{
    public static void Postfix() => KarakLobbyDecal.Hide();
}

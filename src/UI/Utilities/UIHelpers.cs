using UnityEngine;

namespace KarakMenu;

public static class UIHelpers
{
    public static void ApplyAccent()
    {
        KarakTheme.RgbMode = CheatToggles.rgbMode;

        if (CheatToggles.rgbMode)
        {
            // Primary accent cycles the full hue wheel
            KarakTheme.Accent          = Color.HSVToRGB(MenuUI.hue, 1f, 1f);
            // Secondary/tertiary accents ride 120° and 240° ahead, so
            // widgets tinted with different accents form a rainbow across
            // the UI instead of a single flat cycling color
            KarakTheme.SecondaryAccent = Color.HSVToRGB((MenuUI.hue + 0.333f) % 1f, 1f, 1f);
            KarakTheme.TertiaryAccent  = Color.HSVToRGB((MenuUI.hue + 0.666f) % 1f, 1f, 1f);
            return;
        }

        var configHtmlColor = KarakMenu.menuHtmlColor.Value;

        if (ColorUtility.TryParseHtmlString(configHtmlColor, out var uiColor))
        {
            KarakTheme.Accent = uiColor;
        }
        else if (!configHtmlColor.StartsWith("#") &&
                 ColorUtility.TryParseHtmlString("#" + configHtmlColor, out uiColor))
        {
            KarakTheme.Accent = uiColor;
        }

        KarakTheme.SecondaryAccent = KarakTheme.Accent;
        KarakTheme.TertiaryAccent  = KarakTheme.Accent;
    }

    // Pulsing multiplier in [0, 1] that RGB elements use to add a subtle
    // breathing effect on top of the hue cycle
    public static float RgbPulse(float speed = 3f, float min = 0.55f, float max = 1f)
    {
        float t = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * speed);
        return Mathf.Lerp(min, max, t);
    }
}

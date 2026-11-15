using System.Collections.Generic;
using UnityEngine;

namespace KarakMenu;

public static class KarakTheme
{
    // Palette from the "2" ImGui reference (imgui_draw.cpp StyleColorsDark)
    public static Color Accent          = new(209f / 255f, 235f / 255f, 103f / 255f); // ImGuiCol_Scheme
    public static Color SecondaryAccent = new(209f / 255f, 235f / 255f, 103f / 255f); // hue+120° when RgbMode
    public static Color TertiaryAccent  = new(209f / 255f, 235f / 255f, 103f / 255f); // hue+240° when RgbMode
    public static Color WindowBg     = new(15f / 255f, 17f / 255f, 16f / 255f);
    public static Color ChildBg      = new(23f / 255f, 25f / 255f, 24f / 255f);
    public static Color FrameBg      = new(32f / 255f, 35f / 255f, 34f / 255f);
    public static Color Border       = new(32f / 255f, 35f / 255f, 34f / 255f);
    public static Color TextActive   = new(1f, 1f, 1f, 1f);
    public static Color TextDisabled = new(97f / 255f, 101f / 255f, 102f / 255f);

    // Whether RGB mode is currently animating the accent colors
    public static bool RgbMode;

    private static float _opacity = 1f;
    private static int _textureVersion = 1;

    public static float Opacity
    {
        get => _opacity;
        set
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(_opacity, value)) return;
            _opacity = value;
            _textures.Clear();
            _textureVersion++;
        }
    }

    public static int TextureVersion => _textureVersion;

    private static readonly Dictionary<string, Texture2D> _textures = new();

    private static Texture2D NewTexture(int width, int height)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            hideFlags = HideFlags.HideAndDontSave,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        return tex;
    }

    private static Texture2D _clear;

    public static Texture2D ClearTexture()
    {
        if (_clear != null) return _clear;
        _clear = NewTexture(1, 1);
        _clear.SetPixels32(new Color32[1]);
        _clear.Apply();
        return _clear;
    }

    // A white rounded rectangle that callers tint via GUI.color — avoids
    // exploding the texture cache when the RGB accent cycles every frame.
    public static Texture2D WhiteRoundedRect(float radius, int size = 32)
    {
        return RoundedRect(Color.white, radius, 0f, Color.clear, size);
    }

    public static Texture2D SolidColor(Color color, int size = 4)
    {
        string key = $"sc|{color}|{size}|{Opacity}";
        if (_textures.TryGetValue(key, out var cached)) return cached;

        var tex = NewTexture(size, size);
        var pixels = new Color32[size * size];
        var c = new Color(color.r, color.g, color.b, color.a * Opacity);
        var c32 = (Color32)c;
        for (int i = 0; i < pixels.Length; i++) pixels[i] = c32;
        tex.SetPixels32(pixels);
        tex.Apply();
        _textures[key] = tex;
        return tex;
    }

    public static Texture2D RoundedRect(Color fill, float radius, float strokeWidth, Color stroke, int size = 32)
    {
        string key = $"rr|{fill}|{radius}|{strokeWidth}|{stroke}|{size}|{Opacity}";
        if (_textures.TryGetValue(key, out var cached)) return cached;

        var tex = NewTexture(size, size);
        var pixels = new Color32[size * size];
        float half = size / 2f;
        var center = new Vector2(half, half);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                var q = new Vector2(
                    Mathf.Max(Mathf.Abs(p.x - center.x) - (half - radius), 0f),
                    Mathf.Max(Mathf.Abs(p.y - center.y) - (half - radius), 0f));
                float d = q.magnitude - radius;

                var col = default(Color);
                if (d <= 0f)
                {
                    col = fill;
                    col.a = Mathf.Clamp01(0.5f - d);
                }
                else if (strokeWidth > 0f && d <= strokeWidth + 0.5f)
                {
                    col = stroke;
                    col.a = Mathf.Clamp01(0.5f - (d - strokeWidth));
                }

                col.a *= Opacity;
                pixels[y * size + x] = col;
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        _textures[key] = tex;
        return tex;
    }

    public static Texture2D Circle(Color color, int size = 16)
    {
        string key = $"cr|{color}|{size}|{Opacity}";
        if (_textures.TryGetValue(key, out var cached)) return cached;

        var tex = NewTexture(size, size);
        var pixels = new Color32[size * size];
        float radius = size / 2f;
        var center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                float d = (p - center).magnitude - radius;
                var col = default(Color);
                if (d <= 0f)
                {
                    col = color;
                    col.a = Mathf.Clamp01(0.5f - d);
                }
                col.a *= Opacity;
                pixels[y * size + x] = col;
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        _textures[key] = tex;
        return tex;
    }

    // Sidebar icons
    public enum KarakIcon
    {
        Crosshair, Eye, User, Ship, Bubble, Play, Angle, Crown, Pause, Gear, Sliders, Diamond, Chevron, Shield
    }

    public static Texture2D Icon(KarakIcon icon, Color color, int size = 36)
    {
        string key = $"ic|{icon}|{color}|{size}|{Opacity}";
        if (_textures.TryGetValue(key, out var cached)) return cached;

        var tex = NewTexture(size, size);
        var px = new Color32[size * size];
        float c = size / 2f;
        float s = size / 36f;

        switch (icon)
        {
            case KarakIcon.Crosshair:
                Line(px, size, c - 11f * s, c, c - 4f * s, c, 2.5f * s, color);
                Line(px, size, c + 4f * s, c, c + 11f * s, c, 2.5f * s, color);
                Line(px, size, c, c - 11f * s, c, c - 4f * s, 2.5f * s, color);
                Line(px, size, c, c + 4f * s, c, c + 11f * s, 2.5f * s, color);
                CirclePx(px, size, c, c, 2.5f * s, color);
                break;
            case KarakIcon.Eye:
                Ellipse(px, size, c, c, 10f * s, 6f * s, 1.3f * s, color);
                CirclePx(px, size, c, c, 2.4f * s, color);
                break;
            case KarakIcon.User:
                CirclePx(px, size, c, c - 4.5f * s, 3.8f * s, color);
                RRect(px, size, c - 7.5f * s, c, c + 7.5f * s, c + 11f * s, 3.5f * s, color);
                break;
            case KarakIcon.Ship:
                Triangle(px, size, new Vector2(c, c - 11f * s), new Vector2(c - 5.5f * s, c), new Vector2(c + 5.5f * s, c), color);
                RRect(px, size, c - 5.5f * s, c, c + 5.5f * s, c + 9f * s, 2f * s, color);
                CirclePx(px, size, c, c + 2.5f * s, 2.2f * s, color);
                break;
            case KarakIcon.Bubble:
                RRect(px, size, c - 10f * s, c - 7f * s, c + 10f * s, c + 6f * s, 5f * s, color);
                Triangle(px, size, new Vector2(c - 4f * s, c + 6f * s), new Vector2(c - 8f * s, c + 12f * s), new Vector2(c + 1f * s, c + 6f * s), color);
                break;
            case KarakIcon.Play:
                Triangle(px, size, new Vector2(c - 5f * s, c - 9f * s), new Vector2(c - 5f * s, c + 9f * s), new Vector2(c + 8f * s, c), color);
                break;
            case KarakIcon.Angle:
                Line(px, size, c - 5f * s, c - 8f * s, c + 3f * s, c, 2.5f * s, color);
                Line(px, size, c - 5f * s, c + 8f * s, c + 3f * s, c, 2.5f * s, color);
                break;
            case KarakIcon.Crown:
                RRect(px, size, c - 9f * s, c + 3f * s, c + 9f * s, c + 9f * s, 2f * s, color);
                Triangle(px, size, new Vector2(c - 8f * s, c + 3f * s), new Vector2(c - 9f * s, c - 4f * s), new Vector2(c - 4f * s, c + 3f * s), color);
                Triangle(px, size, new Vector2(c - 3f * s, c + 3f * s), new Vector2(c, c - 7f * s), new Vector2(c + 3f * s, c + 3f * s), color);
                Triangle(px, size, new Vector2(c + 4f * s, c + 3f * s), new Vector2(c + 9f * s, c - 4f * s), new Vector2(c + 8f * s, c + 3f * s), color);
                break;
            case KarakIcon.Pause:
                RRect(px, size, c - 7.5f * s, c - 8f * s, c - 3.5f * s, c + 8f * s, 2f * s, color);
                RRect(px, size, c + 3.5f * s, c - 8f * s, c + 7.5f * s, c + 8f * s, 2f * s, color);
                break;
            case KarakIcon.Gear:
                for (int i = 0; i < 8; i++)
                {
                    float a = i * Mathf.PI / 4f;
                    CirclePx(px, size, c + Mathf.Cos(a) * 7f * s, c + Mathf.Sin(a) * 7f * s, 3.4f * s, color);
                    CirclePx(px, size, c + Mathf.Cos(a) * 10f * s, c + Mathf.Sin(a) * 10f * s, 2.1f * s, color);
                }
                CirclePx(px, size, c, c, 2f * s, color);
                break;
            case KarakIcon.Sliders:
                Line(px, size, c - 9f * s, c - 7f * s, c + 9f * s, c - 7f * s, 2f * s, color);
                CirclePx(px, size, c - 2f * s, c - 7f * s, 2.6f * s, color);
                Line(px, size, c - 9f * s, c, c + 9f * s, c, 2f * s, color);
                CirclePx(px, size, c + 4f * s, c, 2.6f * s, color);
                Line(px, size, c - 9f * s, c + 7f * s, c + 9f * s, c + 7f * s, 2f * s, color);
                CirclePx(px, size, c - 5f * s, c + 7f * s, 2.6f * s, color);
                break;
            case KarakIcon.Diamond:
                Triangle(px, size, new Vector2(c, c - 10f * s), new Vector2(c - 9f * s, c), new Vector2(c + 9f * s, c), color);
                Triangle(px, size, new Vector2(c, c + 10f * s), new Vector2(c - 9f * s, c), new Vector2(c + 9f * s, c), color);
                break;
            case KarakIcon.Chevron:
                Line(px, size, c - 5f * s, c - 7f * s, c + 5f * s, c, 2.5f * s, color);
                Line(px, size, c + 5f * s, c, c - 5f * s, c + 7f * s, 2.5f * s, color);
                break;
            case KarakIcon.Shield:
                RRect(px, size, c - 8f * s, c - 9f * s, c + 8f * s, c + 7f * s, 2f * s, color);
                break;
        }

        tex.SetPixels32(px);
        tex.Apply();
        _textures[key] = tex;
        return tex;
    }

    // Drawing primitives for icons

    private static void Blend(Color32[] px, int size, float x, float y, Color col, float a)
    {
        int xi = Mathf.RoundToInt(x), yi = Mathf.RoundToInt(y);
        if (xi < 0 || yi < 0 || xi >= size || yi >= size) return;
        px[yi * size + xi] = new Color32(
            (byte)(col.r * 255f), (byte)(col.g * 255f), (byte)(col.b * 255f),
            (byte)(Mathf.Clamp01(col.a * a * Opacity) * 255f));
    }

    private static void CirclePx(Color32[] px, int size, float cx, float cy, float r, Color col)
    {
        int minX = Mathf.Max(0, Mathf.FloorToInt(cx - r)), maxX = Mathf.Min(size - 1, Mathf.CeilToInt(cx + r));
        int minY = Mathf.Max(0, Mathf.FloorToInt(cy - r)), maxY = Mathf.Min(size - 1, Mathf.CeilToInt(cy + r));
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                float d = Mathf.Sqrt((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy));
                Blend(px, size, x, y, col, Mathf.Clamp01(r - d + 0.5f));
            }
    }

    private static void RRect(Color32[] px, int size, float x0, float y0, float x1, float y1, float r, Color col)
    {
        int minX = Mathf.Max(0, Mathf.FloorToInt(x0)), maxX = Mathf.Min(size - 1, Mathf.CeilToInt(x1));
        int minY = Mathf.Max(0, Mathf.FloorToInt(y0)), maxY = Mathf.Min(size - 1, Mathf.CeilToInt(y1));
        float hx = (x1 - x0) / 2f, hy = (y1 - y0) / 2f;
        float mx = (x0 + x1) / 2f, my = (y0 + y1) / 2f;
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                float qx = Mathf.Max(Mathf.Abs(x + 0.5f - mx) - (hx - r), 0f);
                float qy = Mathf.Max(Mathf.Abs(y + 0.5f - my) - (hy - r), 0f);
                float d = Mathf.Sqrt(qx * qx + qy * qy) - r;
                Blend(px, size, x, y, col, Mathf.Clamp01(-d + 0.5f));
            }
    }

    private static void Triangle(Color32[] px, int size, Vector2 a, Vector2 b, Vector2 cc, Color col)
    {
        float minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, cc.x))));
        float maxX = Mathf.Min(size - 1, Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, cc.x))));
        float minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, cc.y))));
        float maxY = Mathf.Min(size - 1, Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, cc.y))));
        float det = (b.x - a.x) * (cc.y - a.y) - (cc.x - a.x) * (b.y - a.y);
        if (Mathf.Abs(det) < 0.0001f) return;
        for (int y = (int)minY; y <= (int)maxY; y++)
            for (int x = (int)minX; x <= (int)maxX; x++)
            {
                float v2x = x + 0.5f - a.x, v2y = y + 0.5f - a.y;
                float u = (v2x * (cc.y - a.y) - (cc.x - a.x) * v2y) / det;
                float v = ((b.x - a.x) * v2y - v2x * (b.y - a.y)) / det;
                if (u >= 0f && v >= 0f && u + v <= 1f)
                    Blend(px, size, x, y, col, 1f);
            }
    }

    private static void Line(Color32[] px, int size, float x0, float y0, float x1, float y1, float thickness, Color col)
    {
        int steps = Mathf.CeilToInt(Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0)));
        for (int i = 0; i <= steps; i++)
        {
            float t = steps == 0 ? 0f : i / (float)steps;
            CirclePx(px, size, Mathf.Lerp(x0, x1, t), Mathf.Lerp(y0, y1, t), thickness * 0.5f, col);
        }
    }

    private static void Ellipse(Color32[] px, int size, float cx, float cy, float rx, float ry, float thickness, Color col)
    {
        const int segs = 48;
        for (int i = 0; i < segs; i++)
        {
            float a0 = i * Mathf.PI * 2f / segs;
            float a1 = (i + 1) * Mathf.PI * 2f / segs;
            Line(px, size, cx + Mathf.Cos(a0) * rx, cy + Mathf.Sin(a0) * ry,
                        cx + Mathf.Cos(a1) * rx, cy + Mathf.Sin(a1) * ry, thickness, col);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KarakMenu;

// Widget layer for KarakMenu — matches the "2" ImGui reference:
// pill-style toggle switches, accent-fill sliders, subtab bar, rounded
// section panels, animated indicators. All backgrounds are drawn via
// GUI.Box(style) — GUI.DrawTexture is unreliable inside GUI.Window under IL2CPP.
public static class KarakUI
{
    // ------------------------------------------------------------------
    // Animation helper — Time-based lerp keyed by a string id
    // ------------------------------------------------------------------
    private static readonly Dictionary<string, float> _animCache = new();

    public static float Anim(string key, float target, float speed = 10f)
    {
        _animCache.TryGetValue(key, out var v);
        if (Event.current == null || Event.current.type == EventType.Layout)
        {
            v = Mathf.Lerp(v, target, Mathf.Clamp01(Time.unscaledDeltaTime * speed));
            _animCache[key] = v;
        }
        return v;
    }

    public static Color LerpColor(Color a, Color b, float t)
    {
        return Color.Lerp(a, b, Mathf.Clamp01(t));
    }

    // ------------------------------------------------------------------
    // Cache invalidation on theme changes
    // ------------------------------------------------------------------
    private static int _cacheVersion = -1;

    private static void EnsureCurrent()
    {
        if (_cacheVersion == KarakTheme.TextureVersion) return;
        _cacheVersion = KarakTheme.TextureVersion;

        _sidebarBg = _accentStripe = _emptyStripe = _plainIcon = _invisibleButton = null;
        _collapseButton = _subtabIdle = _subtabActive = null;
        _sectionStyle = _sectionTitle = null;
        _switchTrackOff = _switchTrackOn = _switchKnob = _rowSeparator = _rowSepPublic = null;
        _radioOuter = _radioInner = null;
        _trackStyle = _fillStyle = _thumbStyle = null;
        _comboBoxStyle = _popupItem = null;
        _tabRowIdleBg = _tabRowActiveBg = null;
        _subwindowStyle = _windowTitle = _brandStyle = _actionButton = _swatch = _cornerDotStyle = null;
        _normalRowStyle = _selectedRowStyle = null;
    }

    // ------------------------------------------------------------------
    // Cached styles referenced by MenuUI
    // ------------------------------------------------------------------
    private static GUIStyle _sidebarBg;
    public static GUIStyle SidebarBg
    {
        get
        {
            EnsureCurrent();
            return _sidebarBg ??= new GUIStyle
            {
                normal = { background = KarakTheme.RoundedRect(KarakTheme.ChildBg, 12f, 0f, Color.clear, 48) },
                border = new RectOffset { left = 12, right = 12, top = 12, bottom = 12 }
            };
        }
    }

    // White rounded texture — accent color applied via GUI.color at draw time
    // so it can cycle through the RGB palette without spamming the texture cache
    private static GUIStyle _accentStripe;
    public static GUIStyle AccentStripe
    {
        get
        {
            EnsureCurrent();
            return _accentStripe ??= new GUIStyle
            {
                normal = { background = KarakTheme.WhiteRoundedRect(2f, 8) }
            };
        }
    }

    // Convenience: draw the accent stripe tinted to the given color
    public static void DrawAccentStripe(Rect rect, Color color)
    {
        var saved = GUI.color;
        GUI.color = new Color(color.r, color.g, color.b, GUI.color.a);
        GUI.Box(rect, GUIContent.none, AccentStripe);
        GUI.color = saved;
    }

    private static GUIStyle _emptyStripe;
    public static GUIStyle EmptyStripe { get { EnsureCurrent(); return _emptyStripe ??= new GUIStyle(); } }

    private static GUIStyle _plainIcon;
    public static GUIStyle PlainIcon
    {
        get
        {
            EnsureCurrent();
            return _plainIcon ??= new GUIStyle
            {
                margin = new RectOffset { left = 0, right = 0, top = 3, bottom = 3 }
            };
        }
    }

    private static GUIStyle _invisibleButton;
    public static GUIStyle InvisibleButton
    {
        get
        {
            EnsureCurrent();
            return _invisibleButton ??= new GUIStyle
            {
                normal = { background = KarakTheme.ClearTexture() },
                hover = { background = KarakTheme.ClearTexture() },
                active = { background = KarakTheme.ClearTexture() }
            };
        }
    }

    private static GUIStyle _tabRowIdleBg;
    public static GUIStyle TabRowIdleBg
    {
        get
        {
            EnsureCurrent();
            return _tabRowIdleBg ??= new GUIStyle
            {
                normal = { background = KarakTheme.ClearTexture() },
                hover = { background = KarakTheme.RoundedRect(KarakTheme.FrameBg, 6f, 0f, Color.clear, 16) },
                padding = new RectOffset { left = 8, right = 8, top = 0, bottom = 0 }
            };
        }
    }

    // Player list row styles for the two-panel Players tab
    private static GUIStyle _normalRowStyle;
    private static GUIStyle _selectedRowStyle;
    public static GUIStyle NormalRowStyle
    {
        get
        {
            EnsureCurrent();
            return _normalRowStyle ??= new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                richText = true,
                alignment = TextAnchor.MiddleLeft,
                border = new RectOffset { left = 4, right = 4, top = 4, bottom = 4 },
                padding = new RectOffset { left = 8, right = 4, top = 4, bottom = 4 },
                margin = new RectOffset(),
                normal = { background = KarakTheme.RoundedRect(KarakTheme.ChildBg, 4f, 0f, Color.clear, 16), textColor = KarakTheme.TextActive },
                hover  = { background = KarakTheme.RoundedRect(KarakTheme.FrameBg, 4f, 0f, Color.clear, 16), textColor = KarakTheme.TextActive }
            };
        }
    }
    public static GUIStyle SelectedRowStyle
    {
        get
        {
            EnsureCurrent();
            return _selectedRowStyle ??= new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                richText = true,
                alignment = TextAnchor.MiddleLeft,
                border = new RectOffset { left = 4, right = 4, top = 4, bottom = 4 },
                padding = new RectOffset { left = 8, right = 4, top = 4, bottom = 4 },
                margin = new RectOffset(),
                normal = { background = KarakTheme.RoundedRect(KarakTheme.Accent, 4f, 0f, Color.clear, 16), textColor = KarakTheme.WindowBg },
                hover  = { background = KarakTheme.RoundedRect(KarakTheme.Accent, 4f, 0f, Color.clear, 16), textColor = KarakTheme.WindowBg }
            };
        }
    }

    private static GUIStyle _tabRowActiveBg;
    public static GUIStyle TabRowActiveBg
    {
        get
        {
            EnsureCurrent();
            return _tabRowActiveBg ??= new GUIStyle
            {
                normal = { background = KarakTheme.RoundedRect(KarakTheme.FrameBg, 6f, 0f, Color.clear, 16) },
                hover = { background = KarakTheme.RoundedRect(KarakTheme.FrameBg, 6f, 0f, Color.clear, 16) },
                padding = new RectOffset { left = 8, right = 8, top = 0, bottom = 0 }
            };
        }
    }

    private static GUIStyle _collapseButton;
    public static GUIStyle CollapseButton
    {
        get
        {
            EnsureCurrent();
            if (_collapseButton == null)
            {
                _collapseButton = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 },
                    padding = new RectOffset(),
                    normal = { background = KarakTheme.Circle(KarakTheme.FrameBg, 20), textColor = KarakTheme.TextActive }
                };
            }
            return _collapseButton;
        }
    }

    private static GUIStyle _subwindowStyle;
    public static GUIStyle SubwindowStyle
    {
        get
        {
            EnsureCurrent();
            if (_subwindowStyle == null)
            {
                _subwindowStyle = new GUIStyle
                {
                    normal = { background = KarakTheme.RoundedRect(KarakTheme.WindowBg, 12f, 0f, Color.clear, 48), textColor = KarakTheme.TextActive },
                    border = new RectOffset { left = 12, right = 12, top = 12, bottom = 12 },
                    padding = new RectOffset { left = 16, right = 16, top = 40, bottom = 16 },
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft,
                    contentOffset = new Vector2(16f, 12f)
                };
                _subwindowStyle.normal.textColor = KarakTheme.TextActive;
            }
            return _subwindowStyle;
        }
    }

    private static GUIStyle _windowTitle;
    public static GUIStyle WindowTitle
    {
        get
        {
            EnsureCurrent();
            if (_windowTitle == null)
            {
                _windowTitle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = KarakTheme.TextActive }
                };
            }
            return _windowTitle;
        }
    }

    // ------------------------------------------------------------------
    // Section wrapper — rounded child panel with a bold title
    // ------------------------------------------------------------------
    private static GUIStyle _sectionStyle;
    private static GUIStyle _sectionTitle;

    public static void BeginSection(string title, params GUILayoutOption[] options)
    {
        EnsureCurrent();
        if (_sectionStyle == null)
        {
            _sectionStyle = new GUIStyle
            {
                normal = { background = KarakTheme.RoundedRect(KarakTheme.ChildBg, 10f, 0f, Color.clear, 40) },
                border = new RectOffset { left = 10, right = 10, top = 10, bottom = 10 },
                padding = new RectOffset { left = 14, right = 14, top = 12, bottom = 12 },
                margin = new RectOffset()
            };
        }
        if (_sectionTitle == null)
        {
            _sectionTitle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset { left = 0, right = 0, top = 0, bottom = 6 },
                normal = { textColor = KarakTheme.TextActive }
            };
        }
        GUILayout.BeginVertical(_sectionStyle, options);
        if (!string.IsNullOrEmpty(title))
        {
            // Section titles cycle through the secondary accent under RGB
            // mode, one hue off from the big tab title
            if (KarakTheme.RgbMode)
            {
                var rgbTitle = new GUIStyle(_sectionTitle);
                rgbTitle.normal.textColor = KarakTheme.SecondaryAccent;
                GUILayout.Label(title, rgbTitle);
            }
            else
            {
                GUILayout.Label(title, _sectionTitle);
            }
        }
    }

    public static void EndSection()
    {
        GUILayout.EndVertical();
    }

    // ------------------------------------------------------------------
    // Subtab bar — inline group of buttons with an animated accent underline
    // ------------------------------------------------------------------
    private static GUIStyle _subtabIdle;
    private static GUIStyle _subtabActive;
    private static readonly Dictionary<string, Rect> _lastSubtabRects = new();

    public static int SubtabBar(string barKey, string[] labels, int selected)
    {
        EnsureCurrent();

        if (_subtabIdle == null)
        {
            _subtabIdle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(),
                padding = new RectOffset { left = 12, right = 12, top = 6, bottom = 8 },
                margin = new RectOffset { left = 0, right = 4, top = 0, bottom = 0 },
                normal = { background = KarakTheme.ClearTexture(), textColor = KarakTheme.TextDisabled },
                hover = { background = KarakTheme.ClearTexture(), textColor = KarakTheme.TextActive },
                active = { background = KarakTheme.ClearTexture(), textColor = KarakTheme.TextActive }
            };
            _subtabActive = new GUIStyle(_subtabIdle)
            {
                normal = { background = KarakTheme.ClearTexture(), textColor = KarakTheme.Accent },
                hover = { background = KarakTheme.ClearTexture(), textColor = KarakTheme.Accent }
            };
        }

        GUILayout.BeginHorizontal();
        var rects = new Rect[labels.Length];
        for (int i = 0; i < labels.Length; i++)
        {
            var style = i == selected ? _subtabActive : _subtabIdle;
            if (GUILayout.Button(labels[i], style, GUILayout.Height(28f)))
            {
                selected = i;
            }
            if (Event.current.type == EventType.Repaint)
            {
                rects[i] = GUILayoutUtility.GetLastRect();
            }
        }
        GUILayout.EndHorizontal();

        // Animated underline — position and width lerp between the selected tab rects
        if (Event.current.type == EventType.Repaint && selected >= 0 && selected < rects.Length && rects[selected].width > 0f)
        {
            _lastSubtabRects.TryGetValue(barKey, out var last);
            if (last.width < 1f) last = rects[selected];

            float x = Anim($"sub-{barKey}-x", rects[selected].x, 14f);
            float w = Anim($"sub-{barKey}-w", rects[selected].width, 14f);

            var underline = new Rect(x + 6f, rects[selected].yMax - 3f, w - 12f, 2f);
            // Secondary accent under RGB mode — offsets from the sidebar/title
            var uColor = KarakTheme.RgbMode ? KarakTheme.SecondaryAccent : KarakTheme.Accent;
            DrawAccentStripe(underline, uColor);

            _lastSubtabRects[barKey] = rects[selected];
        }

        return selected;
    }

    // ------------------------------------------------------------------
    // Pill toggle switch — rounded track, sliding knob, animated color
    // ------------------------------------------------------------------
    private static GUIStyle _switchTrackOff;
    private static GUIStyle _switchTrackOn;
    private static GUIStyle _switchKnob;
    private static GUIStyle _rowSeparator;

    public static bool Toggle(bool value, string label, string key = null)
    {
        EnsureCurrent();

        if (_switchTrackOff == null)
        {
            _switchTrackOff = new GUIStyle
            {
                normal = { background = KarakTheme.RoundedRect(KarakTheme.FrameBg, 10f, 0f, Color.clear, 40) }
            };
            _switchTrackOn = new GUIStyle
            {
                // White base — tinted via GUI.color at draw time so RGB mode cycles
                normal = { background = KarakTheme.WhiteRoundedRect(10f, 40) }
            };
            _switchKnob = new GUIStyle
            {
                normal = { background = KarakTheme.Circle(Color.white, 16) }
            };
            _rowSeparator = new GUIStyle
            {
                normal = { background = KarakTheme.SolidColor(KarakTheme.FrameBg) },
                fixedHeight = 1f,
                margin = new RectOffset { left = 0, right = 0, top = 4, bottom = 4 }
            };
        }

        var row = GUILayoutUtility.GetRect(0f, 24f, GUILayout.ExpandWidth(true));

        const float switchW = 34f;
        const float switchH = 18f;
        const float knobSize = 14f;

        var labelRect = new Rect(row.x, row.y, row.width - switchW - 8f, row.height);
        var trackRect = new Rect(row.xMax - switchW, row.y + (row.height - switchH) / 2f, switchW, switchH);

        // Label
        var labelStyle = new GUIStyle(GUIStylePreset.Label) { alignment = TextAnchor.MiddleLeft };
        GUI.Label(labelRect, label, labelStyle);

        // Animated track: OFF box + ON box crossfading via GUI.color. The ON
        // box is a white rounded rect tinted with the current Accent so RGB
        // mode makes every toggle cycle through hues in unison.
        float t = Anim($"tog-{key ?? label}", value ? 1f : 0f, 14f);
        var savedColor = GUI.color;
        GUI.color = new Color(savedColor.r, savedColor.g, savedColor.b, savedColor.a * (1f - t));
        GUI.Box(trackRect, GUIContent.none, _switchTrackOff);
        var onTint = KarakTheme.Accent;
        GUI.color = new Color(onTint.r, onTint.g, onTint.b, savedColor.a * t);
        GUI.Box(trackRect, GUIContent.none, _switchTrackOn);
        GUI.color = savedColor;

        // Knob: slides from left to right based on t
        float knobX = trackRect.x + 2f + t * (switchW - knobSize - 4f);
        var knobRect = new Rect(knobX, trackRect.y + (switchH - knobSize) / 2f, knobSize, knobSize);
        GUI.Box(knobRect, GUIContent.none, _switchKnob);

        // Click handling
        var evt = Event.current;
        if (evt.type == EventType.MouseDown && evt.button == 0 && row.Contains(evt.mousePosition))
        {
            value = !value;
            GUI.changed = true;
            evt.Use();
        }

        // Row separator
        GUILayout.Box(GUIContent.none, _rowSeparator, GUILayout.ExpandWidth(true), GUILayout.Height(1f));

        return value;
    }

    // ------------------------------------------------------------------
    // Radio button — filled circle when selected
    // ------------------------------------------------------------------
    private static GUIStyle _radioOuter;
    private static GUIStyle _radioInner;

    public static bool Radio(bool value, string label)
    {
        EnsureCurrent();

        if (_radioOuter == null)
        {
            _radioOuter = new GUIStyle
            {
                normal = { background = KarakTheme.Circle(KarakTheme.FrameBg, 16) }
            };
            _radioInner = new GUIStyle
            {
                normal = { background = KarakTheme.Circle(Color.white, 16) }
            };
        }

        var row = GUILayoutUtility.GetRect(0f, 22f, GUILayout.ExpandWidth(true));
        const float size = 14f;
        var outerRect = new Rect(row.x + 2f, row.y + (row.height - size) / 2f, size, size);
        var labelRect = new Rect(outerRect.xMax + 8f, row.y, row.width - size - 12f, row.height);

        GUI.Box(outerRect, GUIContent.none, _radioOuter);
        float t = Anim($"radio-{label}", value ? 1f : 0f, 14f);
        if (t > 0.01f)
        {
            float innerSize = size * 0.5f * t;
            var innerRect = new Rect(
                outerRect.center.x - innerSize / 2f,
                outerRect.center.y - innerSize / 2f,
                innerSize, innerSize);
            GUI.Box(innerRect, GUIContent.none, _radioInner);
        }

        var labelStyle = new GUIStyle(GUIStylePreset.Label) { alignment = TextAnchor.MiddleLeft };
        GUI.Label(labelRect, label, labelStyle);

        var evt = Event.current;
        if (evt.type == EventType.MouseDown && evt.button == 0 && row.Contains(evt.mousePosition))
        {
            value = true;
            GUI.changed = true;
            evt.Use();
        }
        return value;
    }

    // ------------------------------------------------------------------
    // Slider with accent fill and white circular grab
    // ------------------------------------------------------------------
    private static GUIStyle _trackStyle;
    private static GUIStyle _fillStyle;
    private static GUIStyle _thumbStyle;
    private static readonly Dictionary<int, bool> _dragging = new();

    public static float LabeledSlider(string label, float value, float min, float max, string format = "0.###")
    {
        EnsureSliderStyles();

        GUILayout.BeginVertical(GUILayout.Height(38f));
        GUILayout.BeginHorizontal(GUILayout.Height(16f));
        GUILayout.Label(label, GUIStylePreset.Label);
        GUILayout.FlexibleSpace();
        var valueStyle = new GUIStyle(GUIStylePreset.Label) { alignment = TextAnchor.MiddleRight };
        GUILayout.Label(value.ToString(format), valueStyle, GUILayout.Width(80f));
        GUILayout.EndHorizontal();

        var trackArea = GUILayoutUtility.GetRect(0f, 16f, GUILayout.ExpandWidth(true));
        value = SliderCore(trackArea, value, min, max, label.GetHashCode());
        GUILayout.EndVertical();
        return value;
    }

    public static float Slider(Rect rect, float value, float min, float max)
    {
        EnsureSliderStyles();
        return SliderCore(rect, value, min, max, rect.GetHashCode());
    }

    private static void EnsureSliderStyles()
    {
        EnsureCurrent();
        if (_trackStyle == null)
        {
            _trackStyle = new GUIStyle
            {
                normal = { background = KarakTheme.RoundedRect(KarakTheme.FrameBg, 3f, 0f, Color.clear, 12) }
            };
            _fillStyle = new GUIStyle
            {
                normal = { background = KarakTheme.RoundedRect(KarakTheme.Accent, 3f, 0f, Color.clear, 12) }
            };
            _thumbStyle = new GUIStyle
            {
                normal = { background = KarakTheme.Circle(Color.white, 16) }
            };
        }
    }

    private static float SliderCore(Rect rect, float value, float min, float max, int id)
    {
        float t = Mathf.InverseLerp(min, max, value);
        var track = new Rect(rect.x, rect.y + rect.height / 2f - 3f, rect.width, 6f);
        GUI.Box(track, GUIContent.none, _trackStyle);
        if (t > 0f)
        {
            var fill = new Rect(track.x, track.y, track.width * t, track.height);
            GUI.Box(fill, GUIContent.none, _fillStyle);
        }
        float thumbSize = 14f;
        var thumb = new Rect(track.x + track.width * t - thumbSize / 2f, rect.y + rect.height / 2f - thumbSize / 2f, thumbSize, thumbSize);
        GUI.Box(thumb, GUIContent.none, _thumbStyle);

        var evt = Event.current;
        _dragging.TryGetValue(id, out var drag);
        if (evt.type == EventType.MouseDown && evt.button == 0 && rect.Contains(evt.mousePosition))
        {
            drag = true;
            _dragging[id] = true;
            value = Mathf.Lerp(min, max, Mathf.Clamp01((evt.mousePosition.x - track.x) / track.width));
            GUI.changed = true;
            evt.Use();
        }
        else if (evt.type == EventType.MouseUp)
        {
            _dragging[id] = false;
        }
        if (drag && evt.type == EventType.MouseDrag)
        {
            value = Mathf.Lerp(min, max, Mathf.Clamp01((evt.mousePosition.x - track.x) / track.width));
            GUI.changed = true;
            evt.Use();
        }
        return value;
    }

    // ------------------------------------------------------------------
    // Combo box — click to expand a dropdown
    // ------------------------------------------------------------------
    private static GUIStyle _comboBoxStyle;
    private static GUIStyle _popupItem;

    private static int _openComboId = -1;

    public static int Combo(string label, int selected, string[] options)
    {
        int comboLabelId = label.GetHashCode();
        EnsureCurrent();
        if (_comboBoxStyle == null)
        {
            _comboBoxStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleRight,
                padding = new RectOffset { left = 8, right = 22, top = 4, bottom = 4 },
                margin = new RectOffset(),
                border = new RectOffset { left = 6, right = 6, top = 6, bottom = 6 },
                normal = { background = KarakTheme.RoundedRect(KarakTheme.FrameBg, 6f, 0f, Color.clear, 24), textColor = KarakTheme.TextActive },
                hover = { background = KarakTheme.RoundedRect(KarakTheme.FrameBg, 6f, 0f, Color.clear, 24), textColor = KarakTheme.Accent },
                active = { background = KarakTheme.RoundedRect(KarakTheme.FrameBg, 6f, 0f, Color.clear, 24), textColor = KarakTheme.Accent }
            };
        }
        if (_popupItem == null)
        {
            _popupItem = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset { left = 10, right = 10, top = 3, bottom = 3 },
                margin = new RectOffset { left = 0, right = 0, top = 0, bottom = 2 },
                normal = { background = KarakTheme.RoundedRect(KarakTheme.FrameBg, 6f, 0f, Color.clear, 24), textColor = KarakTheme.TextActive },
                hover = { background = KarakTheme.RoundedRect(KarakTheme.Accent, 6f, 0f, Color.clear, 24), textColor = Color.black },
                active = { background = KarakTheme.RoundedRect(KarakTheme.Accent, 6f, 0f, Color.clear, 24), textColor = Color.black }
            };
        }

        GUILayout.BeginHorizontal(GUILayout.Height(24f));
        GUILayout.Label(label, GUIStylePreset.Label, GUILayout.Width(90f));
        GUILayout.FlexibleSpace();
        var selText = selected >= 0 && selected < options.Length ? options[selected] : "";
        if (GUILayout.Button(selText + "  v", _comboBoxStyle, GUILayout.Width(140f), GUILayout.Height(24f)))
            _openComboId = _openComboId == comboLabelId ? -1 : comboLabelId;
        GUILayout.EndHorizontal();

        // The open options are emitted as ordinary layout rows directly under
        // the button instead of being drawn as an absolutely-positioned popup.
        // An overlay needs an anchor rect from GUILayoutUtility.GetLastRect(),
        // which is only reliable during Layout/Repaint — but the click that
        // OPENS the dropdown lands on a MouseUp pass, so the captured rect was
        // junk, the popup drew at a bogus position, and its hit tests never
        // lined up with what you saw. As real layout rows the options are
        // always in the right place and always clickable, and the value is
        // returned synchronously in the same pass (no pending-result handoff).
        if (_openComboId == comboLabelId)
        {
            GUILayout.BeginVertical();
            GUILayout.Space(2f);
            for (int i = 0; i < options.Length; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(90f); // line the options up under the button
                // NOTE: GUILayout.MaxWidth / MinWidth / MaxHeight / MinHeight all
                // throw ObjectCollectedException in this IL2CPP build. Only
                // GUILayout.Width / GUILayout.Height are safe.
                if (GUILayout.Button(options[i], _popupItem, GUILayout.Width(200f), GUILayout.Height(22f)))
                {
                    selected = i;
                    _openComboId = -1;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(4f);
            GUILayout.EndVertical();
        }

        return selected;
    }

    public static bool HasOpenCombo() => _openComboId >= 0;

    // Called on tab/category switches, and whenever the menu is hidden, so a
    // dropdown opened on one tab can't stay open over a different tab.
    public static void CloseOpenCombo() => _openComboId = -1;

    // ------------------------------------------------------------------
    // Color swatch
    // ------------------------------------------------------------------
    public static Color ColorSwatch(string label, Color color)
    {
        var swatchStyle = new GUIStyle
        {
            normal = { background = KarakTheme.RoundedRect(color, 4f, 0f, Color.clear, 16) }
        };
        GUILayout.BeginHorizontal(GUILayout.Height(22f));
        GUILayout.Label(label, GUIStylePreset.Label);
        GUILayout.FlexibleSpace();
        GUILayout.Box(GUIContent.none, swatchStyle, GUILayout.Width(28f), GUILayout.Height(18f));
        GUILayout.EndHorizontal();
        return color;
    }

    // ------------------------------------------------------------------
    // Flat button
    // ------------------------------------------------------------------
    public static bool Button(string text, params GUILayoutOption[] options)
    {
        return GUILayout.Button(text, GUIStylePreset.Button, options);
    }

    // ------------------------------------------------------------------
    // Text input
    //
    // Unity's own GUILayout.TextField / GUI.TextField cannot be used in this
    // build: they funnel into TextEditor.set_text, which Il2CppInterop reports
    // as "Method unstripping failed" because Unity stripped it from the IL2CPP
    // binary. Every call throws NotSupportedException, which then aborts
    // WindowFunction before GUI.DragWindow runs — killing both typing and the
    // ability to drag the menu. So we draw the field ourselves and read
    // keystrokes straight off the IMGUI event instead.
    // ------------------------------------------------------------------
    private static string _focusedInput;
    private static GUIStyle _inputText;

    public static bool IsTextInputFocused => _focusedInput != null;

    public static string TextInput(string value, string key, params GUILayoutOption[] options)
    {
        EnsureCurrent();
        value ??= "";

        if (_inputText == null)
        {
            _inputText = new GUIStyle(GUIStylePreset.Label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                padding = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 },
                margin = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 }
            };
        }

        var style = GUIStylePreset.TextField;
        Rect rect = GUILayoutUtility.GetRect(60f, 4000f, 26f, 26f, style, options);

        Event e = Event.current;
        bool focused = _focusedInput == key;

        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            _focusedInput = key;
            GUIUtility.keyboardControl = 0;
            e.Use();
            focused = true;
        }
        else if (e.type == EventType.MouseDown && focused)
        {
            _focusedInput = null;
        }

        if (focused && e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Backspace)
            {
                if (value.Length > 0) value = value.Substring(0, value.Length - 1);
                e.Use();
            }
            else if (e.keyCode == KeyCode.Escape || e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                _focusedInput = null;
                e.Use();
            }
            else if (e.character >= ' ' && !char.IsControl(e.character))
            {
                value += e.character;
                e.Use();
            }
        }

        // Draw — plain boxes/labels only, nothing that reaches TextEditor.
        GUI.Box(rect, GUIContent.none, style);

        string shown = value;
        if (focused)
        {
            // Blinking caret without a TextEditor
            if (Time.unscaledTime % 0.55f < 0.32f) shown += "|";
        }
        else if (value.Length == 0)
        {
            shown = "";
        }

        var textRect = new Rect(
            rect.x + style.padding.left,
            rect.y,
            rect.width - style.padding.left - style.padding.right,
            rect.height);
        _inputText.normal.textColor = focused ? KarakTheme.TextActive : KarakTheme.TextDisabled;
        GUI.Label(textRect, shown, _inputText);

        return value;
    }

    // ------------------------------------------------------------------
    // Action row — label on the left, tappable "Run" button on the right.
    // Use this for one-shot actions (Save Profile, Kill Player, …) instead
    // of toggle switches, which would falsely imply stateful on/off.
    // ------------------------------------------------------------------
    private static GUIStyle _actionButton;
    public static bool Action(string label, string buttonText = "Run")
    {
        EnsureCurrent();
        if (_actionButton == null)
        {
            _actionButton = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset { left = 6, right = 6, top = 6, bottom = 6 },
                padding = new RectOffset { left = 10, right = 10, top = 4, bottom = 4 },
                margin = new RectOffset(),
                normal = { background = KarakTheme.RoundedRect(KarakTheme.FrameBg, 6f, 0f, Color.clear, 24), textColor = KarakTheme.TextActive },
                hover = { background = KarakTheme.WhiteRoundedRect(6f, 24), textColor = Color.black },
                active = { background = KarakTheme.WhiteRoundedRect(6f, 24), textColor = Color.black }
            };
        }

        GUILayout.BeginHorizontal(GUILayout.Height(24f));
        GUILayout.Label(label, GUIStylePreset.Label, GUILayout.ExpandWidth(true));
        bool clicked = GUILayout.Button(buttonText, _actionButton, GUILayout.Width(70f), GUILayout.Height(20f));
        GUILayout.EndHorizontal();

        GUILayout.Box(GUIContent.none, RowSeparator, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
        return clicked;
    }

    private static GUIStyle _rowSepPublic;
    public static GUIStyle RowSeparator
    {
        get
        {
            EnsureCurrent();
            return _rowSepPublic ??= new GUIStyle
            {
                normal = { background = KarakTheme.SolidColor(KarakTheme.FrameBg) },
                fixedHeight = 1f,
                margin = new RectOffset { left = 0, right = 0, top = 4, bottom = 4 }
            };
        }
    }

    // ------------------------------------------------------------------
    // Separator line
    // ------------------------------------------------------------------
    public static void Separator(float width)
    {
        GUILayout.Box(GUIContent.none, GUIStylePreset.Separator, GUILayout.Width(width), GUILayout.Height(1f));
    }

    // ------------------------------------------------------------------
    // KARAKMENU brand label — drawn inside the window's top-left corner.
    // Shows "KM" when the sidebar is collapsed (narrow) so the label doesn't
    // spill onto the tab list.
    // ------------------------------------------------------------------
    private static GUIStyle _brandStyle;
    public static void DrawBrand(Rect rect, bool compact)
    {
        _brandStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            richText = true
        };

        var color = KarakTheme.RgbMode ? RgbAt(rect.x, 1f) : KarakTheme.TextActive;
        _brandStyle.normal.textColor = color;
        _brandStyle.alignment = compact ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
        GUI.Label(rect, compact ? "KM" : "KARAKMENU", _brandStyle);
    }

    // ------------------------------------------------------------------
    // Horizontal RGB sway — hue varies across x + slides over time.
    // A window laid out along the x axis reads as a rainbow that flows
    // left → right rather than every element cycling in unison.
    // ------------------------------------------------------------------
    public static Color RgbAt(float x, float widthDivisor = 500f)
    {
        float hue = (MenuUI.hue + x / widthDivisor) % 1f;
        if (hue < 0f) hue += 1f;
        return Color.HSVToRGB(hue, 1f, 1f);
    }

    // Wraps each character of `text` in a TMP <color> tag, sweeping smoothly
    // through the full hue spectrum left-to-right. Same HSV-sweep math as
    // RgbAt/DrawRgbGlow, just keyed by character index instead of a pixel Rect
    // so it works on HUD text (TMP labels driven by game code, no GUILayout
    // Rect available) rather than only inside KarakMenu's own IMGUI window.
    // Driven by Time.unscaledTime rather than MenuUI.hue so it keeps animating
    // even while the menu window itself is closed.
    public static string RainbowText(string text, float speed = 0.15f, float hueStep = 0.05f)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var sb = new StringBuilder(text.Length * 20);
        for (int i = 0; i < text.Length; i++)
        {
            float hue = (Time.unscaledTime * speed + i * hueStep) % 1f;
            if (hue < 0f) hue += 1f;
            var c = Color.HSVToRGB(hue, 1f, 1f);
            sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(c)).Append('>').Append(text[i]).Append("</color>");
        }
        return sb.ToString();
    }

    // ------------------------------------------------------------------
    // RGB perimeter — a hollow rounded ring painted as many small colored
    // segments. Straight edges get axis-aligned pips; the 4 corners are
    // drawn as small rotated squares along a quarter-arc so the ring is
    // smoothly rounded instead of having 4 sharp right angles. No fill, so
    // the window's opacity slider can't leak the border color inside.
    // ------------------------------------------------------------------
    private static GUIStyle _swatch;
    private static GUIStyle _cornerDotStyle;

    public static void DrawRgbGlow(Rect windowRect)
    {
        _swatch ??= new GUIStyle
        {
            normal = { background = KarakTheme.WhiteRoundedRect(1.5f, 8) }
        };
        _cornerDotStyle ??= new GUIStyle
        {
            normal = { background = KarakTheme.Circle(Color.white, 8) }
        };

        const float thickness = 3f;   // ring thickness
        const float inset = -2f;      // sits just outside the window edge
        const float radius = 14f;     // matches the window's corner radius
        const int edgeSegments = 60;  // segments along each long edge
        const int cornerSegments = 14;// dots per quarter arc

        var r = new Rect(windowRect.x + inset, windowRect.y + inset,
                         windowRect.width - inset * 2f, windowRect.height - inset * 2f);

        var saved = GUI.color;

        // Straight edges — inset by `radius` at both ends so the corners fill the gap
        DrawEdgeH(new Rect(r.x + radius, r.y, r.width - radius * 2f, thickness), edgeSegments, ascending: true);
        DrawEdgeH(new Rect(r.x + radius, r.yMax - thickness, r.width - radius * 2f, thickness), edgeSegments, ascending: false);
        DrawEdgeV(new Rect(r.x, r.y + radius, thickness, r.height - radius * 2f), edgeSegments / 2, ascending: false);
        DrawEdgeV(new Rect(r.xMax - thickness, r.y + radius, thickness, r.height - radius * 2f), edgeSegments / 2, ascending: true);

        // 4 quarter-arcs at the corners — small colored dots trace the curve
        DrawCornerArc(new Vector2(r.x + radius, r.y + radius),           radius, Mathf.PI,          Mathf.PI * 1.5f, cornerSegments); // TL
        DrawCornerArc(new Vector2(r.xMax - radius, r.y + radius),        radius, Mathf.PI * 1.5f,   Mathf.PI * 2f,   cornerSegments); // TR
        DrawCornerArc(new Vector2(r.xMax - radius, r.yMax - radius),     radius, 0f,                Mathf.PI * 0.5f, cornerSegments); // BR
        DrawCornerArc(new Vector2(r.x + radius, r.yMax - radius),        radius, Mathf.PI * 0.5f,   Mathf.PI,        cornerSegments); // BL

        GUI.color = saved;
    }

    private static void DrawEdgeH(Rect strip, int count, bool ascending)
    {
        if (strip.width <= 0f) return;
        float segW = strip.width / count;
        for (int i = 0; i < count; i++)
        {
            float t = ascending ? i : (count - 1 - i);
            GUI.color = RgbAt(strip.x + t * segW, strip.width);
            GUI.Box(new Rect(strip.x + i * segW, strip.y, segW + 1f, strip.height), GUIContent.none, _swatch);
        }
    }

    private static void DrawEdgeV(Rect strip, int count, bool ascending)
    {
        if (strip.height <= 0f) return;
        float segH = strip.height / count;
        for (int i = 0; i < count; i++)
        {
            float t = ascending ? i : (count - 1 - i);
            GUI.color = RgbAt(t * segH + strip.height, strip.height);
            GUI.Box(new Rect(strip.x, strip.y + i * segH, strip.width, segH + 1f), GUIContent.none, _swatch);
        }
    }

    // Trace a quarter arc from angle a0 → a1 with N little colored dots.
    // Hue is picked from the dot's x position so it blends into the neighbouring straight edge.
    private static void DrawCornerArc(Vector2 center, float radius, float a0, float a1, int count)
    {
        const float dotSize = 5f;
        for (int i = 0; i <= count; i++)
        {
            float t = i / (float)count;
            float a = Mathf.Lerp(a0, a1, t);
            float x = center.x + (float)Math.Cos(a) * radius;
            float y = center.y + (float)Math.Sin(a) * radius;
            GUI.color = RgbAt(x, 500f);
            GUI.Box(new Rect(x - dotSize / 2f, y - dotSize / 2f, dotSize, dotSize), GUIContent.none, _cornerDotStyle);
        }
    }
}

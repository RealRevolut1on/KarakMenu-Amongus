using UnityEngine;

namespace KarakMenu;

// Global GUI styles, matching the "2" ImGui reference exactly.
public static class GUIStylePreset
{
    private static bool _skinInitialized;
    private static int _skinVersion = -1;
    private static int _styleVersion = -1;

    private static GUIStyle _window;
    private static GUIStyle _label;
    private static GUIStyle _button;
    private static GUIStyle _toggle;
    private static GUIStyle _box;
    private static GUIStyle _track;
    private static GUIStyle _thumb;
    private static GUIStyle _textField;
    private static GUIStyle _section;
    private static GUIStyle _sectionTitle;
    private static GUIStyle _tabTitle;
    private static GUIStyle _tabSubtitle;
    private static GUIStyle _separator;
    private static GUIStyle _darkSeparator;
    private static GUIStyle _normalButton;
    private static GUIStyle _normalToggle;
    private static GUIStyle _tabButton;

    private static void EnsureCurrent()
    {
        if (_styleVersion != KarakTheme.TextureVersion) Invalidate();
    }

    public static void Invalidate()
    {
        _styleVersion = KarakTheme.TextureVersion;
        _skinInitialized = false;
        _window = _label = _button = _toggle = _box = _track = _thumb = _textField = null;
        _section = _sectionTitle = _tabTitle = _tabSubtitle = null;
        _separator = _darkSeparator = _normalButton = _normalToggle = _tabButton = null;
    }

    public static void EnsureSkin()
    {
        if (_skinInitialized && _skinVersion == KarakTheme.TextureVersion) return;

        var skin = Object.Instantiate(GUI.skin);
        skin.name = "KarakSkin";
        skin.font = GUI.skin.font;

        skin.window = Window;
        skin.box = Box;
        skin.label = Label;
        skin.button = Button;
        skin.toggle = Toggle;
        skin.horizontalSlider = Track;
        skin.horizontalSliderThumb = Thumb;
        skin.textField = TextField;
        skin.textArea = TextField;

        skin.scrollView = new GUIStyle { padding = new RectOffset { left = 4, right = 4, top = 4, bottom = 4 } };
        skin.verticalScrollbar = new GUIStyle
        {
            normal = { background = KarakTheme.SolidColor(KarakTheme.ChildBg) },
            fixedWidth = 8f
        };
        skin.verticalScrollbarThumb = new GUIStyle
        {
            normal = { background = KarakTheme.RoundedRect(KarakTheme.Accent, 4f, 0f, Color.clear, 16) },
            fixedWidth = 8f
        };
        skin.horizontalScrollbar = new GUIStyle
        {
            normal = { background = KarakTheme.SolidColor(KarakTheme.ChildBg) },
            fixedHeight = 8f
        };
        skin.horizontalScrollbarThumb = new GUIStyle
        {
            normal = { background = KarakTheme.RoundedRect(KarakTheme.Accent, 4f, 0f, Color.clear, 16) },
            fixedHeight = 8f
        };
        skin.verticalScrollbarUpButton = new GUIStyle();
        skin.verticalScrollbarDownButton = new GUIStyle();
        skin.horizontalScrollbarLeftButton = new GUIStyle();
        skin.horizontalScrollbarRightButton = new GUIStyle();

        GUI.skin = skin;

        _skinInitialized = true;
        _skinVersion = KarakTheme.TextureVersion;
    }

    // Main window â€” flat dark rounded frame, no border
    public static GUIStyle Window
    {
        get
        {
            EnsureCurrent();
            if (_window == null)
            {
                _window = new GUIStyle
                {
                    normal = { background = KarakTheme.RoundedRect(KarakTheme.WindowBg, 14f, 0f, Color.clear, 48), textColor = KarakTheme.TextActive },
                    border = new RectOffset { left = 14, right = 14, top = 14, bottom = 14 },
                    padding = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 },
                    margin = new RectOffset(),
                    fontSize = 14,
                    alignment = TextAnchor.UpperLeft
                };
            }
            return _window;
        }
    }

    // Child panel â€” the "windows" that hold widget groups (General, Window 2, etc.)
    public static GUIStyle Section
    {
        get
        {
            EnsureCurrent();
            if (_section == null)
            {
                _section = new GUIStyle
                {
                    normal = { background = KarakTheme.RoundedRect(KarakTheme.ChildBg, 10f, 0f, Color.clear, 40) },
                    border = new RectOffset { left = 10, right = 10, top = 10, bottom = 10 },
                    padding = new RectOffset { left = 14, right = 14, top = 12, bottom = 12 },
                    margin = new RectOffset()
                };
            }
            return _section;
        }
    }

    // Bold title text at the top of each child section ("General", "Window 2"...)
    public static GUIStyle SectionTitle
    {
        get
        {
            EnsureCurrent();
            if (_sectionTitle == null)
            {
                _sectionTitle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(),
                    margin = new RectOffset { left = 0, right = 0, top = 0, bottom = 8 },
                    normal = { textColor = KarakTheme.TextActive }
                };
            }
            return _sectionTitle;
        }
    }

    // Big title at the top of the content area ("Ragebot")
    public static GUIStyle TabTitle
    {
        get
        {
            EnsureCurrent();
            if (_tabTitle == null)
            {
                _tabTitle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = KarakTheme.TextActive }
                };
            }
            return _tabTitle;
        }
    }

    // Subtab labels ("General", "Accuracy", "Exploits")
    public static GUIStyle TabSubtitle
    {
        get
        {
            EnsureCurrent();
            if (_tabSubtitle == null)
            {
                _tabSubtitle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    margin = new RectOffset { left = 0, right = 0, top = 6, bottom = 6 },
                    normal = { textColor = KarakTheme.TextActive }
                };
            }
            return _tabSubtitle;
        }
    }

    public static GUIStyle Label
    {
        get
        {
            EnsureCurrent();
            if (_label == null)
            {
                _label = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    richText = true,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 },
                    margin = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 },
                    normal = { textColor = KarakTheme.TextActive },
                    hover = { textColor = KarakTheme.TextActive }
                };
            }
            return _label;
        }
    }

    // Plain flat button matching the FrameBg color
    public static GUIStyle Button
    {
        get
        {
            EnsureCurrent();
            if (_button == null)
            {
                _button = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleCenter,
                    border = new RectOffset { left = 6, right = 6, top = 6, bottom = 6 },
                    padding = new RectOffset { left = 10, right = 10, top = 6, bottom = 6 },
                    margin = new RectOffset(),
                    normal   = { background = KarakTheme.RoundedRect(KarakTheme.FrameBg, 6f, 0f, Color.clear, 24), textColor = KarakTheme.TextDisabled },
                    hover    = { background = KarakTheme.RoundedRect(KarakTheme.FrameBg, 6f, 0f, Color.clear, 24), textColor = KarakTheme.TextActive },
                    active   = { background = KarakTheme.RoundedRect(KarakTheme.Accent, 6f, 0f, Color.clear, 24), textColor = new Color(0f, 0f, 0f, 1f) }
                };
            }
            return _button;
        }
    }

    // Native Unity toggle style (background is only drawn during the label row,
    // so we hide it here and let KarakUI.Toggle draw a custom rect instead)
    public static GUIStyle Toggle
    {
        get
        {
            EnsureCurrent();
            if (_toggle == null)
            {
                _toggle = new GUIStyle(GUI.skin.toggle)
                {
                    fontSize = 13,
                    padding = new RectOffset(),
                    margin = new RectOffset(),
                    normal = { background = KarakTheme.ClearTexture(), textColor = KarakTheme.TextActive },
                    hover = { background = KarakTheme.ClearTexture(), textColor = KarakTheme.TextActive },
                    onNormal = { background = KarakTheme.ClearTexture(), textColor = KarakTheme.TextActive },
                    onHover = { background = KarakTheme.ClearTexture(), textColor = KarakTheme.TextActive }
                };
            }
            return _toggle;
        }
    }

    public static GUIStyle Box
    {
        get
        {
            EnsureCurrent();
            if (_box == null)
            {
                _box = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = KarakTheme.RoundedRect(KarakTheme.ChildBg, 10f, 0f, Color.clear, 40), textColor = KarakTheme.TextActive },
                    border = new RectOffset { left = 10, right = 10, top = 10, bottom = 10 },
                    padding = new RectOffset { left = 10, right = 10, top = 10, bottom = 10 },
                    margin = new RectOffset()
                };
            }
            return _box;
        }
    }

    public static GUIStyle Track
    {
        get
        {
            EnsureCurrent();
            if (_track == null)
            {
                _track = new GUIStyle(GUI.skin.horizontalSlider)
                {
                    normal = { background = KarakTheme.RoundedRect(KarakTheme.FrameBg, 2f, 0f, Color.clear, 8) },
                    hover = { background = KarakTheme.RoundedRect(KarakTheme.FrameBg, 2f, 0f, Color.clear, 8) },
                    border = new RectOffset { left = 2, right = 2, top = 2, bottom = 2 },
                    margin = new RectOffset { left = 0, right = 0, top = 9, bottom = 9 }
                };
            }
            return _track;
        }
    }

    public static GUIStyle Thumb
    {
        get
        {
            EnsureCurrent();
            if (_thumb == null)
            {
                _thumb = new GUIStyle(GUI.skin.horizontalSliderThumb)
                {
                    normal = { background = KarakTheme.Circle(KarakTheme.TextActive, 16) },
                    hover = { background = KarakTheme.Circle(KarakTheme.TextActive, 16) },
                    active = { background = KarakTheme.Circle(KarakTheme.TextActive, 16) },
                    fixedWidth = 14f,
                    fixedHeight = 14f
                };
            }
            return _thumb;
        }
    }

    public static GUIStyle TextField
    {
        get
        {
            EnsureCurrent();
            if (_textField == null)
            {
                _textField = new GUIStyle(GUI.skin.textField)
                {
                    fontSize = 13,
                    border = new RectOffset { left = 4, right = 4, top = 4, bottom = 4 },
                    padding = new RectOffset { left = 8, right = 8, top = 5, bottom = 5 },
                    normal   = { background = KarakTheme.RoundedRect(KarakTheme.FrameBg, 6f, 0f, Color.clear, 24), textColor = KarakTheme.TextActive },
                    focused  = { background = KarakTheme.RoundedRect(KarakTheme.FrameBg, 6f, 1f, KarakTheme.Accent, 24), textColor = KarakTheme.TextActive }
                };
            }
            return _textField;
        }
    }

    public static GUIStyle Separator
    {
        get
        {
            EnsureCurrent();
            if (_separator == null)
            {
                _separator = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = KarakTheme.SolidColor(KarakTheme.Border) },
                    margin = new RectOffset { left = 0, right = 0, top = 4, bottom = 4 },
                    padding = new RectOffset(),
                    border = new RectOffset { left = 1, right = 1, top = 1, bottom = 1 },
                    fixedHeight = 1f
                };
            }
            return _separator;
        }
    }

    // Aliases kept so existing tab files continue to compile
    public static GUIStyle DarkSeparator => Separator;
    public static GUIStyle NormalButton
    {
        get
        {
            EnsureCurrent();
            if (_normalButton == null)
                _normalButton = new GUIStyle(Button) { fontSize = 13 };
            return _normalButton;
        }
    }
    public static GUIStyle NormalToggle
    {
        get
        {
            EnsureCurrent();
            if (_normalToggle == null)
                _normalToggle = new GUIStyle(Toggle) { fontSize = 13 };
            return _normalToggle;
        }
    }
    public static GUIStyle TabButton
    {
        get
        {
            EnsureCurrent();
            if (_tabButton == null)
                _tabButton = new GUIStyle(Button)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            return _tabButton;
        }
    }
    public static GUIStyle InnerPanel => Section;
    public static GUIStyle SectionBox => Section;
}

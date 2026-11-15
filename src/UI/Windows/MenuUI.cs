using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace KarakMenu;

// KarakMenu main window — tab-in-tab structure (sidebar categories, top subtabs),
// scrollable content, animated indicators, and pill-toggle widgets.
public class MenuUI : MonoBehaviour
{
    public static int windowHeight = 520;
    public static int windowWidth = 780;
    private Rect _windowRect;

    public static bool isGUIActive = false;
    public static float hue;

    // Whether the mouse is currently over the menu window (only meaningful
    // while isGUIActive) — lets other systems (e.g. the scroll-to-zoom cheat)
    // suppress input that would otherwise fall through underneath the menu.
    public static bool IsMouseOverMenu;

    // Width the tab body has to work with — set every frame so tab layouts
    // can size their columns to fit without triggering horizontal scrollbars
    public static float ContentWidth = 560f;

    private bool _sidebarExpanded = true;
    private const float SidebarWidthCollapsed = 60f;
    private const float SidebarWidthExpanded = 180f;

    private int _selectedCategory;
    private Vector2 _contentScroll;
    private float _lastCategoryChange;
    private float _lastSubtabChange;

    // Top-level categories shown in the sidebar. Each has one or more subtabs.
    private class Category
    {
        public string Name;
        public KarakTheme.KarakIcon Icon;
        public List<ITab> Tabs = new();
        public int Selected;
    }

    private List<Category> _categories = new();

    private void Start()
    {
        _categories.Add(new Category
        {
            Name = "Combat",
            Icon = KarakTheme.KarakIcon.Crosshair,
            Tabs = new List<ITab> { new MovementTab(), new HostOnlyTab() }
        });
        _categories.Add(new Category
        {
            Name = "Visuals",
            Icon = KarakTheme.KarakIcon.Eye,
            Tabs = new List<ITab> { new ESPTab(), new AnimationsTab() }
        });
        _categories.Add(new Category
        {
            Name = "Roles",
            Icon = KarakTheme.KarakIcon.User,
            Tabs = new List<ITab> { new RolesTab() }
        });
        _categories.Add(new Category
        {
            Name = "Players",
            Icon = KarakTheme.KarakIcon.Crown,
            Tabs = new List<ITab> { new PlayersTab() }
        });
        _categories.Add(new Category
        {
            Name = "Ship",
            Icon = KarakTheme.KarakIcon.Ship,
            Tabs = new List<ITab> { new ShipTab(), new ChatTab() }
        });
        _categories.Add(new Category
        {
            Name = "System",
            Icon = KarakTheme.KarakIcon.Gear,
            Tabs = new List<ITab> { new SearchTab(), new ModesTab(), new PassiveTab(), new ConfigTab(), new KeybindsTab(), new ConsoleTab(), new MemoryTab() }
        });
        _categories.Add(new Category
        {
            Name = "AC",
            Icon = KarakTheme.KarakIcon.Shield,
            Tabs = new List<ITab> { new AnticheatTab(), new AnticheatChecksTab(), new AnticheatLimitsTab() }
        });

        _windowRect = new(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
    }

    public void InitStyles() { GUIStylePreset.EnsureSkin(); }

    private void Update()
    {
        // Suppressed while typing in a menu text field, otherwise a letter in
        // the field could double as the menu toggle and close it mid-word.
        if (!KarakUI.IsTextInputFocused
            && Input.GetKeyDown(Utils.StringToKeycode(KarakMenu.menuKeybind.Value)))
        {
            isGUIActive = !isGUIActive;
            if (KarakMenu.menuOpenOnMouse.Value)
            {
                Vector2 mp = Input.mousePosition;
                _windowRect.position = new Vector2(mp.x, Screen.height - mp.y);
            }
        }

        if (CheatToggles.rgbMode)
        {
            hue += Time.deltaTime * 0.6f;
            if (hue > 1f) hue -= 1f;
        }

        if (CheatToggles.stealthMode != KarakMenu.inStealthMode)
        {
            KarakMenu.inStealthMode = CheatToggles.stealthMode;
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name == "MainMenu" || scene.name == "MatchMaking") SceneManager.LoadScene(scene.name);
        }

        if (CheatToggles.panicMode) Utils.Panic();

        KarakStreamProof.Tick();
        Anticheat.Update();
        KarakCheats.NukeLobbyCheat();
        KarakCheats.DisableCamerasCheat();
        KarakCheats.UnlimitedMeetingsCheat();
        KarakCheats.DisableMeetingsCheat();

        var stamp = ModManager.Instance.ModStamp;
        if (stamp) stamp.enabled = !(KarakMenu.inStealthMode || KarakMenu.isPanicked);

        if (KarakLobbyDecal.Instance) KarakLobbyDecal.Instance.enabled = !(KarakMenu.inStealthMode || KarakMenu.isPanicked);

        if (CheatToggles.openConfig) { Utils.OpenConfigFile(); CheatToggles.openConfig = false; }
        if (CheatToggles.reloadConfig) { KarakMenu.Plugin.Config.Reload(); CheatToggles.reloadConfig = false; }
        if (CheatToggles.saveProfile) { CheatToggles.saveProfile = false; CheatToggles.SaveTogglesToProfile(); }
        if (CheatToggles.loadProfile) { CheatToggles.LoadTogglesFromProfile(); CheatToggles.loadProfile = false; }

        if (!Utils.isPlayer)
        {
            CheatToggles.setFakeRole = false; CheatToggles.setFakeAlive = false; CheatToggles.killAll = false;
            CheatToggles.telekillPlayer = false; CheatToggles.killAllCrew = false; CheatToggles.killAllImps = false;
            CheatToggles.teleportPlayer = false; CheatToggles.teleportPlayerToVent = false; CheatToggles.spectate = false; CheatToggles.freecam = false;
            CheatToggles.customSpeed = false;
            CheatToggles.killPlayer = false; CheatToggles.callMeeting = false; CheatToggles.flashKill = false;
            CheatToggles.levelFarm = false;
            CheatToggles.copyOutfitToAll = false; CheatToggles.showRolesMenu = false;
            // Draw calls that need a LocalPlayer
            CheatToggles.autoKill = false; CheatToggles.invisibility = false;
            CheatToggles.confuseNow = false; CheatToggles.immortality = false;
            KarakWhisper.Target = null;
            KarakPlayersCheats.JailTargets.Clear();
            if (CheatToggles.runOverload) OverloadUI.StopOverload();
        }

        if (!Utils.isShip)
        {
            CheatToggles.sabotageMap = false; CheatToggles.unfixableLights = false; CheatToggles.completeMyTasks = false;
            CheatToggles.kickVents = false; CheatToggles.reportBody = false; CheatToggles.closeMeeting = false;
            CheatToggles.reactorSab = false; CheatToggles.oxygenSab = false; CheatToggles.commsSab = false;
            CheatToggles.elecSab = false; CheatToggles.mushSab = false; CheatToggles.closeAllDoors = false;
            CheatToggles.openAllDoors = false; CheatToggles.spamCloseAllDoors = false; CheatToggles.spamOpenAllDoors = false;
            CheatToggles.mushSpore = false;
            CheatToggles.levelFarm = false;
            KarakPlayersCheats.JailTargets.Clear();
            CheatToggles.ssAllIntoSelf = false; CheatToggles.spamShapeshiftAll = false; CheatToggles.unshiftAll = false;
            // Draw calls that need ShipStatus (vent/door tricks)
            CheatToggles.spamTpImps = false; CheatToggles.spamTpAll = false;
            CheatToggles.teleportAllToVent = false; CheatToggles.teleportPlayerToVent = false;
            CheatToggles.doorHallucination = false; CheatToggles.doorHallucinationAll = false;
            KarakCheats.StopShipAnimCheats();
        }

        if (!Utils.isHost && !Utils.isFreePlay)
        {
            CheatToggles.killAll = false; CheatToggles.telekillPlayer = false; CheatToggles.killAllCrew = false;
            CheatToggles.killAllImps = false; CheatToggles.killPlayer = false; CheatToggles.ejectPlayer = false;
            CheatToggles.noKillCd = false; CheatToggles.killAnyone = false; CheatToggles.killVanished = false;
            CheatToggles.forceStartGame = false; CheatToggles.skipMeeting = false; CheatToggles.voteImmune = false;
            CheatToggles.noGameEnd = false; CheatToggles.showProtectMenu = false; CheatToggles.showRolesMenu = false;
            CheatToggles.noOptionsLimits = false;
            CheatToggles.destroyLobby = false; CheatToggles.recreateLobby = false; CheatToggles.nukeLobby = false;
            CheatToggles.forceImpostorAll = false; CheatToggles.sameColorEveryone = false;
            CheatToggles.cycleLobbyColor = false; CheatToggles.lobbyDiscoMode = false;
            CheatToggles.applyZeroCd = false; CheatToggles.endGameNow = false;
            CheatToggles.kickPlayer = false; CheatToggles.banPlayer = false;
            CheatToggles.increaseImpostors = false; CheatToggles.decreaseImpostors = false;
            CheatToggles.increasePlayerSpeed = false; CheatToggles.decreasePlayerSpeed = false;
            CheatToggles.ssAllIntoSelf = false; CheatToggles.spamShapeshiftAll = false; CheatToggles.unshiftAll = false;
            CheatToggles.speedHack = false;
            CheatToggles.disableVents = false; CheatToggles.blockSabotages = false; CheatToggles.disableCameras = false;
            CheatToggles.unlimitedMeetings = false; CheatToggles.disableMeetings = false;
        }

        if (!Utils.isMeeting) { CheatToggles.skipMeeting = false; CheatToggles.ejectPlayer = false; }
    }

    public void OnGUI()
    {
        if (!isGUIActive || KarakMenu.isPanicked)
        {
            IsMouseOverMenu = false;
            KarakUI.CloseOpenCombo();
            return;
        }

        InitStyles();
        UIHelpers.ApplyAccent();

        GUI.color = Color.white;
        GUI.backgroundColor = Color.white;
        GUI.contentColor = Color.white;

        // RGB mode — rainbow glow outside the window frame
        if (KarakTheme.RgbMode)
        {
            KarakUI.DrawRgbGlow(_windowRect);
        }

        _windowRect = GUI.Window((int)WindowId.MenuUI, _windowRect, (GUI.WindowFunction)WindowFunction, "", GUIStylePreset.Window);

        // Input.mousePosition is bottom-left-origin; GUI Rects (incl. _windowRect)
        // are top-left-origin, so flip Y before testing containment.
        IsMouseOverMenu = _windowRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
    }

    private void SelectCategory(int i)
    {
        if (i == _selectedCategory) return;
        _selectedCategory = i;
        _lastCategoryChange = Time.unscaledTime;
        _contentScroll = Vector2.zero;
        KarakUI.CloseOpenCombo();
    }

    private void SelectSubtab(Category cat, int i)
    {
        if (i == cat.Selected) return;
        cat.Selected = i;
        _lastSubtabChange = Time.unscaledTime;
        _contentScroll = Vector2.zero;
        KarakUI.CloseOpenCombo();
    }

    // A tab that throws (e.g. an IL2CPP interop call that isn't available in
    // this build) must never escape WindowFunction: the code after it — most
    // importantly GUI.DragWindow — still has to run, otherwise the menu stops
    // responding to being dragged. Logged once per tab so the console doesn't
    // fill up at 60fps.
    private static readonly System.Collections.Generic.HashSet<string> _loggedTabErrors = new();

    private void DrawActiveTab()
    {
        var cat = _categories[_selectedCategory];
        string key = cat.Tabs[cat.Selected].GetType().FullName;
        try
        {
            cat.Tabs[cat.Selected].Draw();
        }
        catch (System.Exception ex)
        {
            if (_loggedTabErrors.Add(key))
                KarakMenu.Log.LogWarning($"Tab '{key}' threw: {ex}");
        }
    }

    public void WindowFunction(int windowID)
    {
        float sidebarTarget = _sidebarExpanded ? SidebarWidthExpanded : SidebarWidthCollapsed;
        float sidebar = KarakUI.Anim("menu-sidebar-w", sidebarTarget, 12f);

        // Sidebar
        GUI.Box(new Rect(0, 0, sidebar, windowHeight), GUIContent.none, KarakUI.SidebarBg);
        DrawSidebar(sidebar);

        // Content area
        var content = new Rect(sidebar + 12f, 12f, windowWidth - sidebar - 24f, windowHeight - 24f);
        // Reserve room on the right for the vertical scrollbar so 2-column
        // tab layouts don't spill past its inner edge
        ContentWidth = content.width - 16f;
        GUILayout.BeginArea(content);
        {
            var cat = _categories[_selectedCategory];

            // Big title (tinted with the primary accent when RGB mode is on)
            GUILayout.Space(4f);
            var titleStyle = KarakTheme.RgbMode
                ? new GUIStyle(GUIStylePreset.TabTitle) { normal = { textColor = KarakTheme.Accent } }
                : GUIStylePreset.TabTitle;
            GUILayout.Label(cat.Tabs[cat.Selected].name, titleStyle, GUILayout.Height(30f));
            GUILayout.Space(6f);

            // Subtab bar (only when the category has more than one tab)
            if (cat.Tabs.Count > 1)
            {
                var labels = new string[cat.Tabs.Count];
                for (int i = 0; i < cat.Tabs.Count; i++) labels[i] = cat.Tabs[i].name;
                int newSel = KarakUI.SubtabBar($"cat-{_selectedCategory}", labels, cat.Selected);
                if (newSel != cat.Selected) SelectSubtab(cat, newSel);
                GUILayout.Space(10f);
            }
            else
            {
                GUILayout.Space(4f);
            }

            // Scrollable tab body with a subtle fade-in on tab switch
            float sinceChange = Time.unscaledTime - Mathf.Max(_lastCategoryChange, _lastSubtabChange);
            float fade = Mathf.Clamp01(sinceChange * 4f);
            var saved = GUI.color;
            GUI.color = new Color(saved.r, saved.g, saved.b, saved.a * fade);

            // Vertical-only scroll — pass GUIStyle.none for the horizontal
            // scrollbar so oversized rows never spawn a horizontal track
            _contentScroll = GUILayout.BeginScrollView(_contentScroll, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
            GUILayout.BeginVertical();
            DrawActiveTab();
            GUILayout.Space(4f);

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUI.color = saved;
        }
        GUILayout.EndArea();

        // Collapse button — sits on the sidebar edge
        var btnSize = 20f;
        var btnRect = new Rect(sidebar - btnSize / 2f, 44f, btnSize, btnSize);
        GUI.Box(btnRect, _sidebarExpanded ? "<" : ">", KarakUI.CollapseButton);
        var evt = Event.current;
        if (evt.type == EventType.MouseDown && evt.button == 0 && btnRect.Contains(evt.mousePosition))
        {
            _sidebarExpanded = !_sidebarExpanded;
            evt.Use();
        }

        // Drag from the full top bar (sidebar included) so the menu can be moved by its title area
        GUI.DragWindow(new Rect(0, 0, windowWidth, 46f));
    }

    private void DrawSidebar(float width)
    {
        const float rowH = 34f;
        const float startY = 60f;

        // KARAKMENU brand — top-left corner of the sidebar. Collapses to
        // "KM" when the sidebar is narrow so the label doesn't get clipped.
        bool compact = width < SidebarWidthExpanded - 20f;
        var brandRect = compact ? new Rect(0f, 14f, width, 22f) : new Rect(16f, 14f, width - 20f, 22f);
        KarakUI.DrawBrand(brandRect, compact);

        // Selected-tab background pill — position animated between rows
        float targetY = startY + _selectedCategory * (rowH + 4f);
        float pillY = KarakUI.Anim("menu-tab-pill-y", targetY, 12f);
        GUI.Box(new Rect(8f, pillY, width - 16f, rowH), GUIContent.none, KarakUI.TabRowActiveBg);

        // Accent stripe on the selected tab (tertiary accent under RGB mode
        // gives the sidebar a different hue than the title/underline)
        var stripeColor = KarakTheme.RgbMode ? KarakTheme.TertiaryAccent : KarakTheme.Accent;
        KarakUI.DrawAccentStripe(new Rect(4f, pillY + 6f, 3f, rowH - 12f), stripeColor);

        GUILayout.BeginArea(new Rect(0, startY, width, windowHeight - startY));
        {
            for (int i = 0; i < _categories.Count; i++)
            {
                bool selected = i == _selectedCategory;
                var cat = _categories[i];
                // Selected tab uses accent color in RGB mode so it visibly cycles
                var iconColor = selected
                    ? (KarakTheme.RgbMode ? KarakTheme.Accent : KarakTheme.TextActive)
                    : KarakTheme.TextDisabled;
                var textColor = selected
                    ? (KarakTheme.RgbMode ? KarakTheme.Accent : KarakTheme.TextActive)
                    : KarakTheme.TextDisabled;

                GUILayout.BeginHorizontal(GUILayout.Height(rowH));

                GUILayout.Space(20f);
                GUILayout.Box(KarakTheme.Icon(cat.Icon, iconColor, 18),
                    KarakUI.PlainIcon, GUILayout.Width(18f), GUILayout.Height(18f));

                if (_sidebarExpanded)
                {
                    GUILayout.Space(12f);
                    var labelStyle = new GUIStyle(GUIStylePreset.Label)
                    {
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleLeft
                    };
                    labelStyle.normal.textColor = textColor;
                    labelStyle.hover.textColor = textColor;
                    if (GUILayout.Button(cat.Name, labelStyle, GUILayout.ExpandWidth(true), GUILayout.Height(rowH)))
                    {
                        SelectCategory(i);
                    }
                }
                else
                {
                    if (GUILayout.Button("", KarakUI.InvisibleButton, GUILayout.ExpandWidth(true), GUILayout.Height(rowH)))
                    {
                        SelectCategory(i);
                    }
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(4f);
            }
        }
        GUILayout.EndArea();
    }
}

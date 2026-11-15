using UnityEngine;

namespace KarakMenu;

public class DoorsUI : MonoBehaviour
{
    public static int windowHeight = 340;
    public static int windowWidth = 540;
    private Rect _windowRect;

    private void Start()
    {
        _windowRect = new(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
    }

    private void OnGUI()
    {
        if (!CheatToggles.showDoorsMenu || !(MenuUI.isGUIActive || KarakMenu.menuKeepSubwindowsOpen.Value) || KarakMenu.isPanicked) return;

        GUIStylePreset.EnsureSkin();
        UIHelpers.ApplyAccent();

        GUI.color = Color.white; GUI.backgroundColor = Color.white; GUI.contentColor = Color.white;

        if (KarakTheme.RgbMode) KarakUI.DrawRgbGlow(_windowRect);

        _windowRect = GUI.Window((int)WindowId.DoorsUI, _windowRect, (GUI.WindowFunction)DoorsWindow, "", KarakUI.SubwindowStyle);
    }

    private void DoorsWindow(int windowID)
    {
        GUI.Label(new Rect(16f, 12f, windowWidth - 32f, 22f), "Doors", KarakUI.WindowTitle);

        if (!Utils.isShip)
        {
            GUI.DragWindow();
            return;
        }

        var map = (MapNames)Utils.GetCurrentMapID();
        if (map is MapNames.MiraHQ)
        {
            GUI.DragWindow();
            return;
        }

        GUILayout.BeginVertical();

        foreach (var doorRoom in DoorsHandler.GetRoomsWithDoors())
        {
            GUILayout.BeginHorizontal(GUILayout.Height(24f));

            GUILayout.Label($"<color=#{ColorUtility.ToHtmlStringRGB(KarakTheme.Accent)}>{doorRoom}</color>",
                GUIStylePreset.Label, GUILayout.Width(120f));

            GUILayout.Label($"{DoorsHandler.GetStatusOfDoorsInRoom(doorRoom, true)}",
                GUIStylePreset.Label, GUILayout.ExpandWidth(true));

            if (KarakUI.Button("Close", GUILayout.Width(60f), GUILayout.Height(20f)))
            {
                DoorsHandler.CloseDoorsInRoom(doorRoom);
            }

            if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
            {
                GUILayout.Space(4f);
                if (KarakUI.Button("Open", GUILayout.Width(60f), GUILayout.Height(20f)))
                {
                    DoorsHandler.OpenDoorsInRoom(doorRoom);
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.Box(GUIContent.none, KarakUI.RowSeparator, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
        }

        GUILayout.FlexibleSpace();

        // Global spam controls — the only ones that really need to persist
        GUILayout.BeginHorizontal();

        if (KarakUI.Button("Close All", GUILayout.Width(80f), GUILayout.Height(22f)))
        {
            CheatToggles.closeAllDoors = true;
        }

        if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
        {
            GUILayout.Space(4f);
            if (KarakUI.Button("Open All", GUILayout.Width(80f), GUILayout.Height(22f)))
            {
                CheatToggles.openAllDoors = true;
            }
        }

        GUILayout.FlexibleSpace();

        if (Utils.isHost)
        {
            CheatToggles.spamCloseAllDoors = KarakUI.Toggle(CheatToggles.spamCloseAllDoors, "Spam Close All");
            if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
            {
                CheatToggles.spamOpenAllDoors = KarakUI.Toggle(CheatToggles.spamOpenAllDoors, "Spam Open All");
            }
        }
        else
        {
            CheatToggles.spamCloseAllDoors = CheatToggles.spamOpenAllDoors = false;
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUI.DragWindow();
    }
}

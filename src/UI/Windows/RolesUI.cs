using System.Collections.Generic;
using AmongUs.GameOptions;
using UnityEngine;

namespace KarakMenu;

public class RolesUI : MonoBehaviour
{
    public static int windowHeight = 440;
    public static int windowWidth = 560;
    private Rect _windowRect;

    private Vector2 _scrollPosition = Vector2.zero;

    // Roles to assign per player (PlayerId -> Role). Applied by IntroCutscene_CoBegin on game start
    public static readonly Dictionary<byte, RoleTypes> AssignedRoles = new();

    private static readonly RoleTypes[] RoleCycle =
    {
        RoleTypes.Crewmate,
        RoleTypes.Impostor,
        RoleTypes.Shapeshifter,
        RoleTypes.Engineer,
        RoleTypes.Scientist,
        RoleTypes.Tracker,
        RoleTypes.Detective,
        RoleTypes.Noisemaker,
        RoleTypes.Phantom,
        RoleTypes.GuardianAngel
    };

    private void Start()
    {
        // Instantiate 2D area of RolesUI
        _windowRect = new(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
    }

    private void OnGUI()
    {
        if (!CheatToggles.showRolesMenu || !(MenuUI.isGUIActive || KarakMenu.menuKeepSubwindowsOpen.Value) || KarakMenu.isPanicked) return;

        GUIStylePreset.EnsureSkin();
        UIHelpers.ApplyAccent();

        GUI.color = Color.white; GUI.backgroundColor = Color.white; GUI.contentColor = Color.white;

        if (KarakTheme.RgbMode) KarakUI.DrawRgbGlow(_windowRect);

        _windowRect = GUI.Window((int)WindowId.RolesUI, _windowRect, (GUI.WindowFunction)RolesWindow, "", KarakUI.SubwindowStyle);
    }

    private void RolesWindow(int windowID)
    {
        GUI.Label(new Rect(16f, 12f, windowWidth - 32f, 22f), "Assign Roles", KarakUI.WindowTitle);

        GUILayout.BeginVertical();

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || string.IsNullOrEmpty(player.Data.PlayerName)) continue;

            GUILayout.BeginHorizontal(GUIStylePreset.SectionBox);

            // Player name
            GUILayout.Label($"<color=#{ColorUtility.ToHtmlStringRGB(player.Data.Color)}>{player.Data.PlayerName}</color>", GUIStylePreset.Label, GUILayout.Width(170f));

            // Currently assigned role (or current in-game role if none assigned)
            var assigned = AssignedRoles.TryGetValue(player.PlayerId, out var role);
            var roleName = assigned ? role.ToString() : player.Data.Role.Role.ToString();
            GUILayout.Label(roleName, GUIStylePreset.TabSubtitle, GUILayout.Width(150f));

            GUILayout.FlexibleSpace();

            // Cycle to the previous role
            if (GUILayout.Button("◀", GUIStylePreset.NormalButton, GUILayout.Width(36f)))
            {
                int idx = System.Array.IndexOf(RoleCycle, AssignedRoles.TryGetValue(player.PlayerId, out var current) ? current : player.Data.Role.Role);
                int next = (idx - 1 + RoleCycle.Length) % RoleCycle.Length;
                AssignedRoles[player.PlayerId] = RoleCycle[next];
            }

            // Cycle to the next role
            if (GUILayout.Button("▶", GUIStylePreset.NormalButton, GUILayout.Width(36f)))
            {
                int idx = System.Array.IndexOf(RoleCycle, AssignedRoles.TryGetValue(player.PlayerId, out var current) ? current : player.Data.Role.Role);
                int next = (idx + 1) % RoleCycle.Length;
                AssignedRoles[player.PlayerId] = RoleCycle[next];
            }

            // Clear the assigned role for this player
            if (GUILayout.Button("Reset", GUIStylePreset.NormalButton, GUILayout.Width(64f)))
            {
                AssignedRoles.Remove(player.PlayerId);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        GUILayout.Space(8);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear All", GUIStylePreset.NormalButton))
        {
            AssignedRoles.Clear();
        }

        GUILayout.FlexibleSpace();

        GUILayout.Label("Roles will be assigned on the next game start", GUIStylePreset.Label);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUI.DragWindow();
    }
}

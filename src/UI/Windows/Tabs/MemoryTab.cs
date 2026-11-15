using System;
using UnityEngine;

namespace KarakMenu;

// Memory tab: shows the AmongUsMemory offsets and live memory pointers of the game objects
public class MemoryTab : ITab
{
    public string name => "Memory";

    private Vector2 _scrollPosition;

    public void Draw()
    {
        GUILayout.Label("Offsets", GUIStylePreset.TabSubtitle);
        GUILayout.Space(4);

        DrawOffsetRow("PlayerControl", MemoryOffsets.PlayerControlPointer);
        DrawOffsetRow("ShipStatus", MemoryOffsets.ShipStatusPointer);
        DrawOffsetRow("AmongUsClient", MemoryOffsets.AmongUsClientPointer);
        DrawOffsetRow("get_Data RVA", MemoryOffsets.PlayerControlGetDataRva);

        GUILayout.Space(16);

        GUILayout.Label("Live Pointers", GUIStylePreset.TabSubtitle);
        GUILayout.Space(4);

        DrawPointerRow("AmongUsClient", AmongUsClient.Instance?.Pointer);
        DrawPointerRow("ShipStatus", ShipStatus.Instance?.Pointer);
        DrawPointerRow("LocalPlayer", PlayerControl.LocalPlayer?.Pointer);
        DrawPointerRow("GameData", GameData.Instance?.Pointer);

        GUILayout.Space(16);

        GUILayout.Label("Players", GUIStylePreset.TabSubtitle);
        GUILayout.Space(4);

        if (!Utils.isPlayer)
        {
            GUILayout.Label("No local player available in this scene");
            return;
        }

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(230));
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null) continue;

            GUILayout.BeginHorizontal();
            var isImpostor = player.Data.Role != null && player.Data.Role.IsImpostor;
            var nameColor = isImpostor ? "#FF6B6B" : "#FFFFFF";
            GUILayout.Label($"<color={nameColor}>{player.Data.PlayerName}</color>", GUILayout.Width(200));
            GUILayout.Label(FormatPointer(player.Pointer), GUILayout.Width(110));
            GUILayout.Label(FormatPointer(player.Data.Pointer));
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
    }

    private void DrawOffsetRow(string label, string offset)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(150));
        GUILayout.Label($"<color=#{ColorUtility.ToHtmlStringRGB(KarakTheme.Accent)}>{offset}</color>");

        if (GUILayout.Button("Copy", GUIStylePreset.NormalButton, GUILayout.Width(70)))
        {
            GUIUtility.systemCopyBuffer = offset;
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(2);
    }

    private void DrawPointerRow(string label, IntPtr? pointer)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(150));
        GUILayout.Label(FormatPointer(pointer));
        GUILayout.EndHorizontal();
        GUILayout.Space(2);
    }

    private static string FormatPointer(IntPtr? pointer)
    {
        return pointer == null || pointer.Value == IntPtr.Zero ? "N/A" : "0x" + pointer.Value.ToString("X");
    }
}

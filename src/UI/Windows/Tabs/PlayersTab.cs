using System.Linq;
using UnityEngine;
using AmongUs.GameOptions;

namespace KarakMenu;

// Two-panel Players tab: left = scrollable player list, right = features for the selected player.
// Role name + role color shown next to each player name (in-game only — not in lobby).
// Frame SS / Disco / Change Color are host-only. Jail and Kick work without host (best-effort).
public class PlayersTab : ITab
{
    public string name => "Players";

    private static byte _selectedPlayerId = byte.MaxValue;
    private Vector2 _listScroll;

    private const float ListW = 165f;
    private const float ListBtnH = 26f;

    public void Draw()
    {
        float totalW = MenuUI.ContentWidth;
        float detailW = totalW - ListW - 8f;

        GUILayout.BeginHorizontal();

        // ── Left panel: player list ──────────────────────────────────────
        GUILayout.BeginVertical(GUILayout.Width(ListW));
        DrawPlayerList();
        GUILayout.EndVertical();

        GUILayout.Space(8f);

        // ── Right panel: selected player features ────────────────────────
        GUILayout.BeginVertical(GUILayout.Width(detailW));
        if (_selectedPlayerId == byte.MaxValue)
        {
            KarakUI.BeginSection("Actions");
            GUILayout.Label("Select a player on the left to see available actions.", GUIStylePreset.Label);
            KarakUI.EndSection();
        }
        else
        {
            var selected = FindPlayer(_selectedPlayerId);
            if (selected == null)
            {
                _selectedPlayerId = byte.MaxValue;
                KarakUI.BeginSection("Actions");
                GUILayout.Label("Player disconnected.", GUIStylePreset.Label);
                KarakUI.EndSection();
            }
            else
            {
                DrawPlayerDetails(selected, detailW);
            }
        }

        // Mass actions below the detail panel
        GUILayout.Space(8f);
        DrawMassActions(detailW);

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawPlayerList()
    {
        KarakUI.BeginSection("Players");
        bool scrollViewOpen = false;
        try
        {
            _listScroll = GUILayout.BeginScrollView(_listScroll, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
            scrollViewOpen = true;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null) continue;
                var data = player.Data;
                if (data == null || data.Disconnected) continue;

                bool isSelected = player.PlayerId == _selectedPlayerId;
                bool isSelf = player.AmOwner;

                string nameHex = ColorUtility.ToHtmlStringRGB(data.Color);
                string label = $"<color=#{nameHex}>{data.PlayerName}</color>";

                // Role label — only meaningful in-game when roles are assigned
                if (Utils.isShip && data.Role != null)
                {
                    string roleName = GetShortRoleName(data.RoleType);
                    string roleHex = ColorUtility.ToHtmlStringRGB(GetRoleColor(data.RoleType));
                    label += $" <color=#{roleHex}>({roleName})</color>";
                }

                if (isSelf) label += " <color=#888888>(You)</color>";

                // Highlight the selected row with a box around it
                var rowStyle = isSelected ? KarakUI.SelectedRowStyle : KarakUI.NormalRowStyle;
                if (GUILayout.Button(label, rowStyle, GUILayout.Width(ListW - 8f), GUILayout.Height(ListBtnH)))
                {
                    _selectedPlayerId = player.PlayerId;
                }
                GUILayout.Space(2f);
            }
        }
        finally
        {
            if (scrollViewOpen) GUILayout.EndScrollView();
            KarakUI.EndSection();
        }
    }

    private void DrawPlayerDetails(PlayerControl player, float width)
    {
        bool isSelf = player.AmOwner;
        bool isHostPlayer = AmongUsClient.Instance != null
            && AmongUsClient.Instance.GetClientIdFromCharacter(player) == AmongUsClient.Instance.HostId;
        bool isHost = Utils.isHost;

        string nameHex = ColorUtility.ToHtmlStringRGB(player.Data.Color);
        string header = $"<color=#{nameHex}>{player.Data.PlayerName}</color>";
        if (Utils.isShip && player.Data.Role != null)
        {
            string roleHex = ColorUtility.ToHtmlStringRGB(GetRoleColor(player.Data.RoleType));
            header += $" <color=#{roleHex}>({GetShortRoleName(player.Data.RoleType)})</color>";
        }

        KarakUI.BeginSection(header);

        if (isSelf)
        {
            // Self — only speed control applies
            CheatToggles.customSpeed = KarakUI.Toggle(CheatToggles.customSpeed, "Custom Speed", "players-customspeed-self");
            if (CheatToggles.customSpeed)
            {
                bool isGhost = player.Data != null && player.Data.IsDead;
                CheatToggles.customSpeedValue = KarakUI.LabeledSlider(isGhost ? "Ghost Speed" : "Speed",
                    CheatToggles.customSpeedValue, 0f, 20f, "0.00");
            }
        }
        else
        {
            // ── Row 1: Shapeshift + Disco ────────────────────────────────
            GUILayout.BeginHorizontal();
            if (isHost)
            {
                if (KarakUI.Button("Frame SS", GUILayout.Width(80f), GUILayout.Height(22f)))
                    KarakPlayersCheats.FrameShapeshift(player);
            }

            bool discoOn = KarakPlayersCheats.DiscoTargets.Contains(player.PlayerId);
            bool newDiscoOn = KarakUI.Toggle(discoOn, "Disco", $"players-disco-{player.PlayerId}");
            if (newDiscoOn != discoOn)
            {
                if (newDiscoOn) KarakPlayersCheats.DiscoTargets.Add(player.PlayerId);
                else KarakPlayersCheats.DiscoTargets.Remove(player.PlayerId);
            }

            bool jailOn = KarakPlayersCheats.JailTargets.Contains(player.PlayerId);
            bool newJailOn = KarakUI.Toggle(jailOn, "Jail", $"players-jail-{player.PlayerId}");
            if (newJailOn != jailOn)
            {
                if (newJailOn) KarakPlayersCheats.JailTargets.Add(player.PlayerId);
                else KarakPlayersCheats.JailTargets.Remove(player.PlayerId);
            }
            GUILayout.EndHorizontal();

            // Disco speed slider (shown when Disco is active for this player)
            if (discoOn || newDiscoOn)
            {
                CheatToggles.playerDiscoSpeed = KarakUI.LabeledSlider("Disco Speed",
                    CheatToggles.playerDiscoSpeed, 0.10f, 2f, "0.00");
            }

            // ── Row 2: Kick + Vote Kick ──────────────────────────────────
            GUILayout.BeginHorizontal();
            if (!isHostPlayer)
            {
                if (KarakUI.Button("Kick", GUILayout.Width(80f), GUILayout.Height(22f)))
                {
                    string status = KarakPlayersCheats.TryKick(player);
                    HudManager.Instance?.Notifier?.AddDisconnectMessage(status);
                }
                if (KarakUI.Button("Vote Kick", GUILayout.Width(80f), GUILayout.Height(22f)))
                {
                    string status = KarakPlayersCheats.TryVoteKick(player);
                    HudManager.Instance?.Notifier?.AddDisconnectMessage(status);
                }
            }
            else
            {
                GUILayout.Label("<color=#888888>Can't kick host</color>", GUIStylePreset.Label);
            }
            GUILayout.EndHorizontal();

            // ── Row 3: Whisper + Change Color ────────────────────────────
            GUILayout.BeginHorizontal();
            bool isWhisperTarget = KarakWhisper.Target == player;
            if (KarakUI.Button(isWhisperTarget ? "Stop Whisper" : "Whisper",
                GUILayout.Width(100f), GUILayout.Height(22f)))
                KarakWhisper.Target = isWhisperTarget ? null : player;

            if (isHost)
            {
                if (KarakUI.Button("Change Color", GUILayout.Width(100f), GUILayout.Height(22f)))
                    KarakPlayersCheats.CyclePlayerColor(player);
            }
            GUILayout.EndHorizontal();
        }

        KarakUI.EndSection();
    }

    private void DrawMassActions(float width)
    {
        KarakUI.BeginSection("Mass Actions");
        GUILayout.Label("These apply to ALL players and require host.", GUIStylePreset.Label);
        GUILayout.Space(4f);
        if (KarakUI.Action("SS All Into Self", "Do it")) CheatToggles.ssAllIntoSelf = true;
        if (KarakUI.Action("Unshift All", "Do it")) CheatToggles.unshiftAll = true;
        CheatToggles.spamShapeshiftAll = KarakUI.Toggle(CheatToggles.spamShapeshiftAll, "Spam SS All Into Random");
        if (CheatToggles.spamShapeshiftAll)
        {
            GUILayout.Label("<color=#ff5555>Spam Shapeshift is a strong anticheat trigger — use at your own risk.</color>", GUIStylePreset.Label);
        }
        KarakUI.EndSection();
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static PlayerControl FindPlayer(byte playerId)
    {
        foreach (var p in PlayerControl.AllPlayerControls)
            if (p != null && p.Data != null && !p.Data.Disconnected && p.PlayerId == playerId) return p;
        return null;
    }

    private static string GetShortRoleName(RoleTypes rt) => rt switch
    {
        RoleTypes.Impostor      => "Imp",
        RoleTypes.Shapeshifter  => "SS",
        RoleTypes.Phantom       => "Phantom",
        RoleTypes.Viper         => "Viper",
        RoleTypes.Engineer      => "Eng",
        RoleTypes.Scientist     => "Sci",
        RoleTypes.Tracker       => "Tracker",
        RoleTypes.Noisemaker    => "NM",
        RoleTypes.Detective     => "Det",
        RoleTypes.GuardianAngel => "GA",
        RoleTypes.Crewmate      => "Crew",
        RoleTypes.ImpostorGhost => "ImpGhost",
        RoleTypes.CrewmateGhost => "Ghost",
        _                       => rt.ToString()
    };

    private static Color GetRoleColor(RoleTypes rt) => rt switch
    {
        RoleTypes.Impostor      or RoleTypes.ImpostorGhost => new Color(0.73f, 0.13f, 0.10f),
        RoleTypes.Shapeshifter  => new Color(0.55f, 0.08f, 0.08f),
        RoleTypes.Phantom       => new Color(0.45f, 0.00f, 0.55f),
        RoleTypes.Viper         => new Color(0.35f, 0.73f, 0.00f),
        RoleTypes.Engineer      => new Color(0.73f, 0.67f, 0.16f),
        RoleTypes.Scientist     => new Color(0.22f, 0.73f, 0.70f),
        RoleTypes.Tracker       => new Color(0.30f, 0.73f, 0.30f),
        RoleTypes.Noisemaker    => new Color(0.73f, 0.46f, 0.15f),
        RoleTypes.Detective     => new Color(0.23f, 0.32f, 0.73f),
        RoleTypes.GuardianAngel => new Color(0.73f, 0.73f, 0.73f),
        _                       => Color.white
    };
}

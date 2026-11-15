using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;
using KarakMenu.AC;

namespace KarakMenu;

// KarakMenu anticheat.
//
// The RPC/game-data validation core is a check registry: one RpcCheck
// instance per RPC, each with its own Enabled flag, plus GameDataCheck
// instances for InnerNet game-data messages. These replace the older inline
// rules wherever the two covered the same RPC.
//
// Older KarakMenu features the registry doesn't cover are kept and still run:
// ban player/name/word lists, friend-code validation, configurable level
// thresholds, max-level auto-kick, and the RPC rate limiter.
public static class Anticheat
{
    // Notify = report only. ErrorKick fakes a client timeout so the player is
    // removed and everyone else sees "left due to an error" (KarakMenu's trick).
    public enum ResponseMode { Notify, Kick, ErrorKick, Ban }

    // -----------------------------------------------------------------------
    // KarakMenu-owned settings (kept from before)
    // -----------------------------------------------------------------------
    public static bool IsEnabled = true;
    public static float KickCooldownSeconds = 2f;
    public static ResponseMode DetectionResponseMode = ResponseMode.Notify;
    public static bool HideKickReason;

    public static bool DetectInvalidFriendCodes = true;
    public static bool UsePlayerBanList = true;
    public static bool UseNameBanList = true;
    public static bool UseWordBanList = true;
    public static bool BanWordsLobbyOnly = true;
    public static bool DetectPlayerLevels;
    public static int MaxLevelThreshold = 500;
    public static bool AutoKickLevels;
    public static int MinLevelThreshold = 20;
    public static bool DetectInvalidRpcs = true;
    public static bool LimitRpcRate = true;
    public static int PacketRateLimit = 50;

    // -----------------------------------------------------------------------
    // KarakMenu-owned settings (added)
    // -----------------------------------------------------------------------
    public static bool CheckSpoofedPlatforms = true;
    public static bool SendNotifications = true;
    public static bool DiscardInvalidRpc = true;
    public static bool EnableGameDataChecks = true;

    public static readonly Dictionary<RpcCalls, RpcCheck> RpcHandlers = new Dictionary<RpcCalls, RpcCheck>
    {
        // Sorted by RPC id
        { RpcCalls.PlayAnimation, new AC.PlayAnimation() },
        { RpcCalls.CompleteTask, new AC.CompleteTask() },
        { RpcCalls.Exiled, new AC.Exiled() },
        { RpcCalls.CheckName, new AC.CheckName() },
        { RpcCalls.SetName, new AC.SetName() },
        { RpcCalls.SetColor, new AC.SetColor() },
        { RpcCalls.ReportDeadBody, new AC.ReportDeadBody() },
        { RpcCalls.SetScanner, new AC.SetScanner() },
        { RpcCalls.SetStartCounter, new AC.SetStartCounter() },
        { RpcCalls.EnterVent, new AC.EnterVent() },
        { RpcCalls.ExitVent, new AC.ExitVent() },
        { RpcCalls.SnapTo, new AC.SnapTo() },
        { RpcCalls.AddVote, new AC.AddVote() },
        { RpcCalls.CloseDoorsOfType, new AC.CloseDoorsOfType() },
        { RpcCalls.ClimbLadder, new AC.ClimbLadder() },
        { RpcCalls.UsePlatform, new AC.UsePlatform() },
        { RpcCalls.UpdateSystem, new AC.UpdateSystem() },
        { RpcCalls.SetLevel, new AC.SetLevel() }
    };

    public static readonly Dictionary<GameDataTypes, GameDataCheck> GameDataHandlers = new Dictionary<GameDataTypes, GameDataCheck>
    {
        { GameDataTypes.SceneChangeFlag, new SceneChange() },
        { GameDataTypes.ReadyFlag, new ClientReady() }
    };

    public static bool HasActiveGameDataChecks
    {
        get
        {
            foreach (var check in GameDataHandlers.Values) if (check.Enabled) return true;
            return false;
        }
    }

    // -----------------------------------------------------------------------
    // KarakMenu-owned state
    // -----------------------------------------------------------------------
    private const float FriendCodeSettleSeconds = 3f;
    private const float FriendCodeReportInterval = 10f;
    private const float FastTaskInterval = 1.25f;
    private static readonly Dictionary<int, float> LastAction = new();
    private static readonly Dictionary<int, int> RpcCounts = new();
    private static readonly Dictionary<int, TaskState> PlayerStates = new();

    private sealed class TaskState
    {
        public uint LastTaskId = uint.MaxValue;
        public float LastTaskTime = -999f;
        public string LastFriendCode = "";
        public float FriendCodeStableSince = -999f;
        public float LastInvalidFriendCodeReportTime = -999f;
        public bool HasSetLevel;
        public bool HasSetName;
        public int TimesAttemptedKilled;
    }

    private static float lastRateReset;
    private static float lastBanCheck;
    private static readonly string SaveDirectory = Path.Combine(BepInEx.Paths.ConfigPath, "KarakMenu");
    private static string PlayerList => Path.Combine(SaveDirectory, "BanPlayerList.txt");
    private static string NameList => Path.Combine(SaveDirectory, "BanNameList.txt");
    private static string WordList => Path.Combine(SaveDirectory, "BanWordList.txt");

    public static void Initialize() => EnsureLists();

    public static void ResetRoundState()
    {
        PlayerStates.Clear();
        RpcCounts.Clear();
        LastAction.Clear();
    }

    // -----------------------------------------------------------------------
    // KarakMenu-only periodic checks (ban lists, friend codes)
    // -----------------------------------------------------------------------
    public static void Update()
    {
        if (!IsEnabled || !Utils.isInGame || Time.time - lastBanCheck < 1f) return;
        lastBanCheck = Time.time;
        EnsureLists();

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (IsIgnoredPlayer(player) || player.Data == null) continue;
            var state = GetState(player);
            string friendCode = GetFriendCode(player.Data);
            TrackFriendCode(state, friendCode);

            bool settled = !string.IsNullOrWhiteSpace(friendCode) && Time.time - state.FriendCodeStableSince >= FriendCodeSettleSeconds;
            bool reportDue = Time.time - state.LastInvalidFriendCodeReportTime >= FriendCodeReportInterval;
            if (DetectInvalidFriendCodes && settled && reportDue && !IsValidFriendCode(friendCode))
            {
                state.LastInvalidFriendCodeReportTime = Time.time;
                Report(player, "Invalid friend code.");
            }
            if (UsePlayerBanList && MatchesPlayerList(player)) Report(player, "Player is on the ban player list.");
            else if (UseNameBanList && MatchesNameList(player.Data.PlayerName)) Report(player, "Player name is on the ban name list.");
        }
    }

    // -----------------------------------------------------------------------
    // RPC handling — KarakMenu's checks first, then the older extras
    // -----------------------------------------------------------------------
    public static bool HandleRpc(Type sourceNetObj, PlayerControl player, RpcCalls rpc, MessageReader reader)
    {
        if (!IsEnabled) return true;
        if (player != null && (IsIgnoredPlayer(player) || player.Data == null)) return true;

        // 1. Registry check for this call id
        if (RpcHandlers.TryGetValue(rpc, out RpcCheck rpcCheck) && rpcCheck.Enabled)
        {
            // An RPC that belongs to a different net object is an exploit attempt
            if (sourceNetObj != rpcCheck.GetExpectedNetObject())
            {
                return !DiscardInvalidRpc;
            }

            // Host-only RPCs must originate from us, the host
            if (player != null && AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost && rpcCheck.IsHostOnly())
            {
                Flag(player, $"{player.Data.PlayerName} sent the {rpc} RPC while non-host.");
                return !DiscardInvalidRpc;
            }

            int oldReadPosition = reader.Position;
            bool isValid;
            try { isValid = rpcCheck.Validate(player, reader); }
            catch { isValid = true; }
            finally { reader.Position = oldReadPosition; }

            if (!isValid && DiscardInvalidRpc) return false;
        }

        // 2. Supplementary rules (things the registry doesn't cover)
        if (!DetectInvalidRpcs || player == null) return true;
        return CheckSupplementary(player, (byte)rpc, reader);
    }

    private static bool CheckSupplementary(PlayerControl player, byte callId, MessageReader reader)
    {
        int initialPosition = reader.Position;
        try
        {
            if (!AllowRate(player)) return false;
            if (!IsKnownRpc(callId) && player.OwnerId != AmongUsClient.Instance.HostId)
                return Report(player, $"Unregistered RPC received: {callId}");

            if (player.OwnerId != AmongUsClient.Instance.HostId && CheckGameplayRpcRules(player, callId)) return false;
            if (callId == (byte)RpcCalls.SetLevel && !CheckLevel(player, reader)) return false;
            if (callId == (byte)RpcCalls.SetName && !CheckName(player, reader)) return false;
            if (callId == (byte)RpcCalls.CompleteTask && !CheckTask(player, reader)) return false;
            if (callId == (byte)RpcCalls.MurderPlayer && !CheckMurder(player, reader)) return false;
            if (callId == (byte)RpcCalls.SendChat && UseWordBanList && (!BanWordsLobbyOnly || Utils.isLobby) && reader.BytesRemaining > 0 && FileMatches(WordList, reader.ReadString()))
                return Report(player, "Chat message contains a banned word.");
            return true;
        }
        finally { reader.Position = initialPosition; }
    }

    // Rules the registry has no equivalent for. The vent-role and sabotage rules are
    // deliberately absent here — the EnterVent/ExitVent/UpdateSystem checks
    // replaced them.
    private static bool CheckGameplayRpcRules(PlayerControl player, byte callId)
    {
        if (callId is (byte)RpcCalls.SetTasks or (byte)RpcCalls.ExtendLobbyTimer or (byte)RpcCalls.CloseMeeting)
            return !Report(player, "Host-only RPC received from a client.");
        if (Utils.isInGame && callId is (byte)RpcCalls.SetColor or (byte)RpcCalls.SetHatStr or (byte)RpcCalls.SetSkinStr or (byte)RpcCalls.SetVisorStr or (byte)RpcCalls.SetPetStr or (byte)RpcCalls.SetNamePlateStr)
            return !Report(player, "Cosmetic RPC received during gameplay.");
        if (Utils.isLobby && callId is (byte)RpcCalls.StartMeeting or (byte)RpcCalls.ReportDeadBody or (byte)RpcCalls.SendChatNote or (byte)RpcCalls.CloseMeeting or (byte)RpcCalls.Exiled or (byte)RpcCalls.CastVote or (byte)RpcCalls.ClearVote or (byte)RpcCalls.SetRole or (byte)RpcCalls.CompleteTask or (byte)RpcCalls.MurderPlayer)
            return !Report(player, "Gameplay RPC received in the lobby.");
        return false;
    }

    // Level thresholds. The SetLevel check only rejects values
    // above 10k and any SetLevel after the game started.
    private static bool CheckLevel(PlayerControl player, MessageReader reader)
    {
        int level = reader.ReadPackedInt32() + 1;
        var state = GetState(player);
        if (state.HasSetLevel && !Utils.isLocalGame) return Report(player, "Player attempted to set their level more than once.");
        state.HasSetLevel = true;
        if (DetectPlayerLevels && level > MaxLevelThreshold) Report(player, $"Player level {level} exceeds the configured limit.", false);
        return !(AutoKickLevels && level <= MinLevelThreshold) || Report(player, $"Player level {level} is below the configured minimum.");
    }

    // The "changed name more than once" rule. The CheckName/SetName
    // checks cover length and invalid characters.
    private static bool CheckName(PlayerControl player, MessageReader reader)
    {
        if (Utils.isHost || reader.BytesRemaining <= 4) return true;
        var state = GetState(player);
        reader.ReadUInt32();
        reader.ReadString();
        if (state.HasSetName && !Utils.isLocalGame) return Report(player, "Player attempted to change their name more than once.");
        state.HasSetName = true;
        return true;
    }

    // Task-timing rules. The CompleteTask check covers the
    // impostor/unassigned-task cases.
    private static bool CheckTask(PlayerControl player, MessageReader reader)
    {
        uint taskId = reader.ReadPackedUInt32();
        var state = GetState(player);
        if (state.LastTaskId == taskId) return Report(player, $"Task ID {taskId} was completed more than once.");
        float elapsed = Time.time - state.LastTaskTime;
        if (elapsed < FastTaskInterval) return Report(player, $"Tasks were completed too quickly ({elapsed:0.##} seconds apart).");
        state.LastTaskTime = Time.time;
        state.LastTaskId = taskId;
        return true;
    }

    // The registry has no murder validation at all.
    private static bool CheckMurder(PlayerControl player, MessageReader reader)
    {
        PlayerControl target = reader.ReadNetObject<PlayerControl>();
        if (target == null || target.Data == null) return true;
        if (!RoleManager.IsImpostorRole(player.Data.RoleType) || player.Data.IsDead || RoleManager.IsImpostorRole(target.Data.RoleType))
            return Report(player, "Invalid murder attempt.");
        if (!target.Data.IsDead) return true;
        var state = GetState(player);
        state.TimesAttemptedKilled++;
        if (state.TimesAttemptedKilled >= 10) return Report(player, "Repeated murder attempts against a dead player.");
        return false;
    }

    // -----------------------------------------------------------------------
    // Game data handling (KarakMenu's GameDataCheck handlers)
    // -----------------------------------------------------------------------
    public static bool HandleGameData(GameDataTypes type, MessageReader reader)
    {
        if (!IsEnabled) return true;
        if (!GameDataHandlers.TryGetValue(type, out GameDataCheck gameDataCheck) || !gameDataCheck.Enabled) return true;

        int oldReadPosition = reader.Position;
        bool isValid;
        try { isValid = gameDataCheck.Validate(reader); }
        catch { isValid = true; }
        finally { reader.Position = oldReadPosition; }

        return isValid || !DiscardInvalidRpc;
    }

    // -----------------------------------------------------------------------
    // Flagging and punishment
    // -----------------------------------------------------------------------
    public static void Flag(PlayerControl player, string reason, bool shouldPunish = true)
    {
        // Never flag ourselves: without net object impersonation checks a
        // client can send an invalid RPC on our behalf.
        if (player == null || player == PlayerControl.LocalPlayer) return;

        if (SendNotifications) Notify(player, reason);

        if (shouldPunish && AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
            Punish(player);
    }

    public static void Flag(string reason)
    {
        if (SendNotifications) Notify(null, reason);
    }

    private static void Punish(PlayerControl player)
    {
        if (player == null || player.Data == null) return;
        if (Time.time - (LastAction.TryGetValue(player.OwnerId, out float time) ? time : -999f) < KickCooldownSeconds) return;
        LastAction[player.OwnerId] = Time.time;

        switch (DetectionResponseMode)
        {
            case ResponseMode.Notify:
                break;

            case ResponseMode.Kick:
            case ResponseMode.ErrorKick:
                KarakMenu.Log.LogMessage($"{player.Data.PlayerName} was kicked by KarakMenu Anticheat");

                // Vanilla blocks ErrorKick before the game has started, so fall
                // back to a real kick in the lobby.
                if (DetectionResponseMode == ResponseMode.Kick || AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
                {
                    AmongUsClient.Instance.KickPlayer(player.OwnerId, false);
                }
                else
                {
                    // Fake a client timeout: the player is dropped, and in-game
                    // everyone sees them leave "due to an error" instead of a kick.
                    AmongUsClient.Instance.SendLateRejection(player.OwnerId, DisconnectReasons.ClientTimeout);
                }
                break;

            case ResponseMode.Ban:
                KarakMenu.Log.LogMessage($"{player.Data.PlayerName} was banned by KarakMenu Anticheat");
                AmongUsClient.Instance.KickPlayer(player.OwnerId, true);
                break;
        }
    }

    // Notify + optionally punish. Kept for KarakMenu's own rules, which still
    // return a bool to decide whether to drop the RPC.
    private static bool Report(PlayerControl player, string reason, bool respond = true)
    {
        if (IsIgnoredPlayer(player)) return true;
        Notify(player, reason);
        if (respond) Respond(player, reason);
        return !Utils.isHost || DetectionResponseMode == ResponseMode.Notify || !respond;
    }

    private static void Respond(PlayerControl player, string reason)
    {
        if (!Utils.isHost || player == null || DetectionResponseMode == ResponseMode.Notify) return;
        Punish(player);
    }

    private static void Notify(PlayerControl player, string reason)
    {
        if (HideKickReason) reason = "Unauthorized action detected.";
        string who = player?.Data?.PlayerName;
        string prefix = string.IsNullOrEmpty(who) ? "" : $"{who}: ";
        HudManager.Instance?.Notifier?.AddDisconnectMessage($"<color=#4f92ff><b>[KarakMenu Anti-Cheat]</b></color> {prefix}{reason}");
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------
    private static TaskState GetState(PlayerControl player) { if (!PlayerStates.TryGetValue(player.OwnerId, out var state)) PlayerStates[player.OwnerId] = state = new TaskState(); return state; }
    private static void TrackFriendCode(TaskState state, string friendCode) { if (friendCode == state.LastFriendCode) return; state.LastFriendCode = friendCode; state.FriendCodeStableSince = Time.time; state.LastInvalidFriendCodeReportTime = -999f; }
    private static bool AllowRate(PlayerControl player) { if (!LimitRpcRate) return true; if (Time.time - lastRateReset >= 1f) { RpcCounts.Clear(); lastRateReset = Time.time; } RpcCounts.TryGetValue(player.OwnerId, out int count); RpcCounts[player.OwnerId] = ++count; return count <= Mathf.Max(1, PacketRateLimit) || Report(player, "RPC rate limit exceeded."); }
    private static readonly HashSet<byte> KnownRpcIds = BuildKnownRpcIds();
    private static HashSet<byte> BuildKnownRpcIds() { var ids = new HashSet<byte>(); foreach (RpcCalls rpc in Enum.GetValues(typeof(RpcCalls))) ids.Add((byte)rpc); return ids; }
    private static bool IsKnownRpc(byte callId) => KnownRpcIds.Contains(callId);
    private static bool IsValidFriendCode(string value) { if (string.IsNullOrWhiteSpace(value)) return false; int separator = value.LastIndexOf('#'); if (separator <= 0 || value.Length - separator - 1 != 4) return false; for (int i = separator + 1; i < value.Length; i++) if (!char.IsDigit(value[i])) return false; return true; }
    private static bool IsIgnoredPlayer(PlayerControl player) => player == null || player == PlayerControl.LocalPlayer || player.AmOwner || (AmongUsClient.Instance != null && player.OwnerId == AmongUsClient.Instance.ClientId);
    private static string GetFriendCode(NetworkedPlayerInfo data) { try { return data.FriendCode ?? ""; } catch { return ""; } }
    private static string GetPuid(NetworkedPlayerInfo data) { try { return data.Puid ?? ""; } catch { return ""; } }
    private static bool MatchesPlayerList(PlayerControl player) => FileMatches(PlayerList, GetFriendCode(player.Data)) || FileMatches(PlayerList, GetPuid(player.Data));
    private static bool MatchesNameList(string name) => FileMatches(NameList, name);
    private static bool FileMatches(string path, string value) { if (string.IsNullOrWhiteSpace(value) || !File.Exists(path)) return false; foreach (string raw in File.ReadAllLines(path)) { string pattern = raw.Trim(); if (pattern.Length == 0 || pattern.StartsWith("//") || pattern.StartsWith("#")) continue; if (pattern.StartsWith("**") && pattern.EndsWith("**") && value.Contains(pattern[2..^2], StringComparison.OrdinalIgnoreCase)) return true; if (pattern.StartsWith("**") && value.EndsWith(pattern[2..], StringComparison.OrdinalIgnoreCase)) return true; if (pattern.EndsWith("**") && value.StartsWith(pattern[..^2], StringComparison.OrdinalIgnoreCase)) return true; if (string.Equals(pattern, value, StringComparison.OrdinalIgnoreCase)) return true; } return false; }
    private static void EnsureLists() { if (!Directory.Exists(SaveDirectory)) Directory.CreateDirectory(SaveDirectory); foreach (string path in new[] { PlayerList, NameList, WordList }) if (!File.Exists(path)) File.WriteAllText(path, "# Enter one entry per line\n"); }

    // -----------------------------------------------------------------------
    // Harmony hooks (KarakMenu's net-object arguments)
    // -----------------------------------------------------------------------
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    private static class PlayerControlRpcPatch
    {
        private static bool Prefix(PlayerControl __instance, byte callId, MessageReader reader)
            => HandleRpc(typeof(PlayerControl), __instance, (RpcCalls)callId, reader);
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
    private static class PlayerPhysicsRpcPatch
    {
        private static bool Prefix(PlayerPhysics __instance, byte callId, MessageReader reader)
            => HandleRpc(typeof(PlayerPhysics), __instance.myPlayer, (RpcCalls)callId, reader);
    }

    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
    private static class NetworkTransformRpcPatch
    {
        private static bool Prefix(CustomNetworkTransform __instance, byte callId, MessageReader reader)
            => HandleRpc(typeof(CustomNetworkTransform), __instance.myPlayer, (RpcCalls)callId, reader);
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
    private static class ShipStatusRpcPatch
    {
        private static bool Prefix(byte callId, MessageReader reader)
            => HandleRpc(typeof(ShipStatus), null, (RpcCalls)callId, reader);
    }
}

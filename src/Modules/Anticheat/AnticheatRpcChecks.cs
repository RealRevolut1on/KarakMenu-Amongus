using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using System;
using System.Collections.Generic;

namespace KarakMenu.AC;

// ---------------------------------------------------------------------------
// Per-RPC validators for the check registry.
// These supersede KarakMenu's old inline CheckGameplayRpcRules / CheckTask /
// CheckLevel / CheckName / CheckSystemUpdate logic wherever the two overlap.
// ---------------------------------------------------------------------------

internal class PlayAnimation : RpcCheck
{
    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        TaskTypes animation = (TaskTypes)reader.ReadByte();

        if (LobbyBehaviour.Instance != null)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} inside the lobby.");
            return false;
        }

        if (RoleManager.IsImpostorRole(player.Data.RoleType))
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} when they are an imposter.");
            return false;
        }

        if (!GameManager.Instance.LogicOptions.GetVisualTasks())
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} when visual tasks are off.");
            return false;
        }

        return true;
    }

    public override RpcCalls GetId() => RpcCalls.PlayAnimation;
}

internal class CompleteTask : RpcCheck
{
    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        uint taskIndex = reader.ReadPackedUInt32();

        if (ShipStatus.Instance == null)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} tried completing task {taskIndex} when there was no valid instance of ShipStatus.");
            return false;
        }

        if (RoleManager.IsImpostorRole(player.Data.RoleType))
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} tried completing task {taskIndex} while being an imposter.");
            return false;
        }

        // Task IDs are zero-indexed
        if (player.Data.Tasks == null || taskIndex + 1 > player.Data.Tasks.Count)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} tried completing task {taskIndex} when they only have {player.Data.Tasks?.Count ?? 0} tasks.");
            return false;
        }

        return true;
    }

    public override RpcCalls GetId() => RpcCalls.CompleteTask;
}

internal class Exiled : RpcCheck
{
    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        // The Exiled RPC is unused and never sent in-game
        Anticheat.Flag(player, $"{player.Data.PlayerName} sent an invalid Exiled RPC.");
        return false;
    }

    public override RpcCalls GetId() => RpcCalls.Exiled;
}

internal class CheckName : RpcCheck
{
    public readonly int MAX_NAME_LENGTH = 10;

    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        string requestedName = reader.ReadString();

        if (requestedName.Length > MAX_NAME_LENGTH)
        {
            Anticheat.Flag(player, $"{requestedName} tried setting their name to something too long ({requestedName.Length}).");
            return false;
        }

        if (requestedName.Contains('<'))
        {
            Anticheat.Flag(player, $"{requestedName} requested a name with invalid characters.");
            return false;
        }

        return true;
    }

    public override RpcCalls GetId() => RpcCalls.CheckName;
}

internal class SetName : RpcCheck
{
    // 12 instead of 10 to account for the auto-appended numbers when several
    // players share a name.
    public readonly int MAX_NAME_LENGTH = 12;

    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        // On modded lobbies custom names are normal, so skip entirely.
        if (!AcUtil.IsAnticheatPresent()) return true;

        uint netId = reader.ReadUInt32();
        string requestedName = reader.ReadString();

        if (netId != GetExpectedNetId(player))
        {
            Anticheat.Flag(player, $"SetName RPC sent for {requestedName} includes an invalid net id, received {netId}, expected {GetExpectedNetId(player)}.");
            return false;
        }

        if (requestedName.Length > MAX_NAME_LENGTH)
        {
            Anticheat.Flag(player, $"{requestedName} tried setting their name to something too long ({requestedName.Length}).");
            return false;
        }

        if (requestedName.Contains('<'))
        {
            Anticheat.Flag(player, $"{requestedName} requested a name with invalid characters.");
            return false;
        }

        return true;
    }

    private uint GetExpectedNetId(PlayerControl player)
    {
        // In host authority the host sends the net id of the NetworkedPlayerInfo
        // net object; in server authority it's the PlayerControl net object.
        return AcUtil.IsAnticheatPresent() ? player.NetId : player.Data.NetId;
    }

    public override RpcCalls GetId() => RpcCalls.SetName;

    public override bool IsHostOnly() => !AcUtil.IsAnticheatPresent();
}

internal class SetColor : RpcCheck
{
    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        uint netId = reader.ReadUInt32();
        byte color = reader.ReadByte();

        if (netId != player.Data.NetId)
        {
            Anticheat.Flag(player, $"SetColor RPC sent for {player.Data.PlayerName} contains invalid net id, expected {player.Data.NetId}, received {netId}", false);
            player.SetColor((byte)CrewmateColor.Red);
            return false;
        }

        if (color >= Palette.ColorNames.Length)
        {
            Anticheat.Flag(player, $"SetColor RPC sent for {player.Data.PlayerName} contains an invalid color: {color}", false);
            player.SetColor((byte)CrewmateColor.Red);
            return false;
        }

        return true;
    }

    public override RpcCalls GetId() => RpcCalls.SetColor;

    public override bool IsHostOnly() => true;
}

internal class SetLevel : RpcCheck
{
    public readonly uint MAX_PLAYER_LEVEL = 10000;

    // Deliberately does not block valid SetLevel RPCs — KarakMenu's own level
    // spoofer sends one, and KarakMenu's configured level thresholds are
    // checked separately in Anticheat.CheckRpc.
    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        uint level = reader.ReadPackedUInt32();

        // Vanilla bans above 100k; KarakMenu is stricter at 10k.
        if (level > MAX_PLAYER_LEVEL)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} sent SetLevel RPC with a level that is too high ({level}).");
            player.SetLevel(MAX_PLAYER_LEVEL);
            return false;
        }

        // SetLevel should only ever be sent while a player is joining, in the lobby.
        if (ShipStatus.Instance != null)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} sent SetLevel RPC when the game has already started.");
            return false;
        }

        return true;
    }

    public override RpcCalls GetId() => RpcCalls.SetLevel;
}

internal class ReportDeadBody : RpcCheck
{
    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        if (GameManager.Instance.IsHideAndSeek())
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to call a meeting in Hide and Seek");
            return false;
        }

        return true;
    }

    public override RpcCalls GetId() => RpcCalls.ReportDeadBody;
}

internal class SetScanner : RpcCheck
{
    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        bool scanning = reader.ReadBoolean();

        if (ShipStatus.Instance == null && scanning)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} sent the SetScanner RPC while the map has not spawned in yet.");
            return false;
        }

        // A SetScanner RPC with scanning=false is sent on death, for impostors
        // too, so only the true case is suspicious.
        if (RoleManager.IsImpostorRole(player.Data.RoleType) && scanning)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} sent the SetScanner RPC when they are an imposter.");
            return false;
        }

        if (!GameManager.Instance.LogicOptions.GetVisualTasks())
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} sent the SetScanner RPC while visual tasks were disabled.");
            return false;
        }

        bool hasMedbayScanTask = false;
        if (player.Data.Tasks != null)
        {
            foreach (NetworkedPlayerInfo.TaskInfo task in player.Data.Tasks)
            {
                if (task.Id != (byte)TaskTypes.SubmitScan) continue;
                hasMedbayScanTask = true;
                break;
            }
        }

        if (!hasMedbayScanTask && scanning)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} sent the SetScanner RPC without being assigned the medbay scan task.");
            return false;
        }

        return true;
    }

    public override RpcCalls GetId() => RpcCalls.SetScanner;
}

internal class SetStartCounter : RpcCheck
{
    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        reader.ReadPackedInt32();
        sbyte counter = reader.ReadSByte();

        // Non-host clients must always send -1 here.
        if (player.OwnerId != AmongUsClient.Instance.HostId && counter != -1)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} sent a SetStartCounter RPC with an invalid value: {counter}.");

            if (AmongUsClient.Instance.AmHost)
            {
                PlayerControl.LocalPlayer.RpcSetStartCounter(-1);
            }

            return false;
        }

        return true;
    }

    public override RpcCalls GetId() => RpcCalls.SetStartCounter;
}

internal class EnterVent : RpcCheck
{
    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        if (ShipStatus.Instance == null)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} tried to vent when there is no instance of ShipStatus.");
            return false;
        }

        // Skip the role check while dead: a player can send an EnterVent RPC the
        // instant they die, which would otherwise false-flag.
        if (!player.Data.IsDead && !player.Data.Role.CanVent)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} tried to vent when their role ({player.Data.RoleType}) does not support venting.");
            return false;
        }

        if (GameManager.Instance.IsHideAndSeek() && RoleManager.IsImpostorRole(player.Data.RoleType))
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} tried to vent while being the seeker.");
            return false;
        }

        return true;
    }

    public override RpcCalls GetId() => RpcCalls.EnterVent;

    public override Type GetExpectedNetObject() => typeof(PlayerPhysics);
}

internal class ExitVent : RpcCheck
{
    // ExitVent can be abused to teleport without a SnapTo RPC.
    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        if (ShipStatus.Instance == null)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} tried to exit a vent when there is no instance of ShipStatus.");
            return false;
        }

        if (!player.Data.IsDead && !player.Data.Role.CanVent)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} tried to exit a vent when their role ({player.Data.RoleType}) does not support venting.");
            return false;
        }

        if (GameManager.Instance.IsHideAndSeek() && RoleManager.IsImpostorRole(player.Data.RoleType))
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} tried to exit a vent while being the seeker.");
            return false;
        }

        return true;
    }

    public override RpcCalls GetId() => RpcCalls.ExitVent;

    public override Type GetExpectedNetObject() => typeof(PlayerPhysics);
}

internal class SnapTo : RpcCheck
{
    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        if (LobbyBehaviour.Instance != null)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} sent the SnapTo RPC while inside the lobby.");

            // SnapTo for another player's NetTransform only works on servers
            // without net object impersonation checks.
            if (AmongUsClient.Instance.AmHost && !AcUtil.IsAnticheatPresent())
            {
                player.NetTransform.RpcSnapTo(player.transform.position);
            }

            return false;
        }

        return true;
    }

    public override RpcCalls GetId() => RpcCalls.SnapTo;

    public override Type GetExpectedNetObject() => typeof(CustomNetworkTransform);
}

internal class AddVote : RpcCheck
{
    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        int source = reader.ReadInt32();
        int target = reader.ReadInt32();

        ClientData client = AmongUsClient.Instance.FindClientById(source);
        if (client == null || client.Character == null)
        {
            KarakMenu.Log.LogInfo($"An unknown client id ({source}) attempted to votekick {target}");
            return false;
        }

        player = client.Character;

        if (player.Data.IsDead)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to votekick a player while dead.");
            return false;
        }

        if (MeetingHud.Instance == null)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to votekick a player outside of a meeting.");
            return false;
        }

        return true;
    }

    public override RpcCalls GetId() => RpcCalls.AddVote;

    public override Type GetExpectedNetObject() => typeof(VoteBanSystem);
}

internal class CloseDoorsOfType : RpcCheck
{
    // The sender can't be identified for this RPC, hence the player-less Flag.
    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        if (GameManager.Instance.IsHideAndSeek())
        {
            Anticheat.Flag("Someone attempted to close doors while in Hide and Seek.");
            return false;
        }

        return true;
    }

    public override RpcCalls GetId() => RpcCalls.CloseDoorsOfType;

    public override Type GetExpectedNetObject() => typeof(ShipStatus);
}

internal class ClimbLadder : RpcCheck
{
    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        if (ShipStatus.Instance == null)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} tried to climb a ladder when there is no instance of ShipStatus.");
            return false;
        }

        MapNames map = AcUtil.GetCurrentMap();
        if (map != MapNames.Airship && map != MapNames.Fungle)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} tried to climb a ladder outside of the proper map.");
            return false;
        }

        if (player.Data.IsDead)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} tried to climb a ladder while dead.");
            return false;
        }

        return true;
    }

    public override RpcCalls GetId() => RpcCalls.ClimbLadder;

    public override Type GetExpectedNetObject() => typeof(PlayerPhysics);
}

internal class UsePlatform : RpcCheck
{
    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        MapNames map = AcUtil.GetCurrentMap();
        if (map != MapNames.Airship)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} tried to use a platform outside of the proper map.");
            return false;
        }

        if (ShipStatus.Instance == null)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} tried to use a platform when there is no instance of ShipStatus.");
            return false;
        }

        if (GameManager.Instance.IsHideAndSeek())
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} tried to use a platform while in Hide and Seek.");
            return false;
        }

        return true;
    }

    public override RpcCalls GetId() => RpcCalls.UsePlatform;
}

internal class UpdateSystem : RpcCheck
{
    private static readonly SystemTypes[] SystemsThatCanBeUpdatedWhenDead =
    {
        SystemTypes.MedBay,
        SystemTypes.Sabotage,
        // Ghosts update the Security system when closing cameras, but not opening them
        SystemTypes.Security,
        SystemTypes.Ventilation
    };

    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        SystemTypes system = (SystemTypes)reader.ReadByte();
        player = reader.ReadNetObject<PlayerControl>();
        if (player == null || player.Data == null) return true;

        ShipStatus.Instance.Systems.TryGetValue(system, out ISystemType systemInterface);
        if (systemInterface == null)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} tried to update system {system} when the current map has no such system.");
            return false;
        }

        if (player.Data.IsDead && Array.IndexOf(SystemsThatCanBeUpdatedWhenDead, system) < 0)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} tried to update system {system} while dead.");
            return false;
        }

        // Dispatch on the concrete system type. TryCast avoids the interop
        // type-handle dance entirely, without
        // reaching into Il2CppType at all.
        if (systemInterface.TryCast<HudOverrideSystemType>() != null) return ValidateHudOverrideSystem(player, reader);
        if (systemInterface.TryCast<LifeSuppSystemType>() != null) return ValidateLifeSuppSystem(player, reader);
        if (systemInterface.TryCast<MushroomMixupSabotageSystem>() != null) return ValidateMushroomMixupSystem(player, reader);
        if (systemInterface.TryCast<ReactorSystemType>() != null) return ValidateReactorSystem(player, reader);
        if (systemInterface.TryCast<SabotageSystemType>() != null) return ValidateSabotageSystem(player, reader);
        if (systemInterface.TryCast<SwitchSystem>() != null) return ValidateSwitchSystem(player, reader);

        return true;
    }

    private static bool ValidateHudOverrideSystem(PlayerControl player, MessageReader reader)
    {
        HudOverrideSystemOperation operation = (HudOverrideSystemOperation)reader.ReadByte();

        if (operation.HasFlag(HudOverrideSystemOperation.Sabotage))
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to force call the Comms sabotage");
            return false;
        }

        return true;
    }

    private static bool ValidateLifeSuppSystem(PlayerControl player, MessageReader reader)
    {
        LifeSuppSystemOperation operation = (LifeSuppSystemOperation)reader.ReadByte();

        if (operation.HasFlag(LifeSuppSystemOperation.Fix))
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to force repair the LifeSupp sabotage");
            return false;
        }

        if (operation.HasFlag(LifeSuppSystemOperation.Sabotage))
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to force call the LifeSupp sabotage");
            return false;
        }

        return true;
    }

    // The Mushroom Mixup system is only updated by the host in
    // SabotageSystemType::Update, so a player must never send it.
    private static bool ValidateMushroomMixupSystem(PlayerControl player, MessageReader reader)
    {
        MushroomMixupSabotageSystem.Operation operation = (MushroomMixupSabotageSystem.Operation)reader.ReadByte();

        Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to update Mushroom Mixup system with operation {operation}.");
        return false;
    }

    private static bool ValidateReactorSystem(PlayerControl player, MessageReader reader)
    {
        ReactorSystemOperation operation = (ReactorSystemOperation)reader.ReadByte();

        if (operation.HasFlag(ReactorSystemOperation.Fix))
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to forcefully fix the Reactor sabotage");
            return false;
        }

        if (operation.HasFlag(ReactorSystemOperation.Sabotage))
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to force call the Reactor sabotage");
            return false;
        }

        return true;
    }

    private static bool ValidateSabotageSystem(PlayerControl player, MessageReader reader)
    {
        SystemTypes system = (SystemTypes)reader.ReadByte();

        Dictionary<string, SystemTypes> validSabotages = AcUtil.GetSabotages();
        if (!validSabotages.ContainsValue(system))
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to sabotage an invalid system: {system}.");
            return false;
        }

        if (!RoleManager.IsImpostorRole(player.Data.RoleType))
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to sabotage {system} when they are not an imposter.");
            return false;
        }

        if (GameManager.Instance.IsHideAndSeek())
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to sabotage {system} while in Hide and Seek.");
            return false;
        }

        if (player.inVent)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to sabotage {system} while in a vent.");
            return false;
        }

        return true;
    }

    private static bool ValidateSwitchSystem(PlayerControl player, MessageReader reader)
    {
        byte switches = reader.ReadByte();

        if (switches.HasBit(128))
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to bulk-update switches: {switches}.");
            return false;
        }

        if (switches > 5)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to toggle an invalid switch: {switches}.");
            return false;
        }

        // Silent blocks below: these can false-flag on normal play (a player
        // fixing lights before receiving the fix message, or switching during a
        // meeting) so they're blocked without notifying or punishing.
        if (!AcUtil.IsSabotageActive(SystemTypes.Electrical))
        {
            KarakMenu.Log.LogInfo($"Blocked switch update from {player.Data.PlayerName} as lights are not currently sabotaged");
            return false;
        }

        if (MeetingHud.Instance != null)
        {
            KarakMenu.Log.LogInfo($"Blocked switch update from {player.Data.PlayerName} as there is a currently active meeting");
            return false;
        }

        return true;
    }

    public override RpcCalls GetId() => RpcCalls.UpdateSystem;

    public override Type GetExpectedNetObject() => typeof(ShipStatus);
}

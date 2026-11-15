using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using System;
using System.Collections.Generic;

namespace KarakMenu.AC;

// ---------------------------------------------------------------------------
// Check framework. Each RPC is validated through a small
// class with its own Enabled flag, instead of
// monolithic CheckRpc method — that's what makes per-RPC toggles possible.
// ---------------------------------------------------------------------------

public interface ICheck
{
    bool Enabled { get; set; }
}

public abstract class RpcCheck : ICheck
{
    public bool Enabled { get; set; } = true;

    public virtual bool Validate(PlayerControl player, MessageReader reader) => true;

    public abstract RpcCalls GetId();

    // Host-only RPCs must never arrive from a non-host client.
    public virtual bool IsHostOnly() => false;

    // Most RPCs belong to the PlayerControl net object.
    public virtual Type GetExpectedNetObject() => typeof(PlayerControl);
}

public abstract class GameDataCheck : ICheck
{
    public bool Enabled { get; set; } = true;

    public virtual bool Validate(MessageReader reader) => true;

    public abstract GameDataTypes GetId();
}

// ---------------------------------------------------------------------------
// Net object owner ids
// ---------------------------------------------------------------------------
public enum OwnerIds
{
    Everyone = -1,
    Host = -2,
    Self = -3,
    Server = -4
}

// ---------------------------------------------------------------------------
// System operation bit flags used by the UpdateSystem check
//
// ---------------------------------------------------------------------------
[Flags]
public enum HudOverrideSystemOperation
{
    Sabotage = 128
}

[Flags]
public enum LifeSuppSystemOperation
{
    Fix = 16,
    RepairConsole = 64,
    Sabotage = 128
}

[Flags]
public enum ReactorSystemOperation
{
    Fix = 16,
    UnrepairConsole = 32,
    RepairConsole = 64,
    Sabotage = 128
}

// ---------------------------------------------------------------------------
// Helpers shared by the check implementations.
// ---------------------------------------------------------------------------
public static class AcUtil
{
    // KarakMenu already has a working map resolver, so reuse it rather than
    // a SpawnId switch (ShipStatus.SpawnId isn't exposed the same way).
    public static MapNames GetCurrentMap() => (MapNames)Utils.GetCurrentMapID();

    // True when the lobby is server-authoritative (vanilla servers own our
    // NetworkedPlayerInfo). On modded/local/freeplay lobbies this is false and
    // the name checks are skipped, same as vanilla.
    public static bool IsAnticheatPresent()
    {
        try
        {
            if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return false;
            return PlayerControl.LocalPlayer.Data.OwnerId != (int)OwnerIds.Host;
        }
        catch { return false; }
    }

    public static readonly Dictionary<string, SystemTypes> SkeldSabotages = new Dictionary<string, SystemTypes>
    {
        { "Reactor", SystemTypes.Reactor },
        { "Oxygen", SystemTypes.LifeSupp },
        { "Lights", SystemTypes.Electrical },
        { "Communications", SystemTypes.Comms }
    };

    public static readonly Dictionary<string, SystemTypes> MiraSabotages = new Dictionary<string, SystemTypes>
    {
        { "Reactor", SystemTypes.Reactor },
        { "Oxygen", SystemTypes.LifeSupp },
        { "Lights", SystemTypes.Electrical },
        { "Communications", SystemTypes.Comms }
    };

    public static readonly Dictionary<string, SystemTypes> PolusSabotages = new Dictionary<string, SystemTypes>
    {
        { "Reactor", SystemTypes.Laboratory },
        { "Lights", SystemTypes.Electrical },
        { "Communications", SystemTypes.Comms }
    };

    public static readonly Dictionary<string, SystemTypes> AirshipSabotages = new Dictionary<string, SystemTypes>
    {
        { "Reactor", SystemTypes.HeliSabotage },
        { "Lights", SystemTypes.Electrical },
        { "Communications", SystemTypes.Comms }
    };

    public static readonly Dictionary<string, SystemTypes> FungleSabotages = new Dictionary<string, SystemTypes>
    {
        { "Reactor", SystemTypes.Reactor },
        { "Communications", SystemTypes.Comms },
        { "Mushroom Mixup", SystemTypes.MushroomMixupSabotage }
    };

    public static Dictionary<string, SystemTypes> GetSabotages()
    {
        return GetCurrentMap() switch
        {
            MapNames.Skeld or MapNames.Dleks => SkeldSabotages,
            MapNames.MiraHQ => MiraSabotages,
            MapNames.Polus => PolusSabotages,
            MapNames.Airship => AirshipSabotages,
            MapNames.Fungle => FungleSabotages,
            _ => SkeldSabotages
        };
    }

    public static bool IsSabotageActive(SystemTypes system)
    {
        if (ShipStatus.Instance == null) return false;

        ShipStatus.Instance.Systems.TryGetValue(system, out ISystemType systemType);
        if (systemType == null) return false;

        IActivatable activatable = systemType.TryCast<IActivatable>();
        if (activatable == null) return false;

        return activatable.IsActive;
    }
}

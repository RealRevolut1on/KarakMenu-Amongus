using System.Collections.Generic;
using Hazel;

namespace KarakMenu;

// Broadcasts the local Moonwalk toggle so every client suppresses that
// player's walk animation, not just the toggler's own screen. Walk animation
// is derived independently on each client from the position data it already
// receives for a player, so there's no vanilla flag to piggyback on — this
// adds one. RpcId (240) sits well above every real RpcCalls value (vanilla
// tops out in the 30s-40s), so PlayerControl.HandleRpc's switch would just
// fall through it unrecognized; the Harmony prefix on that method intercepts
// our id first and returns false so vanilla never even sees it.
public static class MoonwalkSync
{
    public const byte RpcId = 240;

    private static readonly HashSet<byte> _moonwalking = new();
    private static bool _lastSent;

    public static bool IsMoonwalking(byte playerId) => _moonwalking.Contains(playerId);

    public static void SetLocal(byte playerId, bool on)
    {
        if (on) _moonwalking.Add(playerId);
        else _moonwalking.Remove(playerId);
    }

    public static void Reset()
    {
        _moonwalking.Clear();
        _lastSent = false;
    }

    // Edge-triggered: only sends when the toggle actually changes, called
    // once per frame from the same loop that drives the other cheats.
    public static void Tick()
    {
        if (!Utils.isPlayer || PlayerControl.LocalPlayer == null) return;

        bool on = CheatToggles.moonWalk;
        SetLocal(PlayerControl.LocalPlayer.PlayerId, on);

        if (on == _lastSent) return;
        _lastSent = on;

        var writer = AmongUsClient.Instance.StartRpcImmediately(
            PlayerControl.LocalPlayer.NetId, RpcId, SendOption.Reliable, -1);
        writer.Write(on);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public static void OnRpc(byte playerId, MessageReader reader)
    {
        try { SetLocal(playerId, reader.ReadBoolean()); } catch { }
    }
}

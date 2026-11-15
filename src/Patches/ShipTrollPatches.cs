using System.Collections.Generic;
using HarmonyLib;

namespace KarakMenu;

// Host-authoritative ship-wide moderation patches — both act on the systems
// that already broadcast their state to every client (VentilationSystem /
// SabotageSystemType), so calling them as non-host would only affect local
// rendering, not the actual networked state everyone else sees.

[HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.Deserialize))]
public static class VentilationSystem_Deserialize_DisableVents
{
    // Postfix patch of VentilationSystem.Deserialize to evict every OTHER
    // player caught inside a vent, every time the vent-occupancy state syncs.
    // The local player is left alone so the host can still use vents themselves.
    public static void Postfix(VentilationSystem __instance)
    {
        if (!CheatToggles.disableVents || !Utils.isHost || !Utils.isShip) return;

        var inside = __instance.PlayersInsideVents;
        if (inside == null || inside.Count == 0) return;

        var occupied = new List<(byte playerId, byte ventId)>();
        foreach (var kv in inside)
        {
            if (PlayerControl.LocalPlayer != null && kv.Key == PlayerControl.LocalPlayer.PlayerId) continue;
            occupied.Add((kv.Key, kv.Value));
        }

        foreach (var (_, ventId) in occupied)
            __instance.BootImpostorsFromVent(ventId);
    }
}

[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.Deserialize))]
public static class SabotageSystemType_Deserialize_BlockSabotages
{
    // Postfix patch of SabotageSystemType.Deserialize to instantly cancel any
    // sabotage the moment its active state syncs, regardless of who triggered
    // it or their role — 255 is the game's own "clear" byte for RpcUpdateSystem.
    public static void Postfix(SabotageSystemType __instance)
    {
        if (!CheatToggles.blockSabotages || !Utils.isHost || ShipStatus.Instance == null) return;
        if (!__instance.AnyActive) return;

        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, 255);
    }
}

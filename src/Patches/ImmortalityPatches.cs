using HarmonyLib;

namespace KarakMenu;

[HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.Update))]
public static class VentilationSystem_Update_ImmortalityFilter
{
    // Prefix patch of VentilationSystem.Update to keep the local player's
    // faked inVent state from being cleared by their own real movement while
    // Immortality is active — real vent enter/exit/move calls are dropped
    // unless they're our own fake key, so walking around can't undo it.
    public static bool Prefix(VentilationSystem.Operation op, int ventId)
    {
        if (!CheatToggles.immortality || ventId == KarakImmortality.VirtualVentKey) return true;
        return op is not (VentilationSystem.Operation.Enter or VentilationSystem.Operation.Exit or VentilationSystem.Operation.Move);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class PlayerControl_MurderPlayer_ImmortalityNotify
{
    // Postfix patch of PlayerControl.MurderPlayer to let the local player
    // know when someone tried to kill them while Immortality blocked it —
    // otherwise a blocked attempt is invisible, since the target simply
    // never dies with no other feedback.
    public static void Postfix(PlayerControl __instance, PlayerControl target)
    {
        if (target == null || !target.AmOwner || !CheatToggles.immortality || !target.inVent) return;
        HudManager.Instance?.Notifier?.AddDisconnectMessage($"[Warning] {__instance?.Data?.PlayerName} attempted to kill you!");
    }
}

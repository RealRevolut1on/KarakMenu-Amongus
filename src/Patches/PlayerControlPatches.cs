using HarmonyLib;
using System.Linq;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils;
using System.Collections.Generic;

namespace KarakMenu;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public static class PlayerControl_SetKillTimer
{
    // Prefix patch of PlayerControl.SetKillTimer to remove kill cooldown
    public static void Prefix(PlayerControl __instance, ref float time)
    {
        if (!__instance.AmOwner || !Utils.isHost || !CheatToggles.noKillCd) return;

        time = 0f;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckMurder))]
public static class PlayerControl_CmdCheckMurder
{
    // Re-entry guard for the infinite-reach gap-close below, so the follow-up
    // call skips straight through instead of teleporting a second time.
    private static bool _reachReentry;
    private const float MaxVanillaKillRange = 2.5f;

    public static bool Prefix(PlayerControl __instance, PlayerControl target)
    {
        // Flash Kill is now a one-shot menu action handled by
        // KarakCheats.FlashKillCheat() (auto-targets the nearest player from
        // any distance). CmdCheckMurder only fires when the game's kill button
        // is pressed, which never happens at range, so it can't live here.

        // Infinite Kill Reach: as a non-host we cannot override the host's own
        // range validation, so firing an out-of-range murder is what gets us
        // kicked on the spot. Close the gap first, then let the official
        // host-authority path run — the host sees a normal in-range kill.
        if (!_reachReentry
            && CheatToggles.infiniteKillReach
            && __instance != null && __instance.AmOwner
            && !Utils.isHost && Utils.isPlayer && !Utils.isLobby
            && target != null && target.Data != null && !target.Data.IsDead)
        {
            float distance = Vector2.Distance(__instance.GetTruePosition(), target.GetTruePosition());
            if (distance > MaxVanillaKillRange)
            {
                __instance.StartCoroutine(ReachKillRoutine(__instance, target));
                return false;
            }
        }

        if (!Utils.isHost) return true;

        // __instance.isKilling = true;
        PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);

        return false;
    }

    private static System.Collections.IEnumerator ReachKillRoutine(PlayerControl killer, PlayerControl target)
    {
        if (killer == null || target == null) yield break;

        killer.NetTransform.RpcSnapTo(target.GetTruePosition());

        // Let the host apply our position before we ask it to approve the kill.
        yield return null;
        yield return null;

        if (killer != null && target != null)
        {
            _reachReentry = true;
            try
            {
                killer.CmdCheckMurder(target);
            }
            finally
            {
                _reachReentry = false;
            }
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class PlayerControl_MurderPlayer
{
    // Prefix patch of PlayerControl.MurderPlayer to log on ConsoleUI when a player tries to kill another player,
    // along with who the killer and target are, and where the kill happened.
    // Also logs when a kill gets saved by a guardian angel.
    public static void Prefix(PlayerControl __instance, PlayerControl target)
    {
        // Feed the See Cooldown ESP even when kill logging is off.
        if (__instance != null && target != null) KarakESP.RecordKill(__instance);

        if (!CheatToggles.logDeaths || target == null) return;

        var (realKillerName, displayKillerName, isDisguised) = Utils.GetPlayerIdentity(__instance);
        var targetName = $"<color=#{ColorUtility.ToHtmlStringRGB(target.Data.Color)}>{target.CurrentOutfit.PlayerName}</color>";

        var room = Utils.GetRoomFromPosition(target.GetTruePosition());
        var roomName = room != null ? room.RoomId.ToString() : "an unknown location";

        if (target.protectedByGuardianId != -1)
        {
            ConsoleUI.Log(isDisguised ? $"{realKillerName} (as {displayKillerName}) tried to kill {targetName} in {roomName} (Protected)"
                : $"{realKillerName} tried to kill {targetName} in {roomName} (Protected)");
        }
        else
        {
            ConsoleUI.Log(isDisguised ? $"{realKillerName} (as {displayKillerName}) killed {targetName} in {roomName}"
                : $"{realKillerName} killed {targetName} in {roomName}");
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TurnOnProtection))]
public static class PlayerControl_TurnOnProtection
{
    // Prefix patch of PlayerControl.TurnOnProtection to make all protections visible
    public static void Prefix(ref bool visible)
    {
		if (CheatToggles.seeGhosts)
        {
            visible = true;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckShapeshift))]
public static class PlayerControl_CmdCheckShapeshift
{
    // Prefix patch of PlayerControl.CmdCheckShapeshift to prevent SS animation
    public static void Prefix(ref bool shouldAnimate)
    {
        if (shouldAnimate && CheatToggles.noShapeshiftAnim)
        {
            shouldAnimate = false;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckRevertShapeshift))]
public static class PlayerControl_CmdCheckRevertShapeshift
{
    // Prefix patch of PlayerControl.CmdCheckRevertShapeshift to prevent SS animation
    public static void Prefix(ref bool shouldAnimate){

        if (shouldAnimate && CheatToggles.noShapeshiftAnim)
        {
            shouldAnimate = false;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class PlayerControl_Shapeshift
{
    // Postfix patch of PlayerControl.Shapeshift to log on ConsoleUI when a player shapeshifts into another player,
    // and who they shapeshifted into. Also logs when a shapeshift gets reverted.
    public static void Postfix(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
    {
        if (!CheatToggles.logShapeshifts) return;

        if (__instance.CurrentOutfitType == PlayerOutfitType.MushroomMixup) return;

        var targetPlayerInfo = targetPlayer.Data;

        var room = Utils.GetRoomFromPosition(__instance.GetTruePosition());
        var roomName = room != null ? room.RoomId.ToString() : "an unknown location";

        if (targetPlayerInfo.PlayerId == __instance.Data.PlayerId)
        {
            ConsoleUI.Log($"<color=#{ColorUtility.ToHtmlStringRGB(GameData.Instance.GetPlayerById(__instance.PlayerId).Color)}>" +
                          $"{GameData.Instance.GetPlayerById(__instance.PlayerId)._object.Data.PlayerName}</color> undid their shapeshift in {roomName}");
        }
        else
        {
            ConsoleUI.Log($"<color=#{ColorUtility.ToHtmlStringRGB(GameData.Instance.GetPlayerById(__instance.PlayerId).Color)}>" +
                          $"{GameData.Instance.GetPlayerById(__instance.PlayerId)._object.Data.PlayerName}</color> shapeshifted into " +
                          $"<color=#{ColorUtility.ToHtmlStringRGB(GameData.Instance.GetPlayerById(targetPlayerInfo.PlayerId).Color)}>" +
                          $"{GameData.Instance.GetPlayerById(targetPlayerInfo.PlayerId)._object.Data.PlayerName}</color> in {roomName}");
        }
    }
}

// When infiniteKillReach is on, override the distance check so the kill button
// activates against any valid crewmate regardless of range. Patched on
// ImpostorRole (which owns the kill-target search) rather than PlayerControl —
// the same pattern used for DetectiveRole/TrackerRole infinite reach above.
// Shapeshifter extends Impostor so it inherits the same override.
[HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.FindClosestTarget))]
public static class ImpostorRole_FindClosestTarget_InfiniteReach
{
    public static bool Prefix(ImpostorRole __instance, ref PlayerControl __result)
    {
        if (!CheatToggles.infiniteKillReach || !__instance.Player.AmOwner) return true;
        if (Utils.isLobby || !Utils.isPlayer) return true;

        var playerList = Utils.GetPlayersSortedByDistance();
        foreach (var p in playerList)
        {
            if (p == null || p.Data == null || p.AmOwner) continue;
            if (p.Data.IsDead || p.Data.Disconnected) continue;
            if (!__instance.IsValidTarget(p.Data)) continue;
            __result = p;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetLevel))]
public static class PlayerControl_RpcSetLevel_Spoof
{
    // The game announces the local player's level with SetLevel right after
    // joining. DataManager is often still untouched at that point, so rewriting
    // it alone never reaches anyone — intercept the outgoing RPC instead and
    // substitute the spoofed value on the wire (0-based, same as
    // NetworkedPlayerInfo.PlayerLevel).
    public static void Prefix(ref uint level)
    {
        if (!uint.TryParse(KarakMenu.spoofLevel.Value, out uint target) || target < 1) return;
        level = target - 1;
    }
}

// Typing in a menu text field must not also walk the player around. Unity's
// IMGUI Event.Use() only swallows the IMGUI event — Input.GetAxis/GetKey keep
// feeding the game's own movement code. Pin moveable off for exactly the
// duration of the physics step that reads it, then hand it straight back.
[HarmonyPatch(typeof(PlayerControl), "FixedUpdate")]
public static class PlayerControl_FixedUpdate_InputGuard
{
    private static bool _patched;
    private static bool _wasTyping;
    private static bool _savedMoveable;

    public static void Prefix(PlayerControl __instance)
    {
        if (!__instance.AmOwner || !KarakUI.IsTextInputFocused) return;

        // Kill any residual momentum the instant typing starts
        if (!_wasTyping)
        {
            _wasTyping = true;
            __instance.NetTransform?.Halt();
        }

        _savedMoveable = __instance.moveable;
        __instance.moveable = false;
        _patched = true;
    }

    public static void Postfix(PlayerControl __instance)
    {
        if (!_patched || !__instance.AmOwner) return;

        __instance.moveable = _savedMoveable;
        _patched = false;
        if (!KarakUI.IsTextInputFocused) _wasTyping = false;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public static class PlayerControl_RpcSyncSettings
{
    // Prefix patch of PlayerControl.RpcSyncSettings to prevent the anti-cheat from kicking you
    // for some settings that are out of the "original" valid range
    public static bool Prefix(PlayerControl __instance, byte[] optionsByteArray)
    {
        return !CheatToggles.noOptionsLimits;
    }
}

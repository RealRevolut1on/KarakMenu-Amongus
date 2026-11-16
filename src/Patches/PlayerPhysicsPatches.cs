using System;
using HarmonyLib;
using Hazel;
using UnityEngine;

namespace KarakMenu;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.LateUpdate))]
public static class PlayerPhysics_LateUpdate
{
    public static void Postfix(PlayerPhysics __instance)
    {
        // Per-player rendering: LateUpdate fires once per PlayerPhysics
        // instance in the lobby (i.e. once per PLAYER, not once per frame),
        // and these three calls act on __instance specifically (nametag,
        // ghost visibility, tracer target) — they need to run for every
        // player to draw ESP on everyone, not just the local player.
        KarakESP.PlayerNametags(__instance);
        KarakESP.SeeGhostsCheat(__instance);
        TracersHandler.DrawPlayerTracer(__instance);

        // Everything below operates on global state (PlayerControl.LocalPlayer,
        // AllPlayerControls, the dead-body scan, etc.) and doesn't touch
        // __instance at all, so it only needs to run once per frame. Without
        // this gate it silently ran once per PLAYER in the lobby — with N
        // players that's N-fold the allocations/CPU every single frame, worst
        // offender being the full-scene GameObject.FindGameObjectsWithTag
        // call below. Same fix the (now redundant) commented-out
        // OverloadHandler.Run() gate below was already doing for one call.
        if (!__instance.AmOwner) return;

        KarakESP.TickCooldownTracking();
        KarakCheats.NoClipCheat();
        KarakCheats.ProtectCheat();
        KarakImmortality.Tick();
        KarakCheats.KillAllCheat();
        KarakCheats.KillAllCrewCheat();
        KarakCheats.KillAllImpsCheat();
        KarakCheats.FlashKillCheat();
        KarakCheats.AutoKillCheat();
        KarakCheats.InvisibilityCheat();
        KarakCheats.SpamTpImpsCheat();
        KarakCheats.SpamTpAllCheat();
        KarakCheats.TeleportAllToVentCheat();
        KarakCheats.DoorHallucinationAllCheat();
        KarakCheats.ConfuseNowCheat();
        KarakCheats.ForceImpostorAllCheat();
        KarakCheats.SameColorEveryoneCheat();
        KarakCheats.CycleLobbyColorCheat();
        KarakCheats.LobbyDiscoCheat();
        KarakCheats.IncreaseImpostorsCheat();
        KarakCheats.DecreaseImpostorsCheat();
        KarakCheats.IncreasePlayerSpeedCheat();
        KarakCheats.DecreasePlayerSpeedCheat();
        KarakCheats.DestroyLobbyCheat();
        KarakCheats.RecreateLobbyCheat();
        KarakCheats.EndGameNowCheat();
        KarakCheats.ApplyZeroCdCheat();
        KarakCheats.ForceStartGameCheat();
        KarakCheats.TeleportCursorCheat();
        KarakCheats.CompleteMyTasksCheat();
        KarakCheats.PlayAnimationCheat();
        KarakCheats.PlayScannerCheat();
        KarakCheats.SpeedHackCheat();
        KarakCheats.CopyOutfitToAllCheat();
        MoonwalkSync.Tick();

        KarakPlayersCheats.DiscoTick();
        KarakPlayersCheats.ShapeshiftAllIntoSelfCheat();
        KarakPlayersCheats.SpamShapeshiftAllTick();
        KarakPlayersCheats.UnshiftAllCheat();

        KarakPPMCheats.EjectPlayerPPM();
        KarakPPMCheats.KickPlayerPPM();
        KarakPPMCheats.BanPlayerPPM();
        KarakPPMCheats.DoorHallucinationPPM();
        KarakPPMCheats.SpectatePPM();
        KarakPPMCheats.KillPlayerPPM();
        KarakPPMCheats.TelekillPlayerPPM();
        KarakPPMCheats.TeleportPlayerPPM();
        KarakPPMCheats.TeleportPlayerToVentPPM();
        KarakPPMCheats.SetFakeRolePPM();
        KarakPPMCheats.SetFakeAlivePPM();
        KarakPPMCheats.LevelFarmPPM();
        KarakPlayersCheats.JailTick();
        KarakPPMCheats.CopyOutfitPPM();
        // KarakPPMCheats.ForceRolePPM();

        KarakDetection.Update();
        KarakESP.VentPlayerHighlightCheat();

        GameObject[] bodyObjects = GameObject.FindGameObjectsWithTag("DeadBody");
        foreach(GameObject bodyObject in bodyObjects) // Finds and loops through all dead bodies
        {
            DeadBody deadBody = bodyObject.GetComponent<DeadBody>();
            if (!deadBody) continue;

            TracersHandler.DrawBodyTracer(deadBody);

            if (CheatToggles.autoReportBodies)
            {
                if (deadBody.Reported) continue;

                deadBody.Reported = true;

                PlayerControl.LocalPlayer.CmdReportDeadBody(GameData.Instance.GetPlayerById(deadBody.ParentId));
            }
        }

        try
        {
            if (CheatToggles.invertControls)
            {
                PlayerControl.LocalPlayer.MyPhysics.Speed = -Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.Speed);
                PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = -Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed);
            }
            else
            {
                PlayerControl.LocalPlayer.MyPhysics.Speed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.Speed);
                PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed);
            }
        } catch (NullReferenceException) { }
    }
}

// Custom Speed's real fix: PlayerPhysics.Speed is just a stored field — the
// game actually reads PlayerPhysics.TrueSpeed (a computed getter) to drive
// real movement each tick, which silently ignores whatever we write to
// .Speed directly. Patching the getter itself is what makes an override
// actually stick. This only works for the LOCAL player: TrueSpeed is
// evaluated independently on every client for every PlayerControl it has
// loaded, but each player's OWN real movement is driven by THEIR OWN
// client's evaluation of THEIR OWN PlayerPhysics — so patching this on the
// host for some OTHER (remote) player's PlayerPhysics only affects how the
// host's client locally renders that remote player, never their actual
// movement as everyone else sees it. There's no per-player speed RPC in
// Among Us (PlayerSpeedMod is a single lobby-wide option), so a genuinely
// different speed for one specific other player isn't achievable this way —
// which is why this only checks AmOwner, not any other-player override.
[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.TrueSpeed), MethodType.Getter)]
public static class PlayerPhysics_TrueSpeed
{
    public static void Postfix(PlayerPhysics __instance, ref float __result)
    {
        if (!CheatToggles.customSpeed) return;
        var pc = __instance.myPlayer;
        if (pc == null || !pc.AmOwner) return;
        __result = CheatToggles.customSpeedValue;
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleAnimation))]
public static class PlayerPhysics_HandleAnimation
{
    // Prefix patch of PlayerPhysics.HandleAnimation to disable walking
    // animation. Keyed on MoonwalkSync (networked per-player state) rather
    // than AmOwner + the local toggle, so this also fires on every OTHER
    // client's rendering of a moonwalking player, not just their own screen.
    public static bool Prefix(PlayerPhysics __instance)
    {
        var pc = __instance.myPlayer;
        if (pc != null && MoonwalkSync.IsMoonwalking(pc.PlayerId))
        {
            __instance.ResetAnimState();

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlayerControl_HandleRpc_MoonwalkSync
{
    public static bool Prefix(PlayerControl __instance, byte callId, MessageReader reader)
    {
        if (callId != MoonwalkSync.RpcId) return true;

        MoonwalkSync.OnRpc(__instance.PlayerId, reader);
        return false;
    }
}

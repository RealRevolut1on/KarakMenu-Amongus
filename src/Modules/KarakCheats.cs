using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using InnerNet;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace KarakMenu;
public static class KarakCheats
{
    private static bool _isScanAnimActive;
    private static bool _isCamsAnimActive;

    public static void CloseMeetingCheat()
    {
        if (!CheatToggles.closeMeeting) return;

        if (Utils.isMeeting) // Closes MeetingHud window if it's open
        {

            // Destroy MeetingHud window gameobject
            MeetingHud.Instance.DespawnOnDestroy = false;
            Object.Destroy(MeetingHud.Instance.gameObject);

            // Gameplay must be reenabled
            DestroyableSingleton<HudManager>.Instance.StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f, false));
            PlayerControl.LocalPlayer.SetKillTimer(GameManager.Instance.LogicOptions.GetKillCooldown());
            ShipStatus.Instance.EmergencyCooldown = GameManager.Instance.LogicOptions.GetEmergencyCooldown();
            Camera.main.GetComponent<FollowerCamera>().Locked = false;
            DestroyableSingleton<HudManager>.Instance.SetMapAndInfoButtonsEnabled(true);
            DestroyableSingleton<HudManager>.Instance.SetHudActive(true);
            ControllerManager.Instance.CloseAndResetAll();

        }
        else if (ExileController.Instance) // Ends exile cutscene if it's playing
        {
            ExileController.Instance.ReEnableGameplay();
            ExileController.Instance.WrapUp();
        }

        CheatToggles.closeMeeting = false;
    }

    public static void SkipMeetingCheat()
    {
        if (!CheatToggles.skipMeeting) return;

        if (Utils.isMeeting)
        {
            MeetingHud.Instance.RpcVotingComplete(new Il2CppStructArray<MeetingHud.VoterState>(0L), null, true, false, 0);
        }

        CheatToggles.skipMeeting = false;
    }

    public static void CallMeetingCheat()
    {
        if (!CheatToggles.callMeeting) return;

        if (Utils.isHost)
        {
            // Same as PlayerControl.ReportDeadBody but without additional checks
            MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, null);
            DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
            PlayerControl.LocalPlayer.RpcStartMeeting(null);
        }
        else
        {
            PlayerControl.LocalPlayer.CmdReportDeadBody(null);
        }

        CheatToggles.callMeeting = false;
    }

    public static void ForceStartGameCheat()
    {
        if (!CheatToggles.forceStartGame) return;

        if (Utils.isHost && Utils.isLobby)
        {
            AmongUsClient.Instance.SendStartGame();
        }

        CheatToggles.forceStartGame = false;
    }

    public static void CompleteMyTasksCheat()
    {
        if (CheatToggles.completeMyTasks)
        {
            foreach (var task in PlayerControl.LocalPlayer.myTasks)
            {
                Utils.CompleteTask(task);
            }

            CheatToggles.completeMyTasks = false;
        }
    }

    public static void OpenSabotageMapCheat()
    {
        if (!CheatToggles.sabotageMap) return;

        DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions
        {
            Mode = MapOptions.Modes.Sabotage
        });

        CheatToggles.sabotageMap = false;
    }

    public static void HandleEngineerCheats(EngineerRole engineerRole)
    {
        if (CheatToggles.endlessVentTime)
        {
            // Makes vent time incredibly long (float.MaxValue) so that it never ends
            engineerRole.inVentTimeRemaining = float.MaxValue;
        }
        else if (engineerRole.inVentTimeRemaining > engineerRole.GetCooldown())
        {
            // Vent time is reset to normal value after the cheat is disabled
            engineerRole.inVentTimeRemaining = engineerRole.GetCooldown();
        }

        if (CheatToggles.noVentCooldown)
        {
            if (engineerRole.cooldownSecondsRemaining > 0f)
            {
                engineerRole.cooldownSecondsRemaining = 0f;

                DestroyableSingleton<HudManager>.Instance.AbilityButton.ResetCoolDown();
                DestroyableSingleton<HudManager>.Instance.AbilityButton.SetCooldownFill(0f);
            }
        }
    }

    public static void HandleShapeshifterCheats(ShapeshifterRole shapeshifterRole)
    {
        if (CheatToggles.endlessSsDuration)
        {
            // Makes shapeshift duration so incredibly long (float.MaxValue) so that it never ends
            shapeshifterRole.durationSecondsRemaining = float.MaxValue;
        }
        else if (shapeshifterRole.durationSecondsRemaining > GameManager.Instance.LogicOptions.GetRoleFloat(FloatOptionNames.ShapeshifterDuration))
        {
            // Shapeshift duration is reset to normal value after the cheat is disabled
            shapeshifterRole.durationSecondsRemaining = GameManager.Instance.LogicOptions.GetRoleFloat(FloatOptionNames.ShapeshifterDuration);

        }
    }

    public static void HandleScientistCheats(ScientistRole scientistRole)
    {
        if (CheatToggles.noVitalsCooldown)
        {
            scientistRole.currentCooldown = 0f;
        }

        if (CheatToggles.endlessBattery)
        {
            // Makes vitals battery so incredibly long (float.MaxValue) so that it never ends
            scientistRole.currentCharge = float.MaxValue;
        }
        else if (scientistRole.currentCharge > scientistRole.RoleCooldownValue)
        {
            // Battery charge is reset to normal value after the cheat is disabled
            scientistRole.currentCharge = scientistRole.RoleCooldownValue;
        }
    }

    public static void HandleTrackerCheats(TrackerRole trackerRole)
    {
        if (CheatToggles.noTrackingCooldown)
        {
            trackerRole.cooldownSecondsRemaining = 0f;
            trackerRole.delaySecondsRemaining = 0f;

            DestroyableSingleton<HudManager>.Instance.AbilityButton.ResetCoolDown();
            DestroyableSingleton<HudManager>.Instance.AbilityButton.SetCooldownFill(0f);
        }

        if (CheatToggles.noTrackingDelay && MapBehaviour.Instance != null)
        {
            MapBehaviour.Instance.trackedPointDelayTime = GameManager.Instance.LogicOptions.GetRoleFloat(FloatOptionNames.TrackerDelay);
        }

        if (CheatToggles.endlessTracking)
        {
            // Makes vitals battery so incredibly long (float.MaxValue) so that it never ends
            trackerRole.durationSecondsRemaining = float.MaxValue;
        }
        else if (trackerRole.durationSecondsRemaining > GameManager.Instance.LogicOptions.GetRoleFloat(FloatOptionNames.TrackerDuration))
        {
            // Battery charge is reset to normal value after the cheat is disabled
            trackerRole.durationSecondsRemaining = GameManager.Instance.LogicOptions.GetRoleFloat(FloatOptionNames.TrackerDuration);
        }
    }

    public static void UseVentCheat(HudManager hudManager)
    {
        // try-catch to prevent errors when role is null
        try
        {
            if (!Utils.isShip) return;

            // Engineers & impostors already have the vent button, so only other
            // roles are touched. Ghost venting breaks the vent state, so it is
            // left alone too.
            if (!PlayerControl.LocalPlayer.Data.Role.CanVent && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                hudManager.ImpostorVentButton.gameObject.SetActive(CheatToggles.unlockVents);
            }
        } catch { }
    }

    public static void WalkInVentCheat()
    {
        try
        {
            if (!CheatToggles.walkInVents) return;

            PlayerControl.LocalPlayer.inVent = false;
            PlayerControl.LocalPlayer.moveable = true;

        } catch { }
    }

    public static void KickVentsCheat()
    {
        if (!CheatToggles.kickVents) return;

        foreach(var vent in ShipStatus.Instance.AllVents)
        {
            VentilationSystem.Update(VentilationSystem.Operation.BootImpostors, vent.Id);
        }

        CheatToggles.kickVents = false;
    }

    public static void KillAllCheat()
    {
        if (!CheatToggles.killAll) return;

        if (Utils.isLobby)
        {
            HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
        }
        else
        {
            // Kill all players by sending a successful MurderPlayer RPC call
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                Utils.MurderPlayer(player, MurderResultFlags.Succeeded);
            }
        }

        CheatToggles.killAll = false;
    }

    public static void KillAllCrewCheat()
    {
        if (!CheatToggles.killAllCrew) return;

        if (Utils.isLobby)
        {
            HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
        }
        else
        {
            // Kill all players by sending a successful MurderPlayer RPC call
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.Role.TeamType == RoleTeamTypes.Crewmate)
                {
                    Utils.MurderPlayer(player, MurderResultFlags.Succeeded);
                }
            }
        }

        CheatToggles.killAllCrew = false;
    }

    public static void KillAllImpsCheat()
    {
        if (!CheatToggles.killAllImps) return;

        if (Utils.isLobby)
        {
            HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
        }
        else
        {
            // Kill all players by sending a successful MurderPlayer RPC call
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.Role.TeamType == RoleTeamTypes.Impostor)
                {
                    Utils.MurderPlayer(player, MurderResultFlags.Succeeded);
                }
            }
        }

        CheatToggles.killAllImps = false;
    }

    public static void ProtectCheat()
    {
        if (!Utils.isHost || Utils.isLobby) return;

        foreach (var player in ProtectUI.playersToProtect)
        {
            if (player.protectedByGuardianId == -1) // -1 means no protection is currently active
            {
                //PlayerControl.LocalPlayer.TurnOnProtection(true, PlayerControl.LocalPlayer.cosmetics.ColorId, PlayerControl.LocalPlayer.PlayerId);
                PlayerControl.LocalPlayer.RpcProtectPlayer(player, PlayerControl.LocalPlayer.cosmetics.ColorId);
            }
        }
    }

    public static void TeleportCursorCheat()
    {
        if (!CheatToggles.teleportCursor) return;

        // Works in the lobby too: RpcSnapTo is a normal net transform RPC and
        // the lobby is still a networked scene, so the only thing that used to
        // block this was the isShip gate. Skip meetings and the ejection
        // cutscene, where a snap would fight the game's own positioning.
        if (!Utils.isShip && !Utils.isLobby) return;
        if (Utils.isMeeting || Utils.isExiling) return;
        if (PlayerControl.LocalPlayer?.NetTransform == null || Camera.main == null) return;

        if (Input.GetMouseButtonDown(1))
        {
            var screenPos = Input.mousePosition;
            var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane + 10f));
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(worldPos.x, worldPos.y));
        }
    }

    public static void NoClipCheat()
    {
        try
        {

            PlayerControl.LocalPlayer.Collider.enabled = !(CheatToggles.noClip || PlayerControl.LocalPlayer.onLadder);

        } catch { }
    }

    public static void PlayScannerCheat()
    {
        if (CheatToggles.animMedScan && !_isScanAnimActive)
        {
            Utils.ForceSetScanner(PlayerControl.LocalPlayer, true);
            _isScanAnimActive = true;
        }
        else if (!CheatToggles.animMedScan && _isScanAnimActive)
        {
            Utils.ForceSetScanner(PlayerControl.LocalPlayer, false);
            _isScanAnimActive = false;
        }
    }

    public static void PlayAnimationCheat()
    {
        if (CheatToggles.animPet && Utils.isPlayer && PlayerControl.LocalPlayer.cosmetics != null && PlayerControl.LocalPlayer.cosmetics.CurrentPet != null)
        {
            // Don't move LocalPlayer, just send the RPC so others see the petting animation
            RpcPetMessage rpcMessage = new(PlayerControl.LocalPlayer.MyPhysics.NetId,
                PlayerControl.LocalPlayer.cosmetics.CurrentPet.PettingPlayerPosition,
                PlayerControl.LocalPlayer.cosmetics.CurrentPet.transform.position);
            AmongUsClient.Instance.LateBroadcastReliableMessage(Unsafe.As<IGameDataMessage>(rpcMessage));
        }

        byte mapId = Utils.GetCurrentMapID();

        if (mapId == byte.MaxValue) return;

        var map = (MapNames)mapId;

        if (CheatToggles.animShields)
        {
            if (map is MapNames.Skeld or MapNames.Dleks)
            {
                Utils.ForcePlayAnimation((byte)TaskTypes.PrimeShields);
            }
            CheatToggles.animShields = false;
        }

        if (CheatToggles.animAsteroids)
        {
            if (map is MapNames.Skeld or MapNames.Dleks or MapNames.Polus)
            {
                Utils.ForcePlayAnimation((byte)TaskTypes.ClearAsteroids);
            }
            else
            {
                CheatToggles.animAsteroids = false;
            }
        }

        if (CheatToggles.animEmptyGarbage)
        {
            if (map is MapNames.Skeld or MapNames.Dleks)
            {
                Utils.ForcePlayAnimation((byte)TaskTypes.EmptyGarbage);
            }

            CheatToggles.animEmptyGarbage = false;
        }

        if (map is not (MapNames.MiraHQ or MapNames.Fungle))
        {
            if (CheatToggles.animCamsInUse && !_isCamsAnimActive)
            {
                // ShipStatus.Instance.UpdateSystem(SystemTypes.Security, PlayerControl.LocalPlayer, (byte)(CheatToggles.animCamsInUse ? 1 : 0));
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Security, 1);
                _isCamsAnimActive = true;
            }
            else if (!CheatToggles.animCamsInUse && _isCamsAnimActive)
            {
                // Turn off cams if the cheat was used before and is now disabled
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Security, 0);
                _isCamsAnimActive = false;
            }
        }
        else
        {
            CheatToggles.animCamsInUse = false;
        }
    }

    public static void StopShipAnimCheats()
    {
        CheatToggles.animShields = false;
        CheatToggles.animAsteroids = false;
        CheatToggles.animEmptyGarbage = false;
        CheatToggles.animMedScan = false;
        CheatToggles.animCamsInUse = false;

        // This ensures cams and scan animations don't remain marked as active if the player
        // disconnects while the toggles are on (as this may cause unusual RPCs in lobbies)

        _isCamsAnimActive = false;
        _isScanAnimActive = false;
    }

    // Flash Kill (one-shot): teleport to the nearest living player, kill them,
    // then teleport back to where we started. Triggered from the menu button so
    // it works from any distance — unlike the game's kill button, which only
    // enables when a target is already within kill range.
    public static void FlashKillCheat()
    {
        if (!CheatToggles.flashKill) return;

        CheatToggles.flashKill = false; // One-shot

        if (!Utils.isPlayer || PlayerControl.LocalPlayer == null) return;

        if (Utils.isLobby)
        {
            HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
            return;
        }

        // Nearest VALID target: ask our own role what it can kill. This
        // automatically skips fellow impostors, dead players, disconnected
        // players, and anyone our role otherwise can't target.
        var myRole = PlayerControl.LocalPlayer.Data?.Role;
        var victim = Utils.GetPlayersSortedByDistance()
            .Where(player => player != null
                && player.Data != null
                && player != PlayerControl.LocalPlayer
                && !player.Data.IsDead
                && !player.Data.Disconnected
                && (myRole == null || myRole.IsValidTarget(player.Data)))
            .FirstOrDefault();

        if (victim == null) return;

        var oldPos = PlayerControl.LocalPlayer.GetTruePosition();
        AmongUsClient.Instance.StartCoroutine(FlashKillRoutine(victim, oldPos));
    }

    // Flash kill via the host-authority path. The previous implementation
    // hand-broadcast RpcCalls.MurderPlayer to every client itself, impersonating
    // the host's own kill packet — that signature is exactly what got us kicked
    // the instant the kill went out. Going through CmdCheckMurder lets the real
    // host validate and rebroadcast the kill instead, and because we snap next
    // to the victim first the range check passes legitimately.
    private static System.Collections.IEnumerator FlashKillRoutine(PlayerControl victim, Vector2 oldPos)
    {
        var me = PlayerControl.LocalPlayer;
        if (me == null || victim == null) yield break;

        me.NetTransform.RpcSnapTo(victim.GetTruePosition());

        // Give the host a couple of frames to receive our new position before
        // asking it to approve the kill, otherwise the range check still fails.
        yield return null;
        yield return null;

        if (me != null && victim != null && !victim.Data.IsDead)
            me.CmdCheckMurder(victim);

        yield return new WaitForSeconds(0.2f);
        if (me != null)
            me.NetTransform.RpcSnapTo(oldPos);
    }

    private static readonly System.Collections.Generic.Dictionary<byte, float> _speedHackBase = new();
    private static float _lastSpeedSynced = 1f;

    // Host-only, gated by CheatToggles.speedHack (previously an unused/dead
    // field — this ran unconditionally before, which is exactly why turning
    // off Custom Speed didn't actually restore normal movement: this kept
    // silently reasserting a leftover multiplier every single frame with no
    // way to disable it). The PlayerSpeedMod option is changed on the host
    // and re-broadcast through LogicOptions.SyncOptions — the game's own
    // host-settings sync path — so every client recomputes speeds from the
    // new option. Clamped to vanilla-legal range: going past it gets you
    // instantly kicked by the official server's own validation, confirmed
    // via live testing — noOptionsLimits does not and cannot bypass that,
    // it only suppresses our own client-side RpcSyncSettings guard.
    public static void SpeedHackCheat()
    {
        if (!Utils.isHost) return;

        if (!CheatToggles.speedHack)
        {
            // Restore neutral (1x) so a previously-enabled multiplier doesn't linger
            if (Mathf.Abs(1f - _lastSpeedSynced) > 0.001f)
            {
                _lastSpeedSynced = 1f;
                try
                {
                    if (GameOptionsManager.Instance != null)
                        GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, 1f);
                    if (GameManager.Instance != null && GameManager.Instance.LogicOptions != null)
                        GameManager.Instance.LogicOptions.SyncOptions();
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"SpeedHackCheat reset sync: {e}");
                }
            }
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.MyPhysics == null || player.Data == null) continue;
                if (player.AmOwner && CheatToggles.customSpeed) continue;
                if (_speedHackBase.TryGetValue(player.PlayerId, out var b) && b > 0f)
                    player.MyPhysics.Speed = b;
            }
            return;
        }

        float mult = Mathf.Clamp(CheatToggles.speedHackMultiplier, MinPlayerSpeedMod, MaxPlayerSpeedMod);

        // Push the networked game option (only when the value actually
        // changes, so we don't spam option RPCs every frame)
        if (Mathf.Abs(mult - _lastSpeedSynced) > 0.001f)
        {
            _lastSpeedSynced = mult;
            try
            {
                if (GameOptionsManager.Instance != null)
                {
                    GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, mult);
                }
                if (GameManager.Instance != null && GameManager.Instance.LogicOptions != null)
                {
                    GameManager.Instance.LogicOptions.SyncOptions();
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"SpeedHackCheat sync: {e}");
            }
        }

        // Apply live to every player's physics (host authoritative). Skips the
        // local player while Custom Speed is active so the two features don't
        // fight over the raw Speed field — not that it matters for the actual
        // movement result (see PlayerPhysics_TrueSpeed in PlayerPhysicsPatches.cs),
        // but there's no reason to have them both writing to it either.
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.MyPhysics == null || player.Data == null) continue;
            if (player.AmOwner && CheatToggles.customSpeed) continue;
            if (!_speedHackBase.TryGetValue(player.PlayerId, out var basis) || basis <= 0f)
            {
                basis = player.MyPhysics.Speed;
                if (basis <= 0f) basis = 1f;
                _speedHackBase[player.PlayerId] = basis;
            }
            player.MyPhysics.Speed = basis * mult;
        }
    }

    // Host-only: swaps the Skeld map prefab for the game's own built-in
    // mirrored "Dleks" prefab (index 3), so hosting a Skeld game actually
    // spawns a real flipped map instead of hacking transform scale at
    // runtime. Swap on toggle-edge only, not per-frame.
    private static bool _mapPrefabsSwapped;

    public static void SetFlipMap(bool enabled)
    {
        if (AmongUsClient.Instance == null || _mapPrefabsSwapped == enabled) return;
        var prefabs = AmongUsClient.Instance.ShipPrefabs;
        if (prefabs == null || prefabs.Count <= 3) return;
        (prefabs[0], prefabs[3]) = (prefabs[3], prefabs[0]);
        _mapPrefabsSwapped = enabled;
    }

    // Host-only, in-match-only: makes every other player actually shapeshift
    // into the LocalPlayer, on top of matching their cosmetics. Base machinery
    // ported from OnyxMenu's proven "morph everyone" RPCs (NocturneLobbyPranks.
    // MassMorph/ApplyOnce + NocturneColorAll):
    //   - IN MATCH: everyone is first set to the Shapeshifter role (anticheat lobbies
    //     only), then 0.5s later each player gets RpcRejectShapeshift + RpcSetColor/
    //     RpcSetHat/RpcSetSkin/RpcSetVisor with the host's outfit, then an
    //     RpcShapeshift(LocalPlayer, true) so every client sees them actually turn
    //     into the host (not just recolor) — roles are restored 0.5s after. Onyx
    //     never sends SetPet/SetNamePlate for other players (they may be
    //     server-validated like SetName), so they are skipped here too.
    //   - IN LOBBY: shapeshifting doesn't exist pre-match, so only colors can be
    //     forced — colors are pushed one tick per 0.5s (NocturneColorAll pattern).
    public static void CopyOutfitToAllCheat()
    {
        if (!CheatToggles.copyOutfitToAll) return;

        CheatToggles.copyOutfitToAll = false; // One-shot

        if (!Utils.isHost || PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return;

        var outfit = PlayerControl.LocalPlayer.CurrentOutfit;
        if (outfit == null) return;

        if (ShipStatus.Instance != null && LobbyBehaviour.Instance == null)
            AmongUsClient.Instance.StartCoroutine(CoApplyOutfitToAll(outfit));
        else
            AmongUsClient.Instance.StartCoroutine(CoApplyLobbyColors((byte)outfit.ColorId));
    }

    private static bool IsValidTarget(PlayerControl pc) => pc != null && pc.Data != null && !pc.Data.Disconnected;

    private static bool HasAnticheat()
    {
        try { return AmongUsClient.Instance != null && (int)AmongUsClient.Instance.NetworkMode == 1; } catch { return true; }
    }

    private static System.Collections.IEnumerator CoApplyOutfitToAll(NetworkedPlayerInfo.PlayerOutfit outfit)
    {
        if (PlayerControl.AllPlayerControls == null) yield break;

        bool ac = HasAnticheat();
        var roles = new Dictionary<byte, RoleTypes>();

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (!IsValidTarget(pc)) continue;
            roles[pc.PlayerId] = pc.Data.RoleType;
            if (ac && pc.Data.RoleType != RoleTypes.Shapeshifter)
            {
                try { pc.RpcSetRole(RoleTypes.Shapeshifter, false); }
                catch { try { pc.RpcSetRole(RoleTypes.Shapeshifter); } catch { } }
            }
        }

        if (ac) yield return new WaitForSeconds(0.5f);

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (!IsValidTarget(pc)) continue;

            if (pc.PlayerId != PlayerControl.LocalPlayer.PlayerId)
            {
                try { pc.RpcRejectShapeshift(); } catch { }
            }

            try { pc.RpcSetColor((byte)outfit.ColorId); } catch { }
            try { pc.RpcSetHat(outfit.HatId); } catch { }
            try { pc.RpcSetSkin(outfit.SkinId); } catch { }
            try { pc.RpcSetVisor(outfit.VisorId); } catch { }

            // Shapeshift each OTHER player into the local player (not a
            // self-pulse) so they actually appear to have transformed into
            // you, on top of already matching your cosmetics above.
            if (pc.PlayerId != PlayerControl.LocalPlayer.PlayerId)
            {
                try { pc.RpcShapeshift(PlayerControl.LocalPlayer, true); } catch { }
            }
        }

        if (ac) yield return new WaitForSeconds(0.5f);

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (!IsValidTarget(pc)) continue;
            if (ac && roles.TryGetValue(pc.PlayerId, out RoleTypes r))
            {
                try { pc.RpcSetRole(r, false); }
                catch { try { pc.RpcSetRole(r); } catch { } }
            }
        }
    }

    private static System.Collections.IEnumerator CoApplyLobbyColors(byte col)
    {
        for (int ticks = 0; ticks < 20; ticks++)
        {
            bool any = false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!IsValidTarget(pc)) continue;
                if (pc.Data.DefaultOutfit != null && pc.Data.DefaultOutfit.ColorId == col) continue;
                any = true;
                try { pc.RpcSetColor(col); } catch { }
            }
            if (!any) yield break;
            yield return new WaitForSeconds(0.5f);
        }
    }

    // ---------------------------------------------------------------------
    // Closest-killable targeting
    // ---------------------------------------------------------------------

    // Auto-Kill: every 0.25s, murder the nearest player our role considers a
    // valid target. Same nearest-target logic as FlashKillCheat, but continuous.
    private static float _autoKillTimer;
    public static void AutoKillCheat()
    {
        if (!CheatToggles.autoKill || !Utils.isPlayer || PlayerControl.LocalPlayer == null) return;
        if (Utils.isLobby) return;

        _autoKillTimer -= Time.deltaTime;
        if (_autoKillTimer > 0f) return;
        _autoKillTimer = 0.25f;

        var myRole = PlayerControl.LocalPlayer.Data?.Role;
        var victim = Utils.GetPlayersSortedByDistance()
            .Where(p => p != null && p.Data != null && p != PlayerControl.LocalPlayer
                        && !p.Data.IsDead && !p.Data.Disconnected
                        && (myRole == null || myRole.IsValidTarget(p.Data)))
            .FirstOrDefault();
        if (victim == null) return;

        Utils.MurderPlayer(victim, MurderResultFlags.Succeeded);
    }

    // Invisibility: TP the LocalPlayer far off-map and lock movement so
    // other clients see them frozen in the void. Off-toggle restores position.
    private static bool _isInvisibleActive;
    private static Vector2 _invisibilityOriginalPosition;
    public static void InvisibilityCheat()
    {
        if (!Utils.isPlayer || PlayerControl.LocalPlayer == null) return;
        try
        {
            if (CheatToggles.invisibility)
            {
                if (_isInvisibleActive) return;
                _invisibilityOriginalPosition = PlayerControl.LocalPlayer.GetTruePosition();
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(999f, 999f));
                PlayerControl.LocalPlayer.moveable = false;
                _isInvisibleActive = true;
            }
            else if (_isInvisibleActive)
            {
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(_invisibilityOriginalPosition);
                PlayerControl.LocalPlayer.moveable = true;
                _isInvisibleActive = false;
            }
        }
        catch { }
    }

    // Vent-TP internals: host uses RpcBootFromVent directly.
    // Non-host spoofs the enter/exit vent RPC pair over UpdateSystem, addressed
    // only to the host, so the target player appears to enter and immediately
    // exit the target vent — snapping them to it in the process.
    private static readonly Dictionary<byte, ushort> _ventSeqIds = new();

    private static void SendUpdateSystemToClient(SystemTypes systemType, uint senderNetId, byte[] extraBytes, int targetClientId)
    {
        var writer = AmongUsClient.Instance.StartRpcImmediately(
            ShipStatus.Instance.NetId, (byte)35 /* RpcCalls.UpdateSystem */, SendOption.Reliable, targetClientId);
        writer.Write((byte)(int)systemType);
        writer.WritePacked(senderNetId);
        foreach (var b in extraBytes) writer.Write(b);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    private static void SendVentPair(PlayerControl target, int ventId, int toClientId, ushort seqId)
    {
        byte lo1 = (byte)(seqId & 0xFF), hi1 = (byte)(seqId >> 8);
        ushort seq2 = (ushort)(seqId + 1);
        byte lo2 = (byte)(seq2 & 0xFF), hi2 = (byte)(seq2 >> 8);
        // System 37 = Ventilation; opcode 2 = enter, 5 = exit
        SendUpdateSystemToClient((SystemTypes)37, target.NetId, new byte[] { lo1, hi1, 2, (byte)(ventId & 0xFF) }, toClientId);
        SendUpdateSystemToClient((SystemTypes)37, target.NetId, new byte[] { lo2, hi2, 5, (byte)(ventId & 0xFF) }, toClientId);
    }

    public static void VentTP(PlayerControl target, int ventId)
    {
        if (target == null || target.AmOwner || target.Data == null) return;
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmConnected || ShipStatus.Instance == null) return;

        if (Utils.isHost)
        {
            try { target.MyPhysics?.RpcBootFromVent(ventId); } catch { }
            return;
        }

        var pid = target.Data.PlayerId;
        if (!_ventSeqIds.TryGetValue(pid, out var seq)) seq = 1000;
        SendVentPair(target, ventId, AmongUsClient.Instance.HostId, seq);
        _ventSeqIds[pid] = (ushort)(seq + 2);
    }

    // Best-effort kick fallback for non-host clients — the vent-system trick.
    // Re-sends the same enter+exit vent packet pair VentTP
    // uses to move a player, but addressed DIRECTLY to the target's own client
    // instead of the host, abusing lobbies that don't verify the sender of a
    // SystemTypes.Ventilation update before processing it. This is fundamentally
    // an exploit, not a real kick API — it only works against certain lobby
    // configurations and many anticheats explicitly detect/block it (our
    // own VentTP fallback is exactly this same trick used defensively, i.e. moving
    // a player). Always prefer AmongUsClient.KickPlayer when host; this exists
    // purely so non-host users have *something* to try. Returns false immediately
    // (no attempt made) if the target is the host, since kicking the host this
    // way is not possible.
    public static bool TryNonHostKick(PlayerControl target)
    {
        if (target == null || target.AmOwner || target.Data == null) return false;
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmConnected || ShipStatus.Instance == null) return false;

        int targetClientId = AmongUsClient.Instance.GetClientIdFromCharacter(target);
        if (targetClientId < 0 || targetClientId == AmongUsClient.Instance.HostId) return false;

        var pid = target.Data.PlayerId;
        if (!_ventSeqIds.TryGetValue(pid, out var seq)) seq = 1000;
        SendVentPair(target, 0, targetClientId, seq);
        _ventSeqIds[pid] = (ushort)(seq + 2);
        return true;
    }

    // Spam-TP: every ~1s (imps) / ~2s (all), TP each eligible player to a
    // random vent. Skips self / dead / disconnected.
    private static float _spamTpImpsTimer = -999f;
    private static float _spamTpAllTimer = -999f;

    private static List<Vent> ShuffledVents()
    {
        var list = GetVents();
        if (list == null) return null;
        for (int n = list.Count - 1; n > 0; n--)
        {
            int k = Random.Range(0, n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
        return list;
    }

    // Stable-order vent list (unshuffled) — used wherever a UI index must
    // consistently map back to the same vent frame-to-frame, e.g. the vent
    // picker Combo shared by the teleport/jail features.
    public static List<Vent> GetVents()
    {
        var vents = ShipStatus.Instance?.AllVents;
        if (vents == null || vents.Count == 0) return null;
        var list = new List<Vent>();
        foreach (var v in vents) if (v != null) list.Add(v);
        return list.Count == 0 ? null : list;
    }

    // The game assigns Vent.Id arbitrarily based on the map's vent network —
    // not sequentially, so stepping through a list by raw ID jumps around
    // (0, 2, 11, ...) and gives the user nothing to orient by. The room name
    // is what a player actually recognizes, so resolve that instead via the
    // same room-lookup already used for kill/shapeshift logging.
    //
    // Some rooms (Navigation on Skeld/Airship) have more than one vent, and a
    // bare room name can't tell those apart — both just say "Nav". When that
    // happens, disambiguate by vertical position among the vents sharing that
    // room: the higher one is "<Room> Up", the lower one "<Room> Down".
    public static string GetVentLabel(Vent vent)
    {
        if (vent == null) return "Vent";
        var room = Utils.GetRoomFromPosition(vent.transform.position);
        if (room == null) return $"Vent {vent.Id}";

        var sameRoom = GetVents()?.Where(v => v != null && Utils.GetRoomFromPosition(v.transform.position) == room).ToList();
        if (sameRoom != null && sameRoom.Count > 1)
        {
            sameRoom.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));
            int idx = sameRoom.IndexOf(vent);
            if (idx == 0) return $"{room.RoomId} Up";
            if (idx == sameRoom.Count - 1) return $"{room.RoomId} Down";
            return $"{room.RoomId} {idx + 1}";
        }

        return room.RoomId.ToString();
    }

    // Jail Player's fixed "cell" — the higher of the (up to two) vents inside
    // the Navigation room, resolved dynamically instead of hardcoded per map.
    // Returns null on maps without a Navigation room (Polus, Fungle, MIRA HQ).
    public static Vent GetJailVent()
    {
        var vents = GetVents();
        if (vents == null) return null;

        Vent best = null;
        foreach (var v in vents)
        {
            var room = Utils.GetRoomFromPosition(v.transform.position);
            if (room == null || room.RoomId != SystemTypes.Nav) continue;
            if (best == null || v.transform.position.y > best.transform.position.y) best = v;
        }
        return best;
    }

    // Snaps the LOCAL player into a vent directly (no RPC spoofing needed —
    // VentTP is for moving OTHER players and explicitly refuses self as a target).
    public static void SelfVentTP(Vent vent)
    {
        if (vent == null || PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.NetTransform == null) return;
        try { PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(vent.transform.position); } catch { }
    }

    // Host-aware snap for an OTHER player into a specific vent — used by Jail
    // (KarakPlayersCheats.JailTick). As host, target.NetTransform.RpcSnapTo
    // directly — the host has legitimate authority to move any player, and
    // this is instant/reliable. As non-host, directly commanding ANOTHER
    // player's NetTransform is not something a normal client ever does, and
    // gets flagged/kicked by the official server's anti-cheat (confirmed via
    // live testing) — VentTP's spoofed enter/exit vent pair is safer there,
    // mimicking a legitimate self-initiated vent action instead.
    public static void HostAwareVentSnap(PlayerControl target, Vent vent)
    {
        if (Utils.isHost) target.NetTransform.RpcSnapTo(vent.transform.position);
        else VentTP(target, vent.Id);
    }

    public static void SpamTpImpsCheat()
    {
        if (!CheatToggles.spamTpImps || ShipStatus.Instance == null || Utils.isLobby) return;
        if (Time.realtimeSinceStartup - _spamTpImpsTimer < 1f) return;
        _spamTpImpsTimer = Time.realtimeSinceStartup;

        var vents = ShuffledVents();
        if (vents == null) return;

        var targets = new List<PlayerControl>();
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p.AmOwner || p.Data == null || p.Data.Disconnected || p.Data.IsDead) continue;
            if (p.Data.Role != null && p.Data.Role.IsImpostor) targets.Add(p);
        }
        int n = Mathf.Min(targets.Count, vents.Count);
        for (int i = 0; i < n; i++) if (vents[i] != null) VentTP(targets[i], vents[i].Id);
    }

    public static void SpamTpAllCheat()
    {
        if (!CheatToggles.spamTpAll || ShipStatus.Instance == null || Utils.isLobby) return;
        if (Time.realtimeSinceStartup - _spamTpAllTimer < 2f) return;
        _spamTpAllTimer = Time.realtimeSinceStartup;

        var vents = ShuffledVents();
        if (vents == null) return;

        var targets = new List<PlayerControl>();
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p.Data == null || p.Data.Disconnected || p.Data.IsDead) continue;
            if (p.AmOwner)
            {
                if (CheatToggles.teleportAllIncludeSelf) SelfVentTP(vents[0]);
                continue;
            }
            targets.Add(p);
        }
        int n = Mathf.Min(targets.Count, vents.Count);
        for (int i = 0; i < n; i++) if (vents[i] != null) VentTP(targets[i], vents[i].Id);
    }

    // Teleports everyone to a chosen vent (CheatToggles.selectedVentIndex, falling
    // back to the first vent if the index is out of range) and optionally includes
    // the local player via a direct self-snap.
    public static void TeleportAllToVentCheat()
    {
        if (!CheatToggles.teleportAllToVent) return;
        CheatToggles.teleportAllToVent = false; // One-shot
        if (ShipStatus.Instance == null || Utils.isLobby) return;
        var vents = GetVents();
        if (vents == null) return;
        int idx = Mathf.Clamp(CheatToggles.selectedVentIndex, 0, vents.Count - 1);
        var target = vents[idx];
        if (target == null) return;

        if (CheatToggles.teleportAllIncludeSelf) SelfVentTP(target);

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p.AmOwner || p.Data == null || p.Data.Disconnected || p.Data.IsDead) continue;
            VentTP(p, target.Id);
        }
    }

    // Door Hallucination: send a fake "close" UpdateSystem RPC for every room's
    // doors to a single target client. They see doors slam shut for no reason —
    // no one else does, because the RPC is addressed only to them.
    private static bool _doorHallucinationAllSent;

    private static void HallucinateDoorsFor(PlayerControl target)
    {
        if (target == null || ShipStatus.Instance == null || AmongUsClient.Instance == null || !AmongUsClient.Instance.AmConnected) return;
        int targetClientId = AmongUsClient.Instance.GetClientIdFromCharacter(target);
        if (targetClientId < 0) return;

        var rooms = new HashSet<SystemTypes>();
        var doors = ShipStatus.Instance.AllDoors;
        for (int i = 0; i < doors.Length; i++) if (doors[i] != null) rooms.Add(doors[i].Room);
        if (rooms.Count == 0) return;

        foreach (var room in rooms)
        {
            // RPC 27 = CloseDoorsOfType on ShipStatus
            var writer = AmongUsClient.Instance.StartRpcImmediately(
                ShipStatus.Instance.NetId, (byte)27, SendOption.Reliable, targetClientId);
            writer.Write((byte)(int)room);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }

    public static void DoorHallucinationCheat()
    {
        if (!CheatToggles.doorHallucination) return;
        CheatToggles.doorHallucination = false; // One-shot; PPM version handles target selection
    }

    public static void DoorHallucinationAllCheat()
    {
        if (!CheatToggles.doorHallucinationAll)
        {
            _doorHallucinationAllSent = false;
            return;
        }
        if (_doorHallucinationAllSent) return;
        _doorHallucinationAllSent = true;
        if (ShipStatus.Instance == null)
        {
            HudManager.Instance?.Notifier?.AddDisconnectMessage("Door Hallucination failed: game not started.");
            return;
        }
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p.AmOwner || p.Data == null || p.Data.Disconnected) continue;
            HallucinateDoorsFor(p);
        }
    }

    // Confuse Now: randomize the LocalPlayer's cosmetics (self-only). Cosmetic
    // The IDs below are just game asset names.
    private static readonly string[] ConfuseHats = { "hat_pk05_Fedora", "hat_pk03_Headphones", "hat_screamghostface", "hat_pk08_Cowboy", "hat_pk07_Bandana", "hat_pk01_Beanie", "hat_pk02_PirateHat" };
    private static readonly string[] ConfuseVisors = { "visor_animesunglassesVisor", "visor_D2CGoggles", "visor_pk01_PaperMaskVisor", "visor_Scar", "visor_eliksni" };
    private static readonly string[] ConfuseSkins = { "skin_Hazmat-Greenskin", "skin_Science", "skin_Mech", "skin_SuitW", "skin_rhm", "skin_screamghostface" };

    public static void ConfuseNowCheat()
    {
        if (!CheatToggles.confuseNow) return;
        CheatToggles.confuseNow = false; // One-shot
        if (!Utils.isPlayer || PlayerControl.LocalPlayer == null) return;
        var lp = PlayerControl.LocalPlayer;
        try { lp.RpcSetHat(ConfuseHats[Random.Range(0, ConfuseHats.Length)]); } catch { }
        try { lp.RpcSetVisor(ConfuseVisors[Random.Range(0, ConfuseVisors.Length)]); } catch { }
        try { lp.RpcSetSkin(ConfuseSkins[Random.Range(0, ConfuseSkins.Length)]); } catch { }
    }

    // ---------------------------------------------------------------------
    // Host powers
    // ---------------------------------------------------------------------

    public static void ForceImpostorAllCheat()
    {
        if (!CheatToggles.forceImpostorAll) return;
        CheatToggles.forceImpostorAll = false; // One-shot
        if (!Utils.isHost) return;
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null) continue;
            try { RoleManager.Instance.SetRole(p, RoleTypes.Impostor); } catch { }
        }
    }

    // Same/Cycle/Disco color — all funnel into the existing CoApplyLobbyColors
    // coroutine which already retries against server-side dedupe rejection.
    private static int _lobbyColorId;
    private static float _lobbyDiscoTimer;

    public static void SameColorEveryoneCheat()
    {
        if (!CheatToggles.sameColorEveryone) return;
        CheatToggles.sameColorEveryone = false; // One-shot
        if (!Utils.isHost || PlayerControl.LocalPlayer?.CurrentOutfit == null) return;
        byte col = (byte)PlayerControl.LocalPlayer.CurrentOutfit.ColorId;
        _lobbyColorId = col;
        AmongUsClient.Instance.StartCoroutine(CoApplyLobbyColors(col));
    }

    public static void CycleLobbyColorCheat()
    {
        if (!CheatToggles.cycleLobbyColor) return;
        CheatToggles.cycleLobbyColor = false; // One-shot
        if (!Utils.isHost) return;
        int max = Palette.PlayerColors.Length;
        if (max <= 0) return;
        _lobbyColorId = (_lobbyColorId + 1) % max;
        AmongUsClient.Instance.StartCoroutine(CoApplyLobbyColors((byte)_lobbyColorId));
    }

    public static void LobbyDiscoCheat()
    {
        if (!CheatToggles.lobbyDiscoMode || !Utils.isHost) return;
        if (Time.time - _lobbyDiscoTimer < CheatToggles.lobbyDiscoSpeed) return;
        int max = Palette.PlayerColors.Length;
        if (max <= 0) return;
        _lobbyDiscoTimer = Time.time;
        AmongUsClient.Instance.StartCoroutine(CoApplyLobbyColors((byte)Random.Range(0, max)));
    }

    // Numeric +/- for impostor count and player speed. Both bypass the vanilla
    // range clamp by forcing noOptionsLimits on.
    // Reads the game's OWN adjusted-impostor-count calculation (the same
    // extension method IGameOptionsExtensions_GetAdjustedNumImpostors patches
    // for noOptionsLimits) rather than reimplementing AU's player-count clamp
    // rules by hand — so this always matches what the game will actually do.
    public static int GetNextRoundImpostorCount()
    {
        // GetAdjustedNumImpostors throws internally (NullReferenceException,
        // logged by Il2CppInterop even though this try/catch prevents a crash)
        // when called outside an active lobby/match — its own internal state
        // isn't ready yet. Gating on isLobby/isShip avoids calling it at all
        // in the one context that was flooding the console every frame the
        // Host-Only tab happened to be open (e.g. sitting at the main menu).
        if (!Utils.isLobby && !Utils.isShip) return 0;
        try
        {
            var opts = GameOptionsManager.Instance?.CurrentGameOptions;
            if (opts == null) return 0;

            int connectedCount = 0;
            foreach (var p in PlayerControl.AllPlayerControls)
                if (p != null && p.Data != null && !p.Data.Disconnected) connectedCount++;

            return opts.GetAdjustedNumImpostors(connectedCount);
        }
        catch { return 0; }
    }

    // Currently-alive vs total impostors for the ACTIVE round — distinct from
    // GetNextRoundImpostorCount above, which predicts what the NEXT round will
    // get assigned and says nothing about who's still alive in this one.
    public static (int alive, int total) GetAliveImpostorCount()
    {
        int alive = 0, total = 0;
        if (!Utils.isShip) return (0, 0);
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p.Data == null || p.Data.Disconnected || p.Data.Role == null || !p.Data.Role.IsImpostor) continue;
            total++;
            if (!p.Data.IsDead) alive++;
        }
        return (alive, total);
    }

    // Vanilla-legal bounds for lobby-wide options. Values outside these ranges
    // get flagged and instantly kicked ("Hacking") by the OFFICIAL Innersloth
    // servers' own server-side validation — noOptionsLimits only suppresses
    // OUR client-side RpcSyncSettings guard, it does nothing to what the real
    // server independently checks. Confirmed via live testing.
    //
    // MaxPlayerSpeedMod is the LIVE session ceiling for the slider/SpeedHackCheat
    // — deliberately above the real 3x server cap, so users who want it (private
    // servers, local/free-play, or accepting the official-server kick risk) can
    // still push past 3x. ServerSafePlayerSpeedMod is a SEPARATE, lower ceiling
    // used only by ResetOptionLimits: whatever multiplier was left active gets
    // clamped back down to it on return to the Main Menu, so an illegal value
    // can't silently poison the NEXT game's creation on official servers (this
    // is what caused every subsequent "Create Game" to get instant-kicked
    // before ResetOptionLimits existed — see its own comment below).
    private const int MinImpostors = 1;
    private const int MaxImpostors = 3;
    private const int MinSeekers = 1;
    private const int MaxSeekers = 12;
    private const float MinPlayerSpeedMod = 0.5f;
    private const float MaxPlayerSpeedMod = 10f;
    private const float ServerSafePlayerSpeedMod = 3f;

    // Hide N Seek uses a completely separate IGameOptions object
    // (currentHideNSeekGameOptions) from Classic/Normal — writing to
    // currentNormalGameOptions.NumImpostors while in HnS silently affects
    // options that aren't even the active ones. Both classes expose the same
    // NumImpostors property (the game just relabels it "Seekers" in HnS's own
    // UI), so this stepper doubles as both, targeting whichever is live and
    // clamping to that mode's own range (HnS gets a much higher ceiling since
    // it isn't subject to the vanilla 1-3 Impostor anti-cheat validation).
    public static void IncreaseImpostorsCheat()
    {
        if (!CheatToggles.increaseImpostors) return;
        CheatToggles.increaseImpostors = false;
        if (!Utils.isHost) return;
        if (Utils.isHideNSeek)
        {
            var opts = GameOptionsManager.Instance?.currentHideNSeekGameOptions;
            if (opts != null) opts.NumImpostors = Mathf.Clamp(opts.NumImpostors + 1, MinSeekers, MaxSeekers);
        }
        else
        {
            var opts = GameOptionsManager.Instance?.CurrentGameOptions?.TryCast<NormalGameOptionsV10>();
            if (opts != null) opts.NumImpostors = Mathf.Clamp(opts.NumImpostors + 1, MinImpostors, MaxImpostors);
        }
    }
    public static void DecreaseImpostorsCheat()
    {
        if (!CheatToggles.decreaseImpostors) return;
        CheatToggles.decreaseImpostors = false;
        if (!Utils.isHost) return;
        if (Utils.isHideNSeek)
        {
            var opts = GameOptionsManager.Instance?.currentHideNSeekGameOptions;
            if (opts != null) opts.NumImpostors = Mathf.Clamp(opts.NumImpostors - 1, MinSeekers, MaxSeekers);
        }
        else
        {
            var opts = GameOptionsManager.Instance?.CurrentGameOptions?.TryCast<NormalGameOptionsV10>();
            if (opts != null) opts.NumImpostors = Mathf.Clamp(opts.NumImpostors - 1, MinImpostors, MaxImpostors);
        }
    }
    public static void IncreasePlayerSpeedCheat()
    {
        if (!CheatToggles.increasePlayerSpeed) return;
        CheatToggles.increasePlayerSpeed = false;
        if (!Utils.isHost) return;
        var opts = GameOptionsManager.Instance?.CurrentGameOptions?.TryCast<NormalGameOptionsV10>();
        if (opts != null) opts.PlayerSpeedMod = Mathf.Clamp(opts.PlayerSpeedMod + 0.25f, MinPlayerSpeedMod, MaxPlayerSpeedMod);
    }
    public static void DecreasePlayerSpeedCheat()
    {
        if (!CheatToggles.decreasePlayerSpeed) return;
        CheatToggles.decreasePlayerSpeed = false;
        if (!Utils.isHost) return;
        var opts = GameOptionsManager.Instance?.CurrentGameOptions?.TryCast<NormalGameOptionsV10>();
        if (opts != null) opts.PlayerSpeedMod = Mathf.Clamp(opts.PlayerSpeedMod - 0.25f, MinPlayerSpeedMod, MaxPlayerSpeedMod);
    }

    // Clears any stale noOptionsLimits/out-of-range option state left over
    // from a previous match, so it can't poison the NEXT game's creation —
    // GameOptionsManager's instance persists across matches within the same
    // process, so without this, one earlier excursion past vanilla limits
    // would silently doom every subsequent "Create Game" to an instant
    // server-side "Hacking" kick, with no in-game feedback about why.
    public static void ResetOptionLimits()
    {
        CheatToggles.noOptionsLimits = false;
        var opts = GameOptionsManager.Instance?.CurrentGameOptions?.TryCast<NormalGameOptionsV10>();
        if (opts == null) return;
        opts.NumImpostors = Mathf.Clamp(opts.NumImpostors, MinImpostors, MaxImpostors);
        // Clamped to the real server-safe ceiling here (not MaxPlayerSpeedMod,
        // the higher live-session cap) — this is what stops a >3x value from
        // one match silently dooming every game creation attempt after it.
        opts.PlayerSpeedMod = Mathf.Clamp(opts.PlayerSpeedMod, MinPlayerSpeedMod, ServerSafePlayerSpeedMod);
    }

    public static void DestroyLobbyCheat()
    {
        if (!CheatToggles.destroyLobby) return;
        CheatToggles.destroyLobby = false;
        if (!Utils.isHost || AmongUsClient.Instance == null) return;
        AmongUsClient.Instance.StartCoroutine(CoDestroyLobby());
    }

    // A single RpcSetName only updates the cosmetic display-name layer, not
    // the underlying stored identity — something in the game (an
    // outfit/cosmetics resync) reverts it back almost immediately, which is
    // why the name flashed briefly then disappeared. Re-applying it
    // repeatedly before actually destroying the lobby keeps overriding
    // whatever reverts it. The interval has to be shorter than that revert's
    // cadence or the overwrite itself is what you see flashing — 0.25s was
    // slow enough to show the revert between writes; 0.05s outruns it.
    private static System.Collections.IEnumerator CoDestroyLobby()
    {
        const string message = "This lobby was destroyed by KarakMenu";
        for (int i = 0; i < 20; i++)
        {
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p == null || p.Data == null || p.Data.Disconnected) continue;
                try { p.RpcSetName(message); } catch { }
            }
            yield return new WaitForSeconds(0.05f);
        }

        try
        {
            var lobby = LobbyBehaviour.Instance;
            if (lobby != null) { lobby.Despawn(); LobbyBehaviour.Instance = null; }
        }
        catch { }
    }

    // Nuke Lobby: rapid-renames all players to a hacked message, enables disco at max speed,
    // and destroys the lobby after a short delay — all at once, host-only.
    public static void NukeLobbyCheat()
    {
        if (!CheatToggles.nukeLobby) return;
        CheatToggles.nukeLobby = false;
        if (!Utils.isHost || AmongUsClient.Instance == null) return;
        CheatToggles.lobbyDiscoMode = true;
        CheatToggles.lobbyDiscoSpeed = 0.1f;
        AmongUsClient.Instance.StartCoroutine(CoNukeLobby());
    }

    private static System.Collections.IEnumerator CoNukeLobby()
    {
        const string nukeMsg = "THIS LOBBY HAS BEEN DESTROYED BY KARAKMENU GET THE BEST HACKS AT https://github.com/RealRevolut1on/KarakMenu-Amongus MADE BY REVO-LOTION & Itz_M4LEK";
        for (int i = 0; i < 30; i++)
        {
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p == null || p.Data == null || p.Data.Disconnected) continue;
                try { p.RpcSetName(nukeMsg); } catch { }
            }
            yield return new WaitForSeconds(0.05f);
        }
        // Destroy lobby after names are branded
        try
        {
            var lobby = LobbyBehaviour.Instance;
            if (lobby != null) { lobby.Despawn(); LobbyBehaviour.Instance = null; }
        }
        catch { }
    }

    // Disable Cameras: turns off the security camera active-state for ALL clients.
    // Sends system update bytes that set the camera usage counter to 0.
    private static bool _camerasDisabledActive;
    public static void DisableCamerasCheat()
    {
        if (!Utils.isShip || ShipStatus.Instance == null) return;
        if (CheatToggles.disableCameras)
        {
            if (!_camerasDisabledActive)
            {
                _camerasDisabledActive = true;
                try { ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Security, 0); } catch { }
            }
        }
        else if (_camerasDisabledActive)
        {
            _camerasDisabledActive = false;
        }
    }

    // Unlimited Meetings: sets the emergency meeting count to 9999 (host-only, persistent while toggled).
    private static bool _unlimitedMeetingsApplied;
    public static void UnlimitedMeetingsCheat()
    {
        if (!Utils.isHost) return;
        if (CheatToggles.unlimitedMeetings)
        {
            if (!_unlimitedMeetingsApplied)
            {
                _unlimitedMeetingsApplied = true;
                try
                {
                    var opts = GameOptionsManager.Instance?.CurrentGameOptions?.TryCast<NormalGameOptionsV10>();
                    if (opts != null) { opts.NumEmergencyMeetings = 9999; GameManager.Instance?.LogicOptions?.SyncOptions(); }
                }
                catch { }
            }
        }
        else if (_unlimitedMeetingsApplied)
        {
            _unlimitedMeetingsApplied = false;
            try
            {
                var opts = GameOptionsManager.Instance?.CurrentGameOptions?.TryCast<NormalGameOptionsV10>();
                if (opts != null) { opts.NumEmergencyMeetings = 1; GameManager.Instance?.LogicOptions?.SyncOptions(); }
            }
            catch { }
        }
    }

    // Disable Meetings: sets emergency meeting count to 0 so no one can call meetings.
    private static bool _disableMeetingsApplied;
    public static void DisableMeetingsCheat()
    {
        if (!Utils.isHost) return;
        if (CheatToggles.disableMeetings)
        {
            if (!_disableMeetingsApplied)
            {
                _disableMeetingsApplied = true;
                try
                {
                    var opts = GameOptionsManager.Instance?.CurrentGameOptions?.TryCast<NormalGameOptionsV10>();
                    if (opts != null) { opts.NumEmergencyMeetings = 0; opts.EmergencyCooldown = 999; GameManager.Instance?.LogicOptions?.SyncOptions(); }
                }
                catch { }
            }
        }
        else if (_disableMeetingsApplied)
        {
            _disableMeetingsApplied = false;
            try
            {
                var opts = GameOptionsManager.Instance?.CurrentGameOptions?.TryCast<NormalGameOptionsV10>();
                if (opts != null) { opts.NumEmergencyMeetings = 1; GameManager.Instance?.LogicOptions?.SyncOptions(); }
            }
            catch { }
        }
    }

    public static void RecreateLobbyCheat()
    {
        if (!CheatToggles.recreateLobby) return;
        CheatToggles.recreateLobby = false;
        if (!Utils.isHost) return;
        try
        {
            if (LobbyBehaviour.Instance != null) return;
            var gsm = GameStartManager.Instance;
            if (gsm == null || gsm.LobbyPrefab == null) return;
            LobbyBehaviour.Instance = Object.Instantiate(gsm.LobbyPrefab);
            AmongUsClient.Instance.Spawn(LobbyBehaviour.Instance, -2, SpawnFlags.None);
        }
        catch { }
    }

    public static void EndGameNowCheat()
    {
        if (!CheatToggles.endGameNow) return;
        CheatToggles.endGameNow = false;
        if (!Utils.isHost) return;
        // Raw cast — a version-agnostic trick; enum member names for
        // GameOverReason drift across Among Us versions but ordinal 1 is stable
        // enough for "end the game now".
        try { GameManager.Instance?.RpcEndGame((GameOverReason)1, false); } catch { }
    }

    // Zero-CD preset: a reflection-driven multi-name property writer.
    // Kept intact because the whole point is being resilient to Among Us
    // renaming these fields across versions.
    private static void SetGameOption(object target, string[] names, object value)
    {
        if (target == null) return;
        var type = target.GetType();
        foreach (var name in names)
        {
            var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop == null || !prop.CanWrite) continue;
            try
            {
                object v = value;
                if (prop.PropertyType.IsEnum)
                {
                    v = value is string s
                        ? System.Enum.Parse(prop.PropertyType, s, ignoreCase: true)
                        : System.Convert.ChangeType(value, System.Enum.GetUnderlyingType(prop.PropertyType));
                }
                else if (!prop.PropertyType.IsInstanceOfType(v))
                {
                    v = System.Convert.ChangeType(value, prop.PropertyType);
                }
                prop.SetValue(target, v);
                return;
            }
            catch { }
        }
    }

    private static void ApplyZeroCdToOptions(object opts)
    {
        if (opts == null) return;
        SetGameOption(opts, new[] { "KillCooldown", "KillCooldownSeconds", "KillCd", "KillTimer" }, 1e-6f);
        SetGameOption(opts, new[] { "ImpostorVision", "ImpostorVisionMod", "ImpostorVisionRange", "ImpostorVisionMultiplier" }, 9999f);
        SetGameOption(opts, new[] { "CrewVision", "CrewMateVision", "CrewVisionMod", "CrewVisionRange", "CrewVisionMultiplier" }, 9999f);
        SetGameOption(opts, new[] { "KillDistance", "KillRange", "KillDistanceSetting" }, "Medium");
        SetGameOption(opts, new[] { "PlayerSpeedMod", "PlayerSpeedMultiplier", "PlayerSpeed" }, 2.9f);
        SetGameOption(opts, new[] { "EmergencyMeetings", "EmergencyMeetingCount", "NumEmergencyMeetings" }, 9999);
        SetGameOption(opts, new[] { "EmergencyCooldown", "EmergencyCooldownSeconds", "EmergencyCooldownTime" }, 0f);
        SetGameOption(opts, new[] { "DiscussionTime", "DiscussionTimeSeconds", "DiscussionTimeMinutes" }, 0f);
        SetGameOption(opts, new[] { "VotingTime", "VotingTimeSeconds", "VotingTimeMinutes" }, 33f);
        SetGameOption(opts, new[] { "VentCooldown", "VentUseCooldown", "VentCooldownSeconds" }, 0.001f);
        SetGameOption(opts, new[] { "MaxVentTime", "VentMaxTime", "VentDuration", "VentTime" }, 20f);
    }

    public static void ApplyZeroCdCheat()
    {
        if (!CheatToggles.applyZeroCd) return;
        CheatToggles.applyZeroCd = false;
        if (!Utils.isHost) return;
        try
        {
            CheatToggles.noOptionsLimits = true;
            var normal = GameOptionsManager.Instance?.CurrentGameOptions?.TryCast<NormalGameOptionsV10>();
            var current = GameOptionsManager.Instance?.CurrentGameOptions;
            if (normal != null) ApplyZeroCdToOptions(normal);
            if (current != null && !object.ReferenceEquals(normal, current)) ApplyZeroCdToOptions(current);
        }
        catch { }
    }

}

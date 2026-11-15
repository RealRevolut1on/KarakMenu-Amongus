using Il2CppSystem.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils;
using System;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace KarakMenu;
public static class KarakPPMCheats
{
    private static bool _telekillPlayerActive;
    private static bool _killPlayerActive;
    private static bool _spectateActive;
    private static bool _teleportPlayerActive;
    private static bool _teleportPlayerToVentActive;
    private static bool _reportBodyActive;
    private static bool _ejectPlayerActive;
    private static bool _setFakeRoleActive;
    private static bool _setFakeAliveActive;
    private static bool _forceRoleActive;
    private static RoleTypes? _oldRole = null;

    public static void ReportBodyPPM()
    {
        if (CheatToggles.reportBody)
        {

            if (!_reportBodyActive)
            {
                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("reportBody");
                }

                // Player pick menu to choose any body (alive or dead) and report it
                PlayerPickMenu.OpenPlayerPickMenu(Utils.GetAllPlayerData(), (Action) (() =>
                {
                    PlayerControl.LocalPlayer.CmdReportDeadBody(PlayerPickMenu.targetPlayerData);
                }));

                _reportBodyActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.reportBody = false;
            }

        }
        else
        {
            if (_reportBodyActive)
            {
                _reportBodyActive = false;
            }
        }
    }

    public static void EjectPlayerPPM()
    {
        if (CheatToggles.ejectPlayer)
        {
            if (!_ejectPlayerActive)
            {
                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("ejectPlayer");
                }

                if (!Utils.isMeeting)
                {
                    CheatToggles.ejectPlayer = false;
                    return;
                }

                // RpcVotingComplete concludes the meeting authoritatively — the
                // official servers only accept it from the actual host and flag
                // a non-host client sending it as "Hacking", instantly kicking
                // them (confirmed via live testing). Every other method in this
                // file that fires a host-authoritative RPC checks this first;
                // this one didn't, which is exactly what caused that kick.
                if (!Utils.isHost)
                {
                    CheatToggles.ejectPlayer = false;
                    HudManager.Instance?.Notifier?.AddDisconnectMessage("Eject Player requires you to be the host.");
                    return;
                }

                List<NetworkedPlayerInfo> playerInfo = new List<NetworkedPlayerInfo>();
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.Data.IsDead && !player.Data.Disconnected)
                    {
                        playerInfo.Add(player.Data);
                    }
                }

                // Player pick menu to choose any living player and eject them during meeting
                PlayerPickMenu.OpenPlayerPickMenu(playerInfo, (Action)(() =>
                {
                    NetworkedPlayerInfo playerToEject = PlayerPickMenu.targetPlayerData;
                    MeetingHud.Instance.RpcVotingComplete(new Il2CppStructArray<MeetingHud.VoterState>(0L), playerToEject, false, false, 0);
                }));

                _ejectPlayerActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.ejectPlayer = false;
            }
        }
        else if (_ejectPlayerActive)
        {
            _ejectPlayerActive = false;
        }
    }

    public static void KillPlayerPPM()
    {
        if (CheatToggles.killPlayer)
        {
            if (!_killPlayerActive)
            {
                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("killPlayer");
                }

                if (Utils.isLobby)
                {
                    HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
                    CheatToggles.killPlayer = false;
                    return;
                }

                // Player pick menu made for killing any player by sending a successful MurderPlayer RPC call
                PlayerPickMenu.OpenPlayerPickMenu(Utils.GetAllPlayerData(), (Action)(() =>
                {
                    Utils.MurderPlayer(PlayerPickMenu.targetPlayerData.Object, MurderResultFlags.Succeeded);
                }));

                _killPlayerActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.killPlayer = false;
            }
        }
        else if (_killPlayerActive)
        {
            _killPlayerActive = false;
        }
    }

    public static void TelekillPlayerPPM()
    {
        if (CheatToggles.telekillPlayer)
        {
            if (!_telekillPlayerActive)
            {
                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("telekillPlayer");
                }

                if (Utils.isLobby)
                {
                    HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
                    CheatToggles.telekillPlayer = false;
                    return;
                }

                // Player pick menu made for killing any player by sending a successful MurderPlayer RPC call
                // and immediatly teleporting back to original position
                PlayerPickMenu.OpenPlayerPickMenu(Utils.GetAllPlayerData(), (Action)(() =>
                {
                    var oldPos = PlayerControl.LocalPlayer.GetTruePosition();
                    Utils.MurderPlayer(PlayerPickMenu.targetPlayerData.Object, MurderResultFlags.Succeeded);
                    AmongUsClient.Instance.StartCoroutine(Utils.DelayedSnapTo(oldPos));
                }));

                _telekillPlayerActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.telekillPlayer = false;
            }
        }
        else if (_telekillPlayerActive)
        {
            _telekillPlayerActive = false;
        }
    }

    public static void TeleportPlayerPPM()
    {
        if (CheatToggles.teleportPlayer)
        {
            if (!_teleportPlayerActive)
            {
                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("teleportPlayer");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                // All players are saved to playerList apart from LocalPlayer
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.AmOwner)
                    {
                        playerDataList.Add(player.Data);
                    }
                }

                // Player pick menu made for teleporting LocalPlayer to any player's position
                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action)(() =>
                {
                    PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(PlayerPickMenu.targetPlayerData.Object.transform.position);
                }));

                _teleportPlayerActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.teleportPlayer = false;
            }
        }
        else if (_teleportPlayerActive)
        {
            _teleportPlayerActive = false;
        }
    }

    // Teleports the CHOSEN player into the CHOSEN vent (CheatToggles.selectedVentIndex)
    // — one-shot per pick, unlike Jail below which keeps snapping them back.
    public static void TeleportPlayerToVentPPM()
    {
        if (CheatToggles.teleportPlayerToVent)
        {
            if (!_teleportPlayerToVentActive)
            {
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("teleportPlayerToVent");
                }

                var vents = KarakCheats.GetVents();
                if (vents == null)
                {
                    HudManager.Instance?.Notifier?.AddDisconnectMessage("No vents available.");
                    CheatToggles.teleportPlayerToVent = false;
                    return;
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player == null || player.AmOwner || player.Data == null || player.Data.Disconnected) continue;
                    playerDataList.Add(player.Data);
                }

                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action)(() =>
                {
                    var vents2 = KarakCheats.GetVents();
                    if (vents2 == null) return;
                    int idx = Mathf.Clamp(CheatToggles.selectedVentIndex, 0, vents2.Count - 1);
                    var vent = vents2[idx];
                    var targetObj = PlayerPickMenu.targetPlayerData?.Object;
                    if (vent != null && targetObj != null) KarakCheats.VentTP(targetObj, vent.Id);
                }));

                _teleportPlayerToVentActive = true;
            }

            if (PlayerPickMenu.playerpickMenu == null) CheatToggles.teleportPlayerToVent = false;
        }
        else if (_teleportPlayerToVentActive)
        {
            _teleportPlayerToVentActive = false;
        }
    }

    public static void SetFakeRolePPM()
    {
        if (CheatToggles.setFakeRole)
        {

            if (!_setFakeRoleActive)
            {

                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("setFakeRole");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                // Shapeshifter role can only be used if it was already assigned at the start of the game
                // This is done to prevent the anticheat from kicking players
                if (_oldRole == RoleTypes.Shapeshifter || Utils.isFreePlay)
                {
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Shapeshifter", OutfitPreset.Shapeshifter, Utils.GetBehaviourByRoleType(RoleTypes.Shapeshifter)));
                }

                // Phantom role can only be used if it was already assigned at the start of the game
                // This is done to prevent the anticheat from kicking players
                if (_oldRole == RoleTypes.Phantom || Utils.isFreePlay)
                {
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Phantom", OutfitPreset.Phantom, Utils.GetBehaviourByRoleType(RoleTypes.Phantom)));
                }

                // Viper role can only be used if it was already assigned at the start of the game
                // This is done to prevent the anticheat from kicking players
                if (_oldRole == RoleTypes.Viper || Utils.isFreePlay)
                {
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Viper", OutfitPreset.Viper, Utils.GetBehaviourByRoleType(RoleTypes.Viper)));
                }

                // Impostor role can only be used if it was already assigned at the start of the game or as host
                // This is done to prevent the anticheat from kicking players
                if ((_oldRole != null && Utils.GetBehaviourByRoleType((RoleTypes)_oldRole).TeamType == RoleTeamTypes.Impostor) || Utils.isFreePlay || Utils.isHost)
                {
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Impostor", OutfitPreset.Impostor, Utils.GetBehaviourByRoleType(RoleTypes.Impostor)));
                }

                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Tracker", OutfitPreset.Tracker, Utils.GetBehaviourByRoleType(RoleTypes.Tracker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Noisemaker", OutfitPreset.Noisemaker, Utils.GetBehaviourByRoleType(RoleTypes.Noisemaker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Engineer", OutfitPreset.Engineer, Utils.GetBehaviourByRoleType(RoleTypes.Engineer)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Scientist", OutfitPreset.Scientist, Utils.GetBehaviourByRoleType(RoleTypes.Scientist)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Detective", OutfitPreset.Detective, Utils.GetBehaviourByRoleType(RoleTypes.Detective)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Crewmate", OutfitPreset.Crewmate, Utils.GetBehaviourByRoleType(RoleTypes.Crewmate)));

                // Player pick menu made for changing your roles with a custom choice list
                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action) (() =>
                {
                    // Log the originally assigned role before it gets changed by setFakeRole cheat
                    if (!Utils.isLobby && !Utils.isFreePlay && _oldRole == null)
                    {
                        _oldRole = PlayerControl.LocalPlayer.Data.RoleType;
                    }

                    if (PlayerControl.LocalPlayer.Data.IsDead) // Prevent accidential revives
                    {
                        if (PlayerPickMenu.targetPlayerData.Role.TeamType == RoleTeamTypes.Impostor)
                        {
                            RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.ImpostorGhost);
                        }
                        else
                        {
                            RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.CrewmateGhost);
                        }
                    }
                    else
                    {
                        /* if (PlayerPickMenu.targetPlayerData.Role.Role == RoleTypes.Shapeshifter && oldRole != RoleTypes.Shapeshifter){

                            Utils.showPopup("\n<size=125%>Changing into the Shapeshifter role is not recommended\nsince shapeshifting will get you kicked by the anticheat");

                        } else if (PlayerPickMenu.targetPlayerData.Role.Role == RoleTypes.Noisemaker && oldRole != RoleTypes.Noisemaker){

                            Utils.showPopup("\n<size=125%>Changing into the Noisemaker role is not recommended\nsince dying won't trigger the alert for other players");

                        } else if (oldRole == RoleTypes.Noisemaker){

                            Utils.showPopup("\n<size=125%>Your \"real\" role is still Noisemaker\nso other players will still see the alert when you die");

                        } */

                        RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, PlayerPickMenu.targetPlayerData.Role.Role);
                    }
                }));

                _setFakeRoleActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.setFakeRole = false;
            }

        }
        else
        {
            if (_setFakeRoleActive)
            {
                _setFakeRoleActive = false;
            }
        }
    }

    public static void SetFakeAlivePPM()
    {
        if (CheatToggles.setFakeAlive)
        {

            if (!_setFakeAliveActive)
            {

                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("setFakeAlive");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Alive", OutfitPreset.Crewmate, Utils.GetBehaviourByRoleType(RoleTypes.Crewmate)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Dead", OutfitPreset.Dead, Utils.GetBehaviourByRoleType(RoleTypes.CrewmateGhost)));

                // Player pick menu made for changing your alive state with a custom choice list
                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action) (() =>
                {
                    if (PlayerPickMenu.targetPlayerData.Role.IsDead)
                    {
                        PlayerControl.LocalPlayer.Die(DeathReason.Exile, true);
                    }
                    else
                    {
                        PlayerControl.LocalPlayer.Revive();
                    }
                }));

                _setFakeAliveActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.setFakeAlive = false;
            }

        }
        else
        {
            if (_setFakeAliveActive)
            {
                _setFakeAliveActive = false;
            }
        }
    }

    public static void ForceRolePPM()
    {
        if (CheatToggles.forceRole)
        {
            if (!_forceRoleActive)
            {
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("forceRole");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Shapeshifter", OutfitPreset.Shapeshifter, Utils.GetBehaviourByRoleType(RoleTypes.Shapeshifter)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Phantom", OutfitPreset.Phantom, Utils.GetBehaviourByRoleType(RoleTypes.Phantom)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Viper", OutfitPreset.Viper, Utils.GetBehaviourByRoleType(RoleTypes.Viper)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Impostor", OutfitPreset.Impostor, Utils.GetBehaviourByRoleType(RoleTypes.Impostor)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Tracker", OutfitPreset.Tracker, Utils.GetBehaviourByRoleType(RoleTypes.Tracker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Noisemaker", OutfitPreset.Noisemaker, Utils.GetBehaviourByRoleType(RoleTypes.Noisemaker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Engineer", OutfitPreset.Engineer, Utils.GetBehaviourByRoleType(RoleTypes.Engineer)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Scientist", OutfitPreset.Scientist, Utils.GetBehaviourByRoleType(RoleTypes.Scientist)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Detective", OutfitPreset.Detective, Utils.GetBehaviourByRoleType(RoleTypes.Detective)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Crewmate", OutfitPreset.Crewmate, Utils.GetBehaviourByRoleType(RoleTypes.Crewmate)));

                // Player pick menu made for forcing a role onto another player
                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action)(() =>
                {
                    CheatToggles.forcedRole = PlayerPickMenu.targetPlayerData.Role.Role;
                }));

                _forceRoleActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.forceRole = false;
            }

        }
        else
        {
            if (_forceRoleActive)
            {
                _forceRoleActive = false;
            }
        }
    }

    public static void SpectatePPM()
    {
        if (CheatToggles.spectate)
        {

            if (!_spectateActive)
            {

                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("spectate");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                // All players are saved to playerList apart from LocalPlayer
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.AmOwner)
                    {
                        playerDataList.Add(player.Data);
                    }
                }

                // Player pick menu made for spectating the targeted player
                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action) (() =>
                {
                    Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerPickMenu.targetPlayerData.Object);
                }));

                _spectateActive = true;

                PlayerControl.LocalPlayer.moveable = false; // Can't move while spectating

                CheatToggles.freecam = false; // Disable incompatible cheats while spectating

            }

            // Deactivate cheat if menu is closed and no one is getting spectated
            if (PlayerPickMenu.playerpickMenu == null && Camera.main.gameObject.GetComponent<FollowerCamera>().Target == PlayerControl.LocalPlayer)
            {
                CheatToggles.spectate = false;
                PlayerControl.LocalPlayer.moveable = true;
            }
        }
        else
        {
            // Deactivate cheat when it is disabled from the Karak GUI
            if (_spectateActive)
            {
                _spectateActive = false;
                PlayerControl.LocalPlayer.moveable = true;
                Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerControl.LocalPlayer);
            }
        }
    }

    private static bool _levelFarmActive;
    private static byte _levelFarmTargetId = byte.MaxValue;
    private static float _levelFarmTimer;

    // Repeatedly kills the chosen player until the cheat is turned off (host only)
    public static void LevelFarmPPM()
    {
        if (CheatToggles.levelFarm)
        {
            if (!_levelFarmActive)
            {
                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("levelFarm");
                }

                if (Utils.isLobby)
                {
                    HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
                    CheatToggles.levelFarm = false;
                    return;
                }

                // Player pick menu made for choosing the player to farm
                PlayerPickMenu.OpenPlayerPickMenu(Utils.GetAllPlayerData(), (Action)(() =>
                {
                    _levelFarmTargetId = PlayerPickMenu.targetPlayerData.PlayerId;
                }));

                _levelFarmActive = true;
                _levelFarmTimer = 0f;
            }

            // Kill the chosen player every second until the cheat is turned off
            if (_levelFarmTargetId != byte.MaxValue && Utils.isShip && Utils.isHost)
            {
                _levelFarmTimer += Time.deltaTime;
                if (_levelFarmTimer >= 1f)
                {
                    _levelFarmTimer = 0f;

                    PlayerControl target = null;
                    foreach (var player in PlayerControl.AllPlayerControls)
                    {
                        if (player != null && player.PlayerId == _levelFarmTargetId)
                        {
                            target = player;
                            break;
                        }
                    }

                    if (target == null || target.Data == null || target.Data.Disconnected) return;

                    var oldPos = PlayerControl.LocalPlayer.GetTruePosition();
                    PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(target.transform.position);
                    Utils.MurderPlayer(target, MurderResultFlags.Succeeded);
                    AmongUsClient.Instance.StartCoroutine(Utils.DelayedSnapTo(oldPos));
                }
            }
        }
        else if (_levelFarmActive)
        {
            _levelFarmActive = false;
            _levelFarmTargetId = byte.MaxValue;
        }
    }

    private static bool _copyOutfitActive;

    // Copies the outfit of the chosen player to the LocalPlayer (one-shot)
    public static void CopyOutfitPPM()
    {
        if (CheatToggles.copyOutfit)
        {
            if (!_copyOutfitActive)
            {
                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("copyOutfit");
                }

                // Player pick menu made for choosing the player whose outfit to copy
                PlayerPickMenu.OpenPlayerPickMenu(Utils.GetAllPlayerData(), (Action)(() =>
                {
                    var outfit = PlayerPickMenu.targetPlayerData.Object.CurrentOutfit;
                    PlayerControl.LocalPlayer.SetOutfit(outfit, PlayerOutfitType.Default);
                }));

                _copyOutfitActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.copyOutfit = false;
            }
        }
        else if (_copyOutfitActive)
        {
            _copyOutfitActive = false;
        }
    }

    // ---------------------------------------------------------------------
    // Player-pick targeting (PPM style)
    // ---------------------------------------------------------------------
    private static bool _kickPlayerActive;
    private static bool _banPlayerActive;
    private static bool _doorHallucinationActive;

    private static void OpenKickBanPicker(bool ban, string toggleName)
    {
        if (PlayerPickMenu.playerpickMenu != null)
        {
            PlayerPickMenu.playerpickMenu.Close();
            CheatToggles.DisablePPMCheats(toggleName);
        }
        if (!Utils.isHost)
        {
            if (ban) CheatToggles.banPlayer = false; else CheatToggles.kickPlayer = false;
            return;
        }

        // All non-self connected players can be kicked / banned (dead players too — host can kick anyone)
        var pool = new List<NetworkedPlayerInfo>();
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p.AmOwner || p.Data == null || p.Data.Disconnected) continue;
            pool.Add(p.Data);
        }

        PlayerPickMenu.OpenPlayerPickMenu(pool, (Action)(() =>
        {
            var target = PlayerPickMenu.targetPlayerData;
            if (target == null || target.Object == null) return;
            int clientId = AmongUsClient.Instance.GetClientIdFromCharacter(target.Object);
            if (clientId < 0) return;
            try { AmongUsClient.Instance.KickPlayer(clientId, ban); } catch { }
        }));
    }

    public static void KickPlayerPPM()
    {
        if (CheatToggles.kickPlayer)
        {
            if (!_kickPlayerActive)
            {
                OpenKickBanPicker(ban: false, toggleName: "kickPlayer");
                _kickPlayerActive = true;
            }
            if (PlayerPickMenu.playerpickMenu == null) CheatToggles.kickPlayer = false;
        }
        else if (_kickPlayerActive) _kickPlayerActive = false;
    }

    public static void BanPlayerPPM()
    {
        if (CheatToggles.banPlayer)
        {
            if (!_banPlayerActive)
            {
                OpenKickBanPicker(ban: true, toggleName: "banPlayer");
                _banPlayerActive = true;
            }
            if (PlayerPickMenu.playerpickMenu == null) CheatToggles.banPlayer = false;
        }
        else if (_banPlayerActive) _banPlayerActive = false;
    }

    // Sends the door-hallucination RPCs at a single chosen victim
    public static void DoorHallucinationPPM()
    {
        if (CheatToggles.doorHallucination)
        {
            if (!_doorHallucinationActive)
            {
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("doorHallucination");
                }
                if (ShipStatus.Instance == null)
                {
                    HudManager.Instance?.Notifier?.AddDisconnectMessage("Door Hallucination failed: game not started.");
                    CheatToggles.doorHallucination = false;
                    return;
                }

                var pool = new List<NetworkedPlayerInfo>();
                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    if (p == null || p.AmOwner || p.Data == null || p.Data.Disconnected) continue;
                    pool.Add(p.Data);
                }

                PlayerPickMenu.OpenPlayerPickMenu(pool, (Action)(() =>
                {
                    var t = PlayerPickMenu.targetPlayerData?.Object;
                    if (t != null) KarakCheats_HallucinateDoorsFor(t);
                }));

                _doorHallucinationActive = true;
            }
            if (PlayerPickMenu.playerpickMenu == null) CheatToggles.doorHallucination = false;
        }
        else if (_doorHallucinationActive) _doorHallucinationActive = false;
    }

    // Thin bridge to the private helper in KarakCheats — same logic path as
    // DoorHallucinationAllCheat but for a single target
    private static void KarakCheats_HallucinateDoorsFor(PlayerControl target)
    {
        if (target == null || ShipStatus.Instance == null || AmongUsClient.Instance == null) return;
        int targetClientId = AmongUsClient.Instance.GetClientIdFromCharacter(target);
        if (targetClientId < 0) return;

        var rooms = new Il2CppSystem.Collections.Generic.HashSet<SystemTypes>();
        var doors = ShipStatus.Instance.AllDoors;
        for (int i = 0; i < doors.Length; i++) if (doors[i] != null) rooms.Add(doors[i].Room);
        var seen = new System.Collections.Generic.HashSet<SystemTypes>();
        foreach (var r in rooms) seen.Add(r);
        if (seen.Count == 0) return;
        foreach (var room in seen)
        {
            // RPC 27 = CloseDoorsOfType on ShipStatus
            var writer = AmongUsClient.Instance.StartRpcImmediately(
                ShipStatus.Instance.NetId, (byte)27, Hazel.SendOption.Reliable, targetClientId);
            writer.Write((byte)(int)room);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }
}

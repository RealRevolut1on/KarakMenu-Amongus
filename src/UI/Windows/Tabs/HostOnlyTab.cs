using UnityEngine;

namespace KarakMenu;

public class HostOnlyTab : ITab
{
    public string name => "Host-Only";

    public void Draw()
    {
        float colW = (MenuUI.ContentWidth - 12f) / 2f;

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawGeneral();
        GUILayout.Space(10);
        DrawMurder();
        GUILayout.Space(10);
        DrawKickBan();
        GUILayout.Space(10);
        DrawShapeshift();
        GUILayout.EndVertical();

        GUILayout.Space(12);

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawGameState();
        GUILayout.Space(10);
        DrawMeetings();
        GUILayout.Space(10);
        DrawGameSettings();
        GUILayout.Space(10);
        DrawLobby();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        KarakUI.BeginSection("General");
        CheatToggles.killVanished = KarakUI.Toggle(CheatToggles.killVanished, "Kill While Vanished");
        CheatToggles.killAnyone = KarakUI.Toggle(CheatToggles.killAnyone, "Kill Anyone");
        CheatToggles.noKillCd = KarakUI.Toggle(CheatToggles.noKillCd, "No Kill Cooldown");
        CheatToggles.showProtectMenu = KarakUI.Toggle(CheatToggles.showProtectMenu, "Show Protect Menu");
        CheatToggles.showRolesMenu = KarakUI.Toggle(CheatToggles.showRolesMenu, "Show Roles Menu");
        CheatToggles.disableVents = KarakUI.Toggle(CheatToggles.disableVents, "Disable Vents for Others");
        CheatToggles.blockSabotages = KarakUI.Toggle(CheatToggles.blockSabotages, "Block Sabotages");
        CheatToggles.disableCameras = KarakUI.Toggle(CheatToggles.disableCameras, "Disable Security Cameras");
        KarakUI.EndSection();
    }

    private void DrawMurder()
    {
        KarakUI.BeginSection("Murder");
        if (KarakUI.Action("Kill Player", "Kill"))          CheatToggles.killPlayer = true;
        if (KarakUI.Action("Telekill Player", "Kill"))      CheatToggles.telekillPlayer = true;
        CheatToggles.levelFarm = KarakUI.Toggle(CheatToggles.levelFarm, "Level Farm");
        if (KarakUI.Action("Kill All Crewmates", "Kill"))   CheatToggles.killAllCrew = true;
        if (KarakUI.Action("Kill All Impostors", "Kill"))   CheatToggles.killAllImps = true;
        if (KarakUI.Action("Kill Everyone", "Kill"))        CheatToggles.killAll = true;
        KarakUI.EndSection();
    }

    private void DrawShapeshift()
    {
        KarakUI.BeginSection("Shapeshift");
        if (KarakUI.Action("SS All Into Self", "Do it")) CheatToggles.ssAllIntoSelf = true;
        if (KarakUI.Action("Unshift All", "Do it")) CheatToggles.unshiftAll = true;
        CheatToggles.spamShapeshiftAll = KarakUI.Toggle(CheatToggles.spamShapeshiftAll, "Spam SS All Into Random");
        if (KarakUI.Action("Shapeshift Everyone Into You", "Do it")) CheatToggles.copyOutfitToAll = true;
        KarakUI.EndSection();
    }

    private void DrawGameState()
    {
        KarakUI.BeginSection("Game State");
        if (KarakUI.Action("Force Start Game", "Start"))    CheatToggles.forceStartGame = true;
        CheatToggles.noGameEnd = KarakUI.Toggle(CheatToggles.noGameEnd, "No Game End");
        bool newFlipMap = KarakUI.Toggle(CheatToggles.flipMap, "Flip Map");
        if (newFlipMap != CheatToggles.flipMap)
        {
            CheatToggles.flipMap = newFlipMap;
            KarakCheats.SetFlipMap(newFlipMap);
        }
        if (KarakUI.Action("End Game Now", "End")) CheatToggles.endGameNow = true;
        if (KarakUI.Action("Destroy Lobby", "Destroy")) CheatToggles.destroyLobby = true;
        if (KarakUI.Action("Recreate Lobby", "Recreate")) CheatToggles.recreateLobby = true;
        if (KarakUI.Action("Nuke Lobby", "NUKE")) CheatToggles.nukeLobby = true;
        KarakUI.EndSection();
    }

    private void DrawKickBan()
    {
        KarakUI.BeginSection("Kick / Ban");
        CheatToggles.kickPlayer = KarakUI.Toggle(CheatToggles.kickPlayer, "Kick Player…");
        CheatToggles.banPlayer  = KarakUI.Toggle(CheatToggles.banPlayer,  "Ban Player…");
        if (KarakUI.Action("Eject Player", "Eject"))        CheatToggles.ejectPlayer = true;
        KarakUI.EndSection();
    }

    private void DrawLobby()
    {
        KarakUI.BeginSection("Lobby");
        if (KarakUI.Action("Same Color as Me", "Apply"))    CheatToggles.sameColorEveryone = true;
        if (KarakUI.Action("Cycle Lobby Color", "Cycle"))   CheatToggles.cycleLobbyColor = true;
        CheatToggles.lobbyDiscoMode = KarakUI.Toggle(CheatToggles.lobbyDiscoMode, "Disco Mode");
        if (CheatToggles.lobbyDiscoMode)
        {
            CheatToggles.lobbyDiscoSpeed = KarakUI.LabeledSlider("Disco Speed", CheatToggles.lobbyDiscoSpeed, 0.05f, 2f, "0.00");
        }
        KarakUI.EndSection();
    }

    private void DrawMeetings()
    {
        KarakUI.BeginSection("Meetings");
        if (KarakUI.Action("Skip Meeting", "Skip"))         CheatToggles.skipMeeting = true;
        CheatToggles.voteImmune = KarakUI.Toggle(CheatToggles.voteImmune, "Vote Immune");
        if (KarakUI.Action("Force Meeting", "Call"))        CheatToggles.callMeeting = true;
        CheatToggles.unlimitedMeetings = KarakUI.Toggle(CheatToggles.unlimitedMeetings, "Unlimited Meetings");
        CheatToggles.disableMeetings = KarakUI.Toggle(CheatToggles.disableMeetings, "Disable Meetings");
        KarakUI.EndSection();
    }

    private void DrawGameSettings()
    {
        KarakUI.BeginSection("Game Settings");
        CheatToggles.speedHack = KarakUI.Toggle(CheatToggles.speedHack, "Enable");
        if (CheatToggles.speedHack)
        {
            CheatToggles.speedHackMultiplier = KarakUI.LabeledSlider("Everyone's Speed",
                CheatToggles.speedHackMultiplier, 0.5f, 10f, "0.00");
            if (CheatToggles.speedHackMultiplier > 3f)
            {
                GUILayout.Label(
                    "<color=#ff5555>Warning: the official servers reject anything past 3x and will instantly disconnect you for \"Hacking\" — confirmed via testing. Only safe on private servers.</color>",
                    GUIStylePreset.Label);
            }
        }

        bool hns = Utils.isHideNSeek;
        GUILayout.BeginHorizontal();
        GUILayout.Label(hns ? "Seekers" : "Impostors", GUIStylePreset.Label, GUILayout.ExpandWidth(true));
        if (KarakUI.Button("+", GUILayout.Width(28f), GUILayout.Height(20f))) CheatToggles.increaseImpostors = true;
        if (KarakUI.Button("−", GUILayout.Width(28f), GUILayout.Height(20f))) CheatToggles.decreaseImpostors = true;
        GUILayout.EndHorizontal();

        GUILayout.Label($"Next Round {(hns ? "Seekers" : "Impostors")}: {KarakCheats.GetNextRoundImpostorCount()}", GUIStylePreset.Label);

        if (!hns)
        {
            var (aliveImps, totalImps) = KarakCheats.GetAliveImpostorCount();
            GUILayout.Label($"Alive Impostors: {aliveImps} / {totalImps}", GUIStylePreset.Label);
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Lobby Speed", GUIStylePreset.Label, GUILayout.ExpandWidth(true));
        if (KarakUI.Button("+", GUILayout.Width(28f), GUILayout.Height(20f))) CheatToggles.increasePlayerSpeed = true;
        if (KarakUI.Button("−", GUILayout.Width(28f), GUILayout.Height(20f))) CheatToggles.decreasePlayerSpeed = true;
        GUILayout.EndHorizontal();

        if (KarakUI.Action("Zero-CD Preset", "Apply")) CheatToggles.applyZeroCd = true;
        KarakUI.EndSection();
    }
}

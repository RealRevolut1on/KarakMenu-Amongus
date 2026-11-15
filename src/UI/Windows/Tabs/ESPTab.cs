using UnityEngine;

namespace KarakMenu;

public class ESPTab : ITab
{
    public string name => "ESP";

    public void Draw()
    {
        float colW = (MenuUI.ContentWidth - 12f) / 2f;

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawGeneral();
        GUILayout.Space(10);
        DrawDistanceColors();
        GUILayout.Space(10);
        DrawCamera();
        GUILayout.EndVertical();

        GUILayout.Space(12);

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawTracers();
        GUILayout.Space(10);
        DrawMinimap();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        KarakUI.BeginSection("General");
        CheatToggles.seePlayerInfo = KarakUI.Toggle(CheatToggles.seePlayerInfo, "See Player Info");
        CheatToggles.seeRoles = KarakUI.Toggle(CheatToggles.seeRoles, "See Roles");
        CheatToggles.seeGhosts = KarakUI.Toggle(CheatToggles.seeGhosts, "See Ghosts");
        CheatToggles.seePlayersInVents = KarakUI.Toggle(CheatToggles.seePlayersInVents, "Highlight Players in Vents");
        CheatToggles.seeCooldown = KarakUI.Toggle(CheatToggles.seeCooldown, "See Cooldown");
        CheatToggles.noShadows = KarakUI.Toggle(CheatToggles.noShadows, "No Shadows");
        CheatToggles.taskArrows = KarakUI.Toggle(CheatToggles.taskArrows, "Task Arrows");
        CheatToggles.revealVotes = KarakUI.Toggle(CheatToggles.revealVotes, "Reveal Votes");
        CheatToggles.seeLobbyInfo = KarakUI.Toggle(CheatToggles.seeLobbyInfo, "See Lobby Info");
        KarakUI.EndSection();
    }

    private void DrawDistanceColors()
    {
        KarakUI.BeginSection("Distance Colors");
        CheatToggles.distanceColors = KarakUI.Toggle(CheatToggles.distanceColors, "Distance Colors", "esp-distance-color");
        CheatToggles.espTeamCheck = KarakUI.Toggle(CheatToggles.espTeamCheck, "Team Check", "esp-team-check");
        KarakUI.EndSection();
    }

    private void DrawCamera()
    {
        KarakUI.BeginSection("Camera");
        CheatToggles.zoomOut = KarakUI.Toggle(CheatToggles.zoomOut, "Zoom Out");
        CheatToggles.spectate = KarakUI.Toggle(CheatToggles.spectate, "Spectate");
        CheatToggles.freecam = KarakUI.Toggle(CheatToggles.freecam, "Freecam");
        KarakUI.EndSection();
    }

    private void DrawTracers()
    {
        KarakUI.BeginSection("Tracers");
        CheatToggles.tracersCrew = KarakUI.Toggle(CheatToggles.tracersCrew, "Crewmates", "tracers-crew");
        CheatToggles.tracersImps = KarakUI.Toggle(CheatToggles.tracersImps, "Impostors", "tracers-imps");
        CheatToggles.tracersGhosts = KarakUI.Toggle(CheatToggles.tracersGhosts, "Ghosts", "tracers-ghosts");
        CheatToggles.tracersBodies = KarakUI.Toggle(CheatToggles.tracersBodies, "Dead Bodies", "tracers-bodies");
        CheatToggles.colorBasedTracers = KarakUI.Toggle(CheatToggles.colorBasedTracers, "Color-based", "tracers-color");
        CheatToggles.distanceBasedTracers = KarakUI.Toggle(CheatToggles.distanceBasedTracers, "Distance-based", "tracers-distance");
        KarakUI.EndSection();
    }

    private void DrawMinimap()
    {
        KarakUI.BeginSection("Minimap");
        CheatToggles.mapCrew = KarakUI.Toggle(CheatToggles.mapCrew, "Crewmates", "map-crew");
        CheatToggles.mapImps = KarakUI.Toggle(CheatToggles.mapImps, "Impostors", "map-imps");
        CheatToggles.mapGhosts = KarakUI.Toggle(CheatToggles.mapGhosts, "Ghosts", "map-ghosts");
        CheatToggles.colorBasedMap = KarakUI.Toggle(CheatToggles.colorBasedMap, "Color-based", "map-color");
        KarakUI.EndSection();
    }
}

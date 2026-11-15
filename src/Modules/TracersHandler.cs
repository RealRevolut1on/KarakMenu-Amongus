using UnityEngine;

namespace KarakMenu;

public static class TracersHandler
{
    // Draws a tracer from LocalPlayer to another player.
    public static void DrawPlayerTracer(PlayerPhysics playerPhysics)
    {
        try
        {
            var color = Color.clear; // All tracers are invisible by default

            if (!playerPhysics.myPlayer.Data.IsDead)
            {
                if (CheatToggles.tracersCrew && !playerPhysics.myPlayer.Data.Role.IsImpostor ||
                    CheatToggles.tracersImps && playerPhysics.myPlayer.Data.Role.IsImpostor)
                {
                    if (CheatToggles.distanceBasedTracers || CheatToggles.distanceColors)
                    {
                        // Team Check exempts your own team: they keep their team color
                        color = CheatToggles.espTeamCheck && KarakESP.IsTeammate(playerPhysics.myPlayer.Data)
                            ? playerPhysics.myPlayer.Data.Role.TeamColor
                            : KarakESP.GetDistanceColor(playerPhysics.myPlayer.transform.position);
                    }
                    else if (CheatToggles.colorBasedTracers)
                    {
                        color = playerPhysics.myPlayer.Data.Color; // Color-Based Tracer
                    }
                    else
                    {
                        color = playerPhysics.myPlayer.Data.Role.TeamColor; // Team-Based Tracer
                    }
                }
            }
            else
            {
                if (CheatToggles.tracersGhosts)
                {
                    if (CheatToggles.distanceBasedTracers || CheatToggles.distanceColors)
                    {
                        color = CheatToggles.espTeamCheck && KarakESP.IsTeammate(playerPhysics.myPlayer.Data)
                            ? Palette.White
                            : KarakESP.GetDistanceColor(playerPhysics.myPlayer.transform.position);
                    }
                    else if (CheatToggles.colorBasedTracers)
                    {
                        color = playerPhysics.myPlayer.Data.Color; // Color-Based Tracer
                    }
                    else
                    {
                        color = Palette.White; // Ghost Tracer (White)
                    }
                }
            }

            // Draw tracer between the player and LocalPlayer using the right color
            Utils.DrawTracer(playerPhysics.myPlayer.gameObject, PlayerControl.LocalPlayer.gameObject, color);
        } catch { }
    }

    // Draws a tracer LocalPlayer to a dead body. Only draws tracers for unreported dead bodies.
    public static void DrawBodyTracer(DeadBody deadBody)
    {
        var color = Color.clear; // All tracers are invisible by default

        if (CheatToggles.tracersBodies)
        {
            if (CheatToggles.distanceBasedTracers || CheatToggles.distanceColors)
            {
                var bodyInfo = GameData.Instance.GetPlayerById(deadBody.ParentId);
                color = CheatToggles.espTeamCheck && KarakESP.IsTeammate(bodyInfo)
                    ? Color.yellow
                    : KarakESP.GetDistanceColor(deadBody.transform.position);
            }
            else if (CheatToggles.colorBasedTracers)
            {
                color = GameData.Instance.GetPlayerById(deadBody.ParentId).Color; // Color-Based Tracer
            }
            else
            {
                color = Color.yellow; // Dead Body Tracer (Yellow)
            }
        }

        // Draw tracer between the dead body and LocalPlayer using the right color
        Utils.DrawTracer(deadBody.gameObject, PlayerControl.LocalPlayer.gameObject, color);
    }
}

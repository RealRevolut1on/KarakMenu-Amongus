using UnityEngine;
using System;

namespace KarakMenu;

public class MovementTab : ITab
{
    public string name => "Movement";

    public void Draw()
    {
        float colW = (MenuUI.ContentWidth - 12f) / 2f;

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawGeneral();
        GUILayout.EndVertical();

        GUILayout.Space(12);

        GUILayout.BeginVertical(GUILayout.Width(colW));
        DrawTeleport();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        KarakUI.BeginSection("General");
        {
            CheatToggles.noClip = KarakUI.Toggle(CheatToggles.noClip, "NoClip");
            CheatToggles.invertControls = KarakUI.Toggle(CheatToggles.invertControls, "Invert Controls");
            CheatToggles.immortality = KarakUI.Toggle(CheatToggles.immortality, "Immortality");

            CheatToggles.customSpeed = KarakUI.Toggle(CheatToggles.customSpeed, "Custom Speed");
            if (CheatToggles.customSpeed)
            {
                bool isGhost = PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null && PlayerControl.LocalPlayer.Data.IsDead;
                CheatToggles.customSpeedValue = KarakUI.LabeledSlider(isGhost ? "Ghost Speed" : "Speed",
                    CheatToggles.customSpeedValue, 0f, 20f, "0.00");
            }
        }
        KarakUI.EndSection();
    }

    private void DrawTeleport()
    {
        KarakUI.BeginSection("Teleport");
        {
            CheatToggles.teleportCursor = KarakUI.Toggle(CheatToggles.teleportCursor, "to Cursor");
            CheatToggles.teleportPlayer = KarakUI.Toggle(CheatToggles.teleportPlayer, "to Player");
        }
        KarakUI.EndSection();

        GUILayout.Space(10);

        KarakUI.BeginSection("Vent Teleport");
        {
            var vents = KarakCheats.GetVents();
            if (vents == null || vents.Count == 0)
            {
                GUILayout.Label("No vents available (not on a map yet).", GUIStylePreset.Label);
            }
            else
            {
                // Prev/next stepper instead of a dropdown — the Combo popup widget
                // renders inside a clipped, scrolled GUILayout.BeginScrollView, so
                // its popup gets clipped/misaligned and clicks don't land correctly.
                // This stepper uses plain GUILayout.Button, the same proven pattern
                // as the Impostor count +/- in Host-Only → Game Settings.
                int clamped = Mathf.Clamp(CheatToggles.selectedVentIndex, 0, vents.Count - 1);
                GUILayout.BeginHorizontal();
                // Shows the room the vent is in (e.g. "Cafeteria") instead of the
                // game's raw Vent.Id, which is assigned arbitrarily per-map and
                // jumps around non-sequentially (0, 2, 11, ...) with no way to
                // tell what stepping to it actually gets you.
                GUILayout.Label(KarakCheats.GetVentLabel(vents[clamped]), GUIStylePreset.Label, GUILayout.ExpandWidth(true));
                if (KarakUI.Button("<", GUILayout.Width(28f), GUILayout.Height(20f)))
                    clamped = (clamped - 1 + vents.Count) % vents.Count;
                if (KarakUI.Button(">", GUILayout.Width(28f), GUILayout.Height(20f)))
                    clamped = (clamped + 1) % vents.Count;
                GUILayout.EndHorizontal();
                CheatToggles.selectedVentIndex = clamped;
            }

            CheatToggles.teleportAllIncludeSelf = KarakUI.Toggle(CheatToggles.teleportAllIncludeSelf, "Include Myself");

            CheatToggles.teleportPlayerToVent = KarakUI.Toggle(CheatToggles.teleportPlayerToVent, "Teleport Player to Vent…");
            if (KarakUI.Action("Teleport Everyone to Vent", "Do it")) CheatToggles.teleportAllToVent = true;

            CheatToggles.spamTpImps = KarakUI.Toggle(CheatToggles.spamTpImps, "Spam-TP Impostors");
            CheatToggles.spamTpAll  = KarakUI.Toggle(CheatToggles.spamTpAll,  "Spam-TP Everyone");
        }
        KarakUI.EndSection();
    }
}

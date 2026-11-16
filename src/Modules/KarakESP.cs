using UnityEngine;
using Sentry.Internal.Extensions;

namespace KarakMenu;
public static class KarakESP
{
    private static bool _freecamActive;
    private static bool _resolutionChangeNeeded;

    // ---------- Distance colors (ESP) ----------
    // Green when far, red when close. Endpoints are deliberately bright so
    // "close" red stays readable on dark maps instead of turning into a smear.

    private static readonly Color DistanceNear = new(1f, 0.16f, 0.12f);  // <= 2 units
    private static readonly Color DistanceMid = new(1f, 0.92f, 0.15f);   // ~11 units
    private static readonly Color DistanceFar = new(0.2f, 1f, 0.25f);    // >= 20 units
    private const float DistanceNearUnits = 2f;
    private const float DistanceFarUnits = 20f;

    public static Color GetDistanceColor(Vector3 worldPosition)
    {
        var distance = Vector3.Distance(worldPosition, PlayerControl.LocalPlayer.transform.position);
        var t = Mathf.InverseLerp(DistanceNearUnits, DistanceFarUnits, distance);

        // Interpolate: Red (close) -> Yellow (medium) -> Green (far)
        return t < 0.5f
            ? Color.Lerp(DistanceNear, DistanceMid, t * 2f)
            : Color.Lerp(DistanceMid, DistanceFar, (t - 0.5f) * 2f);
    }

    // Team Check: true when the target is on the same team as the local player.
    public static bool IsTeammate(NetworkedPlayerInfo info)
    {
        try
        {
            if (info == null || info.IsNull() || info.Role.IsNull()) return false;
            return info.Role.TeamType == PlayerControl.LocalPlayer.Data.Role.TeamType;
        }
        catch { return false; }
    }

    // Returns true when distance colors should replace the normal ESP color.
    // Team Check makes teammates fall through to their normal team/role color.
    public static bool ShouldDistanceColor(NetworkedPlayerInfo info)
    {
        if (!CheatToggles.distanceColors) return false;
        if (CheatToggles.espTeamCheck && IsTeammate(info)) return false;
        return info != null && !info.IsNull() && info.Object != null;
    }

    public static void SporeCloudVision(Mushroom mushroom)
    {
        if (CheatToggles.noShadows)
        {
            // Change the Z axis position of spore clouds as to make players appear above them

            mushroom.sporeMask.transform.position = new Vector3(mushroom.sporeMask.transform.position.x, mushroom.sporeMask.transform.position.y, -1);
            return;
        }

        // Normal Z axis position: 5f
        mushroom.sporeMask.transform.position = new Vector3(mushroom.sporeMask.transform.position.x, mushroom.sporeMask.transform.position.y, 5f);
    }

    public static bool IsFullbrightActive()
    {
        // Fullbright is automatically activated when zooming out, spectating other players, or "freecamming"
        // This is done to avoid issues with shadows

        return CheatToggles.noShadows || Camera.main.orthographicSize > 3f || Camera.main.gameObject.GetComponent<FollowerCamera>().Target != PlayerControl.LocalPlayer;
    }

    public static void ZoomOut(HudManager hudManager)
    {
        try
        {
            if (CheatToggles.zoomOut)
            {
                if (hudManager.Chat == null) return;
                if (hudManager.Chat.IsOpenOrOpening || PlayerCustomizationMenu.Instance || (Utils.isLobby && (FriendsListUI.Instance.IsOpen ||
                    GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane.gameObject.active || GameStartManager.Instance.RulesEditPanel))) return;

                // Don't let scroll-to-zoom fall through underneath the KarakMenu
                // window while the user is scrolling over it
                if (MenuUI.isGUIActive && MenuUI.IsMouseOverMenu) return;

                // Camera.main / UICamera can be null during scene transitions
                if (Camera.main == null || hudManager.UICamera == null) return;

                _resolutionChangeNeeded = true;

                if (Input.GetAxis("Mouse ScrollWheel") < 0f ) // Zoom out
                {

                    // Both the main camera and the UI camera need to be adjusted

                    Camera.main.orthographicSize++;
                    hudManager.UICamera.orthographicSize++;

                    // Utils.AdjustResolution() seems to be needed to properly sync the game's UI
                    // after a change in orthographicSize

                    Utils.AdjustResolution();

                }
                else if (Input.GetAxis("Mouse ScrollWheel") > 0f )
                {
                    // Zoom in
                    if (!(Camera.main.orthographicSize > 3f)) return; // Never go below the default orthographicSize: 3f

                    Camera.main.orthographicSize--;
                    hudManager.UICamera.orthographicSize--;

                    Utils.AdjustResolution();
                }
            }
            else
            {
                // orthographicSize is reset to default value: 3f
                if (Camera.main != null) Camera.main.orthographicSize = 3f;
                if (hudManager.UICamera != null) hudManager.UICamera.orthographicSize = 3f;

                // Utils.AdjustResolution() is invoked one last time to prevent issues with UI
                if (_resolutionChangeNeeded)
                {
                    Utils.AdjustResolution();
                    _resolutionChangeNeeded = false;
                }
            }
        }
        catch { }
    }

    public static void MeetingNametags(MeetingHud meetingHud)
    {
        try
        {
            foreach (var playerState in meetingHud.playerStates)
            {
                // Fetch the NetworkedPlayerInfo of each playerState
                var data = GameData.Instance.GetPlayerById(playerState.PlayerId);

                if (data.IsNull() || data.Disconnected || data.Outfits[PlayerOutfitType.Default].IsNull()) continue;

                // Update the player's nametag appropriately
                playerState.NameText.text = Utils.GetNameTag(data, data.DefaultOutfit.PlayerName);

                // Move and resize the nametag to prevent it overlapping with colorblind text
                if (CheatToggles.seeRoles && CheatToggles.seePlayerInfo)
                {
                    playerState.NameText.transform.localPosition = new Vector3(0.33f, 0.08f, 0f);
                    playerState.NameText.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                }
                else if (CheatToggles.seeRoles || CheatToggles.seePlayerInfo)
                {
                    playerState.NameText.transform.localPosition = new Vector3(0.3384f, 0.1125f, -0.1f);
                    playerState.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                }
                else
                {
                    // Reset the position and scale of the nametag to default values (they're kinda weird but whatever)
                    playerState.NameText.transform.localPosition = new Vector3(0.3384f, 0.0311f, -0.1f);
                    playerState.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                }
            }
        } catch { }
    }

    public static void PlayerNametags(PlayerPhysics playerPhysics)
    {
        try
        {
            var player = playerPhysics.myPlayer;

            // Never distance-color your own nametag (distance would always be 0 = red)
            Color? distanceColor = null;
            if (player != PlayerControl.LocalPlayer && ShouldDistanceColor(player.Data))
                distanceColor = GetDistanceColor(player.transform.position);

            var nameTag = Utils.GetNameTag(player.Data, player.CurrentOutfit.PlayerName, false, distanceColor);

            // See Cooldown: prepend the kill-cooldown line (impostors only).
            var cooldownLine = GetCooldownLine(player);
            if (cooldownLine != null) nameTag = cooldownLine + nameTag;

            player.cosmetics.SetName(nameTag);

            // Move the nameText up to prevent it overlapping with colorblind
            // text. One extra 0.093 per additional line keeps the player's actual
            // name at the same height, so prepended lines render above it.
            int extraLines = 0;
            if (CheatToggles.seeRoles) extraLines++;
            if (CheatToggles.seePlayerInfo) extraLines++;
            if (cooldownLine != null) extraLines++;

            player.cosmetics.nameText.transform.localPosition = new Vector3(0f, extraLines * 0.093f, 0f);
        } catch { }
    }

    public static void ChatNametags(ChatBubble chatBubble)
    {
        try
        {
            // Update the player's nametag appropriately
            chatBubble.NameText.text = Utils.GetNameTag(chatBubble.playerInfo, chatBubble.NameText.text, true);

            // Adjust the chatBubble's size to the new nametag to prevent issues
            chatBubble.NameText.ForceMeshUpdate(true, true);
            chatBubble.Background.size = new Vector2(5.52f, 0.2f + chatBubble.NameText.GetNotDumbRenderedHeight() + chatBubble.TextArea.GetNotDumbRenderedHeight());
            chatBubble.MaskArea.size = chatBubble.Background.size - new Vector2(0f, 0.03f);

        } catch { }
    }

    public static void SeeGhostsCheat(PlayerPhysics playerPhysics)
    {
        try{

            if(playerPhysics.myPlayer.Data.IsDead && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                playerPhysics.myPlayer.Visible = CheatToggles.seeGhosts;
            }

        }catch{}
    }

    // Replaces the old vent-visibility patch: instead of
    // unhiding the vented player we tint the VENT they're hiding in, so they
    // still look hidden — red vent = impostor role inside, blue vent = crew
    // role (engineer etc.) inside. The game never stores "which vent is this
    // player in" for remote players, but entering a vent snaps their transform
    // onto the vent, so nearest-vent-to-player is exact.
    //
    // Runs once per frame (not per player) from PlayerPhysics_LateUpdate's
    // AmOwner block, because every vent gets reset to its cached base color
    // before the occupied ones are tinted.
    private static readonly System.Collections.Generic.Dictionary<int, Color> _ventBaseColors = new();

    public static void VentPlayerHighlightCheat()
    {
        try
        {
            if (ShipStatus.Instance?.AllVents == null) return;

            var vents = ShipStatus.Instance.AllVents;

            // Restore every vent's real color first so nothing stays tinted
            // after the player leaves the vent or the toggle goes off.
            for (int i = 0; i < vents.Count; i++)
            {
                var vent = vents[i];
                if (vent == null) continue;
                var rend = vent.myRend;
                if (rend == null) continue;

                if (!_ventBaseColors.TryGetValue(vent.Id, out var baseColor))
                {
                    baseColor = rend.color;
                    _ventBaseColors[vent.Id] = baseColor;
                }
                rend.color = baseColor;
            }

            if (!CheatToggles.seePlayersInVents) return;
            if (PlayerControl.AllPlayerControls == null) return;

            // ventId -> isImpostor. Impostor wins if two players share a vent.
            var occupied = new System.Collections.Generic.Dictionary<int, bool>();

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.AmOwner || player.Data == null || player.Data.IsDead) continue;
                if (!player.inVent) continue;

                Vent nearest = null;
                float bestDistance = 1.5f;
                var playerPosition = player.transform.position;

                foreach (var vent in vents)
                {
                    if (vent == null) continue;
                    float distance = Vector2.Distance(playerPosition, vent.transform.position);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        nearest = vent;
                    }
                }

                if (nearest == null) continue;

                bool isImpostor = player.Data.Role != null && player.Data.Role.IsImpostor;
                if (occupied.TryGetValue(nearest.Id, out bool alreadyImpostor))
                    occupied[nearest.Id] = alreadyImpostor || isImpostor;
                else
                    occupied[nearest.Id] = isImpostor;
            }

            foreach (var entry in occupied)
            {
                Vent vent = null;
                foreach (var candidate in vents)
                {
                    if (candidate != null && candidate.Id == entry.Key) { vent = candidate; break; }
                }
                if (vent == null) continue;

                var rend = vent.myRend;
                if (rend == null) continue;

                float alpha = 1f;
                if (_ventBaseColors.TryGetValue(entry.Key, out var baseColor) && baseColor.a > 0f)
                    alpha = baseColor.a;

                rend.color = entry.Value
                    ? new Color(1f, 0.15f, 0.15f, alpha)   // impostor role
                    : new Color(0.25f, 0.55f, 1f, alpha);  // crew role
            }
        }
        catch { }
    }

    // ---------- See Cooldown (ESP) ----------
    // There is no network message carrying another player's kill timer, so a
    // remote impostor's cooldown can only be reconstructed: everyone starts a
    // round on a full cooldown, every MurderPlayer we see resets that killer's
    // clock, and a meeting resets every killer's clock once gameplay resumes.
    // KillCooldown itself is read live from the game options, so host changes
    // and "no kill cooldown" settings are picked up.
    private static readonly System.Collections.Generic.Dictionary<int, float> _lastKillTime = new();
    private static float _roundStartTime;

    // Set when a meeting closes. The actual reset has to wait until the
    // ejection cutscene finishes and gameplay actually resumes, because that's
    // when Among Us restarts every impostor's kill timer.
    private static bool _meetingResetPending;
    private static float _meetingEndedAt;

    public static void ResetKillCooldownTracking()
    {
        _lastKillTime.Clear();
        _roundStartTime = Time.time;
    }

    public static void MarkMeetingEnded()
    {
        _meetingResetPending = true;
        _meetingEndedAt = Time.time;
    }

    // Called once per frame. Applies the pending post-meeting reset as soon as
    // the meeting HUD is gone and the local player can move again (i.e. the
    // cutscene is over), with a hard fallback in case CanMove never flips.
    public static void TickCooldownTracking()
    {
        if (!_meetingResetPending) return;

        bool inMeeting = MeetingHud.Instance != null;
        bool canMove = PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.CanMove;

        if ((!inMeeting && canMove) || Time.time - _meetingEndedAt > 10f)
        {
            _meetingResetPending = false;
            ResetKillCooldownTracking();
        }
    }

    public static void RecordKill(PlayerControl killer)
    {
        if (killer == null) return;
        _lastKillTime[killer.PlayerId] = Time.time;
    }

    // Returns the "CD 12s" / "Ready" line to render above an impostor's
    // nametag, or null when this player shouldn't show one at all.
    public static string GetCooldownLine(PlayerControl player)
    {
        try
        {
            if (!CheatToggles.seeCooldown) return null;
            if (player == null || player.AmOwner) return null;
            if (player.Data == null || player.Data.Role == null || player.Data.IsDead) return null;
            if (!player.Data.Role.IsImpostor) return null;
            if (MeetingHud.Instance != null || !Utils.isInGame) return null;

            float cooldown = 10f;
            try
            {
                if (GameManager.Instance?.LogicOptions != null)
                    cooldown = GameManager.Instance.LogicOptions.GetKillCooldown();
            }
            catch { }

            // Nobody has killed yet this round -> their clock started at round
            // start, not at full cooldown from "now".
            if (!_lastKillTime.TryGetValue(player.PlayerId, out float lastKill))
                lastKill = _roundStartTime;

            float remaining = Mathf.Max(0f, cooldown - (Time.time - lastKill));

            return remaining > 0.05f
                ? $"<size=70%><color=#ff4d4d>CD {Mathf.CeilToInt(remaining)}s</color></size>\r\n"
                : "<size=70%><color=#4dff4d>Ready</color></size>\r\n";
        }
        catch { return null; }
    }

    public static void FreecamCheat()
    {
        if (CheatToggles.freecam)
        {
            // Completely disable FollowerCamera
            if (!_freecamActive)
            {

                Camera.main.gameObject.GetComponent<FollowerCamera>().enabled = false;
                Camera.main.gameObject.GetComponent<FollowerCamera>().Target = null;

                _freecamActive = true;

            }

            // Prevent the player from moving while in freecam
            PlayerControl.LocalPlayer.moveable = false;

            // Get keyboard input
            var movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0.0f);

            // Change the camera's position depending on the keyboard input
            // Speed: 10f
            Camera.main.transform.position = Camera.main.transform.position + movement * 10f * Time.deltaTime;

        }
        else
        {
            // Re-enable FollowerCamera & movement once freecam is disabled
            if (!_freecamActive) return;
            PlayerControl.LocalPlayer.moveable = true;
            Camera.main.gameObject.GetComponent<FollowerCamera>().enabled = true;
            Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerControl.LocalPlayer);
            _freecamActive = false;
        }
    }
}

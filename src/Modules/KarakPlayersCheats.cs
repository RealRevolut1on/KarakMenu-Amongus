using System.Collections.Generic;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils;
using AmongUs.GameOptions;

namespace KarakMenu;

// Backing logic for the Players tab: per-player frame-shapeshift, solo-disco,
// mass-shapeshift actions, and a kick button that works even without host.
// Disco state is keyed by PlayerId and persists only while the tab's toggle
// stays on — not a CheatToggles bool since there's one per player, not a
// single global on/off. There is no per-OTHER-player speed override here —
// Among Us only has one shared PlayerSpeedMod option (no per-player speed
// RPC exists), so a specific other player can't genuinely be made faster or
// slower than everyone else. Self speed lives in CheatToggles.customSpeed,
// shared with the Movement tab, and is applied via the PlayerPhysics_TrueSpeed
// Harmony patch in PlayerPhysicsPatches.cs (see that file for why).
public static class KarakPlayersCheats
{
    // ---------------------------------------------------------------------
    // Shared shapeshift-with-role-flip machinery. Flips the victim to
    // Shapeshifter just long enough to broadcast a real shapeshift RPC, then
    // reverts their role. Mirrors the exact role-flip/RPC/revert timing
    // already proven in KarakCheats.CoApplyOutfitToAll (0.5s gaps around the
    // anticheat-visible role change). Host-only: RpcSetRole/RpcShapeshift on
    // someone else's PlayerControl is a host-authoritative action.
    //
    // NOTE: repeatedly firing this (Spam below) is a strong anticheat
    // "Hacking" kick trigger — confirmed via live testing, not theoretical.
    private static System.Collections.IEnumerator CoShapeshiftInto(PlayerControl victim, PlayerControl target)
    {
        var originalRole = victim.Data?.RoleType;
        bool needsRoleFlip = originalRole != null && originalRole != RoleTypes.Shapeshifter;

        if (needsRoleFlip)
        {
            try { victim.RpcSetRole(RoleTypes.Shapeshifter, false); }
            catch { try { victim.RpcSetRole(RoleTypes.Shapeshifter); } catch { } }
            yield return new WaitForSeconds(0.5f);
        }

        try { victim.RpcShapeshift(target, true); } catch { }

        if (needsRoleFlip)
        {
            yield return new WaitForSeconds(0.5f);
            try { victim.RpcSetRole(originalRole.Value, false); }
            catch { try { victim.RpcSetRole(originalRole.Value); } catch { } }
        }
    }

    // Frame Shapeshift — one specific victim appears to shapeshift into a
    // random OTHER player (not necessarily you).
    public static void FrameShapeshift(PlayerControl victim)
    {
        if (!Utils.isHost || victim == null || victim.Data == null || victim.AmOwner) return;
        if (AmongUsClient.Instance == null) return;

        var candidates = new List<PlayerControl>();
        foreach (var p in PlayerControl.AllPlayerControls)
            if (p != null && p != victim && p.Data != null && !p.Data.Disconnected) candidates.Add(p);
        if (candidates.Count == 0) return;
        var fakeTarget = candidates[Random.Range(0, candidates.Count)];

        AmongUsClient.Instance.StartCoroutine(CoShapeshiftInto(victim, fakeTarget));
    }

    // SS All Into Self — every other player appears to shapeshift into YOU
    // specifically. One-shot. Staggered 0.3s apart per player rather than
    // firing every RpcSetRole/RpcShapeshift in the same instant — a full
    // lobby's worth of simultaneous role-flip+shapeshift RPC bursts is
    // exactly the pattern that got a tester kicked for "Hacking" during
    // testing. Staggering reduces (does not eliminate) that risk.
    public static void ShapeshiftAllIntoSelfCheat()
    {
        if (!CheatToggles.ssAllIntoSelf) return;
        CheatToggles.ssAllIntoSelf = false; // one-shot

        if (!Utils.isHost || PlayerControl.LocalPlayer == null || ShipStatus.Instance == null || AmongUsClient.Instance == null) return;

        AmongUsClient.Instance.StartCoroutine(CoShapeshiftAllIntoSelfStaggered());
    }

    private static System.Collections.IEnumerator CoShapeshiftAllIntoSelfStaggered()
    {
        var targets = new List<PlayerControl>();
        foreach (var p in PlayerControl.AllPlayerControls)
            if (p != null && !p.AmOwner && p.Data != null && !p.Data.Disconnected) targets.Add(p);

        foreach (var p in targets)
        {
            AmongUsClient.Instance.StartCoroutine(CoShapeshiftInto(p, PlayerControl.LocalPlayer));
            yield return new WaitForSeconds(0.3f);
        }
    }

    // Spam SS All Into Random — every player repeatedly, randomly shapeshifts
    // into someone else. Toggle, throttled to every 3s (this is already a
    // heavy RPC burst — going faster is what gets people kicked for "Hacking").
    private static float _spamShapeshiftTimer;

    public static void SpamShapeshiftAllTick()
    {
        if (!CheatToggles.spamShapeshiftAll || !Utils.isHost || ShipStatus.Instance == null || AmongUsClient.Instance == null) return;
        if (Time.time - _spamShapeshiftTimer < 3f) return;
        _spamShapeshiftTimer = Time.time;

        var players = new List<PlayerControl>();
        foreach (var p in PlayerControl.AllPlayerControls)
            if (p != null && p.Data != null && !p.Data.Disconnected) players.Add(p);
        if (players.Count < 2) return;

        foreach (var victim in players)
        {
            PlayerControl target;
            int guard = 0;
            do { target = players[Random.Range(0, players.Count)]; guard++; }
            while (target == victim && guard < 8);
            if (target == victim) continue;

            AmongUsClient.Instance.StartCoroutine(CoShapeshiftInto(victim, target));
        }
    }

    // Unshift All — reverts everyone's current shapeshift back to normal. One-shot.
    //
    // RpcRejectShapeshift is for DECLINING a PENDING shapeshift request (backing
    // out of the target-picking minigame before it completes) — it does nothing
    // for a shapeshift that's already active, which is why this didn't work.
    // There's no dedicated "revert" broadcast RPC in the game's own API (only
    // CmdCheckRevertShapeshift, which is what a player's OWN client sends to the
    // host to request a revert) — the actual revert is represented over the
    // network as shapeshifting back into YOURSELF, the same self-target trick
    // KarakCheats.CoApplyOutfitToAll already uses for its own re-render pulse.
    public static void UnshiftAllCheat()
    {
        if (!CheatToggles.unshiftAll) return;
        CheatToggles.unshiftAll = false; // one-shot

        if (!Utils.isHost || ShipStatus.Instance == null || AmongUsClient.Instance == null) return;

        AmongUsClient.Instance.StartCoroutine(CoUnshiftAllStaggered());
    }

    private static System.Collections.IEnumerator CoUnshiftAllStaggered()
    {
        var targets = new List<PlayerControl>();
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p.Data == null || p.Data.Disconnected) continue;

            // Never shift YOU — RpcShapeshift(self) from a non-shapeshifter role
            // (Judge/Crewmate/etc.) is an invalid RPC and is exactly what was
            // getting the local player kicked. This is also why SS-All-Into-Self,
            // which skips AmOwner, never kicks.
            if (p.AmOwner) continue;

            // Only players who can actually shapeshift — there's nothing to
            // revert on anyone else, and it just adds to the RPC burst.
            if (p.Data.RoleType != RoleTypes.Shapeshifter) continue;

            targets.Add(p);
        }

        // Staggered like CoShapeshiftAllIntoSelfStaggered: firing a whole
        // lobby's worth of RpcShapeshift in a single frame is the pattern that
        // gets the sender kicked for "Hacking".
        foreach (var p in targets)
        {
            try { p.RpcShapeshift(p, true); } catch { }
            yield return new WaitForSeconds(0.3f);
        }
    }

    // ---------------------------------------------------------------------
    // Solo Disco — same random-color-cycle idea as KarakCheats.LobbyDiscoCheat,
    // restricted to whichever players are in `DiscoTargets`. Host-only: RpcSetColor
    // on someone else's PlayerControl is host-authoritative.
    public static readonly HashSet<byte> DiscoTargets = new();
    private static float _discoTimer;

    public static void DiscoTick()
    {
        if (!Utils.isHost || DiscoTargets.Count == 0) return;
        if (Time.time - _discoTimer < CheatToggles.playerDiscoSpeed) return;
        _discoTimer = Time.time;

        int max = Palette.PlayerColors.Length;
        if (max <= 0) return;

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p.Data == null || p.Data.Disconnected) continue;
            if (!DiscoTargets.Contains(p.PlayerId)) continue;
            try { p.RpcSetColor((byte)Random.Range(0, max)); } catch { }
        }
    }

    // ---------------------------------------------------------------------
    // Jail — keeps every targeted player penned in the map's Navigation vent,
    // snapping them back the moment they leave that room. Uses a periodic
    // room-check (not a blind fixed-interval re-snap) and a multi-target
    // model: a HashSet of PlayerIds toggled per player row, same as
    // DiscoTargets above, instead of a one-at-a-time PlayerPickMenu picker —
    // that picker hijacks the Shapeshifter minigame, which is the likely
    // reason the earlier picker-based version of this kept failing. The vent
    // itself is resolved dynamically via KarakCheats.GetJailVent rather than a
    // hardcoded per-map vent-ID table, since those IDs are pinned to one game
    // version and aren't safe to copy blindly.
    public static readonly HashSet<byte> JailTargets = new();
    private static float _jailTimer;
    private static Vent _jailVent;
    private static PlainShipRoom _jailRoom;

    public static void JailTick()
    {
        if (JailTargets.Count == 0 || !Utils.isShip || ShipStatus.Instance == null)
        {
            _jailVent = null;
            _jailRoom = null;
            return;
        }

        // Resolved once per jail session, not every tick — vents don't move
        // mid-round, and re-resolving lets it recover if a map/round changes.
        if (_jailVent == null)
        {
            _jailVent = KarakCheats.GetJailVent();
            if (_jailVent == null) return; // this map has no Navigation room
            _jailRoom = Utils.GetRoomFromPosition(_jailVent.transform.position);
        }

        _jailTimer += Time.deltaTime;
        if (_jailTimer < 0.5f) return;
        _jailTimer = 0f;

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p.Data == null || p.Data.Disconnected || p.Data.IsDead || p.AmOwner) continue;
            if (!JailTargets.Contains(p.PlayerId)) continue;
            if (Utils.GetRoomFromPosition(p.GetTruePosition()) == _jailRoom) continue;
            KarakCheats.HostAwareVentSnap(p, _jailVent);
        }
    }

    // ---------------------------------------------------------------------
    // Change Color — cycles the target player's color to the next palette entry. Host-only.
    public static void CyclePlayerColor(PlayerControl target)
    {
        if (!Utils.isHost || target == null || target.Data == null) return;
        int max = Palette.PlayerColors.Length;
        if (max <= 0) return;
        int next = (target.Data.DefaultOutfit?.ColorId + 1 ?? 0) % max;
        try { target.RpcSetColor((byte)next); } catch { }
    }

    // Vote-kick via the real host API (same as regular kick — wraps TryKick for label clarity).
    public static string TryVoteKick(PlayerControl target) => TryKick(target);

    // ---------------------------------------------------------------------
    // Kick — real host kick when possible, otherwise the best-effort non-host
    // exploit fallback. Never allows targeting the host either way.
    // Returns a short status string to show the user.
    public static string TryKick(PlayerControl target)
    {
        if (target == null || target.AmOwner || target.Data == null) return "Invalid target.";
        if (AmongUsClient.Instance == null) return "Not connected.";

        int clientId = AmongUsClient.Instance.GetClientIdFromCharacter(target);
        if (clientId < 0) return "Could not resolve player.";
        if (clientId == AmongUsClient.Instance.HostId) return "You can't kick the host.";

        if (Utils.isHost)
        {
            try { AmongUsClient.Instance.KickPlayer(clientId, false); return "Kicked."; }
            catch { return "Kick failed."; }
        }

        bool sent = KarakCheats.TryNonHostKick(target);
        return sent
            ? "Sent (experimental — not guaranteed to work without host)."
            : "Could not send kick attempt.";
    }
}

namespace KarakMenu;

// Client-side kill immunity that works even without host privileges. The
// game already excludes players who are inVent from being valid murder
// targets, so faking that state via VentilationSystem.Update needs no RPC
// and no host authority — unlike the host-only Protect Players panel
// (UI/Windows/ProtectUI.cs), which relies on a host-authoritative RPC.
public static class KarakImmortality
{
    // Outside any real map's vent-ID range (VentilationSystem tracks
    // occupancy in a Dictionary<byte,byte>, not a fixed-size array, so an
    // out-of-range key is safe — it just never collides with a real vent).
    public const int VirtualVentKey = 0x1F3F;

    private static bool _wasActive;

    // Runs every frame from the same per-frame loop as the other cheats —
    // this also transparently handles re-entering after anything that clears
    // inVent out from under us (meetings, round start, etc.) since we just
    // notice next frame that we're no longer inVent and reapply, rather than
    // needing a dedicated hook for every possible reset trigger.
    public static void Tick()
    {
        if (!Utils.isPlayer || !Utils.isShip) return;

        if (CheatToggles.immortality)
        {
            _wasActive = true;
            if (!PlayerControl.LocalPlayer.inVent)
                VentilationSystem.Update(VentilationSystem.Operation.Enter, VirtualVentKey);
        }
        else if (_wasActive)
        {
            _wasActive = false;
            // Cleanly exit the fake vent state instead of leaving the player
            // stuck inVent (frozen animation, vent-only interactions) forever.
            if (PlayerControl.LocalPlayer.inVent)
                VentilationSystem.Update(VentilationSystem.Operation.Exit, VirtualVentKey);
        }
    }
}

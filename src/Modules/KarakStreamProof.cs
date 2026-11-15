using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KarakMenu;

// Hides the whole game window from screen-capture APIs (OBS/Discord/Zoom
// game- or window-capture, Windows Game Bar) while it stays fully visible on
// the local monitor — a real Win32 capability, unlike Stealth/Panic Mode
// (CheatToggles.stealthMode/panicMode, see MenuUI.cs), which only suppress
// KarakMenu's own IMGUI draw calls. Those still leave the underlying game
// window itself capturable; this actually excludes it at the OS level.
// Requires Windows 10 2004+ — SetWindowDisplayAffinity just returns false on
// older builds, which we ignore rather than surface, since there's nothing
// useful to do about it from here.
public static class KarakStreamProof
{
    private const uint WDA_NONE = 0x00000000;
    private const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

    private static IntPtr _hwnd = IntPtr.Zero;
    private static bool _lastApplied;

    // Edge-triggered: only calls the Win32 API when the toggle actually
    // changes, called once per frame from the same loop as the other cheats.
    public static void Tick()
    {
        if (CheatToggles.streamProof == _lastApplied) return;

        if (_hwnd == IntPtr.Zero)
        {
            try { _hwnd = Process.GetCurrentProcess().MainWindowHandle; }
            catch { return; }
            if (_hwnd == IntPtr.Zero) return; // retry next tick
        }

        SetWindowDisplayAffinity(_hwnd, CheatToggles.streamProof ? WDA_EXCLUDEFROMCAPTURE : WDA_NONE);
        _lastApplied = CheatToggles.streamProof;
    }
}

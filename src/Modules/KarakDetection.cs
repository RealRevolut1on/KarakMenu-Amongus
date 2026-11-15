using System.Collections.Generic;
using UnityEngine;
using Hazel;
using InnerNet;

namespace KarakMenu;

// KarakMenu user detection: periodically sends a tiny invisible chat payload to every
// other client. Other KarakMenu users intercept it (see ChatControllerPatches) and
// mark the sender as a KarakMenu user so the DetectUI can show them.
//
// IMPORTANT: pings only happen while fully joined inside a lobby (never during the
// join sequence, never in-game) and are sent one at a time, because bursting RPCs
// right after joining trips the game's anti-cheat ("Client DC because Hacking").
public static class KarakDetection
{
    public const string Marker = "\u2063KM\u2063";

    private static readonly HashSet<byte> _detected = new();
    private static float _pingTimer = 3f;
    private static int _pingIndex;
    private static int _lastFrame = -1;

    public static bool IsKarakUser(byte playerId) => _detected.Contains(playerId);

    public static HashSet<byte> DetectedIds => _detected;

    // Called by the AddChat patch whenever a chat text arrives
    public static void HandleSignal(PlayerControl source, string text)
    {
        if (source == null || text == null || !text.StartsWith(Marker)) return;

        _detected.Add(source.PlayerId);
        _pingTimer = 0f; // Answer quickly so the other user knows about us too
    }

    // Detection pings are DISABLED. The old behavior sent invisible chat
    // packets via targeted RPCs, but non-KarakMenu clients would render
    // the invisible marker characters as boxes, so the recipient's screen
    // showed "(box)KM(box)3.2.0" from the sender. We keep the HandleSignal
    // path in case another user still pings us, but never emit our own.
    public static void Update()
    {
        if (Time.frameCount == _lastFrame) return;
        _lastFrame = Time.frameCount;

        if (!Utils.isLobby || PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
        {
            _detected.Clear();
        }
    }
}

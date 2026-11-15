namespace KarakMenu;

// Holds the currently-selected Whisper target (see Patches/WhisperPatches.cs
// for where the actual redirected chat RPC is sent). Not a CheatToggles bool
// since it's a player reference, not an on/off switch, and it deliberately
// stays set across frames/tabs until the user picks someone else or hits
// Stop Whisper — there's no toggle to auto-clear.
public static class KarakWhisper
{
    public static PlayerControl Target;

    private static string _pendingEchoName;

    // Consumed once by ChatBubble.SetName's prefix, right after the local
    // echo call below arms it — relabels the sender's own bubble to show who
    // the whisper actually went to instead of a normal public-chat bubble.
    public static void ArmPendingEcho(string targetName) => _pendingEchoName = targetName;

    public static string ConsumePendingEchoName()
    {
        var name = _pendingEchoName;
        _pendingEchoName = null;
        return name;
    }
}

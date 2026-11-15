using HarmonyLib;
using Hazel;

namespace KarakMenu;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
public static class PlayerControl_RpcSendChat_Whisper
{
    // Prefix patch of PlayerControl.RpcSendChat to redirect the chat RPC to
    // ONLY the chosen Whisper target's client instead of the normal broadcast.
    // This never calls the original method, so it also bypasses whatever
    // vanilla logic normally restricts what a dead player can send as a
    // broadcast — the receiving client's RPC handler doesn't re-check the
    // sender's alive state before displaying an incoming SendChat, so a
    // directed send works the same whether the local player is alive or a ghost.
    public static bool Prefix(PlayerControl __instance, string chatText, ref bool __result)
    {
        var target = KarakWhisper.Target;
        if (target == null || !__instance.AmOwner || target.Data == null || target.Data.Disconnected)
            return true;

        int targetClientId = AmongUsClient.Instance.GetClientIdFromCharacter(target);

        // The recipient's client has no idea this was a whisper — it just gets a
        // normal SendChat RPC and renders an ordinary-looking bubble. Tag the
        // delivered copy so they know nobody else in the lobby received it.
        // The sender's own echo gets the same tag so both sides read alike.
        string delivered = TagWhisper(chatText);

        var writer = AmongUsClient.Instance.StartRpcImmediately(
            __instance.NetId, (byte)RpcCalls.SendChat, SendOption.Reliable, targetClientId);
        writer.Write(delivered);
        AmongUsClient.Instance.FinishRpcImmediately(writer);

        // Local echo so the sender sees their own message — nobody else's
        // client ever receives this RPC at all, so there's nothing to hide.
        KarakWhisper.ArmPendingEcho(target.Data.PlayerName);
        HudManager.Instance?.Chat?.AddChat(__instance, TagWhisper(chatText), false);

        __result = true;
        return false;
    }

    private static string TagWhisper(string chatText)
    {
        if (string.IsNullOrEmpty(chatText) || chatText.StartsWith(WhisperMarker)) return chatText;

        // Keep the whole thing inside the recipient client's own chat cap
        // (100 on this build) so the tag never pushes the message out of
        // what their bubble will actually display.
        string tagged = WhisperMarker + chatText;
        return tagged.Length > MaxChatLen ? tagged.Substring(0, MaxChatLen) : tagged;
    }

    // Short enough that a full-length message plus the tag still fits inside
    // the recipient client's own chat length cap (100/120 depending on build).
    private const int MaxChatLen = 100;
    private const string WhisperMarker = "[only you see this] ";
}

[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
public static class ChatBubble_SetName_WhisperLabel
{
    // Prefix patch of ChatBubble.SetName to relabel the sender's own echoed
    // bubble right after AddChat above creates it, so it reads who the
    // whisper actually went to instead of looking like public chat.
    public static void Prefix(ref string playerName)
    {
        var pending = KarakWhisper.ConsumePendingEchoName();
        if (pending == null) return;
        playerName = $"[You] -> [{pending}]";
    }
}

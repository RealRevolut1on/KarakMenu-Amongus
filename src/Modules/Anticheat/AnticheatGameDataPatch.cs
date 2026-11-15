using AmongUs.InnerNet.GameDataMessages;
using HarmonyLib;
using Hazel;
using InnerNet;

namespace KarakMenu.AC;

// ---------------------------------------------------------------------------
// Game-data validation hook.
//
// InnerNetClient.HandleGameData is the only place where the game-data message
// type can be read before the vanilla dispatcher consumes the reader, so the
// prefix re-runs the message loop itself.
//
// Gated behind Anticheat.EnableGameDataChecks, and it only takes over when at
// least one game-data check is actually switched on, so a disabled anticheat
// leaves the vanilla path completely untouched.
// ---------------------------------------------------------------------------
[HarmonyPatch(typeof(InnerNetClient), "HandleGameData")]
internal static class InnerNetClient_HandleGameData_Anticheat
{
    private static bool Prefix(InnerNetClient __instance, MessageReader parentReader)
    {
        if (!Anticheat.IsEnabled || !Anticheat.EnableGameDataChecks || !Anticheat.HasActiveGameDataChecks) return true;

        try
        {
            while (parentReader.BytesRemaining > 0)
            {
                MessageReader reader = parentReader.ReadMessageAsNewBuffer();
                HandleGameDataInner(__instance, reader, ++__instance.msgNum);
            }
        }
        finally
        {
            parentReader.Recycle();
        }

        return false;
    }

    private static void HandleGameDataInner(InnerNetClient innerNetClient, MessageReader reader, int msgNum)
    {
        GameDataTypes type = (GameDataTypes)reader.Tag;

        bool isValid = Anticheat.HandleGameData(type, reader);
        if (!isValid)
        {
            reader.Recycle();
            return;
        }

        innerNetClient.StartCoroutine(innerNetClient.HandleGameDataInner(reader, msgNum));
    }
}

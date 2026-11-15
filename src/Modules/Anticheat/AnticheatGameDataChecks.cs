using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using InnerNet;

namespace KarakMenu.AC;

// ---------------------------------------------------------------------------
// Game-data validators for the InnerNet game-data message loop.
// plus the InnerNetClient.HandleGameData hook.
// ---------------------------------------------------------------------------

internal class SceneChange : GameDataCheck
{
    public override bool Validate(MessageReader reader)
    {
        int clientId = reader.ReadPackedInt32();
        string scene = reader.ReadString();

        ClientData client = AmongUsClient.Instance.FindClientById(clientId);
        if (client == null)
        {
            Anticheat.Flag($"Received SceneChange message for unknown client: {clientId}.");
            return false;
        }

        // If the host receives a scene change of Tutorial, it spawns The Skeld.
        if (scene == "Tutorial")
        {
            Anticheat.Flag(client.Character, $"{client.Character.Data.PlayerName} sent a scene change of Tutorial.");
            return false;
        }

        return true;
    }

    public override GameDataTypes GetId() => GameDataTypes.SceneChangeFlag;
}

internal class ClientReady : GameDataCheck
{
    public override bool Validate(MessageReader reader)
    {
        int clientId = reader.ReadPackedInt32();

        ClientData client = AmongUsClient.Instance.FindClientById(clientId);
        if (client == null)
        {
            Anticheat.Flag($"Received ClientReady message for unknown client: {clientId}.");
            return false;
        }

        if (client.IsReady)
        {
            Anticheat.Flag(client.Character, $"{client.Character.Data.PlayerName} sent a ClientReady message while already ready.");
            return false;
        }

        return true;
    }

    public override GameDataTypes GetId() => GameDataTypes.ReadyFlag;
}

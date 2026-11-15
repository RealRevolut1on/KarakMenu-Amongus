using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using InnerNet;

namespace KarakMenu
{
    // Builds a GameData message carrying cosmetic RPCs for an arbitrary
    // player. Sending SetColor for another player is risky: colors are
    // validated server-side and duplicates get you kicked, so callers only
    // queue Set*Str cosmetics. The local apply (AmTarget) keeps the sender's
    // own screen in sync. SetName is deliberately NOT implemented: names are
    // validated even for the host on server-authoritative lobbies and will
    // get you kicked.
    internal class BatchedMessage
    {
        public readonly MessageWriter writer;
        public readonly int targetClientId;

        public BatchedMessage(int targetClientId = -1)
        {
            writer = MessageWriter.Get(SendOption.Reliable);

            this.targetClientId = targetClientId;
            if (targetClientId == -1)
            {
                writer.StartMessage(InnerNet.Tags.GameData);
                writer.Write(AmongUsClient.Instance.GameId);
            }
            else
            {
                writer.StartMessage(InnerNet.Tags.GameDataTo);
                writer.Write(AmongUsClient.Instance.GameId);
                writer.WritePacked(targetClientId);
            }
        }

        private bool AmTarget
        {
            get { return targetClientId == -1 || targetClientId == AmongUsClient.Instance.ClientId; }
        }

        public void QueueSetColor(PlayerControl source, byte color)
        {
            if (AmTarget)
            {
                source.SetColor(color);
            }

            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(source.NetId);
            writer.Write((byte)RpcCalls.SetColor);
            writer.Write(source.Data.NetId);
            writer.Write(color);
            writer.EndMessage();
        }

        public void QueueSetHatStr(PlayerControl source, string hat, byte seqId)
        {
            if (AmTarget)
            {
                source.SetHat(hat, source.Data.DefaultOutfit.ColorId);
            }

            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(source.NetId);
            writer.Write((byte)RpcCalls.SetHatStr);
            writer.Write(hat);
            writer.Write(seqId);
            writer.EndMessage();
        }

        public void QueueSetSkinStr(PlayerControl source, string skin, byte seqId)
        {
            if (AmTarget)
            {
                source.SetSkin(skin, source.Data.DefaultOutfit.ColorId);
            }

            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(source.NetId);
            writer.Write((byte)RpcCalls.SetSkinStr);
            writer.Write(skin);
            writer.Write(seqId);
            writer.EndMessage();
        }

        public void QueueSetVisorStr(PlayerControl source, string visor, byte seqId)
        {
            if (AmTarget)
            {
                source.SetVisor(visor, source.Data.DefaultOutfit.ColorId);
            }

            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(source.NetId);
            writer.Write((byte)RpcCalls.SetVisorStr);
            writer.Write(visor);
            writer.Write(seqId);
            writer.EndMessage();
        }

        public void QueueSetPetStr(PlayerControl source, string pet, byte seqId)
        {
            if (AmTarget)
            {
                source.SetPet(pet, source.Data.DefaultOutfit.ColorId);
            }

            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(source.NetId);
            writer.Write((byte)RpcCalls.SetPetStr);
            writer.Write(pet);
            writer.Write(seqId);
            writer.EndMessage();
        }

        public void QueueSetNamePlateStr(PlayerControl source, string nameplate, byte seqId)
        {
            if (AmTarget)
            {
                source.SetNamePlate(nameplate);
            }

            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(source.NetId);
            writer.Write((byte)RpcCalls.SetNamePlateStr);
            writer.Write(nameplate);
            writer.Write(seqId);
            writer.EndMessage();
        }

        public void FinishBatch()
        {
            writer.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(writer);
            writer.Recycle();
        }
    }
}

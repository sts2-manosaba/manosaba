using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace Manosaba.Multiplayer.Messages.Game;

public sealed class WitchIslandExpeditionParryResolutionMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
    public uint promptId;
    public readonly List<ulong> targetPlayerIds = [];
    public readonly List<ulong> parriedPlayerIds = [];

    public bool ShouldBuffer => false;
    public bool ShouldBroadcast => true;
    public NetTransferMode Mode => NetTransferMode.Reliable;
    public LogLevel LogLevel => LogLevel.Debug;
    public RunLocation Location { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteUInt(promptId);
        writer.WriteByte((byte)Math.Min(byte.MaxValue, targetPlayerIds.Count));
        foreach (ulong playerId in targetPlayerIds.Take(byte.MaxValue))
        {
            writer.WriteULong(playerId);
        }

        writer.WriteByte((byte)Math.Min(byte.MaxValue, parriedPlayerIds.Count));
        foreach (ulong playerId in parriedPlayerIds.Take(byte.MaxValue))
        {
            writer.WriteULong(playerId);
        }

        writer.Write(Location);
    }

    public void Deserialize(PacketReader reader)
    {
        promptId = reader.ReadUInt();
        targetPlayerIds.Clear();
        byte targetCount = reader.ReadByte();
        for (int i = 0; i < targetCount; i++)
        {
            targetPlayerIds.Add(reader.ReadULong());
        }

        parriedPlayerIds.Clear();
        byte count = reader.ReadByte();
        for (int i = 0; i < count; i++)
        {
            parriedPlayerIds.Add(reader.ReadULong());
        }

        Location = reader.Read<RunLocation>();
    }
}

using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace Manosaba.Multiplayer.Messages.Game;

public sealed class WitchIslandExpeditionParryMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
    public uint promptId;
    public double pressElapsedSeconds;

    public bool ShouldBuffer => false;
    public bool ShouldBroadcast => false;
    public NetTransferMode Mode => NetTransferMode.Reliable;
    public LogLevel LogLevel => LogLevel.Debug;
    public RunLocation Location { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteUInt(promptId);
        writer.WriteDouble(pressElapsedSeconds);
        writer.Write(Location);
    }

    public void Deserialize(PacketReader reader)
    {
        promptId = reader.ReadUInt();
        pressElapsedSeconds = reader.ReadDouble();
        Location = reader.Read<RunLocation>();
    }
}

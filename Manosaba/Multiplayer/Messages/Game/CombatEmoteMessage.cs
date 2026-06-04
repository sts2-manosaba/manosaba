using BaseLib.Abstracts;
using Manosaba.Combat.Emotes;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace Manosaba.Multiplayer.Messages.Game;

public sealed class CombatEmoteMessage : ICustomMessage
{
    public string StickerId = string.Empty;

    public bool ShouldBroadcast => true;

    public NetTransferMode Mode => NetTransferMode.Unreliable;

    public LogLevel LogLevel => LogLevel.VeryDebug;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteString(StickerId);
    }

    public void Deserialize(PacketReader reader)
    {
        StickerId = reader.ReadString();
    }

    public void HandleMessage(ulong senderId)
    {
        CombatEmoteService.ShowFromNetwork(senderId, StickerId);
    }
}

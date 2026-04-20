using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace Manosaba.Multiplayer.Messages.Lobby;

/// <summary>選角階段 HOST 廣播的完整 Manosaba 大廳設定（難度 + 隨機池）。</summary>
public struct ManosabaDifficultySettingsMessage : INetMessage, IPacketSerializable
{
    public double enemyHpMultiplierPercent;

    public double murderousImpulseAllyDamageMultiplierPercent;

    /// <summary><see cref="RandomCharacterPoolMode"/> 數值。</summary>
    public byte randomCharacterPoolMode;

    public double enemyAttackDamageMultiplierPercent;

    public bool ShouldBroadcast => true;

    public NetTransferMode Mode => NetTransferMode.Reliable;

    public LogLevel LogLevel => LogLevel.VeryDebug;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteDouble(enemyHpMultiplierPercent);
        writer.WriteDouble(murderousImpulseAllyDamageMultiplierPercent);
        writer.WriteByte(randomCharacterPoolMode);
        writer.WriteDouble(enemyAttackDamageMultiplierPercent);
    }

    public void Deserialize(PacketReader reader)
    {
        enemyHpMultiplierPercent = reader.ReadDouble();
        murderousImpulseAllyDamageMultiplierPercent = reader.ReadDouble();
        randomCharacterPoolMode = reader.ReadByte();
        enemyAttackDamageMultiplierPercent = reader.ReadDouble();
    }
}

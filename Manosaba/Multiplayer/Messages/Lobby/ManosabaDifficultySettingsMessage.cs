using Manosaba.Config;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace Manosaba.Multiplayer.Messages.Lobby;

/// <summary>選角階段 HOST 廣播的完整 Manosaba 大廳設定（難度 + 隨機池）。</summary>
public struct ManosabaDifficultySettingsMessage : INetMessage, IPacketSerializable
{
    public bool enableEnemyHpMultiplier;

    public double enemyHpMultiplierPercent;

    public double murderousImpulseAllyDamageMultiplierPercent;

    /// <summary><see cref="RandomCharacterPoolMode"/> 數值。</summary>
    public byte randomCharacterPoolMode;

    public bool ShouldBroadcast => true;

    public NetTransferMode Mode => NetTransferMode.Reliable;

    public LogLevel LogLevel => LogLevel.VeryDebug;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteBool(enableEnemyHpMultiplier);
        writer.WriteDouble(enemyHpMultiplierPercent);
        writer.WriteDouble(murderousImpulseAllyDamageMultiplierPercent);
        writer.WriteByte(randomCharacterPoolMode);
    }

    public void Deserialize(PacketReader reader)
    {
        enableEnemyHpMultiplier = reader.ReadBool();
        enemyHpMultiplierPercent = reader.ReadDouble();
        murderousImpulseAllyDamageMultiplierPercent = reader.ReadDouble();
        randomCharacterPoolMode = reader.ReadByte();
    }
}

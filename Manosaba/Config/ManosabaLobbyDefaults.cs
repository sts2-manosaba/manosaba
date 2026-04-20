namespace Manosaba.Config;

/// <summary>未經選角同步時的內建預設（已不再寫入 mod 設定檔）。</summary>
public static class ManosabaLobbyDefaults
{
    /// <summary>100% = 倍率 1（不調整敵人血量）。</summary>
    public const double EnemyHpMultiplierPercent = 100d;

    /// <summary>100% = 倍率 1（不調整敵人打在玩家側的傷害）。</summary>
    public const double EnemyAttackDamageMultiplierPercent = 100d;

    public const double MurderousImpulseAllyDamageMultiplierPercent = 10d;

    public const RandomCharacterPoolMode RandomCharacterPool = RandomCharacterPoolMode.ManosabaCharactersOnly;
}

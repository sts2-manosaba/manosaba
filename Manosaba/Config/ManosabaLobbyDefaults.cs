namespace Manosaba.Config;

/// <summary>未經選角同步時的內建預設（已不再寫入 mod 設定檔）。</summary>
public static class ManosabaLobbyDefaults
{
    public const bool EnableEnemyHpMultiplier = false;

    public const double EnemyHpMultiplierPercent = 135d;

    public const double MurderousImpulseAllyDamageMultiplierPercent = 10d;

    public const RandomCharacterPoolMode RandomCharacterPool = RandomCharacterPoolMode.ManosabaCharactersOnly;
}

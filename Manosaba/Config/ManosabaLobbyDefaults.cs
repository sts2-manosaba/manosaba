using System;

namespace Manosaba.Config;

/// <summary>未經選角同步時的內建預設（已不再寫入 mod 設定檔）。</summary>
public static class ManosabaLobbyDefaults
{
    /// <summary>100% = 倍率 1（不調整敵人血量）。</summary>
    public static double EnemyHpMultiplierPercent =>
        Math.Clamp(ManosabaConfig.LobbyEnemyHpMultiplierPercent, 100d, 400d);

    /// <summary>100% = 倍率 1（不調整敵人打在玩家側的傷害）。</summary>
    public static double EnemyAttackDamageMultiplierPercent =>
        Math.Clamp(ManosabaConfig.LobbyEnemyAttackDamageMultiplierPercent, 100d, 400d);

    public static double MurderousImpulseAllyDamageMultiplierPercent =>
        Math.Clamp(ManosabaConfig.LobbyMurderousImpulseAllyDamageMultiplierPercent, 0d, 100d);

    public static RandomCharacterPoolMode RandomCharacterPool =>
        ManosabaConfig.LobbyRandomCharacterPool is RandomCharacterPoolMode.AllCharacters
            ? RandomCharacterPoolMode.AllCharacters
            : RandomCharacterPoolMode.ManosabaCharactersOnly;
}

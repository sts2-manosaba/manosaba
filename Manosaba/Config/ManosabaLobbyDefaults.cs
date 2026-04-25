using System;

namespace Manosaba.Config;

public static class ManosabaLobbyDefaults
{
    public const double SafeEnemyHpMultiplierPercent = 100d;
    public const double SafeEnemyAttackDamageMultiplierPercent = 100d;
    public const double SafeMurderousImpulseAllyDamageMultiplierPercent = 10d;
    public const RandomCharacterPoolMode SafeRandomCharacterPool = RandomCharacterPoolMode.ManosabaCharactersOnly;

    public static double EnemyHpMultiplierPercent =>
        Math.Clamp(ManosabaConfig.LobbyEnemyHpMultiplierPercent, 100d, 400d);

    public static double EnemyAttackDamageMultiplierPercent =>
        Math.Clamp(ManosabaConfig.LobbyEnemyAttackDamageMultiplierPercent, 100d, 400d);

    public static double MurderousImpulseAllyDamageMultiplierPercent =>
        Math.Clamp(ManosabaConfig.LobbyMurderousImpulseAllyDamageMultiplierPercent, 0d, 100d);

    public static RandomCharacterPoolMode RandomCharacterPool =>
        ManosabaConfig.LobbyRandomCharacterPool is RandomCharacterPoolMode.AllCharacters
            ? RandomCharacterPoolMode.AllCharacters
            : RandomCharacterPoolMode.ManosabaCharactersOnly;
}

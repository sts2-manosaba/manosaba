using BaseLib.Config;

namespace Manosaba.Config;

public enum ManosabaFxPlayMode
{
    EveryTime,
    OncePerRun,
    Never
}

public sealed class ManosabaConfig : SimpleModConfig
{
    [ConfigSection("SFXSettings")]
    [ConfigHoverTip]
    public static ManosabaFxPlayMode LaboursOfHiroEffectFrequency { get; set; } = ManosabaFxPlayMode.EveryTime;

    [ConfigHoverTip]
    public static ManosabaFxPlayMode HikamiMeruruExaidEffectFrequency { get; set; } = ManosabaFxPlayMode.EveryTime;

    [ConfigSection("DifficultySettings")]
    [ConfigHoverTip]
    public static bool EnableEnemyHpMultiplier { get; set; } = false;

    [SliderRange(100, 400, 5)]
    [SliderLabelFormat("{0:0}%")]
    [ConfigHoverTip]
    public static double EnemyHpMultiplierPercent { get; set; } = 135d;

    public static decimal GetEnemyHpMultiplier()
    {
        if (!EnableEnemyHpMultiplier)
        {
            return 1m;
        }

        double percent = EnemyHpMultiplierPercent;
        if (percent < 1d)
        {
            percent = 1d;
        }

        return (decimal)(percent / 100d);
    }
}

using BaseLib.Config;

namespace Manosaba.Config;

public enum LaboursOfHiroFxPlayMode
{
    EveryTime,
    OncePerRun,
    Never
}

public sealed class ManosabaConfig : SimpleModConfig
{
    [ConfigSection("LaboursOfHiroSettings")]
    [ConfigHoverTip]
    public static LaboursOfHiroFxPlayMode LaboursOfHiroEffectFrequency { get; set; } = LaboursOfHiroFxPlayMode.OncePerRun;
}

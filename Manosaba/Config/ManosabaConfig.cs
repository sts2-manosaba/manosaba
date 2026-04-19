using System;
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
    [SliderRange(0, 100, 1)]
    [SliderLabelFormat("{0:0}%")]
    [ConfigHoverTip]
    public static double ManosabaSfxVolumePercent { get; set; } = 100d;

    [ConfigHoverTip]
    public static ManosabaFxPlayMode LaboursOfHiroEffectFrequency { get; set; } = ManosabaFxPlayMode.EveryTime;

    [ConfigHoverTip]
    public static ManosabaFxPlayMode HikamiMeruruExaidEffectFrequency { get; set; } = ManosabaFxPlayMode.EveryTime;

    public static float GetManosabaSfxVolume()
    {
        return (float)Math.Clamp(ManosabaSfxVolumePercent / 100d, 0d, 1d);
    }
}

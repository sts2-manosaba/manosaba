using System;
using BaseLib.Config;

namespace Manosaba.Config;

public enum ManosabaFxPlayMode
{
    EveryTime,
    OncePerRun,
    Never
}

/// <summary>角色選擇畫面「隨機」按鈕抽籤範圍。</summary>
/// <remarks>
/// <b>必須讓「僅 Mod」為數值 0。</b>BaseLib 在設定檔缺欄位時會以 0 回填 enum；若 0 代表全角色，缺欄位時會變成原版隨機。
/// 多人連線時<b>所有玩家</b>必須選同一選項，否則開局同步可能失敗（與敵人血量等難度設定相同要求）。
/// </remarks>
public enum RandomCharacterPoolMode
{
    /// <summary>僅從本 Mod 自機（<c>manosaba.Characters</c> 命名空間下的角色類型）抽籤。</summary>
    ManosabaCharactersOnly = 0,

    /// <summary>與原版相同，從遊戲內全部可選角色抽籤。</summary>
    AllCharacters = 1,
}

public sealed class ManosabaConfig : SimpleModConfig
{
    [ConfigSection("CharacterSelect")]
    [ConfigHoverTip]
    public static RandomCharacterPoolMode RandomCharacterPool { get; set; } = RandomCharacterPoolMode.ManosabaCharactersOnly;

    [ConfigSection("SFXSettings")]
    [SliderRange(0, 100, 1)]
    [SliderLabelFormat("{0:0}%")]
    [ConfigHoverTip]
    public static double ManosabaSfxVolumePercent { get; set; } = 100d;

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

    [SliderRange(0, 100, 5)]
    [SliderLabelFormat("{0:0}%")]
    [ConfigHoverTip]
    public static double MurderousImpulseAllyDamageMultiplierPercent { get; set; } = 10d;

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

    public static decimal GetMurderousImpulseAllyDamageMultiplier()
    {
        double percent = MurderousImpulseAllyDamageMultiplierPercent;
        percent = Math.Clamp(percent, 0d, 100d);
        return (decimal)(percent / 100d);
    }

    public static float GetManosabaSfxVolume()
    {
        return (float)Math.Clamp(ManosabaSfxVolumePercent / 100d, 0d, 1d);
    }
}

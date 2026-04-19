namespace Manosaba.Config;

/// <summary>選角「隨機」占位結算時的角色池（由 HOST 在選角畫面同步）。</summary>
/// <remarks>
/// <b>必須讓「僅 Mod」為數值 0。</b>舊版設定檔曾以 0 回填 enum。
/// </remarks>
public enum RandomCharacterPoolMode : byte
{
    /// <summary>僅從本 Mod 自機抽籤。</summary>
    ManosabaCharactersOnly = 0,

    /// <summary>與原版相同，從全部可選角色抽籤。</summary>
    AllCharacters = 1,
}

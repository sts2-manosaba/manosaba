using Manosaba.Audio;

namespace manosaba.Characters.HoshoMago.Helpers;

/// <summary>
/// 與 <see cref="Manosaba.Characters.TachibanaSherry.Helpers.SherrySfx"/> 相同原理：掛在角色 <c>CustomAttackSfx</c> /
/// <c>CustomCastSfx</c> / <c>CustomDeathSfx</c>，並於 <c>Entry</c> 內 <c>CharacterSfxRegistry.Register</c>。
/// </summary>
public sealed class HoshoMagoSfx : CharacterSfxBase
{
    public static readonly HoshoMagoSfx Instance = new();

    // ===== A 區：角色層級（BaseLib CustomAttackSfx / CustomCastSfx；由 TriggerAnim Attack/Cast 觸發）=====
    // 建議路徑（與 Sherry 同層）：res://Manosaba/audio/SFX/hosho_mago/
    // public override string? CharacterAttack => "event:/Manosaba/audio/SFX/hosho_mago/attack.ogg";
    // public override string? CharacterCast => "event:/Manosaba/audio/SFX/hosho_mago/cast.ogg";
    // public override string? CharacterDeath => "event:/Manosaba/audio/SFX/hosho_mago/death.ogg";

    // ===== B 區：卡牌類型（卡牌 OnPlay 中直接呼叫 CharacterSfxBase.Play；多數 Attack 已走 TriggerAnim + A 區）=====
    // public override string? AttackCardHit => null;
    // public override string? SkillCardCast => null;
    // public override string? PowerCardActivate => null;

    // ===== C 區：戰鬥框架附加音效（原版音效保留，額外疊加）=====
    // public override string? BlockGain   => null;
    // public override string? BlockBreak  => null;
    // public override string? BlockHit    => null;
    // public override string? Heal        => null;
    // public override string? BuffApply   => null;
    // public override string? DebuffApply => null;
    // public override string? GainEnergy  => null;

    // ===== D 區：非戰鬥場景附加音效（原版音效保留，額外疊加）=====
    // --- 商店 ---
    // public override string? ShopWelcome    => null;
    // public override string? ShopThankYou   => null;
    // public override string? ShopCantAfford => null;
    // --- 地圖 ---
    // public override string? MapOpen       => null;
    // public override string? MapClose      => null;
    // public override string? MapSelectNode => null;
    // --- 寶箱 ---
    // public override string? TreasureOpen => null;
    // --- 金幣 ---
    // public override string? GoldGainSmall  => null;
    // public override string? GoldGainMedium => null;
    // public override string? GoldGainLarge  => null;
    // --- 戰鬥開始/結束 ---
    // public override string? CombatStart    => null;
    // public override string? CombatVictory  => null;
    // public override string? TurnStartPlayer => null;
    // public override string? TurnStartEnemy  => null;
    // --- 營火 ---
    // public override string? RestSiteHeal  => null;
    // public override string? RestSiteSmith => null;
    // --- 獎勵 ---
    // public override string? RelicGet  => null;
    // public override string? PotionGet => null;
    // --- 卡牌操作 ---
    // public override string? CardDraw    => null;
    // public override string? CardExhaust => null;
    // public override string? CardSelect  => null;
}

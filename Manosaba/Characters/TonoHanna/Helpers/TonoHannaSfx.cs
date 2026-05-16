using Manosaba.Audio;

namespace manosaba.Characters.TonoHanna.Helpers;

/// <summary>
/// 與 <see cref="Manosaba.Characters.TachibanaSherry.Helpers.SherrySfx"/> 相同原理：掛在角色 <c>CustomAttackSfx</c> /
/// <c>CustomCastSfx</c> / <c>CustomDeathSfx</c>，並於 <c>Entry</c> 內 <c>CharacterSfxRegistry.Register</c>。
/// </summary>
public sealed class TonoHannaSfx : CharacterSfxBase
{
    public static readonly TonoHannaSfx Instance = new();

    // ===== A 區：角色層級（BaseLib CustomAttackSfx / CustomCastSfx；由 TriggerAnim Attack/Cast 觸發）=====
    // 建議路徑（與 Sherry 同層）：res://Manosaba/audio/SFX/tono_hanna/
    private static readonly string?[] AttackPool =
    [
        "event:/Manosaba/audio/SFX/hanna/bi.ogg",
        "event:/Manosaba/audio/SFX/hanna/ho.ogg",
        "event:/Manosaba/audio/SFX/hanna/yeee.ogg",
    ];

    private static readonly string?[] CastPool =
    [
        "event:/Manosaba/audio/SFX/hanna/desuwadesuwa.ogg",
        "event:/Manosaba/audio/SFX/hanna/desuwane.ogg",
        "event:/Manosaba/audio/SFX/hanna/stopdesuwa.ogg",
    ];

    public override string? CharacterAttack => SfxPathPick.PickRandomNonEmpty(AttackPool);
    public override string? CharacterCast => SfxPathPick.PickRandomNonEmpty(CastPool);
    public override string? CharacterDeath => "event:/Manosaba/audio/SFX/hanna/muri.ogg";

    // ===== B 區：卡牌類型（卡牌 OnPlay 中直接呼叫 CharacterSfxBase.Play；多數 Attack 已走 TriggerAnim + A 區）=====
    // public override string? AttackCardHit => null;
    // public override string? SkillCardCast => null;
    // public override string? PowerCardActivate => null;

    // ===== C 區：戰鬥框架附加音效（原版音效保留，額外疊加）=====
    private static readonly string?[] BlockBreakPool =
    [
        "event:/Manosaba/audio/SFX/hanna/ah.ogg",
        "event:/Manosaba/audio/SFX/hanna/ge.ogg",
        "event:/Manosaba/audio/SFX/hanna/giii.ogg",
        "event:/Manosaba/audio/SFX/hanna/ni.ogg",
        "event:/Manosaba/audio/SFX/hanna/qq.ogg",
    ];

    private static readonly string?[] BlockHitPool =
    [
        "event:/Manosaba/audio/SFX/hanna/no.ogg",
        "event:/Manosaba/audio/SFX/hanna/souna.ogg",
    ];
    public override string? BlockBreak  => SfxPathPick.PickRandomNonEmpty(BlockBreakPool);
    public override string? BlockHit    => SfxPathPick.PickRandomNonEmpty(BlockHitPool);
    // public override string? BlockHit    => null;
    // public override string? Heal        => null;
    // public override string? BuffApply   => null;
    // public override string? DebuffApply => null;
    // public override string? GainEnergy  => null;

    // ===== D 區：非戰鬥場景附加音效（原版音效保留，額外疊加）=====
    // --- 商店 ---
    // public override string? ShopWelcome    => null;
    public override string? ShopThankYou   => "event:/Manosaba/audio/SFX/hanna/iikangaedesuwane.ogg";
    public override string? ShopCantAfford => "event:/Manosaba/audio/SFX/hanna/bakanano.ogg";
    // --- 地圖 ---
    // public override string? MapOpen       => null;
    // public override string? MapClose      => null;
    public override string? MapSelectNode => "event:/Manosaba/audio/SFX/hanna/eh.ogg";
    // --- 寶箱 ---
    public override string? TreasureOpen => "event:/Manosaba/audio/SFX/hanna/soredemosikasite.ogg";
    // --- 金幣 ---
    // public override string? GoldGainSmall  => null;
    // public override string? GoldGainMedium => null;
    // public override string? GoldGainLarge  => null;
    // --- 戰鬥開始/結束 ---
    // public override string? CombatStart    => null;
    public override string? CombatVictory  => "event:/Manosaba/audio/SFX/hanna/sonodouridesuwa.ogg";
    // public override string? TurnStartPlayer => null;
    // public override string? TurnStartEnemy  => null;
    // --- 營火 ---
    public override string? RestSiteHeal  => "event:/Manosaba/audio/SFX/hanna/warukuwanai.ogg";
    public override string? RestSiteSmith => "event:/Manosaba/audio/SFX/hanna/koredesuwa.ogg";
    // --- 獎勵 ---
    public override string? RelicGet  => "event:/Manosaba/audio/SFX/hanna/urayamasi.ogg";
    public override string? PotionGet => "event:/Manosaba/audio/SFX/hanna/ahahaha.ogg";
    // --- 卡牌操作 ---
    // public override string? CardDraw    => null;
    // public override string? CardExhaust => null;
    // public override string? CardSelect  => null;
}

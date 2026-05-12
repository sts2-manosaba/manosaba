using Manosaba.Audio;

namespace Manosaba.Characters.TachibanaSherry.Helpers;

/// <summary>
/// 角色戰鬥與 UI 疊加音效；於 <c>Entry</c> 內 <c>CharacterSfxRegistry.Register</c>，並由角色 model 的
/// <c>CustomAttackSfx</c> / <c>CustomCastSfx</c> / <c>CustomDeathSfx</c> 引用。其他角請見各 <c>*Sfx</c> 模板。
/// </summary>
public class SherrySfx : CharacterSfxBase
{
    public static readonly SherrySfx Instance = new();

    // ===== A 區：角色層級（BaseLib CustomAttackSfx / CustomCastSfx；由 TriggerAnim Attack/Cast 觸發）=====
    private static readonly string?[] AttackPool =
    [
        "event:/Manosaba/audio/SFX/sherry/pulia.ogg",
        "event:/Manosaba/audio/SFX/sherry/bonbon.MP3",
        "event:/Manosaba/audio/SFX/sherry/A.MP3",
        "event:/Manosaba/audio/SFX/sherry/AAAA.MP3",
        "event:/Manosaba/audio/SFX/sherry/unonono.MP3",
    ];

    private static readonly string?[] CastPool =
    [
        "event:/Manosaba/audio/SFX/sherry/huhuhu.MP3",
        "event:/Manosaba/audio/SFX/sherry/AAO.MP3",
        "event:/Manosaba/audio/SFX/sherry/miao.MP3",
        "event:/Manosaba/audio/SFX/sherry/pupu.MP3",
    ];

    private static readonly string?[] DeathPool = ["event:/Manosaba/audio/SFX/sherry/death.ogg"];

    public override string? CharacterAttack => SfxPathPick.PickRandomNonEmpty(AttackPool);
    public override string? CharacterCast => SfxPathPick.PickRandomNonEmpty(CastPool);
    public override string? CharacterDeath => SfxPathPick.PickRandomNonEmpty(DeathPool);

    // ===== B 區：卡牌類型（卡牌 OnPlay 中直接呼叫 CharacterSfxBase.Play；多數 Attack 已走 TriggerAnim + A 區）=====
    // public override string? AttackCardHit => null;
    // public override string? SkillCardCast => null;
    // public override string? PowerCardActivate => null;
    // 舊 Power 使用 AAO.MP3；現 Skill/Power 皆走 CharacterCast，若要能力不同聲需另設計（例如僅 Power 牌額外 SfxCmd）。

    // ===== C 區：戰鬥框架附加音效（原版音效保留，額外疊加）=====
    // public override string? BlockGain   => null;
    private static readonly string?[] BlockBreakPool =
    [
        "event:/Manosaba/audio/SFX/sherry/ahh.MP3",
        "event:/Manosaba/audio/SFX/sherry/qq.MP3",
        "event:/Manosaba/audio/SFX/sherry/kasu.ogg",
    ];
    public override string? BlockBreak  => SfxPathPick.PickRandomNonEmpty(BlockBreakPool);
    public override string? BlockHit    => "event:/Manosaba/audio/SFX/sherry/alie.ogg";
    // public override string? Heal        => null;
    // public override string? BuffApply   => null;
    // public override string? DebuffApply => null;
    // public override string? GainEnergy  => null;

    // ===== D 區：非戰鬥場景附加音效（原版音效保留，額外疊加）=====
    // --- 商店 ---
    // public override string? ShopWelcome    => null;
    public override string? ShopThankYou   => "event:/Manosaba/audio/SFX/sherry/soredesu.ogg";
    public override string? ShopCantAfford => "event:/Manosaba/audio/SFX/sherry/alala.ogg";
    // --- 地圖 ---
    // public override string? MapOpen       => "event:/Manosaba/audio/SFX/sherry/ho.ogg";
    // public override string? MapClose      => null;
    public override string? MapSelectNode => "event:/Manosaba/audio/SFX/sherry/hi.ogg";
    // --- 寶箱 ---
    public override string? TreasureOpen => "event:/Manosaba/audio/SFX/sherry/jiang.ogg";
    // --- 金幣 ---
    // public override string? GoldGainSmall  => null;
    // public override string? GoldGainMedium => null;
    // public override string? GoldGainLarge  => null;
    // --- 戰鬥開始/結束 ---
    // public override string? CombatStart    => null;
    public override string? CombatVictory  => "event:/Manosaba/audio/SFX/sherry/kawaii.MP3";
    // public override string? TurnStartPlayer => null;
    // public override string? TurnStartEnemy  => null;
    // --- 營火 ---
    public override string? RestSiteHeal  => "event:/Manosaba/audio/SFX/sherry/oyasumi.ogg";
    public override string? RestSiteSmith => "event:/Manosaba/audio/SFX/sherry/koredesuyo.ogg";
    // --- 獎勵 ---
    private static readonly string?[] RelicGetPool =
    [
        "event:/Manosaba/audio/SFX/sherry/yada.MP3",
        "event:/Manosaba/audio/SFX/sherry/medetashi.MP3",
        "event:/Manosaba/audio/SFX/sherry/sugoi.MP3",
    ];
    public override string? RelicGet  => SfxPathPick.PickRandomNonEmpty(RelicGetPool);
    public override string? PotionGet => "event:/Manosaba/audio/SFX/sherry/ruahaha.MP3";
    // --- 卡牌操作 ---
    // public override string? CardDraw    => null;
    // public override string? CardExhaust => null;
    // public override string? CardSelect  => "event:/Manosaba/audio/SFX/sherry/mmm.ogg";
}

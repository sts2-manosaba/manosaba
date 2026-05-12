using MegaCrit.Sts2.Core.Nodes.Audio;

namespace Manosaba.Audio;

public abstract class CharacterSfxBase
{
    // === A 區：角色層級（由 BaseLib Patch 自動觸發；多檔隨機見 SfxPathPick）===
    public virtual string? CharacterAttack => null;
    public virtual string? CharacterCast => null;
    public virtual string? CharacterDeath => null;

    // === B 區：卡牌類型（卡牌 OnPlay 中直接呼叫）===
    public virtual string? AttackCardHit => null;
    public virtual string? SkillCardCast => null;
    public virtual string? PowerCardActivate => null;

    // === C 區：戰鬥框架附加音效（Postfix 疊加在原版之後）===
    public virtual string? BlockGain => null;
    public virtual string? BlockBreak => null;
    public virtual string? BlockHit => null;
    public virtual string? Heal => null;
    public virtual string? BuffApply => null;
    public virtual string? DebuffApply => null;
    public virtual string? GainEnergy => null;

    // === D 區：非戰鬥場景附加音效（Postfix 疊加在原版之後）===
    // 商店
    public virtual string? ShopWelcome => null;
    public virtual string? ShopThankYou => null;
    public virtual string? ShopCantAfford => null;
    // 地圖
    public virtual string? MapOpen => null;
    public virtual string? MapClose => null;
    public virtual string? MapSelectNode => null;
    // 寶箱
    public virtual string? TreasureOpen => null;
    // 金幣
    public virtual string? GoldGainSmall => null;
    public virtual string? GoldGainMedium => null;
    public virtual string? GoldGainLarge => null;
    // 戰鬥開始/結束 (TmpSfx)
    public virtual string? CombatStart => null;
    public virtual string? CombatVictory => null;
    public virtual string? TurnStartPlayer => null;
    public virtual string? TurnStartEnemy => null;
    // 營火 (TmpSfx)
    public virtual string? RestSiteHeal => null;
    public virtual string? RestSiteSmith => null;
    // 獎勵 (TmpSfx)
    public virtual string? RelicGet => null;
    public virtual string? PotionGet => null;
    // 卡牌操作 (TmpSfx)
    public virtual string? CardDraw => null;
    public virtual string? CardExhaust => null;
    public virtual string? CardSelect => null;

    private Dictionary<string, Func<string?>>? _fmodMap;
    private Dictionary<string, Func<string?>>? _tmpSfxMap;

    protected CharacterSfxBase()
    {
        BuildReplacementMaps();
    }

    public static void Play(string? sfx, float volume = 1f)
    {
        if (!string.IsNullOrEmpty(sfx))
            MegaCrit.Sts2.Core.Commands.SfxCmd.Play(sfx, volume);
    }

    /// <summary>
    /// Bypasses SfxCmd to avoid triggering our own Postfix.
    /// Routes through NAudioManager → GodotSfxRouter for event:/Manosaba/ paths.
    /// </summary>
    public static void PlayDirect(string sfx, float volume = 1f)
    {
        NAudioManager.Instance?.PlayOneShot(sfx, volume);
    }

    public string? GetExtraSfx(string originalEvent)
    {
        if (_fmodMap != null && _fmodMap.TryGetValue(originalEvent, out var getter))
            return getter();
        return null;
    }

    public string? GetExtraTmpSfx(string originalFileName)
    {
        if (_tmpSfxMap != null && _tmpSfxMap.TryGetValue(originalFileName, out var getter))
            return getter();
        return null;
    }

    private void BuildReplacementMaps()
    {
        _fmodMap = new Dictionary<string, Func<string?>>
        {
            // C 區
            ["event:/sfx/block_gain"] = () => BlockGain,
            ["event:/sfx/block_break"] = () => BlockBreak,
            ["event:/sfx/block_hit"] = () => BlockHit,
            ["event:/sfx/heal"] = () => Heal,
            ["event:/sfx/buff"] = () => BuffApply,
            ["event:/sfx/debuff"] = () => DebuffApply,
            ["event:/sfx/ui/gain_energy"] = () => GainEnergy,
            // D 區 — 商店
            ["event:/sfx/npcs/merchant/merchant_welcome"] = () => ShopWelcome,
            ["event:/sfx/npcs/merchant/merchant_thank_yous"] = () => ShopThankYou,
            ["event:/sfx/npcs/merchant/merchant_dissapointment"] = () => ShopCantAfford,
            // D 區 — 地圖
            ["event:/sfx/ui/map/map_open"] = () => MapOpen,
            ["event:/sfx/ui/map/map_close"] = () => MapClose,
            ["event:/sfx/ui/map/map_select"] = () => MapSelectNode,
            // D 區 — 寶箱
            ["event:/sfx/ui/treasure/treasure_act1"] = () => TreasureOpen,
            ["event:/sfx/ui/treasure/treasure_act2"] = () => TreasureOpen,
            ["event:/sfx/ui/treasure/treasure_act3"] = () => TreasureOpen,
            // D 區 — 金幣
            ["event:/sfx/ui/gold/gold_1"] = () => GoldGainSmall,
            ["event:/sfx/ui/gold/gold_2"] = () => GoldGainMedium,
            ["event:/sfx/ui/gold/gold_3"] = () => GoldGainLarge,
        };

        _tmpSfxMap = new Dictionary<string, Func<string?>>
        {
            ["battle_start_1.mp3"] = () => CombatStart,
            ["battle_start_2.mp3"] = () => CombatStart,
            ["victory.mp3"] = () => CombatVictory,
            ["player_turn.mp3"] = () => TurnStartPlayer,
            ["enemy_turn.mp3"] = () => TurnStartEnemy,
            ["sleep.tres"] = () => RestSiteHeal,
            ["card_smith.mp3"] = () => RestSiteSmith,
            ["relic_get.mp3"] = () => RelicGet,
            ["gain_potion.mp3"] = () => PotionGet,
            ["card_deal.mp3"] = () => CardDraw,
            ["card_exhaust.mp3"] = () => CardExhaust,
            ["card_select.mp3"] = () => CardSelect,
        };
    }
}

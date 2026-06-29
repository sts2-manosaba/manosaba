using BaseLib.Utils;
using manosaba.Characters.SawatariCoco.Helper;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.SawatariCoco.Equipment;
using Manosaba.Characters.SawatariCoco.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace manosaba.Characters.SawatariCoco.Cards;

public abstract class EquipmentCardModel : PathCustomCardModel
{
    protected EquipmentCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected abstract EquipmentSlot Slot { get; }
    protected abstract EquipmentSeries Series { get; }
    protected abstract int EquipmentScore { get; }
    protected abstract CardTag SeriesTag { get; }

    internal EquipmentSlot EquipSlot => Slot;
    internal EquipmentSeries EquipSeries => Series;
    internal int EquipScore => EquipmentScore;

    protected override HashSet<CardTag> CanonicalTags => [SeriesTag];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        CreateSeriesTagHoverTip(),
        HoverTipFactory.FromPower<EquipmentScorePower>(),
        .. CardExtraHoverTips,
    ];

    private IHoverTip CreateSeriesTagHoverTip()
    {
        string entry = Series switch
        {
            EquipmentSeries.PunkCat => "MANOSABA-PUNK_CAT_EQUIPMENT",
            EquipmentSeries.Cybercat => "MANOSABA-CYBERCAT_EQUIPMENT",
            EquipmentSeries.MysteriousCat => "MANOSABA-MYSTERIOUS_CAT_EQUIPMENT",
            EquipmentSeries.CutieCats => "MANOSABA-CUTIE_CATS_EQUIPMENT",
            _ => throw new InvalidOperationException($"No equipment tag hover tip for series {Series}."),
        };

        return new HoverTip(
            new LocString("card_keywords", $"{entry}.title"),
            new LocString("card_keywords", $"{entry}.description"));
    }

    protected virtual IEnumerable<IHoverTip> CardExtraHoverTips => [];

    protected IEnumerable<DynamicVar> WithEquipmentScore(params DynamicVar[] vars)
    {
        yield return new DynamicVar("EquipmentScore", EquipmentScore);
        foreach (DynamicVar variable in vars)
        {
            yield return variable;
        }
    }

    /// <summary>
    /// <see cref="CalculatedDamageVar"/> requires <see cref="CalculationBaseVar"/> and <see cref="ExtraDamageVar"/> even when damage is entirely fan-count-based.
    /// </summary>
    protected IEnumerable<DynamicVar> WithEquipmentScoreFanCountDamage()
        => WithEquipmentScore(SawatariCocoCardDynamicVars.FanCountDamage().ToArray());

    /// <summary>
    /// <see cref="CalculatedBlockVar"/> requires <see cref="CalculationBaseVar"/> and <see cref="CalculationExtraVar"/> even when block is entirely fan-count-based.
    /// </summary>
    protected IEnumerable<DynamicVar> WithEquipmentScoreFanCountBlock()
        => WithEquipmentScore(SawatariCocoCardDynamicVars.FanCountBlock().ToArray());

    protected IEnumerable<DynamicVar> WithEquipmentScoreLiveStreamBonusDamage(decimal baseDamage, decimal bonusDamage)
        => WithEquipmentScore(SawatariCocoCardDynamicVars.LiveStreamBonusDamage(baseDamage, bonusDamage).ToArray());

    protected IEnumerable<DynamicVar> WithEquipmentScoreLiveStreamBonusBlock(decimal baseBlock, decimal bonusBlock)
        => WithEquipmentScore(SawatariCocoCardDynamicVars.LiveStreamBonusBlock(baseBlock, bonusBlock).ToArray());

    protected IEnumerable<DynamicVar> WithEquipmentScoreHidingBonusBlock(decimal baseBlock)
        => WithEquipmentScore(SawatariCocoCardDynamicVars.HidingBonusBlock(baseBlock).ToArray());

    protected sealed override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature is null)
        {
            return;
        }

        await SawatariCocoEquipmentHelper.TryEquipFromCardAsync(choiceContext, Owner, this);

        await PlayEquipmentEffectAsync(choiceContext, cardPlay);
    }

    protected abstract Task PlayEquipmentEffectAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay);

    protected static int GetLiveStreamHitBonus(Creature creature)
        => (int)creature.GetPowerAmount<LiveStreamModePower>();

    protected static bool IsInLiveStreamMode(Creature creature)
        => SawatariCocoHelper.IsInLiveStreamMode(creature);
}

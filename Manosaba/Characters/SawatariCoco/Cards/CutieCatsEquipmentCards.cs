using BaseLib.Utils;
using manosaba.Characters.SawatariCoco;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.SawatariCoco.Equipment;
using Manosaba.Characters.SawatariCoco.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace manosaba.Characters.SawatariCoco.Cards;

[Pool(typeof(SawatariCocoCardPool))]
public sealed class CutieCatsHeadwear : EquipmentCardModel
{
    protected override EquipmentSlot Slot => EquipmentSlot.Headwear;
    protected override EquipmentSeries Series => EquipmentSeries.CutieCats;
    protected override int EquipmentScore => 2000;
    protected override CardTag SeriesTag => ManosabaCardTags.CutieCatsEquipment;
    protected override IEnumerable<IHoverTip> CardExtraHoverTips =>
    [
        HoverTipFactory.FromPower<LiveStreamModePower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => WithEquipmentScore(new PowerVar<WeakPower>(1m));

    public CutieCatsHeadwear() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task PlayEquipmentEffectAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target is not { } target)
        {
            return;
        }

        await CommonActions.Apply<WeakPower>(choiceContext, target, this, DynamicVars["WeakPower"].BaseValue);

        CardModel? selected = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1),
            null,
            this)).FirstOrDefault();

        if (selected != null)
        {
            await CardCmd.Exhaust(choiceContext, selected);
        }

        if (IsInLiveStreamMode(Owner.Creature))
        {
            await CommonActions.Apply<VulnerablePower>(choiceContext, target, this, 1m);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WeakPower"].UpgradeValueBy(1m);
    }
}

[Pool(typeof(SawatariCocoCardPool))]
public sealed class CutieCatsTop : EquipmentCardModel
{
    protected override EquipmentSlot Slot => EquipmentSlot.Top;
    protected override EquipmentSeries Series => EquipmentSeries.CutieCats;
    protected override int EquipmentScore => 1000;
    protected override CardTag SeriesTag => ManosabaCardTags.CutieCatsEquipment;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> CardExtraHoverTips =>
    [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<WeakPower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => WithEquipmentScore(
        new PowerVar<VulnerablePower>(1m),
        new PowerVar<WeakPower>(1m));

    public CutieCatsTop() : base(0, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task PlayEquipmentEffectAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target is not { } target)
        {
            return;
        }

        await CommonActions.Apply<VulnerablePower>(choiceContext, target, this, DynamicVars["VulnerablePower"].BaseValue);
        await CommonActions.Apply<WeakPower>(choiceContext, target, this, DynamicVars["WeakPower"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

[Pool(typeof(SawatariCocoCardPool))]
public sealed class CutieCatsPants : EquipmentCardModel
{
    protected override EquipmentSlot Slot => EquipmentSlot.Skirt;
    protected override EquipmentSeries Series => EquipmentSeries.CutieCats;
    protected override int EquipmentScore => 2000;
    protected override CardTag SeriesTag => ManosabaCardTags.CutieCatsEquipment;
    protected override IEnumerable<IHoverTip> CardExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PoisonPower>(),
        HoverTipFactory.FromPower<BurnPower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => WithEquipmentScore(
        new PowerVar<PoisonPower>(3m),
        new PowerVar<BurnPower>(3m));

    public CutieCatsPants() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
    }

    protected override async Task PlayEquipmentEffectAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        if (CombatState is not { } combatState)
        {
            return;
        }

        foreach (Creature enemy in combatState.GetOpponentsOf(Owner.Creature).Where(e => e.IsAlive && e.CanReceivePowers))
        {
            await CommonActions.Apply<PoisonPower>(choiceContext, enemy, this, DynamicVars["PoisonPower"].BaseValue);
            await CommonActions.Apply<BurnPower>(choiceContext, enemy, this, DynamicVars["BurnPower"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PoisonPower"].UpgradeValueBy(2m);
        DynamicVars["BurnPower"].UpgradeValueBy(2m);
    }
}

[Pool(typeof(SawatariCocoCardPool))]
public sealed class CutieCatsShoes : EquipmentCardModel
{
    protected override EquipmentSlot Slot => EquipmentSlot.Shoes;
    protected override EquipmentSeries Series => EquipmentSeries.CutieCats;
    protected override int EquipmentScore => 5000;
    protected override CardTag SeriesTag => ManosabaCardTags.CutieCatsEquipment;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> CardExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Stun)];

    public CutieCatsShoes() : base(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => WithEquipmentScore();

    protected override async Task PlayEquipmentEffectAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target is not { } target || target.Monster?.IntendsToAttack != true)
        {
            return;
        }

        await CreatureCmd.Stun(target);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

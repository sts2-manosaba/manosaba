using BaseLib.Utils;
using manosaba.Characters.SawatariCoco;
using manosaba.Characters.SawatariCoco.Helper;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.SawatariCoco.Equipment;
using Manosaba.Characters.SawatariCoco.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace manosaba.Characters.SawatariCoco.Cards;

[Pool(typeof(SawatariCocoCardPool))]
public sealed class MysteriousCatHeadwear : EquipmentCardModel
{
    protected override EquipmentSlot Slot => EquipmentSlot.Headwear;
    protected override EquipmentSeries Series => EquipmentSeries.MysteriousCat;
    protected override int EquipmentScore => 1000;
    protected override CardTag SeriesTag => ManosabaCardTags.MysteriousCatEquipment;
    protected override IEnumerable<IHoverTip> CardExtraHoverTips => [HoverTipFactory.FromPower<HidingPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars => WithEquipmentScore(
        new PowerVar<HidingPower>(1m),
        new CardsVar(1));

    public MysteriousCatHeadwear() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task PlayEquipmentEffectAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        await CommonActions.Apply<HidingPower>(choiceContext, Owner.Creature, this, DynamicVars["HidingPower"].BaseValue);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}

[Pool(typeof(SawatariCocoCardPool))]
public sealed class MysteriousCatTop : EquipmentCardModel
{
    protected override EquipmentSlot Slot => EquipmentSlot.Top;
    protected override EquipmentSeries Series => EquipmentSeries.MysteriousCat;
    protected override int EquipmentScore => 2000;
    protected override CardTag SeriesTag => ManosabaCardTags.MysteriousCatEquipment;
    protected override IEnumerable<IHoverTip> CardExtraHoverTips => [HoverTipFactory.FromPower<HidingPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars => WithEquipmentScore(
        new PowerVar<HidingPower>(1m),
        new CardsVar(1));

    public MysteriousCatTop() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task PlayEquipmentEffectAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        await CommonActions.Apply<HidingPower>(choiceContext, Owner.Creature, this, DynamicVars["HidingPower"].BaseValue);

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

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}

[Pool(typeof(SawatariCocoCardPool))]
public sealed class MysteriousCatPants : EquipmentCardModel
{
    protected override EquipmentSlot Slot => EquipmentSlot.Skirt;
    protected override EquipmentSeries Series => EquipmentSeries.MysteriousCat;
    protected override int EquipmentScore => 5000;
    protected override CardTag SeriesTag => ManosabaCardTags.MysteriousCatEquipment;
    protected override IEnumerable<IHoverTip> CardExtraHoverTips => [HoverTipFactory.FromPower<FanPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars => WithEquipmentScore(
        new CardsVar(1),
        new EnergyVar(1),
        new CalculationBaseVar(0m),
        new SawatariCocoCardDynamicVars.EnemyFanTotalDrawsVar(),
        new SawatariCocoCardDynamicVars.EnemyFanTotalEnergyVar());

    public MysteriousCatPants() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task PlayEquipmentEffectAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        if (CombatState is not { } combatState)
        {
            return;
        }

        int fanCount = SawatariCocoHelper.CountFansOf(Owner.Creature, combatState);

        if (fanCount <= 0)
        {
            return;
        }

        int drawCount = DynamicVars.Cards.IntValue;
        for (int i = 0; i < fanCount; i++)
        {
            await CardPileCmd.Draw(choiceContext, drawCount, Owner);
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}

[Pool(typeof(SawatariCocoCardPool))]
public sealed class MysteriousCatShoes : EquipmentCardModel
{
    protected override EquipmentSlot Slot => EquipmentSlot.Shoes;
    protected override EquipmentSeries Series => EquipmentSeries.MysteriousCat;
    protected override int EquipmentScore => 2000;
    protected override CardTag SeriesTag => ManosabaCardTags.MysteriousCatEquipment;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> CardExtraHoverTips => [HoverTipFactory.FromPower<LiveStreamModePower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars => WithEquipmentScore(new EnergyVar(1));

    public MysteriousCatShoes() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task PlayEquipmentEffectAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        if (!IsInLiveStreamMode(Owner.Creature))
        {
            return;
        }

        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

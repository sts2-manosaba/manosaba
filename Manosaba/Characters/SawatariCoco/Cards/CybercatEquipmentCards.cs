using BaseLib.Utils;
using manosaba.Characters.SawatariCoco;
using manosaba.Characters.SawatariCoco.Helper;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.SawatariCoco.Equipment;
using Manosaba.Characters.SawatariCoco.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace manosaba.Characters.SawatariCoco.Cards;

[Pool(typeof(SawatariCocoCardPool))]
public sealed class CybercatHeadwear : EquipmentCardModel
{
    public override bool GainsBlock => true;

    protected override EquipmentSlot Slot => EquipmentSlot.Headwear;
    protected override EquipmentSeries Series => EquipmentSeries.Cybercat;
    protected override int EquipmentScore => 5000;
    protected override CardTag SeriesTag => ManosabaCardTags.CybercatEquipment;
    protected override string PieceDisplayName => "電幻貓咪頭飾";

    protected override IEnumerable<DynamicVar> CanonicalVars => WithEquipmentScoreFanCountBlock();

    public CybercatHeadwear() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task PlayEquipmentEffectAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        if (CombatState is not { } combatState)
        {
            return;
        }

        decimal block = SawatariCocoHelper.GetTotalFanCount(Owner);
        if (block <= 0m)
        {
            return;
        }

        foreach (Player player in combatState.Players)
        {
            if (player.Creature is { IsAlive: true } creature)
            {
                await CreatureCmd.GainBlock(creature, block, ValueProp.Move, cardPlay);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

[Pool(typeof(SawatariCocoCardPool))]
public sealed class CybercatTop : EquipmentCardModel
{
    public override bool GainsBlock => true;

    protected override EquipmentSlot Slot => EquipmentSlot.Top;
    protected override EquipmentSeries Series => EquipmentSeries.Cybercat;
    protected override int EquipmentScore => 2000;
    protected override CardTag SeriesTag => ManosabaCardTags.CybercatEquipment;
    protected override string PieceDisplayName => "電幻貓咪上衣";

    protected override IEnumerable<IHoverTip> CardExtraHoverTips => [HoverTipFactory.FromPower<HidingPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars => WithEquipmentScore(
        new BlockVar(10, ValueProp.Move),
        new PowerVar<HidingPower>(2m));

    public CybercatTop() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task PlayEquipmentEffectAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await CommonActions.Apply<HidingPower>(choiceContext, Owner.Creature, this, DynamicVars["HidingPower"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(5m);
    }
}

[Pool(typeof(SawatariCocoCardPool))]
public sealed class CybercatSkirt : EquipmentCardModel
{
    public override bool GainsBlock => true;

    protected override EquipmentSlot Slot => EquipmentSlot.Skirt;
    protected override EquipmentSeries Series => EquipmentSeries.Cybercat;
    protected override int EquipmentScore => 1000;
    protected override CardTag SeriesTag => ManosabaCardTags.CybercatEquipment;
    protected override string PieceDisplayName => "電幻貓咪裙子";

    protected override IEnumerable<IHoverTip> CardExtraHoverTips => [HoverTipFactory.FromPower<HidingPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars => WithEquipmentScore(
        new BlockVar(5, ValueProp.Move),
        new CardsVar(1));

    public CybercatSkirt() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task PlayEquipmentEffectAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        decimal hiding = Owner.Creature.GetPowerAmount<HidingPower>();
        decimal block = DynamicVars.Block.BaseValue + hiding;
        await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, cardPlay);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}

[Pool(typeof(SawatariCocoCardPool))]
public sealed class CybercatShoes : EquipmentCardModel
{
    private const decimal liveModeBonusBlock = 4m;

    public override bool GainsBlock => true;

    protected override EquipmentSlot Slot => EquipmentSlot.Shoes;
    protected override EquipmentSeries Series => EquipmentSeries.Cybercat;
    protected override int EquipmentScore => 2000;
    protected override CardTag SeriesTag => ManosabaCardTags.CybercatEquipment;
    protected override string PieceDisplayName => "電幻貓咪鞋子";

    protected override IEnumerable<IHoverTip> CardExtraHoverTips => [HoverTipFactory.FromPower<LiveStreamModePower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars => WithEquipmentScore(
        new BlockVar(8, ValueProp.Move),
        new DynamicVar("LiveModeBonusBlock", liveModeBonusBlock));

    public CybercatShoes() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task PlayEquipmentEffectAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        decimal block = DynamicVars.Block.BaseValue;
        if (IsInLiveStreamMode(Owner.Creature))
        {
            block += DynamicVars["LiveModeBonusBlock"].BaseValue;
        }

        await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
    }
}

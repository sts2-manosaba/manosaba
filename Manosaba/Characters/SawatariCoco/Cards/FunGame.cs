using BaseLib.Utils;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace manosaba.Characters.SawatariCoco.Cards;

[Pool(typeof(TokenCardPool))]
public sealed class FunGame : ShoppingCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const TargetType targetType = TargetType.Self;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => WithGoldCost(
        new PowerVar<StrengthPower>(1m),
        new BlockVar(6, ValueProp.Move));

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
    ];

    public FunGame() : base(energyCost, type, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PayGoldCostAsync();
        await CommonActions.Apply<StrengthPower>(choiceContext, Owner.Creature, this, DynamicVars["StrengthPower"].BaseValue);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthPower"].UpgradeValueBy(1m);
    }
}

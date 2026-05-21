using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.HasumiLeia.Cards;

[Pool(typeof(HasumiLeiaCardPool))]
public sealed class PrisonForTwo : PathCustomCardModel
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyAlly;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PrisonPower>(),
        HoverTipFactory.FromPower<MurderousImpulsePower>(),
        HoverTipFactory.FromPower<PrisonForTwoPower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<PrisonPower>(15m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Unique];

    public PrisonForTwo()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;

        if (Owner.Creature is not { } ownerCreature || cardPlay.Target is not { } teammate)
        {
            return;
        }

        decimal prisonAmount = DynamicVars["PrisonPower"].BaseValue;
        await CommonActions.Apply<PrisonPower>(choiceContext, ownerCreature, this, prisonAmount);
        await CommonActions.Apply<PrisonPower>(choiceContext, teammate, this, prisonAmount);

        await CommonActions.Apply<PrisonForTwoPower>(choiceContext, ownerCreature, this, 1m, silent: true);
        await CommonActions.Apply<PrisonForTwoPower>(choiceContext, teammate, this, 1m, silent: true);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PrisonPower"].UpgradeValueBy(5m);
    }
}

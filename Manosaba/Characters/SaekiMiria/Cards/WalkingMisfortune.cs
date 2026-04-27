using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using Manosaba.Characters.SaekiMiria.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(SaekiMiriaCardPool))]
public sealed class WalkingMisfortune : PathCustomCardModel
{
    private new const int EnergyCost = 1;
    private const CardType CardTypeValue = CardType.Power;
    private new const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType TargetTypeValue = TargetType.Self;
    private new const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DemisePower>(),
        HoverTipFactory.FromPower<WalkingMisfortunePower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<DemisePower>(5m),
        new PowerVar<WalkingMisfortunePower>(1m),
    ];

    public WalkingMisfortune()
        : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        await PowerCmd.Apply<DemisePower>(Owner.Creature, DynamicVars["DemisePower"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<WalkingMisfortunePower>(Owner.Creature, DynamicVars["WalkingMisfortunePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}

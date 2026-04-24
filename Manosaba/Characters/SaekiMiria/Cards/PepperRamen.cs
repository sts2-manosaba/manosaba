using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(SaekiMiriaCardPool))]
public sealed class PepperRamen : PathCustomCardModel
{
    private const int EnergyCost = 1;
    private const CardType CardTypeValue = CardType.Skill;
    private const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType TargetTypeValue = TargetType.Self;
    private const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<RegenPower>(),
        HoverTipFactory.FromPower<PoisonPower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<RegenPower>(6m),
        new PowerVar<PoisonPower>(4m),
    ];

    public PepperRamen()
        : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        await PowerCmd.Apply<RegenPower>(Owner.Creature, DynamicVars["RegenPower"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<PoisonPower>(Owner.Creature, DynamicVars.Poison.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["RegenPower"].UpgradeValueBy(3m);
        DynamicVars["PoisonPower"].UpgradeValueBy(3m);
    }
}

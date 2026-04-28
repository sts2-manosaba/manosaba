using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace manosaba.Characters.NatsumeAnan.Cards;

[Pool(typeof(NatsumeAnanCardPool))]
public sealed class Endurance : NatsumeKotodamaCardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        .. base.ExtraHoverTips,
        HoverTipFactory.FromCard<Dazed>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("KotodamaGain", 4m)];

    public Endurance() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        KotodamaEnergy.Gain(Owner, DynamicVars["KotodamaGain"].IntValue);

        var combatState = CombatState;
        if (combatState == null)
        {
            return;
        }

        CardModel dazed = combatState.CreateCard<Dazed>(Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(dazed, PileType.Discard, addedByPlayer: true));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["KotodamaGain"].UpgradeValueBy(1m);
    }
}

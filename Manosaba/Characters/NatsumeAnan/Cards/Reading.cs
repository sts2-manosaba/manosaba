using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace manosaba.Characters.NatsumeAnan.Cards;

[Pool(typeof(NatsumeAnanCardPool))]
public sealed class Reading : NatsumeKotodamaCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("KotodamaGain", 1m),
        new CardsVar(1),
    ];

    public Reading() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        KotodamaEnergy.Gain(Owner, DynamicVars["KotodamaGain"].IntValue);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["KotodamaGain"].UpgradeValueBy(1m);
    }
}

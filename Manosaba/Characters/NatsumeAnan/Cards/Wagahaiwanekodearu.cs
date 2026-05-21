using BaseLib.Utils;
using manosaba.Characters.NatsumeAnan.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace manosaba.Characters.NatsumeAnan.Cards;

[Pool(typeof(NatsumeAnanCardPool))]
public sealed class Wagahaiwanekodearu : NatsumeKotodamaCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<CatPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        .. base.ExtraHoverTips,
        HoverTipFactory.FromPower<CatPower>(),
    ];

    public Wagahaiwanekodearu() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        await CommonActions.Apply<CatPower>(choiceContext, Owner.Creature, this, DynamicVars["CatPower"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

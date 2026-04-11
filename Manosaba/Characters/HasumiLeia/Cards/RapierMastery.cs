using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using Manosaba.Characters.HasumiLeia.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.HasumiLeia.Cards;

[Pool(typeof(HasumiLeiaCardPool))]
public sealed class RapierMastery : PathCustomCardModel
{
    private const int EnergyCost = 1;
    private const CardType CardTypeValue = CardType.Power;
    private const CardRarity Rarity = CardRarity.Rare;
    private const TargetType TargetTypeValue = TargetType.Self;
    private const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<RapierMasteryPower>()];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<RapierMasteryPower>(1m)];

    public RapierMastery()
        : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        PowerCmd.Apply<RapierMasteryPower>(Owner.Creature, DynamicVars["RapierMasteryPower"].BaseValue, Owner.Creature, this);

    protected override void OnUpgrade()
    {
        DynamicVars["RapierMasteryPower"].UpgradeValueBy(1);
    }
}


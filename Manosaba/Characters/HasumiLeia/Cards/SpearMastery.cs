using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.HasumiLeia.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.HasumiLeia.Cards;

[Pool(typeof(HasumiLeiaCardPool))]
public sealed class SpearMastery : PathCustomCardModel
{
    private const int energyCost = 1;
    private const CardType CardTypeValue = CardType.Power;
    private new const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType TargetTypeValue = TargetType.Self;
    private new const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<SpearMasteryPower>(),
        HoverTipFactory.FromCard<SimpleSpear>(true),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<SpearMasteryPower>(1m)];

    public SpearMastery()
        : base(energyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        PowerCmd.Apply<SpearMasteryPower>(Owner.Creature, DynamicVars["SpearMasteryPower"].BaseValue, Owner.Creature, this);

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

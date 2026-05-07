using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using Manosaba.Characters.HasumiLeia.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.HasumiLeia.Cards;

[Pool(typeof(HasumiLeiaCardPool))]
public sealed class Crowning : PathCustomCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<CrowningPower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<CrowningPower>(1m)];

    public Crowning()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        PowerCmd.Apply<CrowningPower>(Owner.Creature, DynamicVars["CrowningPower"].BaseValue, Owner.Creature, this);

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

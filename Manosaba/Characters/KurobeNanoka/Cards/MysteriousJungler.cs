using BaseLib.Utils;
using manosaba.Characters.KurobeNanoka;
using Manosaba.Extensions;
using Manosaba.Characters.KurobeNanoka.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.KurobeNanoka.Cards;

[Pool(typeof(KurobeNanokaCardPool))]
public sealed class MysteriousJungler : PathCustomCardModel
{
    private const string DexterityLossVar = "DexterityLoss";

    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.ForEnergy(this),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(DexterityLossVar, 2m),
        new EnergyVar(1),
    ];

    public MysteriousJungler()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        await CommonActions.Apply<DexterityPower>(choiceContext, Owner.Creature, this, -DynamicVars[DexterityLossVar].BaseValue);
        await CommonActions.Apply<MysteriousJunglerPower>(choiceContext, Owner.Creature, this, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[DexterityLossVar].UpgradeValueBy(-1m);
    }
}

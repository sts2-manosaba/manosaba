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

    private new const int EnergyCost = 1;
    private const CardType CardTypeValue = CardType.Power;
    private new const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType TargetTypeValue = TargetType.Self;
    private new const bool ShouldShowInCardLibrary = true;

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
        : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        await PowerCmd.Apply<DexterityPower>(Owner.Creature, -DynamicVars[DexterityLossVar].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<MysteriousJunglerPower>(Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[DexterityLossVar].UpgradeValueBy(-1m);
    }
}

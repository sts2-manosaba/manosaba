using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(SaekiMiriaCardPool))]
public sealed class Makeup : PathCustomCardModel
{
    private const string platingPowerVar = "PlatingPower";
    private const string unluckyPowerVar = "UnluckyPower";
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PlatingPower>(),
        HoverTipFactory.FromPower<UnluckyPower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<PlatingPower>(7m),
        new PowerVar<UnluckyPower>(2m),
    ];

    public Makeup()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        await CommonActions.Apply<PlatingPower>(choiceContext, Owner.Creature, this, DynamicVars[platingPowerVar].BaseValue);
        await CommonActions.Apply<UnluckyPower>(choiceContext, Owner.Creature, this, DynamicVars[unluckyPowerVar].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[platingPowerVar].UpgradeValueBy(3m);
        DynamicVars[unluckyPowerVar].UpgradeValueBy(1m);
    }
}

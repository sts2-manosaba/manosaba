using BaseLib.Utils;
using manosaba.Characters.SawatariCoco;
using Manosaba.Characters.SawatariCoco.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace manosaba.Characters.SawatariCoco.Cards;

[Pool(typeof(SawatariCocoCardPool))]
public sealed class ThanksForTheTip : PathCustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IfUpgradedVar(UpgradeDisplay.Normal)];

    public ThanksForTheTip() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        int gold = ThanksForTheTipPower.GetGoldForApplication(Owner, IsUpgraded);
        if (gold <= 0)
        {
            return;
        }

        await CommonActions.Apply<ThanksForTheTipPower>(choiceContext, Owner.Creature, this, gold);
    }
}

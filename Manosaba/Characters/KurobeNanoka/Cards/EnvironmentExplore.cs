using BaseLib.Utils;
using manosaba.Characters.KurobeNanoka;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.KurobeNanoka.Cards;

[Pool(typeof(KurobeNanokaCardPool))]
public sealed class EnvironmentExplore : PathCustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [EnergyHoverTip];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
        new DynamicVar("BonusPerGun", 1),
        new EnergyVar(1),
    ];

    public EnvironmentExplore() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        List<CardModel> gunCardsInHand = PileType.Hand
            .GetPile(Owner)
            .Cards
            .Where(card => card is GunBase)
            .ToList();

        if (gunCardsInHand.Count > 0)
        {
            await CardCmd.Discard(choiceContext, gunCardsInHand);
        }

        decimal bonusPerGun = DynamicVars["BonusPerGun"].BaseValue;
        decimal discardedGunCount = gunCardsInHand.Count;

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

        if (discardedGunCount > 0)
        {
            decimal totalBonus = discardedGunCount * bonusPerGun;
            await CommonActions.Apply<DrawCardsNextTurnPower>(choiceContext, Owner.Creature, this, totalBonus);
            await CommonActions.Apply<EnergyNextTurnPower>(choiceContext, Owner.Creature, this, totalBonus);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}

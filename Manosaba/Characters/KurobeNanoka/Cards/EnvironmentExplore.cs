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
    private const int EnergyCost = 1;
    private const CardType Type = CardType.Skill;
    private const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType TargetTypeValue = TargetType.Self;
    private const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [EnergyHoverTip];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
        new DynamicVar("BonusPerGun", 1),
        new EnergyVar(1),
    ];

    public EnvironmentExplore() : base(EnergyCost, Type, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
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

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue + discardedGunCount * bonusPerGun, Owner);

        if (discardedGunCount > 0)
        {
            await PlayerCmd.GainEnergy(discardedGunCount * bonusPerGun, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}

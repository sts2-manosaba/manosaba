using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;

namespace Manosaba.Characters.HikamiMeruru.Cards;

[Pool(typeof(HikamiMeruruCardPool))]
public class PrimalPotionPower : PathCustomCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPotion<PotionShapedRock>()];

    public PrimalPotionPower() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        List<PotionModel> potions = Owner.Potions
            .Where(potion => !potion.IsQueued)
            .ToList();

        foreach (PotionModel potion in potions)
        {
            int slotIndex = Owner.GetPotionSlotIndex(potion);
            potion.Discard();
            Owner.AddPotionInternal(ModelDb.Potion<PotionShapedRock>().ToMutable(), slotIndex);
        }

        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}

using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Characters.HikamiMeruru.PotionCraft;
using Manosaba.Characters.HikamiMeruru.Potions;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.Common.Cards
{
    [Pool(typeof(HikamiMeruruCardPool))]
    public class MixPotions : PathCustomCardModel
    {

        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Basic;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPotion<Catalyst>()];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("CraftCount", 1), new DynamicVar("CatalystChance", 10)];

        public MixPotions() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            for (int i = 0; i < DynamicVars["CraftCount"].IntValue; i++)
            {
                var recipe = PotionCraftService.FindFirstCraftableRecipe(Owner.PotionSlots);
                await PotionCraftService.TryCraft(Owner, Owner.PotionSlots, recipe);
            }
            if (Owner.RunState.Rng.CombatPotionGeneration.NextInt(100) < DynamicVars["CatalystChance"].IntValue)
            {
                await PotionCmd.TryToProcure<Catalyst>(Owner);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars["CraftCount"].UpgradeValueBy(1m);
            DynamicVars["CatalystChance"].UpgradeValueBy(10m);
        }
    }
}

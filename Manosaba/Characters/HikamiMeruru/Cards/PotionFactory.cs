using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Characters.HikamiMeruru.Potions;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;

namespace Manosaba.Characters.HikamiMeruru.Cards
{
    [Pool(typeof(HikamiMeruruCardPool))]
    public class PotionFactory : PathCustomCardModel
    {

        private const int energyCost = 2;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPotion<Catalyst>()];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3), new DynamicVar("Catalyst", 2)];

        private readonly List<PotionModel> PotionPool =
        [
            ModelDb.Potion<BlockPotion>(),
            ModelDb.Potion<PainKillerPotion>(),
            ModelDb.Potion<FlexPotion>(),
            ModelDb.Potion<BeetleJuice>(),
            ModelDb.Potion<EnergyPotion>(),
        ];
        public PotionFactory() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            for (int i = 0; i < base.DynamicVars.Cards.IntValue; i++)
            {
                PotionModel potionModel = PotionPool[Owner.RunState.Rng.CombatPotionGeneration.NextInt(PotionPool.Count)];
                await PotionCmd.TryToProcure(potionModel.ToMutable(), Owner);
            }
            for (int i = 0; i < base.DynamicVars["Catalyst"].IntValue; i++)
            {
                await PotionCmd.TryToProcure<Catalyst>(Owner);
            }
        }


        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }
    }
}

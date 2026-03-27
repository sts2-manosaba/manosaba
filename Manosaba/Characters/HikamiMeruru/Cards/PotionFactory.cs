using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Characters.HikamiMeruru.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

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
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PotionFactoryPower>()];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<PotionFactoryPower>(2)];

        public PotionFactory() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {

        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<PotionFactoryPower>(base.Owner.Creature, base.DynamicVars["PotionFactoryPower"].BaseValue, base.Owner.Creature, this);

        }


        protected override void OnUpgrade()
        {
            base.DynamicVars["PotionFactoryPower"].UpgradeValueBy(1m);
        }
    }
}

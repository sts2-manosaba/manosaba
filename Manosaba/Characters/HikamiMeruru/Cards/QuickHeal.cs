using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HikamiMeruru.Cards
{
    [Pool(typeof(HikamiMeruruCardPool))]
    public class QuickHeal : PathCustomCardModel
    {
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.AnyPlayer;
        private const bool shouldShowInCardLibrary = true;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(5), new BlockVar(7m, ValueProp.Move)];
        public QuickHeal() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target != null)
            {
                await CreatureCmd.Heal(cardPlay.Target, base.DynamicVars.Heal.BaseValue);
                await CreatureCmd.GainBlock(cardPlay.Target, DynamicVars.Block, cardPlay);
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Heal.UpgradeValueBy(2);
            base.DynamicVars.Block.UpgradeValueBy(3);
        }
    }
}

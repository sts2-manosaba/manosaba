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
    public class DoubleSuicide : PathCustomCardModel
    {
        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Common;
        private const TargetType targetType = TargetType.AnyAlly;
        private const bool shouldShowInCardLibrary = true;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DoubleSuicidePower>()];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DoubleSuicidePower>(15)];
        public DoubleSuicide() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target != null)
                await CommonActions.Apply<DoubleSuicidePower>(choiceContext, cardPlay.Target, this, base.DynamicVars["DoubleSuicidePower"].BaseValue);
            await CommonActions.Apply<DoubleSuicidePower>(choiceContext, base.Owner.Creature, this, base.DynamicVars["DoubleSuicidePower"].BaseValue);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars["DoubleSuicidePower"].UpgradeValueBy(10);
        }
    }
}

using BaseLib.Utils;
using manosaba.Characters.Common;
using Manosaba.Characters.HikamiMeruru.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.Common.Cards
{
    [Pool(typeof(CommonCardPool))]
    public class Inhibition : PathCustomCardModel
    {
        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
        private const int energyCost = 0;
        private const CardType type = CardType.Power;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.AnyPlayer;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<InhibitionPower>()];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<InhibitionPower>(2)];

        public Inhibition() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (Owner.Creature is not { } ownerCreature || cardPlay.Target is not { } target)
            {
                return;
            }

            await CommonActions.Apply<InhibitionPower>(choiceContext, target, this, DynamicVars["InhibitionPower"].BaseValue);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["InhibitionPower"].UpgradeValueBy(1);
        }
    }
}

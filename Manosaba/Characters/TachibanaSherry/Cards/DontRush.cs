using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Extensions;
using Manosaba.Characters.TachibanaSherry.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class DontRush : PathCustomCardModel
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];
        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag>();
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.AnyEnemy;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromPower<SherryDetectiveRewardPower>(),
            HoverTipFactory.FromPower<StrengthPower>(),
            HoverTipFactory.Static(StaticHoverTip.Block),
        ];

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("EnemyStrengthLoss", 7m)];

        public DontRush() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null)
                return;

            await PowerCmd.Apply<DontRushPower>(cardPlay.Target, DynamicVars["EnemyStrengthLoss"].BaseValue, Owner.Creature, this);
            if (Owner.Creature.GetPowerAmount<SherryDetectiveRewardPower>() > 0m)
            {
                await PowerCmd.Apply<StrengthPower>(Owner.Creature, 2m, Owner.Creature, this);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars["EnemyStrengthLoss"].UpgradeValueBy(3m);
        }
    }
}

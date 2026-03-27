using BaseLib.Utils;
using manosaba.Characters.Common;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.Common.Cards
{
    [Pool(typeof(CommonCardPool))]
    public class Dissolve : PathCustomCardModel
    {

        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.AllEnemies;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("StrengthLoss", 5m)];

        public Dissolve() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            foreach (Creature hittableEnemy in base.CombatState.HittableEnemies)
            {
                await PowerCmd.Apply<DissolvePower>(hittableEnemy, base.DynamicVars["StrengthLoss"].BaseValue, base.Owner.Creature, this);
            }
        }
        protected override void OnUpgrade()
        {
            base.DynamicVars["StrengthLoss"].UpgradeValueBy(2m);
        }
    }
}

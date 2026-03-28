using BaseLib.Utils;
using System.Linq;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class Achuba : PathCustomCardModel
    {
        private const int energyCost = 1;
        private const CardType type = CardType.Attack;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.RandomEnemy;
        private const bool shouldShowInCardLibrary = true;
        protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10m, ValueProp.Move)];

        public Achuba() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (base.CombatState == null)
                return;

            int attackCount = 1;
            while (attackCount > 0)
            {
                attackCount--;
                var candidates = base.CombatState.HittableEnemies
                    .Where(e => e.IsAlive && e.GetPowerAmount<SoarPower>() <= 0)
                    .ToList();
                if (candidates.Count == 0)
                    break;

                Creature target = base.Owner.RunState.Rng.CombatTargets.NextItem(candidates);
                IEnumerable<DamageResult> results = await CreatureCmd.Damage(choiceContext, target, base.DynamicVars.Damage.BaseValue, ValueProp.Move, base.Owner.Creature, this);
                if (results.Any(r => r.WasTargetKilled))
                    attackCount++;
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(5m);
        }
    }
}

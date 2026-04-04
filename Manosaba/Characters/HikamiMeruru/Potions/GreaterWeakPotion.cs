using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.HikamiMeruru.Potions
{
    [Pool(typeof(HikamiMeruruPotionPool))]
    public class GreaterWeakPotion : PathCustomPotionModel
    {
        public override PotionUsage Usage => PotionUsage.CombatOnly;
        public override PotionRarity Rarity => PotionRarity.Token;
        public override TargetType TargetType => TargetType.AllEnemies;

        public override bool CanBeGeneratedInCombat => true;
        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<WeakPower>(5m)];
        public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

        protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
        {
            Creature player = base.Owner.Creature;
            IReadOnlyList<Creature> targets = player.CombatState.HittableEnemies;
            foreach (Creature item in targets)
            {
                PotionModel.AssertValidForTargetedPotion(item);
                await PowerCmd.Apply<WeakPower>(item, base.DynamicVars.Weak.BaseValue, base.Owner.Creature, null);
            }
        }
    }
}

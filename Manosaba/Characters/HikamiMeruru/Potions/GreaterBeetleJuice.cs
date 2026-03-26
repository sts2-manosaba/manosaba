using BaseLib.Utils;
using Godot;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Manosaba.Characters.HikamiMeruru.Potions
{
    [Pool(typeof(HikamiMeruruPotionPool))]
    public class GreaterBeetleJuice : PathCustomPotionModel
    {
        public override PotionUsage Usage => PotionUsage.CombatOnly;
        public override PotionRarity Rarity => PotionRarity.Token;
        public override TargetType TargetType => TargetType.AllEnemies;
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("DamageDecrease", 30m), new RepeatVar(4)];

        public override bool CanBeGeneratedInCombat => true;
        protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
        {
            Creature player = base.Owner.Creature;
            IReadOnlyList<Creature> targets = player.CombatState.HittableEnemies;
            foreach (Creature item in targets)
            {
                PotionModel.AssertValidForTargetedPotion(item);
                NCombatRoom.Instance?.PlaySplashVfx(item, new Color("65cf81"));
                await PowerCmd.Apply<ShrinkPower>(item, base.DynamicVars.Repeat.BaseValue, base.Owner.Creature, null);
            }
        }
    }
}

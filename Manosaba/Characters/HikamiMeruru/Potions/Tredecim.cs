using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HikamiMeruru.Potions
{
    [Pool(typeof(HikamiMeruruPotionPool))]
    public class Tredecim : PathCustomPotionModel
    {
        public override PotionUsage Usage => PotionUsage.CombatOnly;
        public override PotionRarity Rarity => PotionRarity.Token;
        public override TargetType TargetType => TargetType.AnyEnemy;
        public override bool CanBeGeneratedInCombat => false;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(50m, ValueProp.Unpowered)];

        protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
        {
            PotionModel.AssertValidForTargetedPotion(target);
            await CreatureCmd.Damage(choiceContext, target, DynamicVars.Damage.BaseValue, DynamicVars.Damage.Props, base.Owner.Creature, null);
        }
    }
}

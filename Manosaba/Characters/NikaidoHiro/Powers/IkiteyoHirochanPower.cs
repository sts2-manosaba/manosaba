using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.NikaidoHiro.Powers
{
    public class IkiteyoHirochanPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;
        public override bool ShouldPlayVfx => false;

        public override Creature ModifyUnblockedDamageTarget(Creature target, decimal _, ValueProp props, Creature? __)
        {
            if (target != base.Owner.PetOwner?.Creature)
            {
                return target;
            }

            if (base.Owner.IsDead)
            {
                return target;
            }

            if (props.HasFlag(ValueProp.Unpowered))
            {
                return target;
            }

            return base.Owner;
        }

        public override bool ShouldAllowHitting(Creature creature)
        {
            return creature.IsAlive;
        }

        public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
        {
            if (creature != base.Owner)
            {
                return true;
            }

            return false;
        }

        public override bool ShouldPowerBeRemovedAfterOwnerDeath()
        {
            return false;
        }
    }
}

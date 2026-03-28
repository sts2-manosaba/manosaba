using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Manosaba.Characters.NikaidoHiro.Powers
{
    public class EmaDogAttackPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;
        public override bool ShouldPlayVfx => false;

        public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
        {
            if (side == CombatSide.Player && Owner.IsAlive)
            {
                await DamageCmd.Attack(Owner.CurrentHp).FromOsty(Owner, null).TargetingRandomOpponents(CombatState).Execute(choiceContext);
            }
        }

        public override bool ShouldPowerBeRemovedAfterOwnerDeath()
        {
            return false;
        }
    }
}

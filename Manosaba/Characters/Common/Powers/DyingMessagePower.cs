using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.NikaidoHiro.Powers
{
    public class DyingMessagePower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        private int buffToApply = 3;

        public void Upgrade()
        {
            buffToApply += 2;
        }

        public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
        {
            if (wasRemovalPrevented)
                return;

            if (creature != Owner)
            {
                return;
            }

            IEnumerable<Creature> enumerable = from c in base.CombatState.GetTeammatesOf(creature)
                                               where c != null && c.IsAlive && c.IsPlayer
                                               select c;
            foreach (Creature item in enumerable)
            {
                if (item == Owner)
                {
                    return;
                }
                await PowerCmd.Apply<DrawCardsNextTurnPower>(item, buffToApply , base.Owner, null);
                await PowerCmd.Apply<EnergyNextTurnPower>(item, buffToApply, base.Owner, null);
            }
        }

        public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
        {
            if (side != base.Owner.Side)
            {
                Flash();
                await PowerCmd.Remove(this);
            }
        }
    }
}

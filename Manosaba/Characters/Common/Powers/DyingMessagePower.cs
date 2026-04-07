using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

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

        public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if(target != base.Owner) return;
            if(amount < base.Owner.CurrentHp ) return;
            IEnumerable<Creature> enumerable = from c in base.CombatState.GetTeammatesOf(base.Owner)
                                               where c != null && c.IsAlive && c.IsPlayer
                                               select c;
            foreach (Creature item in enumerable)
            {
                if (item == Owner)
                {
                    return;
                }
                await PowerCmd.Apply<DrawCardsNextTurnPower>(item, 5, base.Owner, null);
                await PowerCmd.Apply<EnergyNextTurnPower>(item, 10, base.Owner, null);
            }
            return;
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

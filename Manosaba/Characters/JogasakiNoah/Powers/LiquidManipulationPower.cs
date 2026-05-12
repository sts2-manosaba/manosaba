using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.JogasakiNoah.Powers
{
    public class LiquidManipulationPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (base.Owner != target || base.Amount <= 0m || amount <= 0m)
            {
                return amount;
            }

            int absorbed = Math.Min(base.Amount, (int)Math.Ceiling(amount));
            if (absorbed <= 0m)
            {
                return amount;
            }

            Flash();
            SetAmount(base.Amount - absorbed, silent: true);
            if (ShouldRemoveDueToAmount())
            {
                RemoveInternal();
            }

            return Math.Max(0m, amount - absorbed);
        }
    }
}

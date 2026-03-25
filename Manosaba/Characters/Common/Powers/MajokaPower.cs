using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.Common.Powers
{
    public class MajokaPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override bool AllowNegative => false;

        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (base.Owner != dealer)
            {
                return 1m;
            }

            if (props.HasFlag(ValueProp.Unpowered))
            {
                return 1m;
            }

            return 1m + base.Amount / 100m;
        }

        public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (base.Owner != dealer)
            {
                return 0m;
            }

            if (props.HasFlag(ValueProp.Unpowered))
            {
                return 0m;
            }

            return base.Amount / 25;
        }

        public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
        {

            int toApplyMI = base.Amount / 100 - base.Owner.GetPowerAmount<MurderousImpulsePower>();
            PowerCmd.Apply<MurderousImpulsePower>(base.Owner.Player.Creature, toApplyMI, base.Owner.Player.Creature, null);

            return Task.CompletedTask;
        }
    }
}

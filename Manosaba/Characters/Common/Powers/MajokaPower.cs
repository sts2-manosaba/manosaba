using BaseLib.Abstracts;
using manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.Common.Powers
{
    public class MajokaPower : CustomPowerModel
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

            return 1m + base.Amount / 100m;
        }

        public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
        {

            int toApply = base.Amount / 100 - base.Owner.GetPowerAmount<MurderousImpulse>();
            Log.Info($"MajokaPower applied. Current amount: {base.Amount}. MurderousImpulse to apply: {toApply}");
            if (toApply > 0)
            {
                PowerCmd.Apply<MurderousImpulse>(base.Owner.Player.Creature, toApply, base.Owner.Player.Creature, null);
            }
            return Task.CompletedTask;
        }



        public override string CustomPackedIconPath => "Majoka.png".PowerImagePath();
        public override string CustomBigIconPath => "Majoka.png".PowerImagePath();
        public override string CustomBigBetaIconPath => "Majoka.png".PowerImagePath();
    }
}

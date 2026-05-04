using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.SaekiMiria.Powers
{
    public class LawKnowerPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override bool AllowNegative => false;


        public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
        {
            if (power.Owner != base.Owner)
            {
                return Task.CompletedTask;
            }

            if (power is SusPower && amount > 0m)
            {
                return CreatureCmd.GainBlock(base.Owner, new BlockVar(base.Amount * amount, ValueProp.Unpowered), null);
            }
            return Task.CompletedTask;
        }
    }
}

using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

using MegaCrit.Sts2.Core.GameActions.Multiplayer;
namespace Manosaba.Characters.SaekiMiria.Powers
{
    public class LawKnowerPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override bool AllowNegative => false;

        public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
        {
            if (power.Owner != base.Owner || amount <= 0m || power == this)
            {
                return Task.CompletedTask;
            }

            return CreatureCmd.GainBlock(base.Owner, new BlockVar(base.Amount * amount, ValueProp.Unpowered), null);
        }
    }
}

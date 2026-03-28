using BaseLib.Utils;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TachibanaSherry.Powers
{
    public sealed class DateTimeNoMajokaPower : PathCustomPowerModel
    {
        private bool _adjustingMajoka;

        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override async Task AfterPowerAmountChanged(PowerModel power, decimal amountDiff, Creature? applier, CardModel? source)
        {
            if (_adjustingMajoka || power is not MajokaPower || amountDiff <= 0 || power.Owner != Owner)
                return;

            _adjustingMajoka = true;
            await PowerCmd.Apply<MajokaPower>(Owner, -amountDiff, Owner, source);
            _adjustingMajoka = false;
        }

        public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
        {
            if (side == CombatSide.Enemy)
                await PowerCmd.Remove(this);
        }
    }
}

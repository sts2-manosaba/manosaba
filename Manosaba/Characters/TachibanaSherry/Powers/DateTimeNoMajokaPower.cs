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

        public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amountDiff, Creature? applier, CardModel? source)
        {
            if (_adjustingMajoka || power is not MajokaPower || amountDiff <= 0 || power.Owner != Owner)
                return;

            _adjustingMajoka = true;
            await CommonActions.Apply<MajokaPower>(choiceContext, Owner, source, -amountDiff);
            _adjustingMajoka = false;
        }

        public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> creatures)
        {
            if (side == CombatSide.Enemy)
                await PowerCmd.Remove(this);
        }
    }
}

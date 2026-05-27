using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.Common.Powers;

public sealed class WardenCommandPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        _ = target;
        if (card.Owner.Creature != Owner)
        {
            return playCount;
        }

        if (!ExecutionCardHelper.IsExecutionCard(card))
        {
            return playCount;
        }

        return playCount + Amount;
    }
}

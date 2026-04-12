using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace Manosaba.Characters.HoshoMago.Powers;

public sealed class TemperancePower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner.Player == null || Amount <= 0)
        {
            return Task.CompletedTask;
        }

        int gold = (int)Amount;
        if (gold > 0)
        {
            room.AddExtraReward(Owner.Player, new GoldReward(gold, Owner.Player));
        }

        return Task.CompletedTask;
    }
}

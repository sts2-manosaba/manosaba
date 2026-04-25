using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace Manosaba.Characters.TonoHanna.Powers;

/// <summary>Same end-of-combat gold as vanilla <see cref="RoyaltiesPower"/>; separate type for a mod power icon.</summary>
public sealed class LeiaPuppetPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner.Player == null)
        {
            return Task.CompletedTask;
        }

        room.AddExtraReward(Owner.Player, new GoldReward(Amount, Owner.Player));
        return Task.CompletedTask;
    }
}

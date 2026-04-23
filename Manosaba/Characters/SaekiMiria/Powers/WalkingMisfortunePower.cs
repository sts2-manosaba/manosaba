using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Manosaba.Characters.SaekiMiria.Powers;

public sealed class WalkingMisfortunePower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (Owner.Player == null || player != Owner.Player)
        {
            return amount;
        }

        return amount + Amount;
    }
}

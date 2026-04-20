using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace manosaba.Characters.NatsumeAnan.Powers;

public sealed class BrainwashExtraTurnPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldTakeExtraTurn(Player player)
    {
        return player == Owner.Player && Amount > 0m;
    }

    public override async Task AfterTakingExtraTurn(Player player)
    {
        if (player != Owner.Player)
            return;

        Flash();
        await PowerCmd.Decrement(this);
    }
}

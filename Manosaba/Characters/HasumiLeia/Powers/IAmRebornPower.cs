using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.HasumiLeia.Powers;

public sealed class IAmRebornPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        _ = combatState;

        if (side != CombatSide.Player || Owner.Player == null)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<DoubleDamagePower>(Owner, Amount, Owner, null);
        await PowerCmd.Remove(this);
    }
}

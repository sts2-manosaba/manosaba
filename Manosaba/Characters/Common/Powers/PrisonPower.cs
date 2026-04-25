using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Manosaba.Characters.Common.Powers;

public class PrisonPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side != base.Owner.Side)
        {
            return;
        }

        if (base.Owner.Player?.Creature is not { } ownerCreature)
        {
            return;
        }

        await PowerCmd.Apply<MajokaPower>(ownerCreature, base.Amount, ownerCreature, null);
    }
}

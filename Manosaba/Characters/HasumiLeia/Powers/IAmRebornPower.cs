using BaseLib.Utils;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

using MegaCrit.Sts2.Core.Entities.Creatures;
namespace Manosaba.Characters.HasumiLeia.Powers;

public sealed class IAmRebornPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> creatures, ICombatState combatState)
    {
        _ = combatState;

        if (side != CombatSide.Player || Owner.Player == null)
        {
            return;
        }

        Flash();
        await CommonActions.Apply<DoubleDamagePower>(new ThrowingPlayerChoiceContext(), Owner, null, Amount);
        await PowerCmd.Remove(this);
    }
}

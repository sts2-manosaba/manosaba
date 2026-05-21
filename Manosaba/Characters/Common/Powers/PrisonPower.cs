using BaseLib.Utils;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Powers;

using MegaCrit.Sts2.Core.Entities.Creatures;
namespace Manosaba.Characters.Common.Powers;

public class PrisonPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> creatures, ICombatState combatState)
    {
        if (side != base.Owner.Side)
        {
            return;
        }

        if (base.Owner.Player?.Creature is not { } ownerCreature)
        {
            return;
        }

        await CommonActions.Apply<MajokaPower>(new ThrowingPlayerChoiceContext(), ownerCreature, null, base.Amount);
    }
}

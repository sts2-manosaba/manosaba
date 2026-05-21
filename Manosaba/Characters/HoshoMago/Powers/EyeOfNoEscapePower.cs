using BaseLib.Utils;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

using MegaCrit.Sts2.Core.Entities.Creatures;
namespace Manosaba.Characters.HoshoMago.Powers;

public sealed class EyeOfNoEscapePower : PathCustomPowerModel
{
    private const decimal MadnessPerTurn = 20m;
    private static PowerModel MadnessIconSource => ModelDb.Power<MadnessPower>();

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> creatures)
    {
        _ = choiceContext;
        if (side != Owner.Side)
        {
            return;
        }

        await CommonActions.Apply<MadnessPower>(choiceContext, Owner, null, MadnessPerTurn);
        await PowerCmd.TickDownDuration(this);
    }
}

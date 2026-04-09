using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.HoshoMago.Powers;

public sealed class EyeOfNoEscapePower : PathCustomPowerModel
{
    private const decimal MadnessPerTurn = 20m;
    private static PowerModel MadnessIconSource => ModelDb.Power<MadnessPower>();

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        _ = choiceContext;
        if (side != Owner.Side)
        {
            return;
        }

        await PowerCmd.Apply<MadnessPower>(Owner, MadnessPerTurn, Owner, null);
        await PowerCmd.TickDownDuration(this);
    }
}

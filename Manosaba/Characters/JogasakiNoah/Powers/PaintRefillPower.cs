using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoah.Powers;

public class PaintRefillPower : PathCustomPowerModel
{
    private const int MaxRefillAttempts = 64;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> creatures)
    {
        _ = creatures;
        if (Amount < 1m || side != Owner.Side || Owner?.Player is not { } player || player.PlayerCombatState?.OrbQueue is not { } orbQueue)
        {
            return;
        }

        if (orbQueue.Orbs.Count >= orbQueue.Capacity)
        {
            return;
        }

        Flash();
        int attempts = 0;
        while (orbQueue.Orbs.Count < orbQueue.Capacity && attempts++ < MaxRefillAttempts)
        {
            OrbModel randomOrb = JogasakiNoahOrbPool.RollRandomGeneratedOrb(player);
            await OrbCmd.Channel(choiceContext, randomOrb.ToMutable(), player);
        }
    }
}

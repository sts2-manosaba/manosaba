using manosaba.Extensions;
using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Orbs;
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

    public override string CustomPackedIconPath => "power.png".PowerImagePath();
    public override string CustomBigIconPath => "power.png".PowerImagePath();
    public override string CustomBigBetaIconPath => "power.png".PowerImagePath();

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (Amount < 1m || side != CombatSide.Player || Owner?.Player is not { } player || player.PlayerCombatState?.OrbQueue is not { } orbQueue)
        {
            return;
        }

        if (orbQueue.Orbs.Count >= orbQueue.Capacity)
        {
            return;
        }

        Flash();
        IReadOnlyList<OrbModel> paintOrbs = JogasakiNoahOrbPool.AllOrbs;
        int attempts = 0;
        while (orbQueue.Orbs.Count < orbQueue.Capacity && attempts++ < MaxRefillAttempts)
        {
            OrbModel randomOrb = paintOrbs[player.RunState.Rng.CombatOrbGeneration.NextInt(paintOrbs.Count)];
            await OrbCmd.Channel(choiceContext, randomOrb.ToMutable(), player);
        }
    }
}

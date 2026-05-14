using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Manosaba.Characters.JogasakiNoah.Powers;

public sealed class SekirinyakudouPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player || Owner?.Player?.PlayerCombatState == null)
        {
            return;
        }

        foreach (BloodOrb bloodOrb in Owner.Player.PlayerCombatState.OrbQueue.Orbs.OfType<BloodOrb>().ToList())
        {
            bloodOrb.AddLayers(1m);
        }

        await Cmd.Wait(0.1f);
    }
}

using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System;

namespace manosaba.Characters.NatsumeAnan.Powers;

public sealed class BocchanPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _ = choiceContext;

        if (player != Owner.Player)
            return Task.CompletedTask;

        int gain = Math.Max(0, (int)Amount);
        if (gain > 0)
        {
            KotodamaEnergy.Gain(player, gain);
            Flash();
        }

        return Task.CompletedTask;
    }
}

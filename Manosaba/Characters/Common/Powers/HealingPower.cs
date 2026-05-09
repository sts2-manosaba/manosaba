using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Manosaba.Characters.Common.Powers;

public sealed class HealingPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    // Triggered at player-turn start, slightly earlier than BurnPower's side-turn-start hook.
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _ = choiceContext;

        if (player.Creature != Owner || Amount <= 0m || !Owner.IsAlive)
        {
            return;
        }

        decimal currentAmount = Amount;
        await CreatureCmd.Heal(Owner, currentAmount);

        decimal loseAmount = decimal.Floor(currentAmount / 2m);
        if (loseAmount <= 0m)
        {
            return;
        }

        await PowerCmd.Apply<HealingPower>(Owner, -loseAmount, Owner, null);
    }
}

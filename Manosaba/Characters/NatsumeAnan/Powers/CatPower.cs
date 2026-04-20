using BaseLib.Extensions;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace manosaba.Characters.NatsumeAnan.Powers;

public sealed class CatPower : PathCustomPowerModel
{
    private const int DodgeChancePercent = 25;
    private int _pendingEvades;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        _ = choiceContext;
        _ = cardSource;

        if (target != Owner)
            return Task.CompletedTask;

        if (dealer == null)
            return Task.CompletedTask;

        if (!props.IsPoweredAttack_())
            return Task.CompletedTask;

        if (amount <= 0m)
            return Task.CompletedTask;

        Player? ownerPlayer = Owner.Player;
        if (ownerPlayer == null)
            return Task.CompletedTask;

        int roll = ownerPlayer.RunState.Rng.CombatTargets.NextInt(100);
        if (roll < DodgeChancePercent)
        {
            _pendingEvades++;
            Flash();
        }

        return Task.CompletedTask;
    }

    public override decimal ModifyHpLostBeforeOsty(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        _ = cardSource;

        if (target != Owner)
            return amount;

        if (dealer == null)
            return amount;

        if (!props.IsPoweredAttack_())
            return amount;

        if (_pendingEvades <= 0)
            return amount;

        // Evade negates this hit's HP loss.
        return 0m;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        _ = choiceContext;
        _ = cardSource;

        if (target != Owner)
            return;

        if (dealer == null)
            return;

        if (!props.IsPoweredAttack_())
            return;

        if (_pendingEvades <= 0)
            return;

        _pendingEvades--;

        // Restore consumed block so the attack feels like a full dodge.
        if (result.BlockedDamage > 0)
        {
            await CreatureCmd.GainBlock(Owner, result.BlockedDamage, ValueProp.Unpowered, null, fast: true);
        }
    }
}

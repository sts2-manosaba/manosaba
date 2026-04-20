using Manosaba.Characters.Common.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace manosaba.Characters.NatsumeAnan.Powers;

public sealed class KokoroPower : PathCustomPowerModel
{
    private int _pendingSuicidePrevents;

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
        _ = props;

        if (target != Owner)
            return Task.CompletedTask;

        if (dealer != Owner)
            return Task.CompletedTask;

        if (cardSource is not Suicide)
            return Task.CompletedTask;

        if (amount <= 0m)
            return Task.CompletedTask;

        _pendingSuicidePrevents++;
        return Task.CompletedTask;
    }

    public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        _ = props;

        if (target != Owner)
            return amount;

        if (dealer != Owner)
            return amount;

        if (cardSource is not Suicide)
            return amount;

        if (_pendingSuicidePrevents <= 0)
            return amount;

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
        _ = props;

        if (target != Owner)
            return;

        if (dealer != Owner)
            return;

        if (cardSource is not Suicide)
            return;

        if (_pendingSuicidePrevents <= 0)
            return;

        _pendingSuicidePrevents--;

        if (result.BlockedDamage > 0)
        {
            await CreatureCmd.GainBlock(Owner, result.BlockedDamage, ValueProp.Unpowered, null, fast: true);
        }
    }
}

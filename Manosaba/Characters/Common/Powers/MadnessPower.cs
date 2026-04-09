using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.Common.Powers;

public sealed class MadnessPower : PathCustomPowerModel
{
    private const decimal TriggerThreshold = 100m;
    private const decimal MaxHpLossRatio = 0.05m;
    private const int DecayPerTurn = 5;
    private bool _isResolvingThreshold;

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this || _isResolvingThreshold || Amount < TriggerThreshold)
        {
            return;
        }

        _isResolvingThreshold = true;
        try
        {
            Creature effectSource = applier ?? Owner;
            await TriggerMadness(effectSource, cardSource);
            await PowerCmd.Remove(this);
        }
        finally
        {
            _isResolvingThreshold = false;
        }
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        _ = choiceContext;
        if (side != Owner.Side || Amount <= 0)
        {
            return;
        }

        await PowerCmd.Apply<MadnessPower>(Owner, -DecayPerTurn, Owner, null);
    }

    private async Task TriggerMadness(Creature effectSource, CardModel? cardSource)
    {
        decimal hpLoss = Owner.MaxHp * MaxHpLossRatio;
        if (hpLoss > 0)
        {
            await CreatureCmd.Damage(
                new ThrowingPlayerChoiceContext(),
                Owner,
                hpLoss,
                ValueProp.Unblockable | ValueProp.Unpowered,
                effectSource,
                cardSource);
        }

        if (!Owner.IsAlive)
        {
            return;
        }

        if (Owner.Player != null)
        {
            PlayerCmd.EndTurn(Owner.Player, canBackOut: false);
            return;
        }

        await CreatureCmd.Stun(Owner);
    }
}

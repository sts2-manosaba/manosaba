using Manosaba.Characters.KurobeNanoka.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.KurobeNanoka.Powers;

public sealed class SteadyShotPower : PathCustomPowerModel
{
    private const string GunsDealtVar = "GunsDealt";

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => DynamicVars[GunsDealtVar].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(GunsDealtVar, 0m)];

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        _ = target;
        _ = amount;

        if (dealer != Owner)
        {
            return 0m;
        }

        if (cardSource is not GunBase || cardSource.Type != CardType.Attack)
        {
            return 0m;
        }

        if (!props.HasFlag(ValueProp.Move) || props.HasFlag(ValueProp.Unpowered))
        {
            return 0m;
        }

        decimal bonusPerStack = SteadyShot.GetBonusDamagePerStack();

        // Apply the same per-stack bonus to this shot and each prior qualifying shot while the power is active.
        return (DynamicVars[GunsDealtVar].BaseValue + 1m) * bonusPerStack;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player)
        {
            return;
        }

        if (cardPlay.Card is not GunBase)
        {
            return;
        }

        DynamicVars[GunsDealtVar].BaseValue += 1m;
        InvokeDisplayAmountChanged();
        await Task.CompletedTask;
    }
}

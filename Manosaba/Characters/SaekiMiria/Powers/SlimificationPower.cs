using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.SaekiMiria.Powers;

public sealed class SlimificationPower : PathCustomPowerModel
{
    private const string RegenGainVar = "RegenGain";
    private int _pendingHpLossTriggers;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;
    public override int DisplayAmount => Amount;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<RegenPower>()];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(RegenGainVar, 1m)];

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        SyncVars();
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        _ = amount;
        _ = applier;
        _ = cardSource;

        if (ReferenceEquals(power, this))
        {
            SyncVars();
        }

        return Task.CompletedTask;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        _ = dealer;

        if (target != Owner)
            return;

        if (_pendingHpLossTriggers <= 0)
            return;

        _pendingHpLossTriggers--;
        await PowerCmd.Apply<RegenPower>(Owner, Amount, Owner, cardSource);
    }

    public override decimal ModifyHpLostBeforeOsty(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        _ = props;
        _ = dealer;
        _ = cardSource;

        if (target == Owner && amount > 0m)
        {
            _pendingHpLossTriggers++;
        }

        return amount;
    }

    private void SyncVars()
    {
        if (DynamicVars.TryGetValue(RegenGainVar, out DynamicVar? regenGain))
        {
            regenGain.BaseValue = Amount;
        }
    }
}

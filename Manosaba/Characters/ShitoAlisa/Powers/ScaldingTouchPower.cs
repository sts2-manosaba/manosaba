using BaseLib.Utils;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.ShitoAlisa.Powers;

/// <summary>造成未被格擋的攻擊傷害時施加灼燒；Amount = 層數。</summary>
public sealed class ScaldingTouchPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("MajokaGain", 0m)];

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this || amount <= 0m || cardSource == null)
            return Task.CompletedTask;
        if (cardSource.DynamicVars.TryGetValue("MajokaPower", out DynamicVar? majokaVar))
            DynamicVars["MajokaGain"].BaseValue += majokaVar.BaseValue;
        return Task.CompletedTask;
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner || cardSource == null || cardSource.Type != CardType.Attack)
            return;
        if (result.UnblockedDamage <= 0)
            return;
        if (!props.HasFlag(ValueProp.Move) || props.HasFlag(ValueProp.Unpowered))
            return;

        await CommonActions.Apply<BurnPower>(choiceContext, target, cardSource, Amount);
        await CommonActions.Apply<MajokaPower>(choiceContext, Owner, cardSource, DynamicVars["MajokaGain"].BaseValue);
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BurnPower>(), HoverTipFactory.FromPower<MajokaPower>()];
}

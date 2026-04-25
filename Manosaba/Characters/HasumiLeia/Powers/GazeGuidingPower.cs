using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HasumiLeia.Powers;

public sealed class GazeGuidingPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("DamageReductionPercent", 0m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        SyncDamageReductionPercent();
        return Task.CompletedTask;
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await base.AfterPowerAmountChanged(power, amount, applier, cardSource);
        if (power is MajokaPower && power.Owner == Owner)
            SyncDamageReductionPercent();
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == null)
            return 1m;

        decimal majoka = Math.Min(100m, Owner.GetPowerAmount<MajokaPower>());
        decimal reduction = majoka / 200m; // 0..0.5

        CombatState? combatState = CombatState;
        bool isMultiplayerGame = combatState != null
            && combatState.GetTeammatesOf(Owner).Any(c => c != Owner && c.IsPlayer);

        if (!isMultiplayerGame)
        {
            if (target != Owner)
                return 1m;

            return 1m - reduction;
        }

        if (target == Owner)
        {
            decimal increase = majoka / 100m; // 0..1
            return 1m + increase;
        }

        if (target.IsPlayer && target != Owner && combatState != null && combatState.GetTeammatesOf(Owner).Contains(target))
            return 1m - reduction;

        return 1m;
    }

    private void SyncDamageReductionPercent()
    {
        if (Owner == null)
            return;

        decimal majoka = Math.Min(100m, Owner.GetPowerAmount<MajokaPower>());
        DynamicVars["DamageReductionPercent"].BaseValue = majoka / 2m; // 0..50
    }
}


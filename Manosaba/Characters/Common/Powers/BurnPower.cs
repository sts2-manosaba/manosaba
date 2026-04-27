using System.Collections.Generic;
using System.Linq;
using Manosaba.Characters.ShitoAlisa.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.Common.Powers;

public class BurnPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("DamageDecrease", 0.9m)];

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer != base.Owner)
        {
            return 1m;
        }

        if (props.HasFlag(ValueProp.Unpowered))
        {
            return 1m;
        }

        return base.DynamicVars["DamageDecrease"].BaseValue;
    }

    public int CalculateDamageNextTurn()
    {
        if (base.Owner.CombatState is not { } combatState)
        {
            return (int)base.Amount;
        }

        decimal damage = Hook.ModifyDamage(combatState.RunState, combatState, base.Owner, null, base.Amount, ValueProp.Unblockable | ValueProp.Unpowered, null, ModifyDamageHookType.All, CardPreviewMode.None, out IEnumerable<AbstractModel> _);
        return (int)damage;
    }

    /// <summary>
    /// 灼燒在持有者回合觸發次數：1 次 + 我方全體「一切安好」層數總和（與 <see cref="AfterSideTurnStart"/> 一致）。
    /// </summary>
    public int GetBurnProcCountForOwnerTurn(CombatState combatState)
    {
        int extra = combatState.Allies.Sum(c => (int)c.GetPowerAmount<ThisIsFinePower>());
        return 1 + extra;
    }

    /// <summary>下回合灼燒總傷害預覽（單次傷害 × 觸發次數），供血條等 UI；命名對齊 <see cref="PoisonPower.CalculateTotalDamageNextTurn"/>。</summary>
    public int CalculateTotalDamageNextTurn()
    {
        int perProc = CalculateDamageNextTurn();
        if (base.Owner.CombatState is not { } cs)
        {
            return perProc;
        }

        return perProc * GetBurnProcCountForOwnerTurn(cs);
    }

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side != base.Owner.Side)
        {
            return;
        }

        if (!base.Owner.IsAlive)
        {
            return;
        }

        int iterations = GetBurnProcCountForOwnerTurn(combatState);
        for (int i = 0; i < iterations; i++)
        {
            if (!base.Owner.IsAlive)
            {
                break;
            }

            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner, base.Amount, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
            if (!base.Owner.IsAlive)
            {
                await Cmd.CustomScaledWait(0.1f, 0.25f);
            }
        }
    }
}

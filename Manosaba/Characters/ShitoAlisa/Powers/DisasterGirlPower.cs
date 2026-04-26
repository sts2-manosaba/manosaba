using System.Collections.Generic;
using System.Linq;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.ShitoAlisa.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.ShitoAlisa.Powers;

/// <summary>每累積給予敵人足夠層灼燒後，對一名隨機敵人造成傷害。</summary>
public sealed class DisasterGirlPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Damage", 7m)];

    /// <summary>每層能力對應打出該層時 <see cref="DisasterGirl"/> 卡上的傷害值（未升級 7、升級 11 等），觸發時加總結算。</summary>
    private readonly List<decimal> _damagePerStackLayer = [];

    private int _burnCredit;

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await base.AfterPowerAmountChanged(power, amount, applier, cardSource);

        if (power == this)
        {
            if (amount > 0m && cardSource is DisasterGirl dg
                && dg.DynamicVars.TryGetValue("DisasterGirlDmg", out DynamicVar? dmgVar))
            {
                for (int i = 0; i < (int)amount; i++)
                    _damagePerStackLayer.Add(dmgVar.BaseValue);
            }
            else if (amount < 0m)
            {
                int remove = (int)(-amount);
                for (int r = 0; r < remove && _damagePerStackLayer.Count > 0; r++)
                    _damagePerStackLayer.RemoveAt(_damagePerStackLayer.Count - 1);
            }

            DynamicVars["Damage"].BaseValue = TotalTriggerDamage();
        }

        if (power is not BurnPower || applier == null || Owner == null)
            return;
        if (power.Owner == null || power.Owner.Side == applier.Side)
            return;
        if (applier != Owner)
            return;
        if (amount <= 0m)
            return;

        _burnCredit += (int)amount;
        int threshold = 5;
        while (_burnCredit >= threshold)
        {
            _burnCredit -= threshold;
            Flash();
            if (CombatState == null)
                return;
            int stackCount = (int)Math.Max(1m, Amount);
            await PowerCmd.Apply<FireballSwarmPower>(Owner, stackCount, Owner, cardSource);
            decimal dmg = TotalTriggerDamage();
            List<Creature> enemies = CombatState.GetOpponentsOf(Owner)
                .Where(e => e.IsAlive && e.IsHittable)
                .ToList();
            if (enemies.Count == 0)
                continue;
            Creature target = CombatState.RunState.Rng.CombatTargets.NextItem(enemies);
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, dmg, ValueProp.Unpowered, Owner, null);
        }
    }

    private decimal TotalTriggerDamage()
        => _damagePerStackLayer.Count > 0 ? _damagePerStackLayer.Sum() : 7m;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BurnPower>(), HoverTipFactory.FromPower<FireballSwarmPower>()];
}

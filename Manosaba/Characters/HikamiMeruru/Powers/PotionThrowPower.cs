using Godot;
using Manosaba.Characters.HikamiMeruru.PotionCraft;
using Manosaba.Extensions;
using Manosaba.Patches;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HikamiMeruru.Powers;

public sealed class PotionThrowPower : PathCustomPowerModel
{
    private const string BoulderDamageKey = "BoulderDamage";
    private const decimal InitialBoulderDamage = 5m;
    private const decimal BoulderDamageIncrement = 5m;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => false;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new DynamicVar(BoulderDamageKey, InitialBoulderDamage)
    ];

    public override async Task AfterPotionDiscarded(PotionModel potion)
    {
        if (Amount <= 0m || Owner.Player == null || potion.Owner != Owner.Player)
            return;
        if (PotionCraftService.IsCraftDiscardSuppressed)
            return;

        var combatState = Owner.CombatState;
        if (combatState == null)
            return;

        int hitCount = Math.Max(0, (int)decimal.Floor(Amount));
        for (int i = 0; i < hitCount; i++)
        {
            if (potion is PotionShapedRock)
            {
                await ThrowBoulder(new ThrowingPlayerChoiceContext());
                continue;
            }

            List<Creature> enemies = combatState
                .GetOpponentsOf(Owner)
                .Where(e => e.IsHittable)
                .ToList();

            if (enemies.Count == 0)
                return;

            Creature? target = Owner.Player.RunState.Rng.CombatTargets.NextItem(enemies);
            if (target == null)
            {
                return;
            }

            using (EntomancerPersonalHiveFilter.BeginPotionThrowDamageScope())
            {
                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, DynamicVars.Damage.BaseValue, ValueProp.Move, Owner);
            }
        }
    }

    private async Task ThrowBoulder(PlayerChoiceContext choiceContext)
    {
        if (Owner.CombatState == null)
        {
            return;
        }

        List<Creature> targets = Owner.CombatState
            .GetOpponentsOf(Owner)
            .Where(e => e.IsHittable)
            .ToList();

        if (targets.Count == 0)
        {
            return;
        }

        decimal damage = DynamicVars[BoulderDamageKey].BaseValue;
        Flash();

        if (TestMode.IsOn)
        {
            await DoBoulderDamage(choiceContext, targets, damage);
        }
        else
        {
            List<Task> damageTasks = [];
            NRollingBoulderVfx? vfx = NRollingBoulderVfx.Create(targets, damage);
            if (vfx == null)
            {
                await DoBoulderDamage(choiceContext, targets, damage);
            }
            else
            {
                vfx.Connect(NRollingBoulderVfx.SignalName.HitCreature, Callable.From((NCreature c) =>
                {
                    damageTasks.Add(DoBoulderDamage(choiceContext, [c.Entity], damage));
                }));
                Callable.From(() =>
                {
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
                    if (!vfx.IsInsideTree())
                    {
                        throw new InvalidOperationException("VFX is not inside tree after adding it to combat room!");
                    }
                }).CallDeferred();
                await vfx.ToSignal(vfx, Node.SignalName.TreeExiting);
                await Task.WhenAll(damageTasks);
            }
        }

        DynamicVars[BoulderDamageKey].BaseValue = damage + BoulderDamageIncrement;
    }

    private async Task<IEnumerable<DamageResult>> DoBoulderDamage(PlayerChoiceContext choiceContext, IEnumerable<Creature> targets, decimal damage)
    {
        using (EntomancerPersonalHiveFilter.BeginPotionThrowDamageScope())
        {
            return await CreatureCmd.Damage(choiceContext, targets, damage, ValueProp.Move, Owner);
        }
    }
}

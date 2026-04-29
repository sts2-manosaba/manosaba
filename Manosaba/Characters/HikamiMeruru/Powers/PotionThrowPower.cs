using Manosaba.Characters.HikamiMeruru.PotionCraft;
using Manosaba.Extensions;
using Manosaba.Patches;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HikamiMeruru.Powers;

public sealed class PotionThrowPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];

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
}

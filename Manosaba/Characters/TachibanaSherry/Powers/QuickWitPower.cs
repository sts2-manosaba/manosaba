using System;
using System.Collections.Generic;
using System.Linq;

using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TachibanaSherry.Powers
{
    public class QuickWitPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        private static readonly IReadOnlyList<int> StrengthGainValues = new List<int> { 1, 2, 3 };
        private static readonly IReadOnlyList<int> DexterityDeltaValues = new List<int> { -2, -1, 0, 1, 2 };
        private static readonly IReadOnlyList<int> VulnerableValues = new List<int> { 0, 1, 2 };
        private static readonly IReadOnlyList<int> WeakValues = new List<int> { 0, 1, 2 };
        private static readonly IReadOnlyList<int> HpDeltaValues = Enumerable.Range(-10, 21).ToList();

        public override async Task BeforePlayPhaseStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player != Owner.Player)
                return;

            var rng = Owner.Player.RunState.Rng.CombatTargets;

            for (int stack = 0; stack < (int)Amount; stack++)
            {
                int strengthGain = rng.NextItem(StrengthGainValues);
                int dexDelta = rng.NextItem(DexterityDeltaValues);
                int hpDelta = rng.NextItem(HpDeltaValues);
                int vulnerableStacks = rng.NextItem(VulnerableValues);
                int weakStacks = rng.NextItem(WeakValues);

                await PowerCmd.Apply<StrengthPower>(Owner, strengthGain, Owner, null);
                if (dexDelta != 0)
                    await PowerCmd.Apply<DexterityPower>(Owner, dexDelta, Owner, null);

                if (hpDelta >= 0)
                {
                    await CreatureCmd.Heal(Owner, hpDelta);
                }
                else
                {
                    await CreatureCmd.Damage(choiceContext, Owner, -hpDelta, ValueProp.Unpowered, Owner);
                }

                var combatState = Owner.CombatState;
                if (combatState == null)
                    return;

                List<Creature> enemies = combatState.GetOpponentsOf(Owner).ToList();
                foreach (Creature enemy in enemies)
                {
                    if (!enemy.IsAlive)
                        continue;

                    if (vulnerableStacks > 0)
                        await PowerCmd.Apply<VulnerablePower>(enemy, vulnerableStacks, Owner, null);

                    if (weakStacks > 0)
                        await PowerCmd.Apply<WeakPower>(enemy, weakStacks, Owner, null);
                }
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;

using Manosaba;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TachibanaSherry.Powers
{
    public class QuickWitPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;
        public override LocString Description => new("powers", ManosabaFeatureFlags.AprilFoolsModeEnabled ? "MANOSABA-QUICK_WIT_POWER.description" : "MANOSABA-QUICK_WIT_POWER.description.normal");
        protected override string SmartDescriptionLocKey => ManosabaFeatureFlags.AprilFoolsModeEnabled ? "MANOSABA-QUICK_WIT_POWER.smartDescription" : "MANOSABA-QUICK_WIT_POWER.smartDescription.normal";

        private static readonly IReadOnlyList<int> StrengthGainValues = Enumerable.Range(-5, 11).ToList();
        private static readonly IReadOnlyList<int> StrengthGainValuesNormal = new List<int> { 1, 2, 3 };
        private static readonly IReadOnlyList<int> DexterityDeltaValues = Enumerable.Range(-5, 11).ToList();
        private static readonly IReadOnlyList<int> DexterityDeltaValuesNormal = Enumerable.Range(-2, 5).ToList();
        private static readonly IReadOnlyList<int> VulnerableValues = new List<int> { 0, 1, 2 };
        private static readonly IReadOnlyList<int> WeakValues = new List<int> { 0, 1, 2 };
        private static readonly IReadOnlyList<int> HpDeltaValues = Enumerable.Range(-5, 11).ToList();
        private static readonly IReadOnlyList<int> HpDeltaValuesNormal = Enumerable.Range(-5, 11).ToList();

        public override async Task BeforePlayPhaseStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player != Owner.Player)
                return;

            var rng = Owner.Player.RunState.Rng.CombatTargets;

            bool aprilFools = ManosabaFeatureFlags.AprilFoolsModeEnabled;

            int strengthGain = rng.NextItem(aprilFools ? StrengthGainValues : StrengthGainValuesNormal);
            int dexDelta = rng.NextItem(aprilFools ? DexterityDeltaValues : DexterityDeltaValuesNormal);
            int hpDelta = rng.NextItem(aprilFools ? HpDeltaValues : HpDeltaValuesNormal);
            int vulnerableStacks = rng.NextItem(VulnerableValues);
            int weakStacks = rng.NextItem(WeakValues);

            if (aprilFools)
            {
                if (strengthGain > 0)
                    await PowerCmd.Apply<QuickWitTemporaryStrengthPower>(Owner, strengthGain, Owner, null);
                else if (strengthGain < 0)
                    await PowerCmd.Apply<QuickWitTemporaryStrengthDownPower>(Owner, -strengthGain, Owner, null);

                if (dexDelta > 0)
                    await PowerCmd.Apply<QuickWitTemporaryDexterityPower>(Owner, dexDelta, Owner, null);
                else if (dexDelta < 0)
                    await PowerCmd.Apply<QuickWitTemporaryDexterityDownPower>(Owner, -dexDelta, Owner, null);
            }
            else
            {
                await PowerCmd.Apply<StrengthPower>(Owner, strengthGain, Owner, null);
                if (dexDelta != 0)
                    await PowerCmd.Apply<DexterityPower>(Owner, dexDelta, Owner, null);
            }

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

            IEnumerable<Creature> enemies = combatState.GetOpponentsOf(Owner);
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


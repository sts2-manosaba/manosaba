using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.JogasakiNoah.Powers
{
    public class SpaceOutPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;


        public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
        {
            if (side != CombatSide.Player || Owner?.Player == null)
            {
                return;
            }

            ThrowingPlayerChoiceContext choiceContext = new();
            IReadOnlyList<OrbModel> currentOrbs = Owner.Player.PlayerCombatState.OrbQueue.Orbs;
            HashSet<Type> currentOrbTypes = currentOrbs.Select(o => o.GetType()).ToHashSet();

            List<OrbModel> requiredOrbs =
            [
                ModelDb.Orb<RedPaintOrb>(),
                ModelDb.Orb<OrangePaintOrb>(),
                ModelDb.Orb<YellowPaintOrb>(),
                ModelDb.Orb<GreenPaintOrb>(),
                ModelDb.Orb<BluePaintOrb>(),
                ModelDb.Orb<PurplePaintOrb>()
            ];

            List<OrbModel> missingOrbs = requiredOrbs
                .Where(o => !currentOrbTypes.Contains(o.GetType()))
                .ToList();

            if (missingOrbs.Count == 0)
            {
                List<Creature> enemies = combatState.HittableEnemies
                    .Where(e => e.IsHittable && e.IsAlive)
                    .ToList();

                if (enemies.Count > 0)
                {
                    await CreatureCmd.Damage(choiceContext, enemies, 50m, ValueProp.Move, Owner, null);
                }

                int orbCount = Owner.Player.PlayerCombatState.OrbQueue.Orbs.Count;
                for (int i = 0; i < orbCount; i++)
                {
                    await OrbCmd.EvokeNext(choiceContext, Owner.Player);
                }
                return;
            }

            OrbModel randomMissingOrb = missingOrbs[Owner.CombatState.RunState.Rng.CombatOrbGeneration.NextInt(missingOrbs.Count)];
            await OrbCmd.Channel(choiceContext, randomMissingOrb.ToMutable(), Owner.Player);
        }

    }
}

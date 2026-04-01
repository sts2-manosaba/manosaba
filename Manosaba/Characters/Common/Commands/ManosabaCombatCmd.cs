using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Manosaba.Characters.Common.Commands
{
    public static class ManosabaCombatCmd
    {
        public static async Task ForceWinWithoutDeathOrEscape(CombatState combatState)
        {
            CombatManager? combatManager = CombatManager.Instance;
            if (combatManager == null || !combatManager.IsInProgress)
            {
                return;
            }

            IReadOnlyList<Creature> enemies = combatState.Enemies.ToList();
            foreach (Creature enemy in enemies)
            {
                RemoveEnemyWithoutDeathOrEscape(combatState, enemy);
            }

            await combatManager.CheckWinCondition();
        }

        private static void RemoveEnemyWithoutDeathOrEscape(CombatState combatState, Creature enemy)
        {
            if (enemy.IsDead)
            {
                return;
            }

            var creatureNode = NCombatRoom.Instance?.GetCreatureNode(enemy);
            if (creatureNode != null)
            {
                NCombatRoom.Instance?.RemoveCreatureNode(creatureNode);
            }

            CombatManager.Instance.RemoveCreature(enemy);
            if (combatState.Enemies.Contains(enemy))
            {
                combatState.RemoveCreature(enemy);
            }
        }
    }
}

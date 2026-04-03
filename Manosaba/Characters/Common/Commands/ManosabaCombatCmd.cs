using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

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
                enemy.RemoveAllPowersInternalExcept();
                await CreatureCmd.Kill(enemy);
            }
            await combatManager.CheckWinCondition();
        }
    }
}

using Manosaba.Characters.Common.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Patches;

internal static class IgnoreEnemyDamageCapHelper
{
    public static bool ShouldIgnore(CardModel? cardSource, Creature? target)
    {
        if (cardSource is not IIgnoreEnemyDamageCap || target == null || !target.IsEnemy)
        {
            return false;
        }

        return true;
    }
}

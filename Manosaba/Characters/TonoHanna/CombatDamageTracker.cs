using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Manosaba.Characters.TonoHanna;

internal static class CombatDamageTracker
{
    public static async Task<decimal> MeasureDamageToTarget(Creature target, Func<Task> dealDamageAsync)
    {
        decimal beforeHp = target.CurrentHp;
        decimal beforeBlock = target.Block;
        await dealDamageAsync();
        return Math.Max(0m, (beforeHp - target.CurrentHp) + (beforeBlock - target.Block));
    }
}

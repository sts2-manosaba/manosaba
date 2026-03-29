using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;

[HarmonyPatch(typeof(Creature), nameof(Creature.AfterAddedToRoom))]
public static class Patch_Creature_AfterAddedToRoom_HpMultiplier
{
    private const decimal EnemyHpMultiplier = 1.35m; // 你要的難度倍率
    private static readonly HashSet<Creature> _applied = new();

    static void Prefix(Creature __instance)
    {
        if (!__instance.IsEnemy) return;
        if (!_applied.Add(__instance)) return;

        int oldMax = __instance.MaxHp;
        int newMax = Math.Max(1, (int)Math.Round(oldMax * EnemyHpMultiplier, MidpointRounding.AwayFromZero));

        __instance.SetMaxHpInternal(newMax);
        __instance.SetCurrentHpInternal(newMax);
    }
}

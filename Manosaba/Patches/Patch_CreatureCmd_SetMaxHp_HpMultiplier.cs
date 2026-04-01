using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.SetMaxHp))]
public static class Patch_CreatureCmd_SetMaxHp_HpMultiplier
{
    private const decimal EnemyHpMultiplier = 1.35m;

    static void Prefix(Creature creature, ref decimal amount)
    {
        if (!creature.IsEnemy || !creature.IsDead)
        {
            return;
        }

        amount = Math.Max(1m, Math.Round(amount * EnemyHpMultiplier, MidpointRounding.AwayFromZero));
    }
}

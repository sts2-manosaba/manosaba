using HarmonyLib;
using Manosaba.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;

namespace Manosaba.Patches;

/// <summary>
/// Scales damage from enemy monsters to player-side targets (intent UI uses <see cref="Hook.ModifyDamage"/> too).
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyDamage))]
public static class Patch_Hook_ModifyDamage_EnemyAttackMultiplier
{
    [HarmonyPostfix]
    private static void Postfix(Creature? target, Creature? dealer, ref decimal __result)
    {
        if (dealer == null || !dealer.IsEnemy || !dealer.IsMonster || target == null || target.IsEnemy)
        {
            return;
        }

        decimal mult = ManosabaLobbyDifficultyState.GetEnemyAttackDamageMultiplierForGameplay();
        if (mult == 1m)
        {
            return;
        }

        __result = Math.Max(0m, Math.Round(__result * mult, MidpointRounding.AwayFromZero));
    }
}

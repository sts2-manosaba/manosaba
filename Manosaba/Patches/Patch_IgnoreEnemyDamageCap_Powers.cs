using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Patches;

/// <summary>Bypasses per-hit damage caps on enemies when damage comes from <see cref="IIgnoreEnemyDamageCap"/> cards.</summary>
internal static class Patch_IgnoreEnemyDamageCap_Powers
{
    [HarmonyPatch(typeof(IntangiblePower), nameof(IntangiblePower.ModifyDamageCap))]
    private static class IntangibleModifyDamageCap
    {
        [HarmonyPrefix]
        private static bool Prefix(
            Creature? target,
            CardModel? cardSource,
            ref decimal __result)
        {
            if (!IgnoreEnemyDamageCapHelper.ShouldIgnore(cardSource, target))
            {
                return true;
            }

            __result = decimal.MaxValue;
            return false;
        }
    }

    [HarmonyPatch(typeof(IntangiblePower), nameof(IntangiblePower.ModifyHpLostAfterOsty))]
    private static class IntangibleModifyHpLostAfterOsty
    {
        [HarmonyPrefix]
        private static bool Prefix(
            Creature target,
            decimal amount,
            CardModel? cardSource,
            ref decimal __result)
        {
            if (!IgnoreEnemyDamageCapHelper.ShouldIgnore(cardSource, target))
            {
                return true;
            }

            __result = amount;
            return false;
        }
    }

    [HarmonyPatch(typeof(HardToKillPower), nameof(HardToKillPower.ModifyDamageCap))]
    private static class HardToKillModifyDamageCap
    {
        [HarmonyPrefix]
        private static bool Prefix(
            Creature? target,
            CardModel? cardSource,
            ref decimal __result)
        {
            if (!IgnoreEnemyDamageCapHelper.ShouldIgnore(cardSource, target))
            {
                return true;
            }

            __result = decimal.MaxValue;
            return false;
        }
    }

    [HarmonyPatch(typeof(SlipperyPower), nameof(SlipperyPower.ModifyDamageCap))]
    private static class SlipperyModifyDamageCap
    {
        [HarmonyPrefix]
        private static bool Prefix(
            Creature? target,
            CardModel? cardSource,
            ref decimal __result)
        {
            if (!IgnoreEnemyDamageCapHelper.ShouldIgnore(cardSource, target))
            {
                return true;
            }

            __result = decimal.MaxValue;
            return false;
        }
    }
}

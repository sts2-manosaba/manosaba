using HarmonyLib;
using Manosaba.Characters.Common.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Patches;

/// <summary>
/// Central bypass for <see cref="IIgnoreEnemyDamageCap"/> cards instead of patching each cap power.
/// v0.107+: strip <see cref="ModifyDamageHookType.Cap"/> in <see cref="Hook.ModifyDamage"/>,
/// and skip AfterOsty in <see cref="Hook.ModifyHpLost"/> (Slippery / Intangible osty path).
/// </summary>
[HarmonyPatch]
internal static class Patch_Hook_IgnoreEnemyDamageCap
{
    private const ModifyDamageHookType CapFlag = (ModifyDamageHookType)8;

    [HarmonyPatch(typeof(Hook), nameof(Hook.ModifyDamage))]
    private static class ModifyDamage
    {
        [HarmonyPrefix]
        private static void StripDamageCap(Creature? target, CardModel? cardSource, ref ModifyDamageHookType modifyDamageHookType)
        {
            if (!ShouldBypass(cardSource, target))
            {
                return;
            }

            if (modifyDamageHookType.HasFlag(CapFlag))
            {
                modifyDamageHookType &= ~CapFlag;
            }
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.ModifyHpLost))]
    private static class ModifyHpLost
    {
        [HarmonyPrefix]
        private static bool SkipAfterOstyHpLostCaps(
            Creature target,
            decimal amount,
            CardModel? cardSource,
            HpLossHookPhase phases,
            ref decimal __result)
        {
            if (!phases.HasFlag(HpLossHookPhase.AfterOsty) || !ShouldBypass(cardSource, target))
            {
                return true;
            }

            __result = amount;
            return false;
        }
    }

    private static bool ShouldBypass(CardModel? cardSource, Creature? target) =>
        cardSource is IIgnoreEnemyDamageCap && target is { IsEnemy: true };
}

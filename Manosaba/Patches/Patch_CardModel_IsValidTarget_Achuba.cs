using HarmonyLib;
using Manosaba.Characters.TachibanaSherry.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.IsValidTarget))]
internal static class Patch_CardModel_IsValidTarget_Achuba
{
    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, Creature? target, ref bool __result)
    {
        if (!__result || target == null)
        {
            return;
        }

        if (!AchubaPower.AllowsAttackTarget(__instance, target))
        {
            __result = false;
        }
    }
}

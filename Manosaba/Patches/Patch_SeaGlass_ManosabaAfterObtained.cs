using HarmonyLib;
using Manosaba.Characters.Common;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(SeaGlass), nameof(SeaGlass.AfterObtained))]
public static class Patch_SeaGlass_ManosabaAfterObtained
{
    [HarmonyPrefix]
    private static bool Prefix(SeaGlass __instance, ref Task __result)
    {
        if (!ManosabaSeaGlassHelper.TryHandleAfterObtained(__instance, ref __result))
        {
            return true;
        }

        return false;
    }
}

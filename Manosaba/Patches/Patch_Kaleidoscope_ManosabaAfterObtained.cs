using HarmonyLib;
using Manosaba.Characters.Common;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(Kaleidoscope), nameof(Kaleidoscope.AfterObtained))]
public static class Patch_Kaleidoscope_ManosabaAfterObtained
{
    [HarmonyPrefix]
    private static bool Prefix(Kaleidoscope __instance, ref Task __result)
    {
        if (!ManosabaKaleidoscopeHelper.TryHandleAfterObtained(__instance, ref __result))
        {
            return true;
        }

        return false;
    }
}

using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(CardFactory))]
public static class Patch_CardFactory_TransformCommonCards
{
    [HarmonyPatch(nameof(CardFactory.GetDefaultTransformationOptions))]
    [HarmonyPrefix]
    private static bool PrefixGetDefaultTransformationOptions(CardModel original, bool isInCombat, ref IEnumerable<CardModel> __result)
    {
        if (!ManosabaTransformHelper.TryGetTransformOptions(original, isInCombat, out IEnumerable<CardModel> options))
        {
            return true;
        }

        __result = options;
        return false;
    }
}

using HarmonyLib;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(TouchOfOrobas), nameof(TouchOfOrobas.GetUpgradedStarterRelic))]
internal static class Patch_TouchOfOrobas_GetUpgradedStarterRelic
{
    [HarmonyPrefix]
    private static bool Prefix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic is not LevelingPathCustomRelicModel)
        {
            return true;
        }

        __result = ModelDb.GetById<RelicModel>(starterRelic.Id);
        return false;
    }
}

[HarmonyPatch(typeof(RelicModel), nameof(RelicModel.DynamicEventDescription), MethodType.Getter)]
internal static class Patch_TouchOfOrobas_ManosabaDynamicEventDescription
{
    [HarmonyPrefix]
    private static bool Prefix(RelicModel __instance, ref LocString __result)
    {
        if (__instance is not TouchOfOrobas touch
            || !ManosabaTouchOfOrobasHelper.UsesManosabaLevelUpgrade(touch))
        {
            return true;
        }

        __result = ManosabaTouchOfOrobasHelper.BuildDescription(touch, "eventDescription");
        return false;
    }
}

[HarmonyPatch(typeof(RelicModel), nameof(RelicModel.DynamicDescription), MethodType.Getter)]
internal static class Patch_TouchOfOrobas_ManosabaDynamicDescription
{
    [HarmonyPrefix]
    private static bool Prefix(RelicModel __instance, ref LocString __result)
    {
        if (__instance is not TouchOfOrobas touch
            || !ManosabaTouchOfOrobasHelper.UsesManosabaLevelUpgrade(touch))
        {
            return true;
        }

        __result = ManosabaTouchOfOrobasHelper.BuildDescription(touch, "description");
        return false;
    }
}

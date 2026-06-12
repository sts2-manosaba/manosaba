using HarmonyLib;
using Manosaba.Characters.Common;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(RelicCmd), nameof(RelicCmd.Obtain), typeof(RelicModel), typeof(Player), typeof(int))]
public static class Patch_SeaGlass_RelicCmdObtain
{
    [HarmonyPrefix]
    private static void Prefix(RelicModel relic, Player player)
    {
        if (relic is SeaGlass)
        {
            ManosabaSeaGlassHelper.EnsureCurrentMapPointPlayerEntry(player);
        }
    }
}

[HarmonyPatch(typeof(EventOption), nameof(EventOption.Chosen))]
public static class Patch_SeaGlass_EventOptionChosen
{
    [HarmonyFinalizer]
    private static Exception? Finalizer(Exception? __exception, EventOption __instance)
    {
        if (__exception == null || __instance.Relic is not SeaGlass)
        {
            return __exception;
        }

        Log.Error($"Sea Glass ancient option failed; forcing event completion: {__exception}");
        if (__instance.Relic?.Owner != null)
        {
            ManosabaSeaGlassHelper.TryFinishAncientEventForPlayer(__instance.Relic.Owner);
        }
        return null;
    }
}

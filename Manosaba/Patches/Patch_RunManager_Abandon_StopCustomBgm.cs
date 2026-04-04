using HarmonyLib;
using Manosaba.Audio;
using MegaCrit.Sts2.Core.Runs;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(RunManager), nameof(RunManager.Abandon))]
public static class Patch_RunManager_Abandon_StopCustomBgm
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        GodotSfxRouter.StopCustomBgmAndResumeVanilla();
    }
}

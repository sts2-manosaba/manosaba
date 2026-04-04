using HarmonyLib;
using Manosaba.Audio;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(NPauseMenu), "OnSaveAndQuitButtonPressed", new[] { typeof(NButton) })]
public static class Patch_NPauseMenu_SaveAndQuit_StopCustomBgm
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        GodotSfxRouter.StopCustomBgmAndResumeVanilla();
    }
}

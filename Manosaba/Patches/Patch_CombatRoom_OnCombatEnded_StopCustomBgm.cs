using HarmonyLib;
using Manosaba.Audio;
using MegaCrit.Sts2.Core.Rooms;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(CombatRoom), nameof(CombatRoom.OnCombatEnded))]
public static class Patch_CombatRoom_OnCombatEnded_StopCustomBgm
{
    private static void Postfix()
    {
        GodotSfxRouter.StopCustomBgmAndResumeVanilla();
    }

}

using HarmonyLib;
using Manosaba.Characters.HikamiMeruru.Cards;
using Manosaba.Characters.NikaidoHiro.Cards;
using MegaCrit.Sts2.Core.Runs;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(RunState), nameof(RunState.CreateForNewRun))]
public static class Patch_RunState_CreateForNewRun_ResetLaboursVfx
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        LaboursOfHiro.ResetVfxForNewRun();
        HikamiMeruruExaid.ResetSfxForNewRun();
    }
}

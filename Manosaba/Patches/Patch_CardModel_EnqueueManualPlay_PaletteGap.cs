using HarmonyLib;
using Manosaba.Characters.JogasakiNoah.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using System.Reflection;
using System.Threading.Tasks;

namespace Manosaba.Patches
{
    [HarmonyPatch(typeof(CardModel), "EnqueueManualPlay")]
    public static class Patch_CardModel_EnqueueManualPlay_PaletteGap
    {
        private static readonly MethodInfo? OnEnqueuePlayVfxMethod =
            AccessTools.Method(typeof(CardModel), "OnEnqueuePlayVfx");

        [HarmonyPrefix]
        private static bool Prefix(CardModel __instance, Creature? target)
        {
            if (__instance is not PaletteGap paletteGap)
            {
                return true;
            }

            if (OnEnqueuePlayVfxMethod?.Invoke(__instance, [target]) is Task vfxTask)
            {
                TaskHelper.RunSafely(vfxTask);
            }

            RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(
                new PaletteGapPlayCardAction(paletteGap, target, paletteGap.PendingInsertIndex));
            return false;
        }
    }
}

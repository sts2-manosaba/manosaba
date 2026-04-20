using HarmonyLib;
using Godot;
using Manosaba.Characters.Common.Resources;
using MegaCrit.Sts2.Core.Models;
using System.Threading.Tasks;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.SpendResources))]
public static class Patch_CardModel_SpendResources_CustomEnergy
{
    [HarmonyPostfix]
    private static void SpendResources_Postfix(CardModel __instance, ref Task<(int, int)> __result)
    {
        __result = SpendCustomEnergyIfNeeded(__instance, __result);
    }

    private static async Task<(int, int)> SpendCustomEnergyIfNeeded(CardModel card, Task<(int, int)> originalTask)
    {
        (int energySpent, int starsSpent) = await originalTask;

        if (card is not ICustomEnergyCostCard customEnergyCard)
        {
            return (energySpent, starsSpent);
        }

        if (!customEnergyCard.TrySpendCustomEnergyForPlay())
        {
            GD.PushWarning($"[Manosaba] Failed to spend custom energy for card: {card.Id.Entry}");
        }

        return (energySpent, starsSpent);
    }
}

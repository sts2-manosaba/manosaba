using HarmonyLib;
using Godot;
using Manosaba.Characters.Common.Resources;
using manosaba.Characters.NatsumeAnan;
using manosaba.Characters.NatsumeAnan.Cards;
using manosaba.Characters.NatsumeAnan.Relics;
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

        if (card is Urusai urusaiCard)
        {
            int kotodamaSpent = urusaiCard.PrepareKotodamaXCostForPlay();
            if (kotodamaSpent > 0 && !KotodamaEnergy.TrySpend(card.Owner, kotodamaSpent))
            {
                GD.PushWarning($"[Manosaba] Failed to spend Kotodama X cost for card: {card.Id.Entry}");
                kotodamaSpent = 0;
                urusaiCard.OverrideResolvedKotodamaXCostForPlay(0);
            }

            if (kotodamaSpent > 0 && card.Owner?.GetRelic<Clipboard>() is Clipboard clipboardForXCost)
            {
                await clipboardForXCost.OnKotodamaSpentByCardPlay(kotodamaSpent);
            }

            return (energySpent, starsSpent);
        }

        if (card is BookOfGreatOldOnes bookOfGreatOldOnes)
        {
            int kotodamaSpent = bookOfGreatOldOnes.PrepareKotodamaXCostForPlay();
            if (kotodamaSpent > 0 && !KotodamaEnergy.TrySpend(card.Owner, kotodamaSpent))
            {
                GD.PushWarning($"[Manosaba] Failed to spend Kotodama X cost for card: {card.Id.Entry}");
                kotodamaSpent = 0;
                bookOfGreatOldOnes.OverrideResolvedKotodamaXCostForPlay(0);
            }

            if (kotodamaSpent > 0 && card.Owner?.GetRelic<Clipboard>() is Clipboard clipboardForXCost)
            {
                await clipboardForXCost.OnKotodamaSpentByCardPlay(kotodamaSpent);
            }

            return (energySpent, starsSpent);
        }
	
        if (card is not ICustomEnergyCostCard customEnergyCard)
        {
            return (energySpent, starsSpent);
        }

        int customSpentAmount = customEnergyCard.GetCustomEnergyCostForPlay();
        if (customSpentAmount < 0)
        {
            customSpentAmount = 0;
        }
        if (!customEnergyCard.TrySpendCustomEnergyForPlay())
        {
            GD.PushWarning($"[Manosaba] Failed to spend custom energy for card: {card.Id.Entry}");
            return (energySpent, starsSpent);
        }

        if (customSpentAmount > 0
            && customEnergyCard.GetCustomEnergyDefinitionForPlay() is KotodamaEnergy
            && card.Owner?.GetRelic<Clipboard>() is Clipboard clipboardRelic)
        {
            await clipboardRelic.OnKotodamaSpentByCardPlay(customSpentAmount);
        }

        return (energySpent, starsSpent);
    }
}

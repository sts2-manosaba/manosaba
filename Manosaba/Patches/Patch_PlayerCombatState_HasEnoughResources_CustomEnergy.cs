using HarmonyLib;
using Manosaba.Characters.Common.Resources;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(PlayerCombatState), nameof(PlayerCombatState.HasEnoughResourcesFor))]
public static class Patch_PlayerCombatState_HasEnoughResources_CustomEnergy
{
    [HarmonyPostfix]
    private static void HasEnoughResourcesFor_Postfix(CardModel card, ref UnplayableReason reason, ref bool __result)
    {
        if (card is not ICustomEnergyCostCard customEnergyCard)
        {
            return;
        }

        if (customEnergyCard.HasEnoughCustomEnergyForPlay())
        {
            return;
        }

        // Reuse resource-gating flow so the card can still be attempted, similar to star cost checks.
        reason |= UnplayableReason.StarCostTooHigh;
        __result = false;
    }
}

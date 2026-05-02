using HarmonyLib;
using Manosaba.Characters.Common.Resources;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Patches;

public static class Patch_CardEnergyCost_CustomEnergyFreeCost
{
    private static readonly AccessTools.FieldRef<CardEnergyCost, CardModel> CardRef =
        AccessTools.FieldRefAccess<CardEnergyCost, CardModel>("_card");

    [HarmonyPatch(typeof(CardEnergyCost), nameof(CardEnergyCost.SetUntilPlayed))]
    public static class SetUntilPlayedPatch
    {
        private static void Prefix(CardEnergyCost __instance, int cost, bool reduceOnly)
        {
            MarkIfFree(__instance, cost, reduceOnly, LocalCostModifierExpiration.WhenPlayed);
        }
    }

    [HarmonyPatch(typeof(CardEnergyCost), nameof(CardEnergyCost.SetThisTurnOrUntilPlayed))]
    public static class SetThisTurnOrUntilPlayedPatch
    {
        private static void Prefix(CardEnergyCost __instance, int cost, bool reduceOnly)
        {
            MarkIfFree(__instance, cost, reduceOnly, LocalCostModifierExpiration.EndOfTurn | LocalCostModifierExpiration.WhenPlayed);
        }
    }

    [HarmonyPatch(typeof(CardEnergyCost), nameof(CardEnergyCost.SetThisTurn))]
    public static class SetThisTurnPatch
    {
        private static void Prefix(CardEnergyCost __instance, int cost, bool reduceOnly)
        {
            MarkIfFree(__instance, cost, reduceOnly, LocalCostModifierExpiration.EndOfTurn);
        }
    }

    [HarmonyPatch(typeof(CardEnergyCost), nameof(CardEnergyCost.SetThisCombat))]
    public static class SetThisCombatPatch
    {
        private static void Prefix(CardEnergyCost __instance, int cost, bool reduceOnly)
        {
            MarkIfFree(__instance, cost, reduceOnly, LocalCostModifierExpiration.EndOfCombat);
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.EndOfTurnCleanup))]
    public static class EndOfTurnCleanupPatch
    {
        private static void Postfix(CardModel __instance)
        {
            CustomEnergyFreeCost.ClearEndOfTurn(__instance);
        }
    }

    [HarmonyPatch(typeof(CardEnergyCost), nameof(CardEnergyCost.AfterCardPlayedCleanup))]
    public static class AfterCardPlayedCleanupPatch
    {
        private static void Postfix(CardEnergyCost __instance)
        {
            CustomEnergyFreeCost.ClearAfterPlayed(CardRef(__instance));
        }
    }

    private static void MarkIfFree(CardEnergyCost energyCost, int cost, bool reduceOnly, LocalCostModifierExpiration expiration)
    {
        if (cost != 0)
        {
            return;
        }

        CardModel card = CardRef(energyCost);
        if (reduceOnly && energyCost.GetWithModifiers(CostModifiers.Local) <= 0)
        {
            return;
        }

        if (card is ICustomEnergyCostCard)
        {
            CustomEnergyFreeCost.MarkFree(card, expiration);
        }
    }
}

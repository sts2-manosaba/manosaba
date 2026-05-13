using HarmonyLib;
using Manosaba.Characters.TachibanaSherry.Cards;
using Manosaba.Characters.TachibanaSherry.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Patches;

/// <summary>雪莉：三！依本回合疑點獲取量減費；六！/九！條件免費耗能。</summary>
public static class Patch_CardEnergyCost_SherryDynamicCosts
{
    private static readonly AccessTools.FieldRef<CardEnergyCost, CardModel> CardRef =
        AccessTools.FieldRefAccess<CardEnergyCost, CardModel>("_card");

    [HarmonyPatch(typeof(CardEnergyCost), nameof(CardEnergyCost.GetWithModifiers))]
    private static class GetWithModifiersPatch
    {
        private static void Postfix(CardEnergyCost __instance, ref int __result)
        {
            CardModel card = CardRef(__instance);
            if (card.IsCanonical || card.Owner?.Creature is not { } creature)
            {
                return;
            }

            if (card is SanFindsClue)
            {
                int clues = creature.GetPowerAmount<CluesGainedThisTurnPower>();
                int discount = System.Math.Min(3, clues);
                __result = System.Math.Max(0, __result - discount);
                return;
            }

            if (card is RokuBraveFace && creature.GetPowerAmount<PlayedSanThisTurnPower>() > 0)
            {
                __result = 0;
                return;
            }

            if (card is KyuMiraclePartners && creature.GetPowerAmount<SherryDetectiveRewardPower>() > 0)
            {
                __result = 0;
            }
        }
    }
}

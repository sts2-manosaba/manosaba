using HarmonyLib;
using manosaba.Characters.ShitoAlisa;
using Manosaba.Characters.Common.Overrides;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(CardModel))]
public static class Patch_CardModel_CombustKeywordCount
{
    private static readonly LocString _combustTitle = new("card_keywords", "MANOSABA-COMBUST.title");
    private static readonly LocString _period = new("card_keywords", "PERIOD");

    [HarmonyPatch(nameof(CardModel.GetDescriptionForPile), [typeof(PileType), typeof(Creature)])]
    [HarmonyPostfix]
    private static void GetDescriptionForPilePostfix(CardModel __instance, ref string __result)
    {
        __result = InjectCombustCountIntoKeywordLine(__instance, __result);
    }

    [HarmonyPatch(nameof(CardModel.GetDescriptionForUpgradePreview))]
    [HarmonyPostfix]
    private static void GetDescriptionForUpgradePreviewPostfix(CardModel __instance, ref string __result)
    {
        __result = InjectCombustCountIntoKeywordLine(__instance, __result);
    }

    private static string InjectCombustCountIntoKeywordLine(CardModel card, string description)
    {
        if (!card.Keywords.Contains(ManosabaKeywords.Combust))
        {
            return description;
        }

        bool hasState = ShitoCombustOperations.TryGetCombustState(card, out int current, out int max);
        if (!hasState || max <= 0)
        {
            Log.Warn(
                $"[Manosaba CombustKeyword] skip_inject_no_state card={card.Id.Entry} pile={card.Pile?.Type} " +
                $"hasState={hasState} current={current} max={max}");
            return description;
        }

        string title = _combustTitle.GetFormattedText();
        string period = _period.GetRawText() ?? ".";
        string plainKeywordLine = $"[gold]{title}[/gold]{period}";
        string countedKeywordLine = $"[gold]{title}[/gold] {current}{period}";
        if (description.Contains(plainKeywordLine))
        {
            return description.Replace(plainKeywordLine, countedKeywordLine);
        }

        const int descSnipLen = 160;
        string descSnip = description.Length <= descSnipLen
            ? description
            : description[..descSnipLen] + "...";
        Log.Warn(
            $"[Manosaba CombustKeyword] skip_inject_plain_mismatch card={card.Id.Entry} current={current} max={max} " +
            $"plainLine={plainKeywordLine} desc_snip={descSnip}");

        // Do not append extra lines; avoid duplicated keyword display.
        return description;
    }
}

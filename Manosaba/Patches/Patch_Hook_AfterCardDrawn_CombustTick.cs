using HarmonyLib;
using manosaba.Characters.ShitoAlisa;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(Hook))]
public static class Patch_Hook_AfterCardDrawn_CombustTick
{
    [HarmonyPatch(nameof(Hook.AfterCardDrawn))]
    [HarmonyPostfix]
    private static Task AfterCardDrawnPostfix(
        Task __result,
        ICombatState combatState,
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        return AfterCardDrawnWrapped(__result, choiceContext, card, fromHandDraw);
    }

    private static async Task AfterCardDrawnWrapped(
        Task originalTask,
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        await originalTask;

        try
        {
            if (card.Keywords.Contains(Manosaba.Characters.Common.Overrides.ManosabaKeywords.Combust))
            {
                Log.Info($"[Manosaba Combust] Hook draw card={card.Id.Entry} fromHandDraw={fromHandDraw}");
            }
            await ShitoCombustOperations.AfterCardDrawn(card, choiceContext, card, fromHandDraw);
        }
        catch (Exception ex)
        {
            Log.Error($"[Manosaba Combust] Global draw hook failed: {ex}");
        }
    }
}

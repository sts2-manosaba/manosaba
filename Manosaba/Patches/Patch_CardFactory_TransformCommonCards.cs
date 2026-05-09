using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(CardFactory))]
public static class Patch_CardFactory_TransformCommonCards
{
    [HarmonyPatch(nameof(CardFactory.GetDefaultTransformationOptions))]
    [HarmonyPrefix]
    private static bool PrefixGetDefaultTransformationOptions(CardModel original, bool isInCombat, ref IEnumerable<CardModel> __result)
    {
        if (!ManosabaTransformHelper.TryGetTransformOptions(original, isInCombat, out IEnumerable<CardModel> options))
        {
            Log.Debug(
                $"[Manosaba Transform] CommonPatch_TryGetFailed_VanillaPath original={original.Id.Entry} inCombat={isInCombat} " +
                $"ownerNetId={(original.Owner as Player)?.NetId} poolType={original.Pool?.GetType().Name ?? "null"}");
            return true;
        }

        CardModel[] raw = options.ToArray();
        ManosabaTransformHelper.LogTransformCandidatePool("CommonPatch_RawPool", original, isInCombat, raw, original.Owner as Player);

        if (original.Owner is Player player && ManosabaTransformHelper.ShouldApplyCommonPathUniqueDeckFilter(original))
        {
            CardModel[] afterUnique = ManosabaTransformHelper.FilterUniqueAlreadyInDeck(player, raw);
            ManosabaTransformHelper.LogTransformCandidatePool("CommonPatch_AfterUniqueDeckFilter", original, isInCombat, afterUnique, player);
            __result = afterUnique;
        }
        else
        {
            string skipReason = original.Owner is Player
                ? ManosabaTransformHelper.GetCommonPathUniqueDeckFilterSkipReason(original) ?? "ShouldApply_true_but_branch_mismatch"
                : $"Owner_not_Player({original.Owner?.GetType().Name ?? "null"})";
            Log.Debug(
                $"[Manosaba Transform] CommonPatch_SkipUniqueDeckFilter original={original.Id.Entry} inCombat={isInCombat} " +
                $"ownerNetId={(original.Owner as Player)?.NetId} reason={skipReason} rawCount={raw.Length}");
            __result = raw;
        }

        return false;
    }
}

using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace Manosaba.Patches;

/// <summary>
/// Logs the final card rolled by <see cref="CardFactory.CreateRandomCardForTransform"/> for transform debugging.
/// </summary>
[HarmonyPatch]
public static class Patch_CardFactory_TransformDiagnosticsTwoArg
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            typeof(CardFactory),
            nameof(CardFactory.CreateRandomCardForTransform),
            [typeof(CardModel), typeof(bool), typeof(Rng)])!;
    }

    [HarmonyPostfix]
    private static void Postfix(CardModel original, bool isInCombat, Rng rng, ref CardModel __result)
    {
        string rolled = __result?.Id.Entry ?? "(null)";
        string netId = original.Owner is Player p ? p.NetId.ToString() : "n/a";
        Log.Debug(
            $"[Manosaba Transform] TransformRolled_Result overload=2arg original={original.Id.Entry} inCombat={isInCombat} rolled={rolled} ownerNetId={netId}");
    }
}

[HarmonyPatch]
public static class Patch_CardFactory_TransformDiagnosticsFourArg
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            typeof(CardFactory),
            nameof(CardFactory.CreateRandomCardForTransform),
            [typeof(CardModel), typeof(IEnumerable<CardModel>), typeof(bool), typeof(Rng)])!;
    }

    [HarmonyPostfix]
    private static void Postfix(CardModel original, IEnumerable<CardModel> options, bool isInCombat, Rng rng, ref CardModel __result)
    {
        string rolled = __result?.Id.Entry ?? "(null)";
        string netId = original.Owner is Player p ? p.NetId.ToString() : "n/a";
        Log.Debug(
            $"[Manosaba Transform] TransformRolled_Result overload=4arg original={original.Id.Entry} inCombat={isInCombat} rolled={rolled} ownerNetId={netId}");
    }
}

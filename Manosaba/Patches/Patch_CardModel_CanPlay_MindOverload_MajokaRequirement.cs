using HarmonyLib;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.SaekiMiria.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using System.Reflection;

namespace Manosaba.Patches;

[HarmonyPatch]
public static class Patch_CardModel_CanPlay_MindOverload_MajokaRequirement
{
    private const int RequiredMajoka = 100;

    private static MethodBase? TargetMethod()
    {
        return AccessTools.Method(
            typeof(CardModel),
            nameof(CardModel.CanPlay),
            [typeof(UnplayableReason).MakeByRefType(), typeof(AbstractModel).MakeByRefType()]);
    }

    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, ref bool __result)
    {
        if (!__result || __instance is not MindOverload mindOverload)
        {
            return;
        }

        if (mindOverload.Owner?.Creature == null)
        {
            return;
        }

        if (mindOverload.Owner.Creature.GetPowerAmount<MajokaPower>() < RequiredMajoka)
        {
            __result = false;
        }
    }
}


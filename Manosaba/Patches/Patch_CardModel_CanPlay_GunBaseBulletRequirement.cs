using HarmonyLib;
using manosaba.Characters.KurobeNanoka.Relics;
using Manosaba.Characters.KurobeNanoka.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using System.Reflection;

namespace Manosaba.Patches;

[HarmonyPatch]
public static class Patch_CardModel_CanPlay_GunBaseBulletRequirement
{
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
        if (!__result || __instance is not GunBase gunCard)
        {
            return;
        }

        if (gunCard.Owner?.Creature?.CombatState == null)
        {
            return;
        }

        if (!gunCard.DynamicVars.TryGetValue("BulletCost", out var bulletCostVar))
        {
            return;
        }

        MagicalGun? magicalGun = gunCard.Owner?.GetRelic<MagicalGun>();
        if (magicalGun?.HasEnoughBullets(bulletCostVar.IntValue) == true)
        {
            return;
        }

        Console.WriteLine($"[GunBase] CanPlay denied by patch. card={gunCard.Id} ownerNetId={gunCard.Owner?.NetId} ownerName={gunCard.Owner?.Creature?.Name} cost={bulletCostVar.IntValue} currentBullets={magicalGun?.DisplayAmount ?? -1}");
        __result = false;
    }
}

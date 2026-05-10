using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
public static class Patch_CardModel_OnPlayWrapper_AnyPlayerNullTarget
{
    [HarmonyPrefix]
    private static void Prefix(CardModel __instance, ref Creature? target)
    {
        if (target != null || __instance.TargetType != TargetType.AnyPlayer)
        {
            return;
        }

        Creature? ownerCreature = __instance.Owner?.Creature;
        if (ownerCreature?.IsAlive == true)
        {
            target = ownerCreature;
        }
    }
}

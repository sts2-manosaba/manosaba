using HarmonyLib;
using manosaba.Characters.NatsumeAnan.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(ChemicalX), nameof(ChemicalX.BeforeCardPlayed))]
public static class Patch_ChemicalX_BeforeCardPlayed_KotodamaX
{
    [HarmonyPostfix]
    private static void BeforeCardPlayed_Postfix(ChemicalX __instance, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == __instance.Owner && cardPlay.Card is Urusai or BookOfGreatOldOnes)
        {
            __instance.Flash();
        }
    }
}

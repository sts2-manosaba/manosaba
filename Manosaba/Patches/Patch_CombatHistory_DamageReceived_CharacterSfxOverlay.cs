using HarmonyLib;
using Manosaba.Audio;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(CombatHistory), nameof(CombatHistory.DamageReceived))]
public static class Patch_CombatHistory_DamageReceived_CharacterSfxOverlay
{
    [HarmonyPostfix]
    private static void Postfix(
        CombatState combatState,
        Creature receiver,
        Creature? dealer,
        DamageResult result,
        CardModel? cardSource)
    {
        _ = (combatState, dealer, cardSource);

        if (!CombatManager.Instance.IsInProgress || CombatManager.Instance.IsEnding)
            return;

        if (result.WasFullyBlocked)
            CharacterSfxOverlayOneShot.EnqueueBlockHit(receiver.Player?.NetId);

        if (result.WasBlockBroken
            && !result.WasFullyBlocked
            && result.UnblockedDamage + result.OverkillDamage > 0)
        {
            CharacterSfxOverlayOneShot.EnqueueBlockBreakDamagePath(receiver.Player?.NetId);
        }
    }
}

[HarmonyPatch(typeof(CombatHistory), nameof(CombatHistory.Clear))]
public static class Patch_CombatHistory_Clear_CharacterSfxOverlay
{
    [HarmonyPostfix]
    private static void Postfix() =>
        CharacterSfxOverlayOneShot.ClearCombatQueues();
}

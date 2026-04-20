using Godot;
using HarmonyLib;
using Manosaba.Characters.Common.Resources;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace Manosaba.Patches;

[HarmonyPatch]
public static class Patch_CustomEnergyCardPlayDialogue
{
    [HarmonyPatch(typeof(NControllerCardPlay), nameof(NControllerCardPlay.Start))]
    [HarmonyPrefix]
    private static bool ControllerStart_Prefix(NControllerCardPlay __instance)
    {
        return !TryHandleInsufficientCustomEnergy(__instance, canCancelPlayCard: true);
    }

    [HarmonyPatch(typeof(NMouseCardPlay), "StartAsync")]
    [HarmonyPrefix]
    private static bool MouseStartAsync_Prefix(NMouseCardPlay __instance, ref Task __result)
    {
        if (!TryHandleInsufficientCustomEnergy(__instance, canCancelPlayCard: true))
        {
            return true;
        }

        __result = Task.CompletedTask;
        return false;
    }

    private static bool TryHandleInsufficientCustomEnergy(NCardPlay cardPlay, bool canCancelPlayCard)
    {
        CardModel? card = cardPlay.Holder?.CardModel;
        if (card is not ICustomEnergyCostCard customEnergyCard)
        {
            return false;
        }

        if (customEnergyCard.HasEnoughCustomEnergyForPlay())
        {
            return false;
        }

        if (canCancelPlayCard)
        {
            cardPlay.CancelPlayCard();
        }

        CharacterCustomEnergyDefinition definition = customEnergyCard.GetCustomEnergyDefinitionForPlay();
        var messageLoc = definition.GetNotEnoughMessageLocString();
        string message = messageLoc.Exists()
            ? messageLoc.GetFormattedText()
            : definition.NotEnoughFallbackText;
        if (NCombatRoom.Instance?.CombatVfxContainer is Node vfxContainer)
        {
            vfxContainer.AddChildSafely(NThoughtBubbleVfx.Create(message, card.Owner.Creature, 1.0));
        }

        return true;
    }
}

using HarmonyLib;
using Manosaba.Combat.Emotes;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(NCombatUi))]
public static class Patch_NCombatUi_EmotePicker
{
    [HarmonyPatch(nameof(NCombatUi.Activate))]
    [HarmonyPostfix]
    private static void Activate_Postfix(NCombatUi __instance, CombatState state)
    {
        _ = __instance;
        _ = state;
        EmotePickerUi.EnsureShown();
    }

    [HarmonyPatch(nameof(NCombatUi.Deactivate))]
    [HarmonyPostfix]
    private static void Deactivate_Postfix(NCombatUi __instance)
    {
        _ = __instance;
        EmotePickerUi.Remove();
    }

    [HarmonyPatch(nameof(NCombatUi.AnimOut))]
    [HarmonyPostfix]
    private static void AnimOut_Postfix(NCombatUi __instance)
    {
        _ = __instance;
        EmotePickerUi.CollapseIfPresent();
    }
}

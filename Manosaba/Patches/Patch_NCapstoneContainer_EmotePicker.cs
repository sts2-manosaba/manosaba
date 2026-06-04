using HarmonyLib;
using Manosaba.Combat.Emotes;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(NCapstoneContainer))]
public static class Patch_NCapstoneContainer_EmotePicker
{
    [HarmonyPatch(nameof(NCapstoneContainer.Open))]
    [HarmonyPostfix]
    private static void Open_Postfix()
    {
        EmotePickerUi.CollapseIfPresent();
    }
}

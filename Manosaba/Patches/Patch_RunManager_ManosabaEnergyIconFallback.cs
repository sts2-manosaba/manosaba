using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(RunManager), nameof(RunManager.GetLocalCharacterEnergyIconPrefix))]
public static class Patch_RunManager_ManosabaEnergyIconFallback
{
    [HarmonyPostfix]
    private static void Postfix(ref string? __result)
    {
        if (string.IsNullOrEmpty(__result) || !__result.Contains("manosaba", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string spriteFontPath = $"res://images/packed/sprite_fonts/{__result}_energy_icon.png";
        if (!ResourceLoader.Exists(spriteFontPath))
        {
            __result = "colorless";
        }
    }
}

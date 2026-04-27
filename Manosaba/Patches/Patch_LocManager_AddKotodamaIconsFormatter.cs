using HarmonyLib;
using Manosaba.Localization.Formatters;
using MegaCrit.Sts2.Core.Localization;
using SmartFormat;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(LocManager), "LoadLocFormatters")]
public static class Patch_LocManager_AddKotodamaIconsFormatter
{
    private static bool _isRegistered;

    [HarmonyPostfix]
    private static void LoadLocFormatters_Postfix()
    {
        if (_isRegistered || Smart.Default == null)
        {
            return;
        }

        Smart.Default.AddExtensions(new KotodamaIconsFormatter());
        _isRegistered = true;
    }
}

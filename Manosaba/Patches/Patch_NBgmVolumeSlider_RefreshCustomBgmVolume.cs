using HarmonyLib;
using Manosaba.Audio;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(NBgmVolumeSlider), "OnValueChanged")]
public static class Patch_NBgmVolumeSlider_RefreshCustomBgmVolume
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        GodotSfxRouter.RefreshCustomBgmVolumeFromSettings();
    }
}

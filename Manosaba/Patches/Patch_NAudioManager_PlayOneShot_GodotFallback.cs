using HarmonyLib;
using Manosaba.Audio;
using MegaCrit.Sts2.Core.Nodes.Audio;

namespace Manosaba.Patches;

[HarmonyPatch]
public static class Patch_NAudioManager_PlayOneShot_GodotFallback
{
    [HarmonyPatch(typeof(NAudioManager), nameof(NAudioManager.PlayOneShot), new[] { typeof(string), typeof(float) })]
    [HarmonyPrefix]
    private static bool PrefixSimple(string path, float volume)
    {
        return !GodotSfxRouter.TryPlay(path, volume);
    }

    [HarmonyPatch(typeof(NAudioManager), nameof(NAudioManager.PlayOneShot), new[] { typeof(string), typeof(System.Collections.Generic.Dictionary<string, float>), typeof(float) })]
    [HarmonyPrefix]
    private static bool PrefixWithParams(string path, System.Collections.Generic.Dictionary<string, float> parameters, float volume)
    {
        return !GodotSfxRouter.TryPlay(path, volume);
    }
}

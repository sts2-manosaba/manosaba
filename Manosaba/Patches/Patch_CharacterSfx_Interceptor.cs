using HarmonyLib;
using Manosaba.Audio;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;

namespace Manosaba.Patches;

[HarmonyPatch]
public static class Patch_CharacterSfx_Interceptor
{
    private static bool _isPlayingExtra;

    [HarmonyPatch(typeof(SfxCmd), nameof(SfxCmd.Play), new[] { typeof(string), typeof(float) })]
    [HarmonyPostfix]
    private static void AppendFmodSfx(string sfx, float volume)
    {
        if (_isPlayingExtra)
            return;

        var charSfx = CharacterSfxRegistry.GetCurrentPlayerSfx();
        if (charSfx == null)
            return;

        string? extra = charSfx.GetExtraSfx(sfx);
        if (extra == null)
            return;

        _isPlayingExtra = true;
        try
        {
            CharacterSfxBase.PlayDirect(extra, volume);
        }
        finally
        {
            _isPlayingExtra = false;
        }
    }

    [HarmonyPatch(typeof(NDebugAudioManager), nameof(NDebugAudioManager.Play),
        new[] { typeof(string), typeof(float), typeof(PitchVariance) })]
    [HarmonyPostfix]
    private static void AppendTmpSfx(string streamName)
    {
        if (_isPlayingExtra)
            return;

        var charSfx = CharacterSfxRegistry.GetCurrentPlayerSfx();
        if (charSfx == null)
            return;

        string? extra = charSfx.GetExtraTmpSfx(streamName);
        if (extra == null)
            return;

        _isPlayingExtra = true;
        try
        {
            CharacterSfxBase.PlayDirect(extra);
        }
        finally
        {
            _isPlayingExtra = false;
        }
    }
}

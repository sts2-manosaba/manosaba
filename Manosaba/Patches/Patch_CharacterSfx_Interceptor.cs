using HarmonyLib;
using Manosaba.Audio;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Runs;

namespace Manosaba.Patches;

[HarmonyPatch]
public static class Patch_CharacterSfx_Interceptor
{
    private const string FmodBlockBreak = "event:/sfx/block_break";
    private const string FmodBlockHit = "event:/sfx/block_hit";

    private const string TmpGainPotion = "gain_potion.mp3";
    private const string TmpRelicGet = "relic_get.mp3";
    private const string TmpCardSmith = "card_smith.mp3";

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

        IRunState? run = RunManager.Instance?.DebugOnlyGetState();
        if (!ShouldPlayCharacterOverlayForFmod(sfx, run))
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

        IRunState? run = RunManager.Instance?.DebugOnlyGetState();
        if (!ShouldPlayCharacterOverlayForTmp(streamName, run))
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

    private static bool ShouldPlayCharacterOverlayForFmod(string sfx, IRunState? run)
    {
        if (run == null || !CharacterSfxOverlayBeneficiary.IsMultiplayerRun(run))
            return true;

        if (sfx == FmodBlockBreak)
        {
            bool hadDamageQueue = CharacterSfxOverlayOneShot.TryDequeueBlockBreakDamagePath(out ulong? fromQueue);
            ulong? beneficiary = hadDamageQueue ? fromQueue : CharacterSfxOverlayBeneficiary.PeekOrNull();

            if (!hadDamageQueue && beneficiary == null)
            {
                // No LoseBlock stack and no damage-queue entry (ordering / stale queue cleared).
                return true;
            }

            return CharacterSfxOverlayBeneficiary.ShouldOverlayForLocalInMultiplayer(beneficiary);
        }

        if (sfx == FmodBlockHit)
        {
            if (!CharacterSfxOverlayOneShot.TryDequeueBlockHit(out ulong? beneficiary))
            {
                // Missing queue entry (Harmony bind / ordering); prefer hearing local line over silence.
                return true;
            }

            return CharacterSfxOverlayBeneficiary.ShouldOverlayForLocalInMultiplayer(beneficiary);
        }

        return true;
    }

    private static bool ShouldPlayCharacterOverlayForTmp(string streamName, IRunState? run)
    {
        if (run == null || !CharacterSfxOverlayBeneficiary.IsMultiplayerRun(run))
            return true;

        if (streamName is not (TmpGainPotion or TmpRelicGet or TmpCardSmith))
            return true;

        ulong? beneficiary;
        if (streamName == TmpCardSmith)
        {
            if (!CharacterSfxOverlayBeneficiary.TryDequeuePendingSmithCardSound(out ulong? smithNetId))
                beneficiary = null;
            else
                beneficiary = smithNetId;
        }
        else
        {
            beneficiary = CharacterSfxOverlayBeneficiary.PeekOrNull();
        }

        return CharacterSfxOverlayBeneficiary.ShouldOverlayForLocalInMultiplayer(beneficiary);
    }
}


using HarmonyLib;
using Manosaba.Multiplayer;
using MegaCrit.Sts2.Core.Runs;

namespace Manosaba.Patches;

/// <summary>
/// Loaded runs never go through character select; lock difficulty to local config for consistent combat math.
/// </summary>
[HarmonyPatch(typeof(RunState), nameof(RunState.FromSerializable))]
public static class Patch_RunState_FromSerializable_ManosabaDifficulty
{
    private static void Postfix()
    {
        ManosabaLobbyDifficultyState.ClearRunSnapshot();
        ManosabaLobbyDifficultyState.FreezeForRunFromDefaults();
    }
}

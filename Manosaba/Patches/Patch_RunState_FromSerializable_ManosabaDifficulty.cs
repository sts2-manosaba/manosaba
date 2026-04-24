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
        // For loaded multiplayer runs, host settings may already be synchronized into lobby snapshot.
        // Do not clobber synchronized values with local defaults.
        if (ManosabaLobbyDifficultyState.RunFrozen)
        {
            return;
        }

        if (ManosabaLobbyDifficultyState.LobbySessionActive)
        {
            ManosabaLobbyDifficultyState.FreezeForRun();
            return;
        }

        ManosabaLobbyDifficultyState.ClearRunSnapshot();
        ManosabaLobbyDifficultyState.FreezeForRunFromDefaults();
    }
}

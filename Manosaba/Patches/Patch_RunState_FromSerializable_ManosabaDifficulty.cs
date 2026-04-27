using HarmonyLib;
using Manosaba.Multiplayer;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(RunState), nameof(RunState.FromSerializable))]
public static class Patch_RunState_FromSerializable_ManosabaDifficulty
{
    private static void Postfix(SerializableRun save)
    {
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
        if (save.Players?.Count > 1)
        {
            ManosabaLobbyDifficultyState.FreezeForRunFromSafeDefaults();
            return;
        }

        ManosabaLobbyDifficultyState.FreezeForRunFromDefaults();
    }
}

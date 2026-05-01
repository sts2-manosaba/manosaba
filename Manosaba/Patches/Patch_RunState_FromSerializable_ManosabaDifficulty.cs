using HarmonyLib;
using Manosaba.Multiplayer;
using Manosaba.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace Manosaba.Patches;

/// <summary>
/// Loaded runs never go through character select; lock difficulty for consistent combat math.
/// Singleplayer <see cref="SerializableRun"/> loads must not keep a stale frozen snapshot from a prior multiplayer session
/// (e.g. main-menu Continue after quitting an MP host run).
/// </summary>
[HarmonyPatch(typeof(RunState), nameof(RunState.FromSerializable))]
public static class Patch_RunState_FromSerializable_ManosabaDifficulty
{
    private static void Postfix(SerializableRun save)
    {
        int playerCount = save.Players?.Count ?? 0;
        if (playerCount < 2)
        {
            ManosabaLobbyDifficultyState.ClearRunSnapshot();
            long startTime = save.StartTime;
            if (startTime != 0 && ManosabaPerSaveDifficultyStore.TryLoadForRun(startTime, Math.Max(playerCount, 1), out ManosabaDifficultySettingsMessage persisted))
            {
                ManosabaLobbyDifficultyState.ApplyFromHost(persisted);
            }
            else
            {
                ManosabaLobbyDifficultyState.ResetToLobbyDefaults();
            }

            ManosabaLobbyDifficultyState.FreezeForRun();
            return;
        }

        // For loaded multiplayer runs, host settings may already be synchronized into lobby snapshot.
        // Do not clobber synchronized values with local defaults.
        if (ManosabaLobbyDifficultyState.RunFrozen)
        {
            return;
        }

        if (ManosabaLobbyDifficultyState.LobbySessionActive)
        {
            // Clients must not freeze from local lobby here: it may still reflect a prior singleplayer session.
            // Host-authoritative difficulty arrives via ManosabaDifficultySettingsMessage -> FreezeForRunFromHost.
            if (RunManager.Instance?.NetService.Type == NetGameType.Client)
            {
                return;
            }

            ManosabaLobbyDifficultyState.FreezeForRun();
            return;
        }

        ManosabaLobbyDifficultyState.ClearRunSnapshot();
        ManosabaLobbyDifficultyState.FreezeForRunFromDefaults();
    }
}

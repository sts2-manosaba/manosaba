using HarmonyLib;
using Manosaba.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace Manosaba.Patches;

/// <summary>
/// Persist lobby difficulty as soon as a run is wired up, so settings survive even if the player quits before the first autosave.
/// </summary>
[HarmonyPatch(typeof(RunManager))]
public static class Patch_RunManager_SetUp_ManosabaPerSaveDifficulty
{
    [HarmonyPatch(nameof(RunManager.SetUpNewMultiplayer))]
    [HarmonyPostfix]
    private static void Postfix_SetUpNewMultiplayer(RunManager __instance, bool shouldSave)
    {
        if (!shouldSave || !__instance.ShouldSave)
        {
            return;
        }

        if (__instance.NetService.Type != NetGameType.Host && __instance.NetService.Type != NetGameType.Singleplayer)
        {
            return;
        }

        ManosabaLobbyDifficultyState.SyncSnapshotToConfig();
        ManosabaPerSaveDifficultyStore.TryPersistActiveRun(flushImmediately: true);
    }

    [HarmonyPatch(nameof(RunManager.SetUpNewSingleplayer))]
    [HarmonyPostfix]
    private static void Postfix_SetUpNewSingleplayer(RunManager __instance, bool shouldSave)
    {
        if (!shouldSave || !__instance.ShouldSave)
        {
            return;
        }

        ManosabaLobbyDifficultyState.SyncSnapshotToConfig();
        ManosabaPerSaveDifficultyStore.TryPersistActiveRun(flushImmediately: true);
    }

    [HarmonyPatch(nameof(RunManager.SetUpSavedMultiplayer))]
    [HarmonyPostfix]
    private static void Postfix_SetUpSavedMultiplayer(LoadRunLobby lobby)
    {
        SerializableRun save = lobby.Run;
        if (save.StartTime == 0)
        {
            return;
        }

        ManosabaLobbyDifficultyState.SyncSnapshotToConfig();
        ManosabaPerSaveDifficultyStore.TryPersistFromSnapshot(
            save.StartTime,
            save.Players?.Count ?? 1,
            flushImmediately: true);
    }

    [HarmonyPatch(nameof(RunManager.SetUpSavedSingleplayer))]
    [HarmonyPostfix]
    private static void Postfix_SetUpSavedSingleplayer(SerializableRun save)
    {
        if (save == null || save.StartTime == 0)
        {
            return;
        }

        ManosabaLobbyDifficultyState.SyncSnapshotToConfig();
        ManosabaPerSaveDifficultyStore.TryPersistFromSnapshot(save.StartTime, 1, flushImmediately: true);
    }
}

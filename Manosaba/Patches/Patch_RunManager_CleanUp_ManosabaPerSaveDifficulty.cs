using HarmonyLib;
using Manosaba.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace Manosaba.Patches;

/// <summary>
/// Flush per-save difficulty before <see cref="RunManager.CleanUp"/> clears <see cref="RunManager.ShouldSave"/>.
/// </summary>
[HarmonyPatch(typeof(RunManager), nameof(RunManager.CleanUp))]
public static class Patch_RunManager_CleanUp_ManosabaPerSaveDifficulty
{
    [HarmonyPrefix]
    private static void Prefix(RunManager __instance)
    {
        if (!__instance.IsInProgress || !__instance.ShouldSave)
        {
            return;
        }

        NetGameType net = __instance.NetService.Type;
        if (net != NetGameType.Singleplayer && net != NetGameType.Host)
        {
            return;
        }

        SerializableRun snapshot = __instance.ToSave(null);
        if (snapshot.StartTime == 0)
        {
            return;
        }

        ManosabaLobbyDifficultyState.SyncSnapshotToConfig();
        ManosabaPerSaveDifficultyStore.TryPersistFromSnapshot(
            snapshot.StartTime,
            snapshot.Players?.Count ?? 1,
            flushImmediately: true);
    }
}

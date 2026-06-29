using HarmonyLib;
using Manosaba.Multiplayer;
using MegaCrit.Sts2.Core.Nodes;

namespace Manosaba.Patches;

/// <summary>
/// Sync frozen run difficulty into mod config before BaseLib's <c>NGame.Quit</c> prefix saves all ModConfigs.
/// </summary>
[HarmonyPatch(typeof(NGame), nameof(NGame.Quit))]
public static class Patch_NGame_Quit_ManosabaDifficultyFlush
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static void Prefix()
    {
        ManosabaLobbyDifficultyState.SyncSnapshotToConfig();
        ManosabaPerSaveDifficultyStore.TryPersistActiveRun(flushImmediately: false);
    }
}

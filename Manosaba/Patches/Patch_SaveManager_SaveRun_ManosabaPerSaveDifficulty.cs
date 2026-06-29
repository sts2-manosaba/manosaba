using HarmonyLib;
using Manosaba.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace Manosaba.Patches;

/// <summary>
/// Persist lobby difficulty alongside vanilla run saves (same timing as <see cref="Patch_CustomEnergySavePersistence"/>).
/// Ensures sidecar is flushed even if the debounced mod-config write has not completed yet.
/// </summary>
[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveRun))]
public static class Patch_SaveManager_SaveRun_ManosabaPerSaveDifficulty
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        RunManager? rm = RunManager.Instance;
        if (rm == null || !rm.ShouldSave)
        {
            return;
        }

        NetGameType net = rm.NetService.Type;
        if (net != NetGameType.Singleplayer && net != NetGameType.Host)
        {
            return;
        }

        SerializableRun snapshot = rm.ToSave(null);
        if (snapshot.StartTime == 0)
        {
            return;
        }

        int playerCount = snapshot.Players?.Count ?? 1;
        ManosabaPerSaveDifficultyStore.TryPersistFromSnapshot(snapshot.StartTime, playerCount, flushImmediately: true);
    }
}

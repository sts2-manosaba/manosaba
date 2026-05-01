using HarmonyLib;
using Manosaba.Config;
using Manosaba.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace Manosaba.Patches;

/// <summary>
/// Persist lobby difficulty sidecar keyed by <see cref="SerializableRun.StartTime"/> whenever vanilla builds a save blob.
/// </summary>
[HarmonyPatch(typeof(RunManager), nameof(RunManager.ToSave))]
public static class Patch_RunManager_ToSave_ManosabaPerSaveDifficulty
{
    [HarmonyPostfix]
    private static void Postfix_ToSave(SerializableRun __result)
    {
        if (__result == null)
        {
            return;
        }

        RunManager rm = RunManager.Instance;
        if (!rm.ShouldSave)
        {
            return;
        }

        NetGameType net = rm.NetService.Type;
        if (net != NetGameType.Singleplayer && net != NetGameType.Host)
        {
            return;
        }

        (double hp, double atk, double mur, RandomCharacterPoolMode pool) =
            ManosabaLobbyDifficultyState.GetPersistedDifficultySnapshot();

        int playerCount = __result.Players?.Count ?? 1;
        ManosabaPerSaveDifficultyStore.SaveForRun(__result.StartTime, playerCount, hp, atk, mur, pool);
    }
}

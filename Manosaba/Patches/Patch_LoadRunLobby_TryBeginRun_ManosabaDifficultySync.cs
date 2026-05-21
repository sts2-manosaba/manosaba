using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

namespace Manosaba.Patches;

/// <summary>
/// Broadcast lobby difficulty before a loaded run begins.
/// v0.105+ renamed <c>TryBeginRun</c> → <c>TryBeginRunForAllPlayers</c> (private).
/// </summary>
[HarmonyPatch]
public static class Patch_LoadRunLobby_TryBeginRun_ManosabaDifficultySync
{
    private static MethodBase TargetMethod() =>
        AccessTools.DeclaredMethod(typeof(LoadRunLobby), "TryBeginRunForAllPlayers")
        ?? AccessTools.DeclaredMethod(typeof(LoadRunLobby), "TryBeginRun")
        ?? throw new InvalidOperationException("LoadRunLobby.TryBeginRun* not found.");

    [HarmonyPrefix]
    private static void Prefix(LoadRunLobby __instance)
    {
        if (__instance.NetService.Type == NetGameType.Client)
        {
            return;
        }

        Patch_LoadRunLobby_ManosabaDifficultySync.BroadcastCurrentDifficulty(__instance.NetService);
    }
}

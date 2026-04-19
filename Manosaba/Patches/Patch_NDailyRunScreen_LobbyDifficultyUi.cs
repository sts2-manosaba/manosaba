using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;

namespace Manosaba.Patches;

[HarmonyPatch]
public static class Patch_NDailyRunScreen_LobbyDifficultyUi
{
    private static readonly AccessTools.FieldRef<NDailyRunScreen, StartRunLobby?> LobbyRef =
        AccessTools.FieldRefAccess<NDailyRunScreen, StartRunLobby?>("_lobby");

    [HarmonyPatch(typeof(NDailyRunScreen), "AfterLobbyInitialized")]
    [HarmonyPostfix]
    private static void Postfix_AfterLobbyInitialized(NDailyRunScreen __instance)
    {
        ManosabaLobbyDifficultyUiHost.OnEnter(
            __instance,
            __instance,
            __instance,
            () => LobbyRef(__instance),
            LobbyDifficultyPanelLayout.DailyRunDefault,
            LobbyDifficultyUiEnterKind.DailyLobbyReady);
    }

    [HarmonyPatch(typeof(NDailyRunScreen), "CleanUpLobby", new[] { typeof(bool) })]
    [HarmonyPrefix]
    private static void Prefix_CleanUpLobby(NDailyRunScreen __instance)
    {
        ManosabaLobbyDifficultyUiHost.OnCleanup(__instance, () => LobbyRef(__instance));
    }

    [HarmonyPatch(typeof(NDailyRunScreen), nameof(NDailyRunScreen._Process))]
    [HarmonyPostfix]
    private static void Postfix_Process(NDailyRunScreen __instance, double delta)
    {
        ManosabaLobbyDifficultyUiHost.OnProcess(__instance, __instance, () => LobbyRef(__instance));
    }
}

using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;

namespace Manosaba.Patches;

[HarmonyPatch]
public static class Patch_NCustomRunScreen_LobbyDifficultyUi
{
    private static readonly AccessTools.FieldRef<NCustomRunScreen, StartRunLobby?> LobbyRef =
        AccessTools.FieldRefAccess<NCustomRunScreen, StartRunLobby?>("_lobby");

    [HarmonyPatch(typeof(NCustomRunScreen), "AfterInitialized")]
    [HarmonyPostfix]
    private static void Postfix_AfterInitialized(NCustomRunScreen __instance)
    {
        ManosabaLobbyDifficultyUiHost.OnEnter(
            __instance,
            __instance,
            __instance,
            () => LobbyRef(__instance),
            LobbyDifficultyPanelLayout.CustomRunDefault,
            LobbyDifficultyUiEnterKind.FirstOpen);
    }

    [HarmonyPatch(typeof(NCustomRunScreen), nameof(NCustomRunScreen.OnSubmenuOpened))]
    [HarmonyPostfix]
    private static void Postfix_OnSubmenuOpened(NCustomRunScreen __instance)
    {
        ManosabaLobbyDifficultyUiHost.OnEnter(
            __instance,
            __instance,
            __instance,
            () => LobbyRef(__instance),
            LobbyDifficultyPanelLayout.CustomRunDefault,
            LobbyDifficultyUiEnterKind.SubmenuReopened);
    }

    [HarmonyPatch(typeof(NCustomRunScreen), "CleanUpLobby", new[] { typeof(bool) })]
    [HarmonyPrefix]
    private static void Prefix_CleanUpLobby(NCustomRunScreen __instance)
    {
        ManosabaLobbyDifficultyUiHost.OnCleanup(__instance, () => LobbyRef(__instance));
    }

    [HarmonyPatch(typeof(NCustomRunScreen), nameof(NCustomRunScreen._Process))]
    [HarmonyPostfix]
    private static void Postfix_Process(NCustomRunScreen __instance, double delta)
    {
        ManosabaLobbyDifficultyUiHost.OnProcess(__instance, __instance, () => LobbyRef(__instance));
    }
}

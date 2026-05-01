using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;

namespace Manosaba.Patches;

[HarmonyPatch]
public static class Patch_NDailyRunLoadScreen_LobbyDifficultyUi
{
    private static readonly AccessTools.FieldRef<NDailyRunLoadScreen, LoadRunLobby?> LobbyRef =
        AccessTools.FieldRefAccess<NDailyRunLoadScreen, LoadRunLobby?>("_lobby");

    [HarmonyPatch(typeof(NDailyRunLoadScreen), nameof(NDailyRunLoadScreen.OnSubmenuOpened))]
    [HarmonyPostfix]
    private static void Postfix_OnSubmenuOpened(NDailyRunLoadScreen __instance)
    {
        ManosabaLobbyDifficultyUiHost.OnEnter(
            __instance,
            __instance,
            __instance,
            () => LobbyDifficultyUiNetContext.From(LobbyRef(__instance)),
            LobbyDifficultyPanelLayout.CharacterSelectDefault,
            LobbyDifficultyUiEnterKind.LoadRunLobbyOpen);
    }

    [HarmonyPatch(typeof(NDailyRunLoadScreen), "CleanUpLobby", new[] { typeof(bool) })]
    [HarmonyPrefix]
    private static void Prefix_CleanUpLobby(NDailyRunLoadScreen __instance)
    {
        ManosabaLobbyDifficultyUiHost.OnCleanup(__instance, () => LobbyDifficultyUiNetContext.From(LobbyRef(__instance)));
    }

    [HarmonyPatch(typeof(NDailyRunLoadScreen), nameof(NDailyRunLoadScreen._Process))]
    [HarmonyPostfix]
    private static void Postfix_Process(NDailyRunLoadScreen __instance, double delta)
    {
        ManosabaLobbyDifficultyUiHost.OnProcess(__instance, __instance, () => LobbyDifficultyUiNetContext.From(LobbyRef(__instance)));
    }
}

using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;

namespace Manosaba.Patches;

[HarmonyPatch]
public static class Patch_NCustomRunLoadScreen_LobbyDifficultyUi
{
    private static readonly AccessTools.FieldRef<NCustomRunLoadScreen, LoadRunLobby?> LobbyRef =
        AccessTools.FieldRefAccess<NCustomRunLoadScreen, LoadRunLobby?>("_lobby");

    [HarmonyPatch(typeof(NCustomRunLoadScreen), nameof(NCustomRunLoadScreen.OnSubmenuOpened))]
    [HarmonyPostfix]
    private static void Postfix_OnSubmenuOpened(NCustomRunLoadScreen __instance)
    {
        ManosabaLobbyDifficultyUiHost.OnEnter(
            __instance,
            __instance,
            __instance,
            () => LobbyDifficultyUiNetContext.From(LobbyRef(__instance)),
            LobbyDifficultyPanelLayout.CustomRunDefault,
            LobbyDifficultyUiEnterKind.LoadRunLobbyOpen);
    }

    [HarmonyPatch(typeof(NCustomRunLoadScreen), "CleanUpLobby", new[] { typeof(bool) })]
    [HarmonyPrefix]
    private static void Prefix_CleanUpLobby(NCustomRunLoadScreen __instance)
    {
        ManosabaLobbyDifficultyUiHost.OnCleanup(__instance, () => LobbyDifficultyUiNetContext.From(LobbyRef(__instance)));
    }

    [HarmonyPatch(typeof(NCustomRunLoadScreen), nameof(NCustomRunLoadScreen._Process))]
    [HarmonyPostfix]
    private static void Postfix_Process(NCustomRunLoadScreen __instance, double delta)
    {
        ManosabaLobbyDifficultyUiHost.OnProcess(__instance, __instance, () => LobbyDifficultyUiNetContext.From(LobbyRef(__instance)));
    }
}

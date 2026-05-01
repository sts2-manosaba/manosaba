using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace Manosaba.Patches;

[HarmonyPatch]
public static class Patch_NCharacterSelectScreen_LobbyDifficultyUi
{
    private static readonly AccessTools.FieldRef<NCharacterSelectScreen, StartRunLobby?> LobbyRef =
        AccessTools.FieldRefAccess<NCharacterSelectScreen, StartRunLobby?>("_lobby");

    [HarmonyPatch(typeof(NCharacterSelectScreen), "AfterInitialized")]
    [HarmonyPostfix]
    private static void Postfix_AfterInitialized(NCharacterSelectScreen __instance)
    {
        ManosabaLobbyDifficultyUiHost.OnEnter(
            __instance,
            __instance,
            __instance,
            () => LobbyDifficultyUiNetContext.From(LobbyRef(__instance)),
            LobbyDifficultyPanelLayout.CharacterSelectDefault,
            LobbyDifficultyUiEnterKind.FirstOpen);
    }

    [HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.OnSubmenuOpened))]
    [HarmonyPostfix]
    private static void Postfix_OnSubmenuOpened(NCharacterSelectScreen __instance)
    {
        ManosabaLobbyDifficultyUiHost.OnEnter(
            __instance,
            __instance,
            __instance,
            () => LobbyDifficultyUiNetContext.From(LobbyRef(__instance)),
            LobbyDifficultyPanelLayout.CharacterSelectDefault,
            LobbyDifficultyUiEnterKind.SubmenuReopened);
    }

    [HarmonyPatch(typeof(NCharacterSelectScreen), "CleanUpLobby", new[] { typeof(bool) })]
    [HarmonyPrefix]
    private static void Prefix_CleanUpLobby(NCharacterSelectScreen __instance)
    {
        ManosabaLobbyDifficultyUiHost.OnCleanup(__instance, () => LobbyDifficultyUiNetContext.From(LobbyRef(__instance)));
    }

    [HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen._Process))]
    [HarmonyPostfix]
    private static void Postfix_Process(NCharacterSelectScreen __instance, double delta)
    {
        ManosabaLobbyDifficultyUiHost.OnProcess(__instance, __instance, () => LobbyDifficultyUiNetContext.From(LobbyRef(__instance)));
    }
}

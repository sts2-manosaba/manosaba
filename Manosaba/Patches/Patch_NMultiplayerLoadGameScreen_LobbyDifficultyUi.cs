using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace Manosaba.Patches;

[HarmonyPatch]
public static class Patch_NMultiplayerLoadGameScreen_LobbyDifficultyUi
{
    private static readonly AccessTools.FieldRef<NMultiplayerLoadGameScreen, LoadRunLobby?> RunLobbyRef =
        AccessTools.FieldRefAccess<NMultiplayerLoadGameScreen, LoadRunLobby?>("_runLobby");

    [HarmonyPatch(typeof(NMultiplayerLoadGameScreen), nameof(NMultiplayerLoadGameScreen.OnSubmenuOpened))]
    [HarmonyPostfix]
    private static void Postfix_OnSubmenuOpened(NMultiplayerLoadGameScreen __instance)
    {
        ManosabaLobbyDifficultyUiHost.OnEnter(
            __instance,
            __instance,
            __instance,
            () => LobbyDifficultyUiNetContext.From(RunLobbyRef(__instance)),
            LobbyDifficultyPanelLayout.CharacterSelectDefault,
            LobbyDifficultyUiEnterKind.LoadRunLobbyOpen);
    }

    [HarmonyPatch(typeof(NMultiplayerLoadGameScreen), nameof(NMultiplayerLoadGameScreen._Process))]
    [HarmonyPostfix]
    private static void Postfix_Process(NMultiplayerLoadGameScreen __instance, double delta)
    {
        ManosabaLobbyDifficultyUiHost.OnProcess(__instance, __instance, () => LobbyDifficultyUiNetContext.From(RunLobbyRef(__instance)));
    }
}

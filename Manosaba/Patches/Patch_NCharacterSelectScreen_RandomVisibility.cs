using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(NCharacterSelectScreen), "UpdateRandomCharacterVisibility")]
public static class Patch_NCharacterSelectScreen_RandomVisibility
{
    private static readonly AccessTools.FieldRef<NCharacterSelectScreen, StartRunLobby?> LobbyRef =
        AccessTools.FieldRefAccess<NCharacterSelectScreen, StartRunLobby?>("_lobby");

    private static readonly AccessTools.FieldRef<NCharacterSelectScreen, NCharacterSelectButton> RandomButtonRef =
        AccessTools.FieldRefAccess<NCharacterSelectScreen, NCharacterSelectButton>("_randomCharacterButton");

    [HarmonyPostfix]
    private static void Postfix(NCharacterSelectScreen __instance)
    {
        StartRunLobby? lobby = LobbyRef(__instance);
        if (lobby == null)
        {
            return;
        }

        RandomButtonRef(__instance).Visible = ManosabaCharacterSelectRandomGate.ShouldShowRandomForLobbyPlayers(lobby.Players);
    }
}

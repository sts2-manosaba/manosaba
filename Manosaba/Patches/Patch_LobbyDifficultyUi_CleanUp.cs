using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;

namespace Manosaba.Patches;

/// <summary>
/// CleanUpLobby is private and its signature differs by game version; each screen needs its own
/// <see cref="HarmonyTargetMethod"/> class (cannot mix with per-method <see cref="HarmonyPatch"/> annotations).
/// </summary>
[HarmonyPatch]
public static class Patch_NCharacterSelectScreen_LobbyDifficultyUiCleanUp
{
    private static readonly AccessTools.FieldRef<NCharacterSelectScreen, StartRunLobby?> LobbyRef =
        AccessTools.FieldRefAccess<NCharacterSelectScreen, StartRunLobby?>("_lobby");

    [HarmonyTargetMethod]
    private static MethodBase Target() =>
        LobbyDifficultyUiPatchUtil.ResolveCleanUpLobbyMethod(typeof(NCharacterSelectScreen))
        ?? throw new InvalidOperationException($"Could not resolve CleanUpLobby on {nameof(NCharacterSelectScreen)}");

    [HarmonyPrefix]
    private static void Prefix(NCharacterSelectScreen __instance) =>
        ManosabaLobbyDifficultyUiHost.OnCleanup(__instance, () => LobbyDifficultyUiNetContext.From(LobbyRef(__instance)));
}

[HarmonyPatch]
public static class Patch_NCustomRunScreen_LobbyDifficultyUiCleanUp
{
    private static readonly AccessTools.FieldRef<NCustomRunScreen, StartRunLobby?> LobbyRef =
        AccessTools.FieldRefAccess<NCustomRunScreen, StartRunLobby?>("_lobby");

    [HarmonyTargetMethod]
    private static MethodBase Target() =>
        LobbyDifficultyUiPatchUtil.ResolveCleanUpLobbyMethod(typeof(NCustomRunScreen))
        ?? throw new InvalidOperationException($"Could not resolve CleanUpLobby on {nameof(NCustomRunScreen)}");

    [HarmonyPrefix]
    private static void Prefix(NCustomRunScreen __instance) =>
        ManosabaLobbyDifficultyUiHost.OnCleanup(__instance, () => LobbyDifficultyUiNetContext.From(LobbyRef(__instance)));
}

[HarmonyPatch]
public static class Patch_NCustomRunLoadScreen_LobbyDifficultyUiCleanUp
{
    private static readonly AccessTools.FieldRef<NCustomRunLoadScreen, LoadRunLobby?> LobbyRef =
        AccessTools.FieldRefAccess<NCustomRunLoadScreen, LoadRunLobby?>("_lobby");

    [HarmonyTargetMethod]
    private static MethodBase Target() =>
        LobbyDifficultyUiPatchUtil.ResolveCleanUpLobbyMethod(typeof(NCustomRunLoadScreen))
        ?? throw new InvalidOperationException($"Could not resolve CleanUpLobby on {nameof(NCustomRunLoadScreen)}");

    [HarmonyPrefix]
    private static void Prefix(NCustomRunLoadScreen __instance) =>
        ManosabaLobbyDifficultyUiHost.OnCleanup(__instance, () => LobbyDifficultyUiNetContext.From(LobbyRef(__instance)));
}

[HarmonyPatch]
public static class Patch_NDailyRunScreen_LobbyDifficultyUiCleanUp
{
    private static readonly AccessTools.FieldRef<NDailyRunScreen, StartRunLobby?> LobbyRef =
        AccessTools.FieldRefAccess<NDailyRunScreen, StartRunLobby?>("_lobby");

    [HarmonyTargetMethod]
    private static MethodBase Target() =>
        LobbyDifficultyUiPatchUtil.ResolveCleanUpLobbyMethod(typeof(NDailyRunScreen))
        ?? throw new InvalidOperationException($"Could not resolve CleanUpLobby on {nameof(NDailyRunScreen)}");

    [HarmonyPrefix]
    private static void Prefix(NDailyRunScreen __instance) =>
        ManosabaLobbyDifficultyUiHost.OnCleanup(__instance, () => LobbyDifficultyUiNetContext.From(LobbyRef(__instance)));
}

[HarmonyPatch]
public static class Patch_NDailyRunLoadScreen_LobbyDifficultyUiCleanUp
{
    private static readonly AccessTools.FieldRef<NDailyRunLoadScreen, LoadRunLobby?> LobbyRef =
        AccessTools.FieldRefAccess<NDailyRunLoadScreen, LoadRunLobby?>("_lobby");

    [HarmonyTargetMethod]
    private static MethodBase Target() =>
        LobbyDifficultyUiPatchUtil.ResolveCleanUpLobbyMethod(typeof(NDailyRunLoadScreen))
        ?? throw new InvalidOperationException($"Could not resolve CleanUpLobby on {nameof(NDailyRunLoadScreen)}");

    [HarmonyPrefix]
    private static void Prefix(NDailyRunLoadScreen __instance) =>
        ManosabaLobbyDifficultyUiHost.OnCleanup(__instance, () => LobbyDifficultyUiNetContext.From(LobbyRef(__instance)));
}

[HarmonyPatch]
public static class Patch_NMultiplayerLoadGameScreen_LobbyDifficultyUiCleanUp
{
    private static readonly AccessTools.FieldRef<NMultiplayerLoadGameScreen, LoadRunLobby?> RunLobbyRef =
        AccessTools.FieldRefAccess<NMultiplayerLoadGameScreen, LoadRunLobby?>("_runLobby");

    [HarmonyTargetMethod]
    private static MethodBase Target() =>
        LobbyDifficultyUiPatchUtil.ResolveCleanUpLobbyMethod(typeof(NMultiplayerLoadGameScreen))
        ?? throw new InvalidOperationException($"Could not resolve CleanUpLobby on {nameof(NMultiplayerLoadGameScreen)}");

    [HarmonyPrefix]
    private static void Prefix(NMultiplayerLoadGameScreen __instance) =>
        ManosabaLobbyDifficultyUiHost.OnCleanup(__instance, () => LobbyDifficultyUiNetContext.From(RunLobbyRef(__instance)));
}

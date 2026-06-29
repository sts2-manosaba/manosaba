using System.Runtime.CompilerServices;
using HarmonyLib;
using Manosaba.Config;
using Manosaba.Multiplayer;
using Manosaba.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Saves;

namespace Manosaba.Patches;

/// <summary>
/// Sync Manosaba lobby difficulty for loaded multiplayer runs.
/// Character-select UI hooks are not active in this flow.
/// </summary>
[HarmonyPatch(typeof(LoadRunLobby))]
public static class Patch_LoadRunLobby_ManosabaDifficultySync
{
    private static readonly ConditionalWeakTable<LoadRunLobby, MessageHandlerDelegate<ManosabaDifficultySettingsMessage>> Handlers = new();

    [HarmonyPatch(MethodType.Constructor, new[] { typeof(INetGameService), typeof(ILoadRunLobbyListener), typeof(SerializableRun) })]
    [HarmonyPostfix]
    private static void Postfix_Ctor_HostOrSingle(LoadRunLobby __instance)
    {
        // Singleplayer runs leave RunFrozen=true; FromSerializable returns early while frozen, so ApplyFromHost
        // would only update lobby (UI) while combat still reads stale _snap* until cleared.
        ManosabaLobbyDifficultyState.ClearRunSnapshot();
        ManosabaLobbyDifficultyState.SetLobbySessionActive(true);
        EnsureHandlerRegistered(__instance);
        if (__instance.NetService.Type != NetGameType.Client)
        {
            long startTime = __instance.Run.StartTime;
            int playerCount = __instance.Run.Players?.Count ?? 1;
            if (startTime != 0 && ManosabaPerSaveDifficultyStore.TryLoadForRun(startTime, playerCount, out ManosabaDifficultySettingsMessage saved))
            {
                ManosabaLobbyDifficultyState.ApplyFromHost(saved);
                ManosabaPerSaveDifficultyStore.TryPersistFromSnapshot(startTime, playerCount, flushImmediately: true);
            }
            else
            {
                ManosabaLobbyDifficultyState.ResetToLobbyDefaults();
            }

            BroadcastCurrentDifficulty(__instance.NetService);
        }
    }

    [HarmonyPatch(MethodType.Constructor, new[] { typeof(INetGameService), typeof(ILoadRunLobbyListener), typeof(ClientLoadJoinResponseMessage) })]
    [HarmonyPostfix]
    private static void Postfix_Ctor_Client(LoadRunLobby __instance)
    {
        ManosabaLobbyDifficultyState.ClearRunSnapshot();
        ManosabaLobbyDifficultyState.ApplyNeutralLobbyPlaceholderBeforeHostDifficultySync();
        ManosabaLobbyDifficultyState.SetLobbySessionActive(true);
        EnsureHandlerRegistered(__instance);
    }

    [HarmonyPatch("HandleClientLoadJoinRequestMessage")]
    [HarmonyPostfix]
    private static void Postfix_HandleClientLoadJoinRequestMessage(LoadRunLobby __instance, ulong senderId)
    {
        if (__instance.NetService is not INetHostGameService hostService)
        {
            return;
        }

        hostService.SendMessage(BuildMessageFromLobbySnapshot(), senderId);
    }

    [HarmonyPatch(nameof(LoadRunLobby.CleanUp))]
    [HarmonyPrefix]
    private static void Prefix_CleanUp(LoadRunLobby __instance)
    {
        ManosabaLobbyDifficultyState.SetLobbySessionActive(false);

        if (!Handlers.TryGetValue(__instance, out MessageHandlerDelegate<ManosabaDifficultySettingsMessage>? handler))
        {
            return;
        }

        __instance.NetService.UnregisterMessageHandler(handler);
        Handlers.Remove(__instance);
    }

    private static void EnsureHandlerRegistered(LoadRunLobby lobby)
    {
        if (Handlers.TryGetValue(lobby, out _))
        {
            return;
        }

        MessageHandlerDelegate<ManosabaDifficultySettingsMessage> handler = delegate (ManosabaDifficultySettingsMessage message, ulong _)
        {
            ManosabaLobbyDifficultyState.FreezeForRunFromHost(message);
        };

        lobby.NetService.RegisterMessageHandler(handler);
        Handlers.Add(lobby, handler);
    }

    internal static void BroadcastCurrentDifficulty(INetGameService netService)
    {
        netService.SendMessage(BuildMessageFromLobbySnapshot());
    }

    private static ManosabaDifficultySettingsMessage BuildMessageFromLobbySnapshot()
    {
        (double hpPct, double atkPct, double murPct, RandomCharacterPoolMode pool) = ManosabaLobbyDifficultyState.GetLobbySnapshot();
        return new ManosabaDifficultySettingsMessage
        {
            enemyHpMultiplierPercent = hpPct,
            enemyAttackDamageMultiplierPercent = atkPct,
            murderousImpulseAllyDamageMultiplierPercent = murPct,
            randomCharacterPoolMode = (byte)pool,
        };
    }
}

using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;

namespace Manosaba.Patches;

public static class ManosabaPlayerChoiceGuard
{
    private sealed class ChoiceState
    {
        public int Count;
        public TaskCompletionSource Completion = NewCompletionSource();
    }

    private static readonly object Lock = new();
    private static readonly Dictionary<ulong, ChoiceState> ActiveChoices = [];

    public static bool IsChoosing(Player player)
    {
        lock (Lock)
        {
            return ActiveChoices.TryGetValue(player.NetId, out ChoiceState? state) && state.Count > 0;
        }
    }

    public static Task WaitUntilNotChoosing(Player player)
    {
        lock (Lock)
        {
            return ActiveChoices.TryGetValue(player.NetId, out ChoiceState? state) && state.Count > 0
                ? state.Completion.Task
                : Task.CompletedTask;
        }
    }

    internal static void BeginChoice(Player player, uint choiceId)
    {
        lock (Lock)
        {
            if (!ActiveChoices.TryGetValue(player.NetId, out ChoiceState? state))
            {
                state = new ChoiceState();
                ActiveChoices[player.NetId] = state;
            }

            state.Count++;
        }

        Log.Debug($"[Manosaba ChoiceGuard] begin player={player.NetId} choice={choiceId}");
    }

    internal static void EndChoice(Player player, uint choiceId)
    {
        TaskCompletionSource? completion = null;
        lock (Lock)
        {
            if (!ActiveChoices.TryGetValue(player.NetId, out ChoiceState? state))
            {
                return;
            }

            state.Count--;
            if (state.Count <= 0)
            {
                ActiveChoices.Remove(player.NetId);
                completion = state.Completion;
            }
        }

        completion?.TrySetResult();
        Log.Debug($"[Manosaba ChoiceGuard] end player={player.NetId} choice={choiceId}");
    }

    private static TaskCompletionSource NewCompletionSource() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}

[HarmonyPatch(typeof(PlayerChoiceSynchronizer))]
public static class Patch_PlayerChoiceSynchronizer_ManosabaPlayerChoiceGuard
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerChoiceSynchronizer.ReserveChoiceId))]
    private static void ReserveChoiceIdPostfix(Player player, uint __result)
    {
        ManosabaPlayerChoiceGuard.BeginChoice(player, __result);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerChoiceSynchronizer.SyncLocalChoice))]
    private static void SyncLocalChoicePostfix(Player player, uint choiceId)
    {
        ManosabaPlayerChoiceGuard.EndChoice(player, choiceId);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerChoiceSynchronizer.WaitForRemoteChoice))]
    private static void WaitForRemoteChoicePostfix(Player player, uint choiceId, ref Task<PlayerChoiceResult> __result)
    {
        __result = EndChoiceWhenRemoteChoiceArrives(player, choiceId, __result);
    }

    private static async Task<PlayerChoiceResult> EndChoiceWhenRemoteChoiceArrives(
        Player player,
        uint choiceId,
        Task<PlayerChoiceResult> choiceTask)
    {
        try
        {
            return await choiceTask;
        }
        finally
        {
            ManosabaPlayerChoiceGuard.EndChoice(player, choiceId);
        }
    }
}

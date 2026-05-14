using System.Collections.Generic;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Runs;

namespace Manosaba.Audio;

/// <summary>
/// AsyncLocal stack of "who this vanilla UI/SFX call belongs to" so character overlay lines
/// only play for the local player in multiplayer when another player triggers the same sound.
/// </summary>
public static class CharacterSfxOverlayBeneficiary
{
    private static readonly AsyncLocal<Stack<ulong?>?> StackHolder = new();

    /// <summary>
    /// <c>card_smith.mp3</c> is played from <c>NCardSmithVfx</c> after async <c>_Ready</c>, so AsyncLocal stack
    /// from <c>SmithRestSiteOption.DoLocalPostSelectVfx</c> is already popped. Use a tiny FIFO instead.
    /// </summary>
    private static readonly object PendingSmithGate = new();
    private static readonly Queue<ulong?> PendingSmithNetIds = new();

    public static void EnqueuePendingSmithCardSound(ulong? ownerNetId)
    {
        lock (PendingSmithGate)
            PendingSmithNetIds.Enqueue(ownerNetId);
    }

    public static bool TryDequeuePendingSmithCardSound(out ulong? ownerNetId)
    {
        lock (PendingSmithGate)
        {
            if (PendingSmithNetIds.Count == 0)
            {
                ownerNetId = null;
                return false;
            }

            ownerNetId = PendingSmithNetIds.Dequeue();
            return true;
        }
    }

    public static bool IsMultiplayerRun(IRunState? run) =>
        run != null && run.Players.Count > 1;

    public static void Push(ulong? netId)
    {
        Stack<ulong?> stack = StackHolder.Value ??= new Stack<ulong?>();
        stack.Push(netId);
    }

    public static void Pop()
    {
        Stack<ulong?>? stack = StackHolder.Value;
        if (stack == null || stack.Count == 0)
            return;

        stack.Pop();
        if (stack.Count == 0)
            StackHolder.Value = null;
    }

    public static ulong? PeekOrNull()
    {
        Stack<ulong?>? stack = StackHolder.Value;
        return stack is { Count: > 0 } ? stack.Peek() : null;
    }

    /// <summary>
    /// In multiplayer, only overlay when the beneficiary is a player and matches <see cref="LocalContext.NetId"/>.
    /// </summary>
    public static bool ShouldOverlayForLocalInMultiplayer(ulong? beneficiaryNetId)
    {
        if (!LocalContext.NetId.HasValue)
            return true;

        if (!beneficiaryNetId.HasValue)
            return false;

        return LocalContext.NetId.Value == beneficiaryNetId.Value;
    }
}

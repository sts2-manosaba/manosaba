using System.Collections.Generic;

namespace Manosaba.Audio;

/// <summary>
/// FIFO beneficiaries for FMOD sounds that are not wrapped in a single command we can Push/Pop
/// (<c>block_break</c> / <c>block_hit</c> from <c>CreatureCmd.Damage</c>), filled from combat history hooks.
/// </summary>
public static class CharacterSfxOverlayOneShot
{
    private static readonly object Gate = new();
    private static readonly Queue<ulong?> BlockBreakDamagePath = new();
    private static readonly Queue<ulong?> BlockHit = new();

    public static void EnqueueBlockBreakDamagePath(ulong? receiverPlayerNetId)
    {
        lock (Gate)
            BlockBreakDamagePath.Enqueue(receiverPlayerNetId);
    }

    public static void EnqueueBlockHit(ulong? receiverPlayerNetId)
    {
        lock (Gate)
            BlockHit.Enqueue(receiverPlayerNetId);
    }

    public static bool TryDequeueBlockBreakDamagePath(out ulong? receiverPlayerNetId)
    {
        lock (Gate)
        {
            if (BlockBreakDamagePath.Count == 0)
            {
                receiverPlayerNetId = null;
                return false;
            }

            receiverPlayerNetId = BlockBreakDamagePath.Dequeue();
            return true;
        }
    }

    public static bool TryDequeueBlockHit(out ulong? receiverPlayerNetId)
    {
        lock (Gate)
        {
            if (BlockHit.Count == 0)
            {
                receiverPlayerNetId = null;
                return false;
            }

            receiverPlayerNetId = BlockHit.Dequeue();
            return true;
        }
    }

    public static void ClearCombatQueues()
    {
        lock (Gate)
        {
            BlockBreakDamagePath.Clear();
            BlockHit.Clear();
        }
    }
}

using HarmonyLib;
using Manosaba;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using System.Collections.Generic;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(AttackIntent), nameof(AttackIntent.GetSingleDamage))]
public static class Patch_AttackIntent_GetSingleDamage_GlobalDisplayOffset
{
    // Global display jitter for monster intents (display only).
    // The offset is stable per monster + round + move, so UI won't flicker.
    private const int MinDisplayOffset = -5;
    private const int MaxDisplayOffset = 5;
    private static readonly System.Random DisplayRng = new();
    private static readonly Dictionary<(uint CombatId, int Round, string MoveId), int> CachedOffsets = [];

    public static void Postfix(Creature owner, ref int __result)
    {
        if (!ManosabaFeatureFlags.AprilFoolsModeEnabled)
            return;

        if (owner == null || !owner.IsMonster)
            return;

        uint? combatId = owner.CombatId;
        var combatState = owner.CombatState;
        if (!combatId.HasValue || combatState == null)
            return;

        string moveId = owner.Monster?.NextMove.Id ?? "UNKNOWN";
        var key = (combatId.Value, combatState.RoundNumber, moveId);
        if (!CachedOffsets.TryGetValue(key, out int offset))
        {
            offset = DisplayRng.Next(MinDisplayOffset, MaxDisplayOffset + 1);
            CachedOffsets[key] = offset;
        }

        __result = System.Math.Max(0, __result + offset);
    }
}

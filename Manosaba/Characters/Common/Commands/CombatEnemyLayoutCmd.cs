using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Manosaba.Characters.Common.Commands;

/// <summary>Mid-combat enemy spawns that respect encounter slot layout and avoid visual overlap.</summary>
public static class CombatEnemyLayoutCmd
{
    private const float DefaultPadding = 70f;
    private const float MinimumAutoPadding = 5f;
    private const float CenterSafeZone = 150f;
    private const float MinAlternatingYPos = 40f;
    private const float MaxAlternatingYPos = 60f;
    private const float AlternateYPosBeginPadding = 30f;

    public static async Task<Creature?> TryAddEnemy<T>(CombatState combatState) where T : MonsterModel
    {
        if (!CanSpawnAnotherEnemy(combatState))
        {
            return null;
        }

        string? slotName = TryGetNextEnemySlot(combatState);
        Creature creature = await CreatureCmd.Add<T>(combatState, slotName);

        if (slotName == null)
        {
            RepositionNonSlottedEnemies(combatState);
        }

        return creature;
    }

    /// <summary>
    /// Slotted encounters only spawn while an encounter slot is free.
    /// Non-slotted encounters use the same practical cap as the widest vanilla layouts.
    /// </summary>
    public static bool CanSpawnAnotherEnemy(CombatState combatState)
    {
        int aliveEnemyCount = combatState.Enemies.Count(enemy => enemy.IsAlive);

        if (combatState.Encounter is { HasScene: true } encounter)
        {
            return !string.IsNullOrEmpty(encounter.GetNextSlot(combatState));
        }

        const int maxNonSlottedEnemies = 6;
        return aliveEnemyCount < maxNonSlottedEnemies;
    }

    public static int CountAvailableEnemySlots(CombatState combatState)
    {
        if (combatState.Encounter is not { HasScene: true } encounter)
        {
            return Math.Max(0, 6 - combatState.Enemies.Count(enemy => enemy.IsAlive));
        }

        return encounter.Slots.Count(slot => combatState.Enemies.All(enemy => enemy.SlotName != slot));
    }

    public static string? TryGetNextEnemySlot(CombatState combatState)
    {
        if (combatState.Encounter is not { HasScene: true } encounter)
        {
            return null;
        }

        string slot = encounter.GetNextSlot(combatState);
        return string.IsNullOrEmpty(slot) ? null : slot;
    }

    private static void RepositionNonSlottedEnemies(CombatState combatState)
    {
        NCombatRoom? room = NCombatRoom.Instance;
        if (room == null)
        {
            return;
        }

        List<NCreature> enemies = GetAliveEnemyNodes(room);
        if (enemies.Count == 0)
        {
            return;
        }

        float scaling = combatState.Encounter?.GetCameraScaling() ?? 1f;
        float availableWidth = 960f / scaling;
        float padding = DefaultPadding;
        float totalWidth = enemies.Sum(creature => creature.Visuals.Bounds.Size.X);
        float rowWidth = totalWidth + (enemies.Count - 1) * padding;
        float cursorX = (availableWidth - rowWidth) * 0.5f;
        cursorX = Math.Max(cursorX, CenterSafeZone);
        float alternatingYOffset = 0f;

        if (cursorX + rowWidth > availableWidth)
        {
            padding = Math.Max((availableWidth - CenterSafeZone - totalWidth) / (enemies.Count - 1), MinimumAutoPadding);
            rowWidth = totalWidth + (enemies.Count - 1) * padding;
            cursorX = (availableWidth - rowWidth) * 0.5f;
            if (padding < AlternateYPosBeginPadding)
            {
                alternatingYOffset = float.Lerp(MaxAlternatingYPos, MinAlternatingYPos, (padding - MinimumAutoPadding) / (AlternateYPosBeginPadding - MinimumAutoPadding));
            }
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            NCreature enemyNode = enemies[i];
            enemyNode.Position = new Vector2(cursorX + enemyNode.Visuals.Bounds.Size.X * 0.5f, 200f - (i % 2 != 0 ? alternatingYOffset : 0f));
            cursorX += enemyNode.Visuals.Bounds.Size.X + padding;
        }
    }

    private static List<NCreature> GetAliveEnemyNodes(NCombatRoom room)
    {
        return room.CreatureNodes
            .Where(node => node.Entity is { IsMonster: true, IsAlive: true, Side: CombatSide.Enemy, PetOwner: null })
            .OrderBy(node => node.Position.X)
            .ToList();
    }
}

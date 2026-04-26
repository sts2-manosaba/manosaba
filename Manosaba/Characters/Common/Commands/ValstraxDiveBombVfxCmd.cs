using Godot;
using Manosaba.Characters.Common.Vfx;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Manosaba.Characters.Common.Commands;

public static class ValstraxDiveBombVfxCmd
{
    private const string ScenePath = "res://Manosaba/scenes/common/vfx/valstrax_dive_bomb.tscn";
    private const float ImpactYOffsetRatio = 0.1f;

    public static async Task PlayUntilImpactForTargets(IReadOnlyList<Creature> targets)
    {
        NCombatRoom? combatRoom = NCombatRoom.Instance;
        NGame? game = NGame.Instance;
        if (combatRoom == null || game == null)
        {
            return;
        }

        Node2D node = PreloadManager.Cache.GetScene(ScenePath).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
        combatRoom.CombatVfxContainer.AddChildSafely(node);
        if (node is not ValstraxDiveBombVfx vfx)
        {
            node.QueueFree();
            return;
        }

        Vector2 viewportSize = game.GetViewportRect().Size;
        Vector2 impact = ResolveImpactPosition(targets, viewportSize);
        Vector2 start = new(-viewportSize.X * 0.18f, -viewportSize.Y * 0.22f);

        vfx.Configure(start, impact);

        Task playTask = vfx.PlayAsync();
        await vfx.ToSignal(vfx, ValstraxDiveBombVfx.SignalName.ImpactStarted);
        _ = playTask;
    }

    public static async Task PlayForTargets(IReadOnlyList<Creature> targets)
    {
        NCombatRoom? combatRoom = NCombatRoom.Instance;
        NGame? game = NGame.Instance;
        if (combatRoom == null || game == null)
        {
            return;
        }

        Node2D node = PreloadManager.Cache.GetScene(ScenePath).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
        combatRoom.CombatVfxContainer.AddChildSafely(node);
        if (node is not ValstraxDiveBombVfx vfx)
        {
            node.QueueFree();
            return;
        }

        Vector2 viewportSize = game.GetViewportRect().Size;
        Vector2 impact = ResolveImpactPosition(targets, viewportSize);
        Vector2 start = new(-viewportSize.X * 0.18f, -viewportSize.Y * 0.22f);

        vfx.Configure(start, impact);
        await vfx.PlayAsync();
    }

    private static Vector2 ResolveImpactPosition(IReadOnlyList<Creature> targets, Vector2 viewportSize)
    {
        List<Vector2> positions = [];
        foreach (Creature target in targets)
        {
            if (target == null || !target.IsAlive)
            {
                continue;
            }

            NCreature? targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
            if (targetNode != null)
            {
                positions.Add(targetNode.GlobalPosition);
            }
        }

        if (positions.Count == 0)
        {
            Vector2 fallback = new(viewportSize.X * 0.78f, viewportSize.Y * 0.56f);
            return ShiftImpactUp(fallback, viewportSize);
        }

        Vector2 sum = Vector2.Zero;
        foreach (Vector2 position in positions)
        {
            sum += position;
        }

        Vector2 average = sum / positions.Count;
        return ShiftImpactUp(average, viewportSize);
    }

    private static Vector2 ShiftImpactUp(Vector2 impact, Vector2 viewportSize)
    {
        float yOffset = viewportSize.Y * ImpactYOffsetRatio;
        float shiftedY = Mathf.Clamp(impact.Y - yOffset, 0f, viewportSize.Y);
        return new Vector2(impact.X, shiftedY);
    }
}

using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Manosaba.Characters.TonoHanna.Visuals;

/// <summary>Lifts character art when they receive Hanna's soar-like buff from a same-side source (self or ally), e.g. <see cref="Cards.FlyingBroom"/>.</summary>
internal static class HannaSoarLiftVisual
{
    private const float BaseLiftPixels = 72f;
    private const float LiftPixels = BaseLiftPixels * 3f;
    private const float DurationSeconds = 0.28f;

    private static readonly Dictionary<Creature, float> s_baseYByCreature = new();
    private static readonly object s_lock = new();

    public static async Task TryLiftAsync(Creature owner, Creature? applier)
    {
        if (applier == null || applier.Side != owner.Side)
            return;

        NCombatRoom? room = NCombatRoom.Instance;
        NCreature? node = room?.GetCreatureNode(owner);
        Node2D? visuals = node?.Visuals;
        if (room == null || visuals == null)
            return;

        float baseY;
        lock (s_lock)
        {
            if (!s_baseYByCreature.ContainsKey(owner))
                s_baseYByCreature[owner] = visuals.Position.Y;
            baseY = s_baseYByCreature[owner];
        }

        float targetY = baseY - LiftPixels;
        Tween tween = room.CreateTween();
        tween.TweenProperty(visuals, "position:y", targetY, DurationSeconds)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
        await room.ToSignal(tween, Tween.SignalName.Finished);
    }

    public static async Task TryRestoreAsync(Creature oldOwner)
    {
        float baseY;
        lock (s_lock)
        {
            if (!s_baseYByCreature.TryGetValue(oldOwner, out baseY))
                return;
            s_baseYByCreature.Remove(oldOwner);
        }

        NCombatRoom? room = NCombatRoom.Instance;
        NCreature? creatureNode = room?.GetCreatureNode(oldOwner);
        Node2D? visuals = creatureNode?.Visuals;
        if (room == null || visuals == null)
            return;

        Tween tween = room.CreateTween();
        tween.TweenProperty(visuals, "position:y", baseY, DurationSeconds * 0.9f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.In);
        await room.ToSignal(tween, Tween.SignalName.Finished);
    }
}

using Godot;
using manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace Manosaba.Characters.ShitoAlisa.Visuals;

public static class RedBindVisuals
{
    public static void Sync(Creature owner, bool active)
    {
        if (TestMode.IsOn)
            return;

        NCreature? creature = NCombatRoom.Instance?.GetCreatureNode(owner);
        if (creature == null)
            return;

        RedBindRing ring = RedBindRing.GetOrCreate(creature);
        ring.SetActive(active);
    }
}

/// <summary>
/// A lightweight "binding ring" overlay that follows target <see cref="NCreature"/> model.
/// </summary>
public sealed partial class RedBindRing : Node2D
{
    private const string RedBindIconPath = "red_bind_power.png";

    private readonly Sprite2D _icon = new();
    private NCreature? _creatureNode;
    private bool _active;

    public static RedBindRing GetOrCreate(NCreature creature)
    {
        foreach (Node child in creature.GetChildren())
        {
            if (child is RedBindRing existing)
                return existing;
        }

        var ring = new RedBindRing();
        creature.AddChildSafely(ring);
        ring._creatureNode = creature;
        ring.Position = Vector2.Zero;
        return ring;
    }

    public override void _Ready()
    {
        Texture2D? texture = ResourceLoader.Load<Texture2D>(RedBindIconPath.PowerImagePath());

        _icon.Texture = texture;
        _icon.Centered = true;
        _icon.Modulate = new Color(1f, 1f, 1f, 0.85f);
        _icon.Scale = new Vector2(1.08f, 0.44f);
        _icon.ZIndex = 1;
        AddChild(_icon);
        Visible = false;
    }

    public void SetActive(bool active)
    {
        _active = active;
        Visible = active;
    }

    public override void _Process(double delta)
    {
        if (!_active || _creatureNode == null || !GodotObject.IsInstanceValid(_creatureNode))
            return;

        GlobalPosition = _creatureNode.VfxSpawnPosition + new Vector2(0f, -12f);

        int top = _creatureNode.GetChildCount() - 1;
        if (GetIndex() != top)
            _creatureNode.MoveChild(this, top);
    }

}


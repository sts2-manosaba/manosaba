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
    private const float BaseScaleX = 1.08f;
    private const float BaseScaleY = 0.44f;
    private const float WidthCoverageRatio = 0.82f;
    private const float MinScaleX = 0.42f;
    private const float MaxScaleX = 1.24f;
    private static readonly string[] BoundsExclusionTokens = ["shadow", "fx", "vfx", "weapon", "trail"];

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
        _icon.Scale = new Vector2(BaseScaleX, BaseScaleY);
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

        SyncScaleWithCreatureWidth();
        GlobalPosition = _creatureNode.VfxSpawnPosition + new Vector2(0f, -12f);

        int top = _creatureNode.GetChildCount() - 1;
        if (GetIndex() != top)
            _creatureNode.MoveChild(this, top);
    }

    private void SyncScaleWithCreatureWidth()
    {
        if (_icon.Texture == null || _creatureNode == null || !GodotObject.IsInstanceValid(_creatureNode.Visuals))
            return;

        float bodyWidth = GetCreatureWorldWidth(_creatureNode);
        if (bodyWidth <= 0f)
        {
            // Fallback to a narrow look when no reliable body bounds were found.
            _icon.Scale = new Vector2(MinScaleX, MinScaleX * (BaseScaleY / BaseScaleX));
            return;
        }

        float targetWidth = bodyWidth * WidthCoverageRatio;
        float iconWidth = _icon.Texture.GetSize().X;
        if (targetWidth <= 0f || iconWidth <= 0f)
            return;

        float scaleX = Mathf.Clamp(targetWidth / iconWidth, MinScaleX, MaxScaleX);
        float yPerX = BaseScaleY / BaseScaleX;
        _icon.Scale = new Vector2(scaleX, scaleX * yPerX);
    }

    private static float GetCreatureWorldWidth(NCreature creatureNode)
    {
        float hitboxWidth = 0f;
        if (GodotObject.IsInstanceValid(creatureNode.Hitbox))
            hitboxWidth = creatureNode.Hitbox.Size.X;

        float visualWidth = 0f;
        Node2D? body = creatureNode.Visuals.GetCurrentBody();
        if (body != null && GodotObject.IsInstanceValid(body))
            visualWidth = GetBodyWorldWidth(body);

        // Hitbox is generally the most gameplay-accurate creature footprint.
        return Mathf.Max(hitboxWidth, visualWidth);
    }

    private static float GetBodyWorldWidth(Node2D? body)
    {
        if (body == null || !GodotObject.IsInstanceValid(body))
            return 0f;

        if (TryGetSpriteBounds(body, out Rect2 bodyBounds))
            return bodyBounds.Size.X;

        if (TryGetBoundsFromVisualTree(body, out Rect2 worldBounds))
            return worldBounds.Size.X;

        return 0f;
    }

    private static bool TryGetBoundsFromVisualTree(Node root, out Rect2 bounds)
    {
        bool hasBounds = false;
        bounds = default;

        var stack = new Stack<Node>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            Node node = stack.Pop();
            if (ShouldExcludeNode(node))
                continue;

            if (TryGetSpriteBounds(node, out Rect2 spriteBounds))
            {
                if (!hasBounds)
                {
                    bounds = spriteBounds;
                    hasBounds = true;
                }
                else
                {
                    bounds = bounds.Merge(spriteBounds);
                }
            }

            foreach (Node child in node.GetChildren())
                stack.Push(child);
        }

        return hasBounds;
    }

    private static bool ShouldExcludeNode(Node node)
    {
        string lowerName = node.Name.ToString().ToLowerInvariant();
        foreach (string token in BoundsExclusionTokens)
        {
            if (lowerName.Contains(token))
                return true;
        }

        return false;
    }

    private static bool TryGetSpriteBounds(Node node, out Rect2 rect)
    {
        rect = default;

        if (node is Sprite2D sprite && sprite.Texture != null)
        {
            Rect2 localRect = sprite.GetRect();
            if (localRect.Size.X <= 0f || localRect.Size.Y <= 0f)
                return false;

            Vector2 a = sprite.ToGlobal(localRect.Position);
            Vector2 b = sprite.ToGlobal(new Vector2(localRect.End.X, localRect.Position.Y));
            Vector2 c = sprite.ToGlobal(new Vector2(localRect.Position.X, localRect.End.Y));
            Vector2 d = sprite.ToGlobal(localRect.End);
            float minX = Mathf.Min(Mathf.Min(a.X, b.X), Mathf.Min(c.X, d.X));
            float minY = Mathf.Min(Mathf.Min(a.Y, b.Y), Mathf.Min(c.Y, d.Y));
            float maxX = Mathf.Max(Mathf.Max(a.X, b.X), Mathf.Max(c.X, d.X));
            float maxY = Mathf.Max(Mathf.Max(a.Y, b.Y), Mathf.Max(c.Y, d.Y));
            rect = new Rect2(new Vector2(minX, minY), new Vector2(maxX - minX, maxY - minY));
            return true;
        }

        if (node is AnimatedSprite2D animated && animated.SpriteFrames != null)
        {
            Texture2D? frameTexture = animated.SpriteFrames.GetFrameTexture(animated.Animation, animated.Frame);
            if (frameTexture == null)
                return false;

            Vector2 size = frameTexture.GetSize() * animated.GlobalScale.Abs();
            if (size.X <= 0f || size.Y <= 0f)
                return false;

            Vector2 origin = animated.Centered ? animated.GlobalPosition - size * 0.5f : animated.GlobalPosition;
            rect = new Rect2(origin, size);
            return true;
        }

        return false;
    }
}


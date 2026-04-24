using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System;
using System.Collections.Generic;

namespace Manosaba.Characters.TonoHanna.Visuals;

/// <summary>
/// Child of <see cref="NCreature"/>; <see cref="PuppetCompanionLayout"/> defines 13 positions (top / sides / corners).
/// </summary>
public partial class PuppetCollectionRing : Node2D
{
    public const int MaxSlots = 13;

    private const float SpriteScale = 0.3f;

    private readonly PuppetBobSlot[] _slots = new PuppetBobSlot[MaxSlots];

    public static PuppetCollectionRing GetOrCreate(NCreature creature)
    {
        foreach (Node child in creature.GetChildren())
        {
            if (child is PuppetCollectionRing existing)
                return existing;
        }

        var ring = new PuppetCollectionRing();
        creature.AddChildSafely(ring);
        ring.Position = Vector2.Zero;
        return ring;
    }

    public override void _Ready()
    {
        for (int i = 0; i < MaxSlots; i++)
        {
            var slot = new PuppetBobSlot(i);
            slot.Position = PuppetCompanionLayout.SlotLocalPositions[i];
            AddChild(slot);
            _slots[i] = slot;
        }
    }

    public void SetSlotActive(int slotIndex, bool active, string textureResourcePath)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots)
            return;
        _slots[slotIndex].SetActive(active, textureResourcePath, SpriteScale);
    }
}

public partial class PuppetBobSlot : Node2D
{
    private const float BobAmplitude = 8f;
    private const float BobSpeed = 2.4f;
    private static readonly Dictionary<string, Texture2D?> TextureCache = new(StringComparer.Ordinal);

    private readonly float _phase;
    private Sprite2D? _sprite;
    private bool _active;
    private Vector2 _basePos;
    private string? _currentTexturePath;

    public PuppetBobSlot(int slotIndex)
    {
        _phase = slotIndex * 0.52f;
    }

    public override void _Ready()
    {
        _basePos = Position;
    }

    public void SetActive(bool active, string textureResourcePath, float spriteScale)
    {
        _active = active;
        if (!active)
        {
            if (_sprite != null)
                _sprite.Visible = false;
            return;
        }

        if (_sprite == null)
        {
            _sprite = new Sprite2D();
            AddChild(_sprite);
        }

        if (!string.Equals(_currentTexturePath, textureResourcePath, StringComparison.Ordinal))
        {
            Texture2D? tex = GetCachedTexture(textureResourcePath);
            if (tex != null)
            {
                _sprite.Texture = tex;
                _currentTexturePath = textureResourcePath;
            }
        }
        _sprite.Scale = new Vector2(spriteScale, spriteScale);
        _sprite.Visible = true;
    }

    public override void _Process(double delta)
    {
        if (!_active || _sprite == null || !_sprite.Visible)
            return;

        float t = (float)Time.GetTicksMsec() / 1000f * BobSpeed + _phase;
        Position = new Vector2(_basePos.X, _basePos.Y + Mathf.Sin(t) * BobAmplitude);
    }

    private static Texture2D? GetCachedTexture(string textureResourcePath)
    {
        if (TextureCache.TryGetValue(textureResourcePath, out Texture2D? cached))
        {
            return cached;
        }

        Texture2D? loaded = ResourceLoader.Load<Texture2D>(textureResourcePath);
        TextureCache[textureResourcePath] = loaded;
        return loaded;
    }
}

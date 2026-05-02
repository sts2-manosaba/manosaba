using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Manosaba.Characters.Common.Visuals;

public static class WitchIslandExpeditionParryCueVisuals
{
    public static void PlayForLocalTarget(IEnumerable<Creature> targets, float durationSeconds)
    {
        foreach (Creature target in targets)
        {
            if (!LocalContext.IsMe(target))
            {
                continue;
            }

            NCreature? creature = NCombatRoom.Instance?.GetCreatureNode(target);
            if (creature == null)
            {
                return;
            }

            WitchIslandExpeditionParryCueNode.GetOrCreate(creature).Play(durationSeconds);
            return;
        }
    }
}

public sealed partial class WitchIslandExpeditionParryCueNode : Node2D
{
    private const float DefaultDurationSeconds = 0.3f;
    private const float VerticalOffset = -78f;

    private const float BeamScaleX = 0.92f;
    private const float BeamScaleY = 0.8f;
    private const float VerticalBeamScaleX = 0.72f;
    private const float VerticalBeamScaleY = 0.9f;
    private const float HaloScaleX = 1.55f;
    private const float HaloScaleY = 0.44f;
    private const float CoreScale = 0.5f;
    private const float StarScale = 0.36f;
    private const float RingScale = 0.74f;
    private const float GlintScaleX = 0.74f;
    private const float GlintScaleY = 0.035f;
    private const float VerticalGlintScaleX = 0.46f;
    private const float VerticalGlintScaleY = 0.028f;

    private static readonly Color HaloColor = new(1f, 0.02f, 0.01f, 1f);
    private static readonly Color BeamColor = new(1f, 0.035f, 0.015f, 1f);
    private static readonly Color CoreColor = new(1f, 0.48f, 0.18f, 1f);
    private static readonly Color StarColor = new(1f, 0.97f, 0.9f, 1f);
    private static readonly Color RingColor = new(1f, 0.95f, 0.88f, 1f);
    private static readonly Color GlintColor = new(1f, 0.96f, 0.9f, 1f);

    private NCreature? _creatureNode;
    private Sprite2D? _halo;
    private Sprite2D? _beam;
    private Sprite2D? _verticalBeam;
    private Sprite2D? _core;
    private Sprite2D? _whiteGlint;
    private Sprite2D? _verticalGlint;
    private Sprite2D? _star;
    private Sprite2D? _ring;
    private float _durationSeconds = DefaultDurationSeconds;
    private float _elapsedSeconds;
    private bool _active;

    public static WitchIslandExpeditionParryCueNode GetOrCreate(NCreature creature)
    {
        foreach (Node child in creature.GetChildren())
        {
            if (child is WitchIslandExpeditionParryCueNode existing && !existing.IsQueuedForDeletion())
            {
                return existing;
            }
        }

        var cue = new WitchIslandExpeditionParryCueNode
        {
            _creatureNode = creature,
            ZIndex = 120,
        };
        creature.AddChildSafely(cue);
        return cue;
    }

    public void Play(float durationSeconds)
    {
        EnsureSprites();
        _durationSeconds = durationSeconds > 0f ? durationSeconds : DefaultDurationSeconds;
        _elapsedSeconds = 0f;
        _active = true;
        Visible = true;
        ApplyVisualState(0f);
    }

    public override void _Ready()
    {
        EnsureSprites();
    }

    public override void _Process(double delta)
    {
        if (!_active)
        {
            return;
        }

        if (_creatureNode == null || !GodotObject.IsInstanceValid(_creatureNode))
        {
            QueueFree();
            return;
        }

        GlobalPosition = _creatureNode.VfxSpawnPosition + new Vector2(0f, VerticalOffset);
        EnsureTopmostCreatureChild();

        _elapsedSeconds += (float)delta;
        if (_elapsedSeconds >= _durationSeconds)
        {
            QueueFree();
            return;
        }

        ApplyVisualState(Mathf.Clamp(_elapsedSeconds / _durationSeconds, 0f, 1f));
    }

    private void EnsureSprites()
    {
        if (_halo != null && GodotObject.IsInstanceValid(_halo))
        {
            return;
        }

        Texture2D beamTexture = ParryCueTextureCache.BeamTexture;
        Texture2D radialTexture = ParryCueTextureCache.RadialTexture;
        Texture2D starTexture = ParryCueTextureCache.StarTexture;
        Texture2D ringTexture = ParryCueTextureCache.RingTexture;
        ShaderMaterial additiveMaterial = ParryCueTextureCache.AdditiveMaterial;

        _halo = CreateSprite(radialTexture, additiveMaterial, zIndex: 0);
        _beam = CreateSprite(beamTexture, additiveMaterial, zIndex: 1);
        _verticalBeam = CreateSprite(beamTexture, additiveMaterial, zIndex: 2);
        _core = CreateSprite(radialTexture, additiveMaterial, zIndex: 2);
        _whiteGlint = CreateSprite(beamTexture, additiveMaterial, zIndex: 3);
        _verticalGlint = CreateSprite(beamTexture, additiveMaterial, zIndex: 4);
        _star = CreateSprite(starTexture, additiveMaterial, zIndex: 5);
        _ring = CreateSprite(ringTexture, additiveMaterial, zIndex: 6);
        _verticalBeam.Rotation = Mathf.Pi * 0.5f;
        _verticalGlint.Rotation = Mathf.Pi * 0.5f;

        AddChild(_halo);
        AddChild(_beam);
        AddChild(_verticalBeam);
        AddChild(_core);
        AddChild(_whiteGlint);
        AddChild(_verticalGlint);
        AddChild(_star);
        AddChild(_ring);
        ApplyVisualState(0f);
    }

    private static Sprite2D CreateSprite(Texture2D texture, Material material, int zIndex)
    {
        return new Sprite2D
        {
            Texture = texture,
            Centered = true,
            Material = material,
            ZIndex = zIndex,
        };
    }

    private void ApplyVisualState(float progress)
    {
        if (_halo == null ||
            _beam == null ||
            _verticalBeam == null ||
            _core == null ||
            _whiteGlint == null ||
            _verticalGlint == null ||
            _star == null ||
            _ring == null)
        {
            return;
        }

        float alpha = FadeAlpha(progress);
        float openT = SmoothStep(Mathf.Clamp(progress / 0.27f, 0f, 1f));
        float stretchT = SmoothStep(Mathf.Clamp((progress - 0.8f) / 0.2f, 0f, 1f));
        float lengthScale = Mathf.Lerp(0.18f, 1f, openT) * Mathf.Lerp(1f, 1.14f, stretchT);
        float pulse = 0.82f + 0.18f * Mathf.Sin(progress * Mathf.Tau * 4f);

        _halo.Scale = new Vector2(HaloScaleX * lengthScale, HaloScaleY * Mathf.Lerp(0.86f, 1f, openT));
        _beam.Scale = new Vector2(BeamScaleX * lengthScale, BeamScaleY);
        _verticalBeam.Scale = new Vector2(VerticalBeamScaleX * lengthScale, VerticalBeamScaleY);
        _core.Scale = Vector2.One * CoreScale * Mathf.Lerp(0.9f, 1.18f, pulse);
        _whiteGlint.Scale = new Vector2(GlintScaleX * lengthScale * Mathf.Lerp(0.9f, 1.04f, pulse), GlintScaleY);
        _verticalGlint.Scale = new Vector2(VerticalGlintScaleX * lengthScale * Mathf.Lerp(0.9f, 1.08f, pulse), VerticalGlintScaleY);
        _star.Scale = Vector2.One * StarScale * Mathf.Lerp(0.9f, 1.16f, pulse);
        _ring.Scale = Vector2.One * RingScale * Mathf.Lerp(0.96f, 1.04f, pulse);

        _halo.Modulate = WithAlpha(HaloColor, alpha * 0.72f);
        _beam.Modulate = WithAlpha(BeamColor, alpha);
        _verticalBeam.Modulate = WithAlpha(BeamColor, alpha * 0.95f);
        _core.Modulate = WithAlpha(CoreColor, alpha);
        _whiteGlint.Modulate = WithAlpha(GlintColor, alpha);
        _verticalGlint.Modulate = WithAlpha(GlintColor, alpha * 0.95f);
        _star.Modulate = WithAlpha(StarColor, alpha);
        _ring.Modulate = WithAlpha(RingColor, alpha * 0.72f);
    }

    private void EnsureTopmostCreatureChild()
    {
        if (_creatureNode == null || GetParent() != _creatureNode)
        {
            return;
        }

        int top = _creatureNode.GetChildCount() - 1;
        if (top >= 0 && GetIndex() != top)
        {
            _creatureNode.MoveChild(this, top);
        }
    }

    private static float FadeAlpha(float progress)
    {
        float fadeIn = SmoothStep(Mathf.Clamp(progress / 0.22f, 0f, 1f));
        float fadeOut = SmoothStep(Mathf.Clamp((1f - progress) / 0.2f, 0f, 1f));
        return Mathf.Min(fadeIn, fadeOut);
    }

    private static float SmoothStep(float value)
    {
        return value * value * (3f - 2f * value);
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        return new Color(color.R, color.G, color.B, Mathf.Clamp(alpha, 0f, 1f));
    }
}

internal static class ParryCueTextureCache
{
    private const int BeamTextureWidth = 640;
    private const int BeamTextureHeight = 128;
    private const int RadialTextureSize = 192;
    private const int StarTextureSize = 256;
    private const int RingTextureSize = 256;

    private static Texture2D? _beamTexture;
    private static Texture2D? _radialTexture;
    private static Texture2D? _starTexture;
    private static Texture2D? _ringTexture;
    private static ShaderMaterial? _additiveMaterial;

    public static Texture2D BeamTexture => _beamTexture ??= CreateBeamTexture(BeamTextureWidth, BeamTextureHeight);
    public static Texture2D RadialTexture => _radialTexture ??= CreateRadialTexture(RadialTextureSize);
    public static Texture2D StarTexture => _starTexture ??= CreateStarTexture(StarTextureSize);
    public static Texture2D RingTexture => _ringTexture ??= CreateRingTexture(RingTextureSize);
    public static ShaderMaterial AdditiveMaterial => _additiveMaterial ??= CreateAdditiveMaterial();

    private static ShaderMaterial CreateAdditiveMaterial()
    {
        Shader shader = new()
        {
            Code = """
                shader_type canvas_item;
                render_mode blend_add, unshaded;

                void fragment() {
                    COLOR = texture(TEXTURE, UV) * COLOR;
                }
                """
        };

        return new ShaderMaterial
        {
            Shader = shader,
        };
    }

    private static Texture2D CreateBeamTexture(int width, int height)
    {
        Image image = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        float maxX = Math.Max(1f, width - 1f);
        float maxY = Math.Max(1f, height - 1f);

        for (int y = 0; y < height; y++)
        {
            float normalizedY = y / maxY * 2f - 1f;
            for (int x = 0; x < width; x++)
            {
                float normalizedX = x / maxX * 2f - 1f;
                float taper = MathF.Pow(Mathf.Clamp(1f - MathF.Abs(normalizedX), 0f, 1f), 0.35f);
                float thinCore = MathF.Exp(-MathF.Pow(normalizedY / 0.055f, 2f)) * taper;
                float softRadius = 0.16f + 0.12f * taper;
                float softBody = MathF.Exp(-MathF.Pow(normalizedY / softRadius, 2f)) * taper;
                float alpha = Mathf.Clamp(MathF.Max(thinCore, softBody * 0.92f), 0f, 1f);
                image.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        return ImageTexture.CreateFromImage(image);
    }

    private static Texture2D CreateRadialTexture(int size)
    {
        Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        Vector2 center = new((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = Math.Max(1f, size * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float normalized = center.DistanceTo(new Vector2(x, y)) / radius;
                float alpha = Mathf.Clamp(1f - normalized, 0f, 1f);
                if (alpha > 0f)
                {
                    alpha = MathF.Pow(alpha, 2.1f);
                }

                image.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        return ImageTexture.CreateFromImage(image);
    }

    private static Texture2D CreateStarTexture(int size)
    {
        Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        float max = Math.Max(1f, size - 1f);

        for (int y = 0; y < size; y++)
        {
            float normalizedY = y / max * 2f - 1f;
            for (int x = 0; x < size; x++)
            {
                float normalizedX = x / max * 2f - 1f;
                float absX = MathF.Abs(normalizedX);
                float absY = MathF.Abs(normalizedY);
                float horizontalTaper = MathF.Pow(Mathf.Clamp(1f - absX, 0f, 1f), 0.72f);
                float verticalTaper = MathF.Pow(Mathf.Clamp(1f - absY, 0f, 1f), 0.72f);
                float horizontalBulge = MathF.Exp(-MathF.Pow(absX / 0.2f, 2.2f));
                float verticalBulge = MathF.Exp(-MathF.Pow(absY / 0.2f, 2.2f));
                float horizontalRadius = 0.035f + 0.48f * horizontalBulge;
                float verticalRadius = 0.035f + 0.48f * verticalBulge;
                float horizontalArm = MathF.Exp(-MathF.Pow(absY / horizontalRadius, 2f)) * horizontalTaper;
                float verticalArm = MathF.Exp(-MathF.Pow(absX / verticalRadius, 2f)) * verticalTaper;
                float center = MathF.Exp(-(normalizedX * normalizedX + normalizedY * normalizedY) / 0.075f);
                float alpha = Mathf.Clamp(MathF.Max(MathF.Max(horizontalArm, verticalArm), center), 0f, 1f);
                image.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        return ImageTexture.CreateFromImage(image);
    }

    private static Texture2D CreateRingTexture(int size)
    {
        Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        Vector2 center = new((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = Math.Max(1f, size * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float normalized = center.DistanceTo(new Vector2(x, y)) / radius;
                float ring = MathF.Exp(-MathF.Pow((normalized - 0.62f) / 0.012f, 2f));
                float alpha = Mathf.Clamp(ring, 0f, 1f);
                image.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        return ImageTexture.CreateFromImage(image);
    }
}

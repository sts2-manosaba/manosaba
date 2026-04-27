using Godot;
using MegaCrit.Sts2.Core.Commands;

namespace Manosaba.Characters.Common.Vfx;

public partial class ValstraxDiveBombVfx : Node2D
{
    [Signal]
    public delegate void ImpactStartedEventHandler();

    private const float DiveDuration = 0.36f;
    private const float AmbushReadyDuration = 1.8f;
    private const float DiveStartDelay = AmbushReadyDuration - DiveDuration;
    private const float CleanupDelay = 2.3f;
    private const string AmbushReadySfx = "event:/Manosaba/audio/monsters/valstrax_ambush_ready";
    private const string SuperImpactSfx = "event:/Manosaba/audio/monsters/valstrax_super_impact";

    private static Texture2D? _softCircleTexture;

    private readonly RandomNumberGenerator _rng = new();

    private Sprite2D _dragon = null!;
    private Node2D _fxRoot = null!;

    private bool _configured;
    private Vector2 _startPosition;
    private Vector2 _impactPosition;

    private static Texture2D SoftCircleTexture => _softCircleTexture ??= CreateSoftCircleTexture(128, 2.35f);

    public override void _Ready()
    {
        _dragon = GetNode<Sprite2D>("Dragon");
        _fxRoot = GetNode<Node2D>("Fx");
        _dragon.Visible = false;
        _rng.Randomize();

        if (!_configured)
        {
            Vector2 viewport = GetViewportRect().Size;
            _startPosition = new Vector2(-viewport.X * 0.18f, -viewport.Y * 0.22f);
            _impactPosition = new Vector2(viewport.X * 0.78f, viewport.Y * 0.56f);
            _configured = true;
        }
    }

    public void Configure(Vector2 startPosition, Vector2 impactPosition)
    {
        _startPosition = startPosition;
        _impactPosition = impactPosition;
        _configured = true;
    }

    public async Task PlayAsync()
    {
        if (!IsNodeReady())
        {
            await ToSignal(this, Node.SignalName.Ready);
        }

        SfxCmd.Play(AmbushReadySfx, 1f);
        if (DiveStartDelay > 0f)
        {
            await ToSignal(GetTree().CreateTimer(DiveStartDelay), SceneTreeTimer.SignalName.Timeout);
        }

        _dragon.Visible = true;
        _dragon.GlobalPosition = _startPosition;
        _dragon.Scale = Vector2.One * 0.32f;
        _dragon.Modulate = new Color(1f, 1f, 1f, 0.96f);

        Vector2 direction = _impactPosition - _startPosition;
        if (direction.LengthSquared() < 0.001f)
        {
            direction = Vector2.Right;
        }

        _dragon.Rotation = direction.Angle();

        Tween diveTween = CreateTween().SetParallel(true);
        diveTween.TweenProperty(_dragon, "global_position", _impactPosition, DiveDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.In);
        diveTween.TweenProperty(_dragon, "scale", Vector2.One * 0.26f, DiveDuration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);
        diveTween.TweenProperty(_dragon, "modulate:a", 0.78f, DiveDuration * 0.9f);

        await ToSignal(diveTween, Tween.SignalName.Finished);

        _dragon.Visible = false;
        EmitSignal(SignalName.ImpactStarted);
        SfxCmd.Play(SuperImpactSfx, 1f);
        SpawnImpactFx();

        await ToSignal(GetTree().CreateTimer(CleanupDelay), SceneTreeTimer.SignalName.Timeout);
        QueueFree();
    }

    private void SpawnImpactFx()
    {
        SpawnFlashLayer(_impactPosition, 0.26f, 4.6f, new Color(1f, 0.96f, 0.96f, 1f));
        SpawnFlashLayer(_impactPosition, 0.42f, 8.4f, new Color(1f, 0.2f, 0.2f, 1f));
        SpawnFlashLayer(_impactPosition, 0.56f, 10.8f, new Color(1f, 0.06f, 0.06f, 0.88f));
        SpawnFlashLayer(_impactPosition, 0.34f, 5.2f, new Color(1f, 0.72f, 0.72f, 0.94f));

        for (int i = 0; i < 54; i++)
        {
            Vector2 dir = Vector2.FromAngle(_rng.RandfRange(0f, Mathf.Tau));
            float travel = _rng.RandfRange(180f, 620f);
            float startOffset = _rng.RandfRange(0f, 64f);
            float life = _rng.RandfRange(0.34f, 0.66f);

            Sprite2D spark = new()
            {
                Texture = SoftCircleTexture,
                GlobalPosition = _impactPosition + dir * startOffset,
                Scale = Vector2.One * _rng.RandfRange(0.18f, 0.38f),
                Modulate = new Color(1f, _rng.RandfRange(0.06f, 0.24f), 0.12f, _rng.RandfRange(0.82f, 1f)),
                ZIndex = 50
            };
            _fxRoot.AddChild(spark);

            Tween tween = CreateTween().SetParallel(true);
            tween.TweenProperty(spark, "global_position", spark.GlobalPosition + dir * travel, life)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(spark, "scale", Vector2.One * _rng.RandfRange(0.08f, 0.18f), life);
            tween.TweenProperty(spark, "modulate:a", 0f, life);
            _ = QueueFreeAfterAsync(spark, life + 0.02f);
        }

        for (int i = 0; i < 44; i++)
        {
            Vector2 dir = Vector2.FromAngle(_rng.RandfRange(0f, Mathf.Tau));
            float travel = _rng.RandfRange(220f, 560f);
            float life = _rng.RandfRange(1.8f, 2.7f);

            Sprite2D smoke = new()
            {
                Texture = SoftCircleTexture,
                GlobalPosition = _impactPosition + dir * _rng.RandfRange(0f, 62f),
                Scale = Vector2.One * _rng.RandfRange(0.46f, 0.78f),
                Modulate = new Color(_rng.RandfRange(0.4f, 0.56f), _rng.RandfRange(0.1f, 0.18f), _rng.RandfRange(0.1f, 0.18f), _rng.RandfRange(0.62f, 0.9f)),
                ZIndex = 45
            };
            _fxRoot.AddChild(smoke);

            Tween tween = CreateTween().SetParallel(true);
            tween.TweenProperty(smoke, "global_position", smoke.GlobalPosition + dir * travel, life)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(smoke, "scale", smoke.Scale * _rng.RandfRange(4.4f, 6.8f), life)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(smoke, "modulate:a", 0f, life)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.In);
            _ = QueueFreeAfterAsync(smoke, life + 0.02f);
        }
    }

    private void SpawnFlashLayer(Vector2 center, float duration, float scaleMultiplier, Color color)
    {
        Sprite2D flash = new()
        {
            Texture = SoftCircleTexture,
            GlobalPosition = center,
            Scale = Vector2.One * 0.22f,
            Modulate = color,
            ZIndex = 60
        };
        _fxRoot.AddChild(flash);

        Tween tween = CreateTween().SetParallel(true);
        tween.TweenProperty(flash, "scale", Vector2.One * scaleMultiplier, duration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
        tween.TweenProperty(flash, "modulate:a", 0f, duration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
        _ = QueueFreeAfterAsync(flash, duration + 0.02f);
    }

    private async Task QueueFreeAfterAsync(Node node, float delay)
    {
        await ToSignal(GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);
        if (GodotObject.IsInstanceValid(node))
        {
            node.QueueFree();
        }
    }

    private static Texture2D CreateSoftCircleTexture(int size, float falloffPower)
    {
        Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        Vector2 center = new((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float normalized = center.DistanceTo(new Vector2(x, y)) / radius;
                float alpha = Mathf.Clamp(1f - normalized, 0f, 1f);
                if (alpha > 0f)
                {
                    alpha = Mathf.Pow(alpha, falloffPower);
                }

                image.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        return ImageTexture.CreateFromImage(image);
    }
}

using Godot;
using Manosaba.Characters.ShitoAlisa.Visuals;

namespace Manosaba.Characters.ShitoAlisa.Vfx;

/// <summary>Screen-filling elliptical ring: 20 flame sprites light up in order, then fade; root must <see cref="QueueFree"/> for <c>ManosabaVfxCmd.PlaySceneAtCombatCenterAndWait</c>.</summary>
public partial class FinalCountdownRingFlamesVfx : Node2D
{
    private const int FlameCount = 20;
    private const float StepDelay = 0.07f;
    private const float LightDuration = 0.11f;
    private const float HoldAfterLit = 0.42f;
    private const float FadeDuration = 0.48f;

    public override async void _Ready()
    {
        Texture2D? texture = GD.Load<Texture2D>(FireballOrbitTexturePaths.OrbitSprite);
        if (texture == null)
        {
            QueueFree();
            return;
        }

        ZIndex = 200;

        Vector2 viewportSize = GetViewportRect().Size;
        // Ellipse so the ring reaches near all four edges on rectangular viewports (inscribed circle only fills the short axis).
        const float edgeMargin = 0.88f;
        float halfW = viewportSize.X * 0.5f * edgeMargin;
        float halfH = viewportSize.Y * 0.5f * edgeMargin;
        float refRadius = Mathf.Min(halfW, halfH);
        Vector2 textureSize = texture.GetSize();
        float texMax = Mathf.Max(textureSize.X, textureSize.Y);
        float baseScale = texMax > 0f ? (refRadius * 0.34f) / texMax : 0.65f;
        baseScale = Mathf.Clamp(baseScale, 0.4f, 1.65f);

        var flames = new Sprite2D[FlameCount];
        for (int i = 0; i < FlameCount; i++)
        {
            float angle = i * Mathf.Tau / FlameCount - Mathf.Pi / 2f;
            Vector2 pos = new Vector2(Mathf.Cos(angle) * halfW, Mathf.Sin(angle) * halfH);
            var sprite = new Sprite2D
            {
                Texture = texture,
                Position = pos,
                Scale = Vector2.One * (baseScale * 0.58f),
                Modulate = new Color(1f, 1f, 1f, 0f),
                ZIndex = i
            };
            AddChild(sprite);
            flames[i] = sprite;
        }

        for (int i = 0; i < FlameCount; i++)
        {
            Sprite2D sprite = flames[i];
            Tween lightTween = CreateTween().SetParallel(true);
            lightTween.TweenProperty(sprite, "modulate:a", 1f, LightDuration)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);
            lightTween.TweenProperty(sprite, "scale", Vector2.One * (baseScale * 1.28f), LightDuration)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
            await ToSignal(lightTween, Tween.SignalName.Finished);
            if (i < FlameCount - 1)
            {
                await ToSignal(GetTree().CreateTimer(StepDelay), SceneTreeTimer.SignalName.Timeout);
            }
        }

        await ToSignal(GetTree().CreateTimer(HoldAfterLit), SceneTreeTimer.SignalName.Timeout);

        Tween fadeTween = CreateTween().SetParallel(true);
        foreach (Sprite2D sprite in flames)
        {
            fadeTween.TweenProperty(sprite, "modulate:a", 0f, FadeDuration);
        }

        await ToSignal(fadeTween, Tween.SignalName.Finished);
        QueueFree();
    }
}

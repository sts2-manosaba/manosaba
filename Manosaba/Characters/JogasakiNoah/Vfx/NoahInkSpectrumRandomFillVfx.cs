using Godot;

namespace Manosaba.Characters.JogasakiNoah.Vfx;

public partial class NoahInkSpectrumRandomFillVfx : Node2D
{
	private const int SplashCount = 80;
	private const float TextureWidth = 3000f;
	private const float TextureHeight = 2000f;

	private static readonly Color[] Palette =
	[
		new Color(1f, 0f, 0f, 1f),       // Red
		new Color(1f, 0.5f, 0f, 1f),     // Orange
		new Color(1f, 0.95f, 0f, 1f),    // Yellow
		new Color(0.1f, 0.9f, 0.25f, 1f),// Green
		new Color(0.1f, 0.35f, 1f, 1f),  // Blue
		new Color(0.62f, 0.2f, 1f, 1f),  // Purple
		new Color(0.03f, 0.03f, 0.03f, 1f), // Black
		new Color(1f, 1f, 1f, 1f)        // White
	];

	private static readonly Shader InkMaskShader = new()
	{
		Code = @"shader_type canvas_item;
uniform vec4 tint_color : source_color = vec4(1.0);

void fragment()
{
    float alpha_mask = texture(TEXTURE, UV).a;
    float alpha = alpha_mask * tint_color.a * COLOR.a;
    COLOR = vec4(tint_color.rgb, alpha);
}"
	};

	public override async void _Ready()
	{
		Texture2D? inkTexture = GD.Load<Texture2D>("res://Manosaba/images/characters/jogasaki_noah/vfx/ink.png");
		if (inkTexture == null)
		{
			QueueFree();
			return;
		}

		Vector2 viewportSize = GetViewportRect().Size;
		RandomNumberGenerator rng = new();
		rng.Randomize();

		float coverScale = Mathf.Max(viewportSize.X / TextureWidth, viewportSize.Y / TextureHeight);
		float startScale = coverScale * 1.25f;
		float endScale = coverScale * 0.82f;

		float appearDelay = 0.16f;

		for (int i = 0; i < SplashCount; i++)
		{
			Color splashColor = Palette[i % Palette.Length];
			Vector2 target = new(
				rng.RandfRange(-viewportSize.X * 0.45f, viewportSize.X * 0.45f),
				rng.RandfRange(-viewportSize.Y * 0.45f, viewportSize.Y * 0.45f)
			);

			Sprite2D splash = new()
			{
				Texture = inkTexture,
				Position = target,
				Scale = Vector2.One * startScale,
				Rotation = rng.RandfRange(0f, Mathf.Tau),
				Modulate = new Color(1f, 1f, 1f, 0f),
				ZIndex = i
			};

			ShaderMaterial material = new()
			{
				Shader = InkMaskShader
			};
			material.SetShaderParameter("tint_color", splashColor);
			splash.Material = material;

			AddChild(splash);

			Tween enterTween = CreateTween().SetParallel(true);
			enterTween.TweenProperty(splash, "scale", Vector2.One * endScale, 0.28f)
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.Out);
			enterTween.TweenProperty(splash, "modulate:a", 0.88f, 0.08f);

			await ToSignal(GetTree().CreateTimer(appearDelay), SceneTreeTimer.SignalName.Timeout);
			appearDelay = Mathf.Max(0.025f, appearDelay * 0.88f);
		}

		await ToSignal(GetTree().CreateTimer(1.25f), SceneTreeTimer.SignalName.Timeout);

		Tween fadeTween = CreateTween().SetParallel(true);
		foreach (Node child in GetChildren())
		{
			if (child is Sprite2D splash)
			{
				fadeTween.TweenProperty(splash, "modulate:a", 0.0f, 1f);
			}
		}

		await ToSignal(fadeTween, Tween.SignalName.Finished);
		QueueFree();
	}
}

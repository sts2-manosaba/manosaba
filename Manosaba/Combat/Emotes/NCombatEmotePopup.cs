using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Random;

namespace Manosaba.Combat.Emotes;

public partial class NCombatEmotePopup : Control
{
    public const float DisplaySize = 256f;

    private TextureRect? _image;

    public static NCombatEmotePopup Create(Texture2D texture)
    {
        NCombatEmotePopup popup = new();
        popup.CustomMinimumSize = new Vector2(DisplaySize, DisplaySize);
        popup.Size = popup.CustomMinimumSize;
        popup.MouseFilter = MouseFilterEnum.Ignore;
        popup.PivotOffset = new Vector2(DisplaySize * 0.5f, DisplaySize * 0.5f);

        TextureRect image = new()
        {
            AnchorsPreset = (int)LayoutPreset.FullRect,
            AnchorRight = 1f,
            AnchorBottom = 1f,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both,
            Texture = texture,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        popup.AddChild(image);
        popup._image = image;
        return popup;
    }

    public void PlayAt(Vector2 globalCenter)
    {
        GlobalPosition = globalCenter - new Vector2(DisplaySize * 0.5f, DisplaySize * 0.5f);
        TaskHelper.RunSafely(PlayAnimAsync());
    }

    private async Task PlayAnimAsync()
    {
        Color modulate = Modulate;
        modulate.A = 0f;
        Modulate = modulate;

        float riseDistance = Rng.Chaotic.NextFloat(50f, 70f);
        float deg = Rng.Chaotic.NextFloat(-20f, 20f);
        Vector2 targetPosition = Position + Vector2.Up.Rotated(Mathf.DegToRad(deg)) * riseDistance;

        Tween tween = CreateTween();
        tween.SetParallel();
        tween.TweenProperty(this, "position", targetPosition, 0.3)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Expo);
        tween.TweenProperty(this, "modulate:a", 1f, 0.3)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Expo);
        tween.SetParallel(false);
        tween.TweenProperty(this, "modulate:a", 0f, 0.2)
            .SetDelay(0.9)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Expo);

        await ToSignal(tween, Tween.SignalName.Finished);
        this.QueueFreeSafely();
    }
}

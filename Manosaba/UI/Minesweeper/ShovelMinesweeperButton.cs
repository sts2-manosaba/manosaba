using Godot;
using MegaCrit.Sts2.Core.Helpers;

namespace Manosaba.UI.Minesweeper;

public sealed partial class ShovelMinesweeperButton : Control
{
    private bool _hovered;
    private bool _pressed;

    public Action? Clicked;

    public override void _Ready()
    {
        if (CustomMinimumSize == Vector2.Zero)
        {
            CustomMinimumSize = new Vector2(56f, 56f);
        }

        MouseFilter = MouseFilterEnum.Stop;
        Connect(SignalName.MouseEntered, Callable.From(() =>
        {
            _hovered = true;
            QueueRedraw();
        }));
        Connect(SignalName.MouseExited, Callable.From(() =>
        {
            _hovered = false;
            _pressed = false;
            QueueRedraw();
        }));
    }

    public override void _GuiInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventMouseButton mouse || mouse.ButtonIndex != MouseButton.Left)
        {
            return;
        }

        _pressed = mouse.Pressed;
        QueueRedraw();
        AcceptEvent();

        if (!mouse.Pressed)
        {
            Clicked?.Invoke();
        }
    }

    public override void _Draw()
    {
        Vector2 center = Size * 0.5f;
        float radius = MathF.Min(Size.X, Size.Y) * 0.47f;
        Color bg = _pressed
            ? new Color("6E5B37E8")
            : _hovered
                ? new Color("574929E8")
                : new Color("332C21D8");

        DrawCircle(center, radius, bg);
        DrawArc(center, radius - 1f, 0f, MathF.Tau, 72, StsColors.gold, 2.5f);

        Vector2 handleTop = center + new Vector2(7f, -18f);
        Vector2 handleBottom = center + new Vector2(-9f, 8f);
        DrawLine(handleTop, handleBottom, StsColors.cream, 6f, true);
        DrawLine(handleTop, handleBottom, new Color("7A5129"), 3f, true);

        DrawLine(center + new Vector2(13f, -22f), center + new Vector2(4f, -14f), StsColors.gold, 4f, true);
        DrawLine(center + new Vector2(10f, -24f), center + new Vector2(17f, -17f), StsColors.gold, 4f, true);

        Vector2 scoopCenter = center + new Vector2(-13f, 16f);
        DrawCircle(scoopCenter, 10f, new Color("A8A8A8"));
        DrawCircle(scoopCenter + new Vector2(-2f, -2f), 7f, new Color("D3D3D3"));
        DrawLine(center + new Vector2(-18f, 23f), center + new Vector2(-5f, 10f), new Color("737373"), 3f, true);
    }
}

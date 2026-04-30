using Godot;

namespace Manosaba.UI.Minesweeper;

public sealed partial class MinesweeperCellButton : Button
{
    public int Row { get; init; }
    public int Column { get; init; }
    public Action<MinesweeperCellButton>? RightClicked;

    public override void _GuiInput(InputEvent inputEvent)
    {
        base._GuiInput(inputEvent);

        if (inputEvent is InputEventMouseButton { ButtonIndex: MouseButton.Right, Pressed: false })
        {
            RightClicked?.Invoke(this);
            AcceptEvent();
        }
    }
}

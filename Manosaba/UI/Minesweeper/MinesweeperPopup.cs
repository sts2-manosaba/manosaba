using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace Manosaba.UI.Minesweeper;

public sealed partial class MinesweeperPopup : Control, IScreenContext
{
    private const int BoardSize = 9;
    private const int MineCount = 10;
    private const string DefaultStatusText = "左鍵翻開，右鍵插旗。";
    private const string LostStatusText = "踩到地雷。";
    private const string WonStatusText = "清除完成。";

    private static readonly Color[] NumberColors =
    [
        StsColors.cream,
        new Color("6EC6FF"),
        new Color("83E377"),
        new Color("FF7777"),
        new Color("B8A1FF"),
        new Color("FFB15E"),
        new Color("62E5D4"),
        StsColors.cream,
        new Color("B8B8B8"),
    ];

    private sealed record SavedGameState(
        bool[,] Mines,
        bool[,] Revealed,
        bool[,] Flagged,
        bool MinesPlaced,
        bool GameOver,
        int RevealedCount,
        string StatusText,
        Color StatusColor);

    private static SavedGameState? SavedState;

    private readonly bool[,] _mines = new bool[BoardSize, BoardSize];
    private readonly bool[,] _revealed = new bool[BoardSize, BoardSize];
    private readonly bool[,] _flagged = new bool[BoardSize, BoardSize];
    private readonly MinesweeperCellButton[,] _cells = new MinesweeperCellButton[BoardSize, BoardSize];

    private Label? _statusLabel;
    private Label? _mineLabel;
    private Button? _resetButton;
    private bool _minesPlaced;
    private bool _gameOver;
    private int _revealedCount;
    private string _statusText = DefaultStatusText;
    private Color _statusColor = StsColors.cream;

    public Control? DefaultFocusedControl => _resetButton;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        SetAnchorsPreset(LayoutPreset.FullRect);
        OffsetLeft = 0f;
        OffsetRight = 0f;
        OffsetTop = 0f;
        OffsetBottom = 0f;

        AddChild(BuildPanel());
        if (SavedState != null)
        {
            RestoreGameState(SavedState);
        }
        else
        {
            ResetGame();
        }
    }

    public override void _ExitTree()
    {
        SaveGameState();
    }

    private Control BuildPanel()
    {
        PanelContainer panel = new()
        {
            Name = "MinesweeperPanel",
            MouseFilter = MouseFilterEnum.Stop,
        };
        panel.AnchorLeft = 0.5f;
        panel.AnchorRight = 0.5f;
        panel.AnchorTop = 0.5f;
        panel.AnchorBottom = 0.5f;
        panel.OffsetLeft = -284f;
        panel.OffsetRight = 284f;
        panel.OffsetTop = -310f;
        panel.OffsetBottom = 310f;
        panel.AddThemeStyleboxOverride("panel", MakeStyle(new Color("201B16F2"), new Color("EFC851CC"), 2, 8));

        MarginContainer margin = new();
        margin.AddThemeConstantOverride("margin_left", 18);
        margin.AddThemeConstantOverride("margin_right", 18);
        margin.AddThemeConstantOverride("margin_top", 16);
        margin.AddThemeConstantOverride("margin_bottom", 16);
        panel.AddChild(margin);

        VBoxContainer root = new();
        root.AddThemeConstantOverride("separation", 12);
        margin.AddChild(root);

        HBoxContainer header = new();
        header.AddThemeConstantOverride("separation", 10);
        root.AddChild(header);

        Label title = new()
        {
            Text = "踩地雷",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            VerticalAlignment = VerticalAlignment.Center,
        };
        title.AddThemeFontSizeOverride("font_size", 28);
        title.AddThemeColorOverride("font_color", StsColors.gold);
        header.AddChild(title);

        _mineLabel = new()
        {
            Text = "10",
            CustomMinimumSize = new Vector2(74f, 0f),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _mineLabel.AddThemeFontSizeOverride("font_size", 22);
        _mineLabel.AddThemeColorOverride("font_color", StsColors.cream);
        header.AddChild(_mineLabel);

        _resetButton = MakeTextButton("新局", new Vector2(84f, 42f));
        _resetButton.Connect(BaseButton.SignalName.Pressed, Callable.From(ResetGame));
        header.AddChild(_resetButton);

        Button closeButton = MakeTextButton("關閉", new Vector2(84f, 42f));
        closeButton.Connect(BaseButton.SignalName.Pressed, Callable.From(Close));
        header.AddChild(closeButton);

        _statusLabel = new()
        {
            Text = "左鍵翻開，右鍵插旗。",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _statusLabel.AddThemeFontSizeOverride("font_size", 18);
        _statusLabel.AddThemeColorOverride("font_color", StsColors.cream);
        root.AddChild(_statusLabel);

        GridContainer grid = new()
        {
            Columns = BoardSize,
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
        };
        grid.AddThemeConstantOverride("h_separation", 4);
        grid.AddThemeConstantOverride("v_separation", 4);
        root.AddChild(grid);

        for (int r = 0; r < BoardSize; r++)
        {
            for (int c = 0; c < BoardSize; c++)
            {
                MinesweeperCellButton cell = new()
                {
                    Row = r,
                    Column = c,
                    CustomMinimumSize = new Vector2(52f, 52f),
                    FocusMode = FocusModeEnum.None,
                };
                cell.Connect(BaseButton.SignalName.Pressed, Callable.From(() => RevealCell(cell.Row, cell.Column)));
                cell.RightClicked = OnCellRightClicked;
                ApplyHiddenStyle(cell);
                _cells[r, c] = cell;
                grid.AddChild(cell);
            }
        }

        return panel;
    }

    private void ResetGame()
    {
        Array.Clear(_mines);
        Array.Clear(_revealed);
        Array.Clear(_flagged);
        _minesPlaced = false;
        _gameOver = false;
        _revealedCount = 0;
        SetStatus(DefaultStatusText, StsColors.cream);

        RenderBoard();
        SaveGameState();
    }

    private void RenderBoard()
    {
        if (_statusLabel != null)
        {
            _statusLabel.Text = _statusText;
            _statusLabel.AddThemeColorOverride("font_color", _statusColor);
        }

        for (int r = 0; r < BoardSize; r++)
        {
            for (int c = 0; c < BoardSize; c++)
            {
                MinesweeperCellButton? cell = _cells[r, c];
                if (cell == null)
                {
                    continue;
                }

                cell.Text = "";
                cell.Disabled = false;
                ApplyHiddenStyle(cell);

                if (_revealed[r, c])
                {
                    int adjacent = CountAdjacentMines(r, c);
                    cell.Disabled = true;
                    cell.Text = adjacent == 0 ? "" : adjacent.ToString();
                    ApplyRevealedStyle(cell, adjacent);
                    continue;
                }

                if (_gameOver)
                {
                    cell.Disabled = true;
                    if (_mines[r, c])
                    {
                        cell.Text = _statusText == WonStatusText ? "F" : "*";
                        if (_statusText == WonStatusText)
                        {
                            ApplyFlaggedWinStyle(cell);
                        }
                        else
                        {
                            ApplyMineStyle(cell);
                        }
                    }
                    else if (_flagged[r, c])
                    {
                        cell.Text = "X";
                        ApplyWrongFlagStyle(cell);
                    }

                    continue;
                }

                if (_flagged[r, c])
                {
                    cell.Text = "F";
                    cell.AddThemeColorOverride("font_color", StsColors.gold);
                    cell.AddThemeColorOverride("font_disabled_color", StsColors.gold);
                }
            }
        }

        RefreshMineLabel();
    }

    private void RevealCell(int row, int column)
    {
        if (_gameOver || !IsInside(row, column) || _flagged[row, column] || _revealed[row, column])
        {
            return;
        }

        if (!_minesPlaced)
        {
            PlaceMines(row, column);
        }

        if (_mines[row, column])
        {
            _gameOver = true;
            RevealAllMines();
            SetStatus(LostStatusText, StsColors.red);
            SaveGameState();
            return;
        }

        RevealSafeRegion(row, column);
        if (_revealedCount >= BoardSize * BoardSize - MineCount)
        {
            _gameOver = true;
            FlagAllMines();
            SetStatus(WonStatusText, StsColors.gold);
        }

        SaveGameState();
    }

    private void OnCellRightClicked(MinesweeperCellButton cell)
    {
        int row = cell.Row;
        int column = cell.Column;
        if (_gameOver || _revealed[row, column])
        {
            return;
        }

        _flagged[row, column] = !_flagged[row, column];
        cell.Text = _flagged[row, column] ? "F" : "";
        cell.AddThemeColorOverride("font_color", _flagged[row, column] ? StsColors.gold : StsColors.cream);
        cell.AddThemeColorOverride("font_disabled_color", _flagged[row, column] ? StsColors.gold : StsColors.cream);
        RefreshMineLabel();
        SaveGameState();
    }

    private void PlaceMines(int safeRow, int safeColumn)
    {
        _minesPlaced = true;
        int placed = 0;
        while (placed < MineCount)
        {
            int row = Random.Shared.Next(BoardSize);
            int column = Random.Shared.Next(BoardSize);
            if (_mines[row, column] || IsInFirstClickSafeZone(row, column, safeRow, safeColumn))
            {
                continue;
            }

            _mines[row, column] = true;
            placed++;
        }
    }

    private static bool IsInFirstClickSafeZone(int row, int column, int safeRow, int safeColumn)
    {
        return Math.Abs(row - safeRow) <= 1 && Math.Abs(column - safeColumn) <= 1;
    }

    private void RevealSafeRegion(int startRow, int startColumn)
    {
        Queue<(int Row, int Column)> queue = [];
        queue.Enqueue((startRow, startColumn));

        while (queue.Count > 0)
        {
            (int row, int column) = queue.Dequeue();
            if (!IsInside(row, column) || _revealed[row, column] || _flagged[row, column])
            {
                continue;
            }

            _revealed[row, column] = true;
            _revealedCount++;
            int adjacent = CountAdjacentMines(row, column);
            MinesweeperCellButton cell = _cells[row, column];
            cell.Disabled = true;
            cell.Text = adjacent == 0 ? "" : adjacent.ToString();
            ApplyRevealedStyle(cell, adjacent);

            if (adjacent != 0)
            {
                continue;
            }

            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr != 0 || dc != 0)
                    {
                        queue.Enqueue((row + dr, column + dc));
                    }
                }
            }
        }
    }

    private int CountAdjacentMines(int row, int column)
    {
        int count = 0;
        for (int dr = -1; dr <= 1; dr++)
        {
            for (int dc = -1; dc <= 1; dc++)
            {
                if ((dr != 0 || dc != 0) && IsInside(row + dr, column + dc) && _mines[row + dr, column + dc])
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void RevealAllMines()
    {
        for (int r = 0; r < BoardSize; r++)
        {
            for (int c = 0; c < BoardSize; c++)
            {
                MinesweeperCellButton cell = _cells[r, c];
                cell.Disabled = true;
                if (_mines[r, c])
                {
                    cell.Text = "*";
                    ApplyMineStyle(cell);
                }
                else if (_flagged[r, c])
                {
                    cell.Text = "X";
                    ApplyWrongFlagStyle(cell);
                }
            }
        }
    }

    private void FlagAllMines()
    {
        for (int r = 0; r < BoardSize; r++)
        {
            for (int c = 0; c < BoardSize; c++)
            {
                MinesweeperCellButton cell = _cells[r, c];
                cell.Disabled = true;
                if (_mines[r, c])
                {
                    _flagged[r, c] = true;
                    cell.Text = "F";
                    ApplyFlaggedWinStyle(cell);
                }
            }
        }

        RefreshMineLabel();
    }

    private void RefreshMineLabel()
    {
        if (_mineLabel == null)
        {
            return;
        }

        int flags = 0;
        for (int r = 0; r < BoardSize; r++)
        {
            for (int c = 0; c < BoardSize; c++)
            {
                if (_flagged[r, c])
                {
                    flags++;
                }
            }
        }

        _mineLabel.Text = $"剩餘 {Math.Max(0, MineCount - flags)}";
    }

    private void SetStatus(string text, Color color)
    {
        _statusText = text;
        _statusColor = color;

        if (_statusLabel == null)
        {
            return;
        }

        _statusLabel.Text = text;
        _statusLabel.AddThemeColorOverride("font_color", color);
    }

    private void SaveGameState()
    {
        SavedState = new SavedGameState(
            (bool[,])_mines.Clone(),
            (bool[,])_revealed.Clone(),
            (bool[,])_flagged.Clone(),
            _minesPlaced,
            _gameOver,
            _revealedCount,
            _statusText,
            _statusColor);
    }

    private void RestoreGameState(SavedGameState state)
    {
        Array.Copy(state.Mines, _mines, _mines.Length);
        Array.Copy(state.Revealed, _revealed, _revealed.Length);
        Array.Copy(state.Flagged, _flagged, _flagged.Length);
        _minesPlaced = state.MinesPlaced;
        _gameOver = state.GameOver;
        _revealedCount = state.RevealedCount;
        _statusText = state.StatusText;
        _statusColor = state.StatusColor;
        RenderBoard();
    }

    private void Close()
    {
        if (NModalContainer.Instance != null)
        {
            NModalContainer.Instance.Clear();
            return;
        }

        QueueFree();
    }

    private static bool IsInside(int row, int column)
    {
        return row >= 0 && row < BoardSize && column >= 0 && column < BoardSize;
    }

    private static Button MakeTextButton(string text, Vector2 minSize)
    {
        Button button = new()
        {
            Text = text,
            CustomMinimumSize = minSize,
            FocusMode = FocusModeEnum.None,
        };
        button.AddThemeStyleboxOverride("normal", MakeStyle(new Color("3B3327"), new Color("8C7345"), 1, 6));
        button.AddThemeStyleboxOverride("hover", MakeStyle(new Color("51442D"), StsColors.gold, 1, 6));
        button.AddThemeStyleboxOverride("pressed", MakeStyle(new Color("6E5B37"), StsColors.gold, 1, 6));
        button.AddThemeColorOverride("font_color", StsColors.cream);
        button.AddThemeColorOverride("font_hover_color", StsColors.cream);
        button.AddThemeColorOverride("font_pressed_color", StsColors.gold);
        button.AddThemeFontSizeOverride("font_size", 18);
        return button;
    }

    private static void ApplyHiddenStyle(Button button)
    {
        button.AddThemeStyleboxOverride("normal", MakeStyle(new Color("393126"), new Color("796647"), 1, 4));
        button.AddThemeStyleboxOverride("hover", MakeStyle(new Color("4A3D2A"), StsColors.gold, 1, 4));
        button.AddThemeStyleboxOverride("pressed", MakeStyle(new Color("5D4D33"), StsColors.gold, 1, 4));
        button.AddThemeStyleboxOverride("disabled", MakeStyle(new Color("393126"), new Color("796647"), 1, 4));
        button.AddThemeColorOverride("font_color", StsColors.cream);
        button.AddThemeColorOverride("font_disabled_color", StsColors.cream);
        button.AddThemeFontSizeOverride("font_size", 22);
    }

    private static void ApplyRevealedStyle(Button button, int adjacent)
    {
        button.AddThemeStyleboxOverride("disabled", MakeStyle(new Color("7A705F"), new Color("9D8E70"), 1, 4));
        Color color = NumberColors[Math.Clamp(adjacent, 0, NumberColors.Length - 1)];
        button.AddThemeColorOverride("font_disabled_color", color);
        button.AddThemeFontSizeOverride("font_size", 24);
    }

    private static void ApplyMineStyle(Button button)
    {
        button.AddThemeStyleboxOverride("disabled", MakeStyle(new Color("6B2020"), StsColors.red, 1, 4));
        button.AddThemeColorOverride("font_disabled_color", StsColors.cream);
        button.AddThemeFontSizeOverride("font_size", 26);
    }

    private static void ApplyWrongFlagStyle(Button button)
    {
        button.AddThemeStyleboxOverride("disabled", MakeStyle(new Color("4D3030"), StsColors.red, 1, 4));
        button.AddThemeColorOverride("font_disabled_color", StsColors.red);
    }

    private static void ApplyFlaggedWinStyle(Button button)
    {
        button.AddThemeStyleboxOverride("disabled", MakeStyle(new Color("3D4A2D"), StsColors.gold, 1, 4));
        button.AddThemeColorOverride("font_disabled_color", StsColors.gold);
    }

    private static StyleBoxFlat MakeStyle(Color bg, Color border, int borderWidth, int cornerRadius)
    {
        StyleBoxFlat style = new()
        {
            BgColor = bg,
            BorderColor = border,
        };
        style.SetBorderWidthAll(borderWidth);
        style.SetCornerRadiusAll(cornerRadius);
        return style;
    }
}

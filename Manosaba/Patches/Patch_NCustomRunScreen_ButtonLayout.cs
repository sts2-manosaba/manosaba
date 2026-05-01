using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using System.Runtime.CompilerServices;

namespace Manosaba.Patches;

/// <summary>
/// Custom run lobby: fewer characters per row than standard select; paginate with 5 per page and keep arrows inside the left strip.
/// </summary>
[HarmonyPatch]
public static class Patch_NCustomRunScreen_ButtonLayout
{
    private static readonly AccessTools.FieldRef<NCustomRunScreen, Control> CharButtonContainerRef =
        AccessTools.FieldRefAccess<NCustomRunScreen, Control>("_charButtonContainer");

    private static readonly AccessTools.FieldRef<NCustomRunScreen, NCharacterSelectButton?> SelectedButtonRef =
        AccessTools.FieldRefAccess<NCustomRunScreen, NCharacterSelectButton?>("_selectedButton");

    private static readonly AccessTools.FieldRef<NCustomRunScreen, LineEdit> SeedInputRef =
        AccessTools.FieldRefAccess<NCustomRunScreen, LineEdit>("_seedInput");

    private const int CharactersPerPage = 5;
    private const float ArrowSize = 80f;
    private const float ArrowGap = 18f;
    private const float CustomArrowRowYOffset = -12f;
    private const int ArrowZIndex = 220;

    private const string LeftArrowName = "PageLeftArrowCustomRun";
    private const string RightArrowName = "PageRightArrowCustomRun";

    private sealed class PaginationState
    {
        public int CurrentPage;
        public readonly Dictionary<NCharacterSelectButton, bool> BaseVisible = new();
        public NGoldArrowButton? LeftArrow;
        public NGoldArrowButton? RightArrow;
    }

    private static readonly ConditionalWeakTable<NCustomRunScreen, PaginationState> States = new();

    [HarmonyPatch(typeof(NCustomRunScreen), nameof(NCustomRunScreen._Ready))]
    [HarmonyPostfix]
    private static void Postfix_Ready(NCustomRunScreen __instance)
    {
        PaginationState state = GetState(__instance);
        state.CurrentPage = 0;
        EnsurePagerControls(__instance);
        SnapshotBaseVisibility(__instance);
        ApplyPagination(__instance, selectionMayBeInvalid: false);
    }

    [HarmonyPatch(typeof(NCustomRunScreen), nameof(NCustomRunScreen.OnSubmenuOpened))]
    [HarmonyPostfix]
    private static void Postfix_OnSubmenuOpened(NCustomRunScreen __instance)
    {
        PaginationState state = GetState(__instance);
        state.CurrentPage = 0;
        SnapshotBaseVisibility(__instance);
        ApplyPagination(__instance, selectionMayBeInvalid: true);
    }

    private static PaginationState GetState(NCustomRunScreen screen) => States.GetOrCreateValue(screen);

    private static void SnapshotBaseVisibility(NCustomRunScreen screen)
    {
        PaginationState state = GetState(screen);
        Control? container = CharButtonContainerRef(screen);
        if (container == null)
        {
            return;
        }

        foreach (NCharacterSelectButton button in state.BaseVisible.Keys.ToArray())
        {
            if (!GodotObject.IsInstanceValid(button))
            {
                state.BaseVisible.Remove(button);
            }
        }

        foreach (NCharacterSelectButton button in container.GetChildren().OfType<NCharacterSelectButton>())
        {
            if (!state.BaseVisible.ContainsKey(button))
            {
                state.BaseVisible[button] = button.Visible;
            }
        }
    }

    private static void EnsurePagerControls(NCustomRunScreen screen)
    {
        Control? outerContainer = screen.GetNodeOrNull<Control>("LeftContainer/CharSelectButtons");
        if (outerContainer == null)
        {
            return;
        }

        PaginationState state = GetState(screen);
        if (state.LeftArrow == null || !GodotObject.IsInstanceValid(state.LeftArrow))
        {
            NGoldArrowButton left = CreateArrowButton(isLeft: true);
            left.Name = LeftArrowName;
            left.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(_ => TryChangePage(screen, -1)));
            outerContainer.AddChild(left);
            outerContainer.MoveChild(left, -1);
            state.LeftArrow = left;
        }

        if (state.RightArrow == null || !GodotObject.IsInstanceValid(state.RightArrow))
        {
            NGoldArrowButton right = CreateArrowButton(isLeft: false);
            right.Name = RightArrowName;
            right.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(_ => TryChangePage(screen, +1)));
            outerContainer.AddChild(right);
            outerContainer.MoveChild(right, -1);
            state.RightArrow = right;
        }
    }

    private static NGoldArrowButton CreateArrowButton(bool isLeft)
    {
        NGoldArrowButton button = new NGoldArrowButton
        {
            CustomMinimumSize = new Vector2(ArrowSize, ArrowSize),
            MouseFilter = Control.MouseFilterEnum.Stop,
            ZIndex = ArrowZIndex,
        };

        TextureRect icon = new TextureRect
        {
            Name = "TextureRect",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
        };
        icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        icon.Texture = ResourceLoader.Load<Texture2D>(
            isLeft
                ? "res://images/atlases/ui_atlas.sprites/settings_tiny_left_arrow.tres"
                : "res://images/atlases/ui_atlas.sprites/settings_tiny_right_arrow.tres");

        Shader shader = ResourceLoader.Load<Shader>("res://shaders/hsv.gdshader");
        ShaderMaterial material = new ShaderMaterial { Shader = shader };
        material.SetShaderParameter("h", 1f);
        material.SetShaderParameter("s", 1f);
        material.SetShaderParameter("v", 0.9f);
        icon.Material = material;

        button.AddChild(icon);
        return button;
    }

    private static void TryChangePage(NCustomRunScreen screen, int delta)
    {
        PaginationState state = GetState(screen);
        state.CurrentPage += delta;
        ApplyPagination(screen, selectionMayBeInvalid: true);
    }

    private static void ApplyPagination(NCustomRunScreen screen, bool selectionMayBeInvalid)
    {
        EnsurePagerControls(screen);
        SnapshotBaseVisibility(screen);

        Control? outerContainer = screen.GetNodeOrNull<Control>("LeftContainer/CharSelectButtons");
        Control? buttonContainer = CharButtonContainerRef(screen);
        if (outerContainer == null || buttonContainer == null)
        {
            return;
        }

        PaginationState state = GetState(screen);
        NCharacterSelectButton[] allButtons = buttonContainer.GetChildren().OfType<NCharacterSelectButton>().ToArray();
        if (allButtons.Length == 0)
        {
            return;
        }

        List<NCharacterSelectButton> baseVisibleButtons = allButtons
            .Where(b => state.BaseVisible.TryGetValue(b, out bool visible) && visible)
            .ToList();

        int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)baseVisibleButtons.Count / CharactersPerPage));
        state.CurrentPage = Mathf.Clamp(state.CurrentPage, 0, totalPages - 1);

        int pageStart = state.CurrentPage * CharactersPerPage;
        HashSet<NCharacterSelectButton> pageButtons = baseVisibleButtons
            .Skip(pageStart)
            .Take(CharactersPerPage)
            .ToHashSet();

        foreach (NCharacterSelectButton button in allButtons)
        {
            bool shouldShow = pageButtons.Contains(button);
            button.Visible = shouldShow;
            if (shouldShow)
            {
                if (!button.IsEnabled)
                {
                    button.Enable();
                }
            }
            else if (button.IsEnabled)
            {
                button.Disable();
            }
        }

        NCharacterSelectButton[] visibleButtons = allButtons.Where(b => b.Visible).ToArray();
        if (visibleButtons.Length == 0)
        {
            return;
        }

        if (selectionMayBeInvalid)
        {
            NCharacterSelectButton? selected = SelectedButtonRef(screen);
            if (selected == null || !selected.Visible)
            {
                visibleButtons[0].Select();
            }
        }

        Control? hbox = screen.GetNodeOrNull<Control>("LeftContainer/CharSelectButtons/ButtonContainer");
        float rowTop = hbox?.OffsetTop ?? -189f;
        float rowBottom = hbox?.OffsetBottom ?? -35f;
        float y = rowTop + (rowBottom - rowTop - ArrowSize) * 0.5f + CustomArrowRowYOffset;

        float buttonWidth = visibleButtons.Max(b => Mathf.Max(b.Size.X, b.CustomMinimumSize.X));
        if (buttonWidth <= 0f)
        {
            buttonWidth = 100f;
        }

        float separation = (hbox as BoxContainer)?.GetThemeConstant("separation") ?? 16f;
        float rowWidth = (visibleButtons.Length * buttonWidth) + (Mathf.Max(0, visibleButtons.Length - 1) * separation);
        float leftX = (-rowWidth * 0.5f) - ArrowGap - ArrowSize;
        float rightX = (rowWidth * 0.5f) + ArrowGap;

        if (state.LeftArrow != null && GodotObject.IsInstanceValid(state.LeftArrow))
        {
            state.LeftArrow.Visible = totalPages > 1;
            state.LeftArrow.SetEnabled(totalPages > 1 && state.CurrentPage > 0);
            state.LeftArrow.Position = new Vector2(leftX, y);
        }

        if (state.RightArrow != null && GodotObject.IsInstanceValid(state.RightArrow))
        {
            state.RightArrow.Visible = totalPages > 1;
            state.RightArrow.SetEnabled(totalPages > 1 && state.CurrentPage < totalPages - 1);
            state.RightArrow.Position = new Vector2(rightX, y);
        }

        RefreshFocusNeighborsForVisibleRow(screen, visibleButtons);
    }

    private static void RefreshFocusNeighborsForVisibleRow(NCustomRunScreen screen, NCharacterSelectButton[] visibleButtons)
    {
        LineEdit? seed = SeedInputRef(screen);
        int n = visibleButtons.Length;
        for (int i = 0; i < n; i++)
        {
            NCharacterSelectButton btn = visibleButtons[i];
            NCharacterSelectButton left = i == 0 ? visibleButtons[n - 1] : visibleButtons[i - 1];
            NCharacterSelectButton right = i == n - 1 ? visibleButtons[0] : visibleButtons[i + 1];
            btn.FocusNeighborLeft = left.GetPath();
            btn.FocusNeighborRight = right.GetPath();
            if (seed != null)
            {
                btn.FocusNeighborTop = seed.GetPath();
            }

            btn.FocusNeighborBottom = btn.GetPath();
        }
    }
}

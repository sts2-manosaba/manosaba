using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

[HarmonyPatch]
public static class Patch_NCharacterSelectScreen_ButtonLayout
{
    private static readonly AccessTools.FieldRef<NCharacterSelectScreen, Control> _charButtonContainerRef =
        AccessTools.FieldRefAccess<NCharacterSelectScreen, Control>("_charButtonContainer");

    private static readonly AccessTools.FieldRef<NCharacterSelectScreen, NCharacterSelectButton> _selectedButtonRef =
        AccessTools.FieldRefAccess<NCharacterSelectScreen, NCharacterSelectButton>("_selectedButton");

    private static readonly AccessTools.FieldRef<NCharacterSelectScreen, NCharacterSelectButton> _randomButtonRef =
        AccessTools.FieldRefAccess<NCharacterSelectScreen, NCharacterSelectButton>("_randomCharacterButton");

    private const int CharactersPerPage = 8;
    private const float ArrowSize = 80f;
    private const float ArrowGap = 18f;
    private const string LeftArrowName = "PageLeftArrow";
    private const string RightArrowName = "PageRightArrow";

    private sealed class PaginationState
    {
        public int CurrentPage;
        public readonly Dictionary<NCharacterSelectButton, bool> BaseVisible = new();
        public NGoldArrowButton? LeftArrow;
        public NGoldArrowButton? RightArrow;
    }

    private static readonly Dictionary<ulong, PaginationState> _states = new();

    [HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen._Ready))]
    [HarmonyPostfix]
    private static void Postfix_Ready(NCharacterSelectScreen __instance)
    {
        EnsurePagerControls(__instance);
        SnapshotBaseVisibility(__instance);
        SyncRandomBaseVisibility(__instance);
        ApplyPagination(__instance, selectionMayBeInvalid: false);
    }

    [HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.OnSubmenuOpened))]
    [HarmonyPostfix]
    private static void Postfix_OnSubmenuOpened(NCharacterSelectScreen __instance)
    {
        SnapshotBaseVisibility(__instance);
        SyncRandomBaseVisibility(__instance);
        ApplyPagination(__instance, selectionMayBeInvalid: true);
    }

    [HarmonyPatch(typeof(NCharacterSelectScreen), "UpdateRandomCharacterVisibility")]
    [HarmonyPostfix]
    private static void Postfix_UpdateRandomCharacterVisibility(NCharacterSelectScreen __instance)
    {
        SyncRandomBaseVisibility(__instance);
        ApplyPagination(__instance, selectionMayBeInvalid: true);
    }

    private static PaginationState GetState(NCharacterSelectScreen screen)
    {
        ulong key = screen.GetInstanceId();
        if (!_states.TryGetValue(key, out PaginationState? state))
        {
            state = new PaginationState();
            _states[key] = state;
        }
        return state;
    }

    private static void SnapshotBaseVisibility(NCharacterSelectScreen screen)
    {
        PaginationState state = GetState(screen);
        Control container = _charButtonContainerRef(screen);
        if (container == null)
        {
            return;
        }

        foreach (NCharacterSelectButton button in container.GetChildren().OfType<NCharacterSelectButton>())
        {
            if (!state.BaseVisible.ContainsKey(button))
            {
                state.BaseVisible[button] = button.Visible;
            }
        }
    }

    private static void SyncRandomBaseVisibility(NCharacterSelectScreen screen)
    {
        PaginationState state = GetState(screen);
        NCharacterSelectButton randomButton = _randomButtonRef(screen);
        if (randomButton != null)
        {
            state.BaseVisible[randomButton] = randomButton.Visible;
        }
    }

    private static void EnsurePagerControls(NCharacterSelectScreen screen)
    {
        Control? outerContainer = screen.GetNodeOrNull<Control>("CharSelectButtons");
        if (outerContainer == null)
        {
            return;
        }

        PaginationState state = GetState(screen);
        if (state.LeftArrow == null || !GodotObject.IsInstanceValid(state.LeftArrow))
        {
            NGoldArrowButton left = CreateArrowButton(isLeft: true);
            left.Name = LeftArrowName;
            left.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(_ =>
            {
                TryChangePage(screen, -1);
            }));
            outerContainer.AddChild(left);
            state.LeftArrow = left;
        }

        if (state.RightArrow == null || !GodotObject.IsInstanceValid(state.RightArrow))
        {
            NGoldArrowButton right = CreateArrowButton(isLeft: false);
            right.Name = RightArrowName;
            right.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(_ =>
            {
                TryChangePage(screen, +1);
            }));
            outerContainer.AddChild(right);
            state.RightArrow = right;
        }
    }

    private static NGoldArrowButton CreateArrowButton(bool isLeft)
    {
        NGoldArrowButton button = new NGoldArrowButton
        {
            CustomMinimumSize = new Vector2(ArrowSize, ArrowSize),
            MouseFilter = Control.MouseFilterEnum.Stop,
            ZIndex = 100
        };

        TextureRect icon = new TextureRect
        {
            Name = "TextureRect",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
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

    private static void TryChangePage(NCharacterSelectScreen screen, int delta)
    {
        PaginationState state = GetState(screen);
        state.CurrentPage += delta;
        ApplyPagination(screen, selectionMayBeInvalid: true);
    }

    private static void ApplyPagination(NCharacterSelectScreen screen, bool selectionMayBeInvalid)
    {
        EnsurePagerControls(screen);
        SnapshotBaseVisibility(screen);

        Control? outerContainer = screen.GetNodeOrNull<Control>("CharSelectButtons");
        Control buttonContainer = _charButtonContainerRef(screen);
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
        NCharacterSelectButton randomButton = _randomButtonRef(screen);
        if (randomButton != null &&
            state.BaseVisible.TryGetValue(randomButton, out bool randomShouldBeVisible) &&
            randomShouldBeVisible)
        {
            pageButtons.Add(randomButton);
        }

        foreach (NCharacterSelectButton button in allButtons)
        {
            bool shouldShow = pageButtons.Contains(button);
            button.Visible = shouldShow;
            if (shouldShow)
            {
                if (!button.IsEnabled) button.Enable();
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
            NCharacterSelectButton selected = _selectedButtonRef(screen);
            if (selected == null || !selected.Visible)
            {
                visibleButtons[0].Select();
            }
        }

        Control? hbox = screen.GetNodeOrNull<Control>("CharSelectButtons/ButtonContainer");
        float rowTop = hbox?.OffsetTop ?? -189f;
        float rowBottom = hbox?.OffsetBottom ?? -35f;
        float y = rowTop + (rowBottom - rowTop - ArrowSize) * 0.5f;

        float buttonWidth = visibleButtons.Max(b => Mathf.Max(b.Size.X, b.CustomMinimumSize.X));
        if (buttonWidth <= 0f) buttonWidth = 100f;
        float separation = (hbox as BoxContainer)?.GetThemeConstant("separation") ?? 16f;
        float rowWidth = visibleButtons.Length * buttonWidth + Mathf.Max(0, visibleButtons.Length - 1) * separation;
        float leftX = -rowWidth * 0.5f - ArrowGap - ArrowSize;
        float rightX = rowWidth * 0.5f + ArrowGap;

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
    }
}

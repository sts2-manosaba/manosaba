using Godot;
using HarmonyLib;
using Manosaba.UI.Minesweeper;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(NTopBar))]
public static class Patch_NTopBar_Minesweeper
{
    private const string ButtonNodeName = "ManosabaMinesweeperShovelButton";
    private const string PopupNodeName = "ManosabaMinesweeperPopup";

    [HarmonyPatch(nameof(NTopBar._Ready))]
    [HarmonyPostfix]
    private static void Ready_Postfix(NTopBar __instance)
    {
        EnsureButton(__instance);
    }

    [HarmonyPatch(nameof(NTopBar._ExitTree))]
    [HarmonyPostfix]
    private static void ExitTree_Postfix(NTopBar __instance)
    {
        ClosePopup(__instance);
    }

    private static void EnsureButton(NTopBar topBar)
    {
        if (FindDescendantByName(topBar, ButtonNodeName) != null)
        {
            return;
        }

        Control? rightAligned = topBar.GetNodeOrNull<Control>("RightAlignedStuff");
        Control? map = topBar.GetNodeOrNull<Control>("%Map");
        if (rightAligned == null || map == null)
        {
            return;
        }

        ShovelMinesweeperButton button = new()
        {
            Name = ButtonNodeName,
            CustomMinimumSize = new Vector2(64f, 80f),
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
        };
        button.Clicked = () => OpenMinesweeper(topBar);

        rightAligned.AddChild(button);
        rightAligned.MoveChild(button, map.GetIndex());
    }

    private static void OpenMinesweeper(NTopBar topBar)
    {
        MinesweeperPopup popup = new()
        {
            Name = PopupNodeName,
            ZIndex = 2000,
        };

        NModalContainer? modalContainer = NModalContainer.Instance;
        if (modalContainer != null)
        {
            if (modalContainer.OpenModal != null)
            {
                popup.QueueFree();
                return;
            }

            modalContainer.Add(popup);
            return;
        }

        if (FindDescendantByName(topBar, PopupNodeName) != null)
        {
            popup.QueueFree();
            return;
        }

        topBar.AddChild(popup);
    }

    private static void ClosePopup(NTopBar topBar)
    {
        NModalContainer? modalContainer = NModalContainer.Instance;
        if (modalContainer?.OpenModal is MinesweeperPopup)
        {
            modalContainer.Clear();
            return;
        }

        FindDescendantByName(topBar, PopupNodeName)?.QueueFree();
    }

    private static Node? FindDescendantByName(Node root, string targetName)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child.Name == targetName)
            {
                return child;
            }

            Node? nested = FindDescendantByName(child, targetName);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }
}

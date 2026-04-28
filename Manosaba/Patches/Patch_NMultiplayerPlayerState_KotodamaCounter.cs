using Godot;
using HarmonyLib;
using Manosaba.Characters.Common.Resources;
using manosaba.Characters.NatsumeAnan;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(NMultiplayerPlayerState))]
public static class Patch_NMultiplayerPlayerState_KotodamaCounter
{
    private const string ContainerName = "ManosabaKotodamaCountContainer";
    private const string ImageName = "Image";
    private const string LabelName = "KotodamaCount";

    private static readonly Dictionary<ulong, Action<Player, CharacterCustomEnergyDefinition, int, int>> Subscriptions = new();

    [HarmonyPatch(nameof(NMultiplayerPlayerState._Ready))]
    [HarmonyPostfix]
    private static void Ready_Postfix(NMultiplayerPlayerState __instance)
    {
        EnsureCounter(__instance);
        RefreshCounter(__instance);
    }

    [HarmonyPatch("OnCombatSetUp")]
    [HarmonyPostfix]
    private static void OnCombatSetUp_Postfix(NMultiplayerPlayerState __instance, CombatState _)
    {
        if (!LocalContext.IsMe(__instance.Player))
        {
            EnsureSubscribed(__instance);
        }

        RefreshCounter(__instance);
    }

    [HarmonyPatch("OnCombatEnded")]
    [HarmonyPostfix]
    private static void OnCombatEnded_Postfix(NMultiplayerPlayerState __instance, CombatRoom _)
    {
        Unsubscribe(__instance);
        RefreshCounter(__instance, forceHidden: true);
    }

    [HarmonyPatch("RefreshCombatValues")]
    [HarmonyPostfix]
    private static void RefreshCombatValues_Postfix(NMultiplayerPlayerState __instance)
    {
        RefreshCounter(__instance);
    }

    [HarmonyPatch(nameof(NMultiplayerPlayerState._ExitTree))]
    [HarmonyPostfix]
    private static void ExitTree_Postfix(NMultiplayerPlayerState __instance)
    {
        Unsubscribe(__instance);
    }

    private static void EnsureSubscribed(NMultiplayerPlayerState state)
    {
        ulong instanceId = state.GetInstanceId();
        if (Subscriptions.ContainsKey(instanceId))
        {
            return;
        }

        Action<Player, CharacterCustomEnergyDefinition, int, int> handler = (player, definition, _, _) =>
        {
            if (player.NetId == state.Player.NetId &&
                string.Equals(definition.EnergyId, KotodamaEnergy.Instance.EnergyId, StringComparison.Ordinal))
            {
                RefreshCounter(state);
            }
        };
        Subscriptions[instanceId] = handler;
        CharacterCustomEnergyService.EnergyChanged += handler;
    }

    private static void Unsubscribe(NMultiplayerPlayerState state)
    {
        ulong instanceId = state.GetInstanceId();
        if (!Subscriptions.Remove(instanceId, out Action<Player, CharacterCustomEnergyDefinition, int, int>? handler))
        {
            return;
        }

        CharacterCustomEnergyService.EnergyChanged -= handler;
    }

    private static Control? EnsureCounter(NMultiplayerPlayerState state)
    {
        Control? topInfoContainer = state.GetNodeOrNull<Control>("TopInfoContainer");
        if (topInfoContainer == null)
        {
            return null;
        }

        Control? existing = topInfoContainer.GetNodeOrNull<Control>(ContainerName);
        if (existing != null)
        {
            return existing;
        }

        Control container = new()
        {
            Name = ContainerName,
            CustomMinimumSize = new Vector2(28f, 28f),
            Visible = false
        };

        TextureRect image = new()
        {
            Name = ImageName,
            Texture = KotodamaEnergy.GetHoverTipIcon(),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
        };
        image.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        image.OffsetBottom = -1f;

        Label label = new()
        {
            Name = LabelName,
            Text = "0",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        label.OffsetTop = 1f;
        label.AddThemeColorOverride("font_color", StsColors.cream);
        label.AddThemeColorOverride("font_shadow_color", new Color(0f, 0f, 0f, 0.1254902f));
        label.AddThemeColorOverride("font_outline_color", new Color(0.2f, 0.188235f, 0.14902f, 1f));
        label.AddThemeConstantOverride("shadow_offset_x", 3);
        label.AddThemeConstantOverride("shadow_offset_y", 2);
        label.AddThemeConstantOverride("outline_size", 8);
        label.AddThemeFontSizeOverride("font_size", 16);
        CopyLabelFont(state, label);

        container.AddChildSafely(image);
        container.AddChildSafely(label);
        topInfoContainer.AddChildSafely(container);

        Control? cardContainer = topInfoContainer.GetNodeOrNull<Control>("CardCountContainer");
        if (cardContainer != null)
        {
            topInfoContainer.MoveChild(container, cardContainer.GetIndex());
        }

        return container;
    }

    private static void CopyLabelFont(NMultiplayerPlayerState state, Label label)
    {
        Label? starLabel = state.GetNodeOrNull<Label>("%StarCount");
        if (starLabel == null)
        {
            return;
        }

        Font? font = starLabel.GetThemeFont("font");
        if (font != null)
        {
            label.AddThemeFontOverride("font", font);
        }

        int fontSize = starLabel.GetThemeFontSize("font_size");
        if (fontSize > 0)
        {
            label.AddThemeFontSizeOverride("font_size", fontSize);
        }
    }

    private static void RefreshCounter(NMultiplayerPlayerState state, bool forceHidden = false)
    {
        Control? container = EnsureCounter(state);
        if (container == null)
        {
            return;
        }

        Player player = state.Player;
        bool visible = !forceHidden &&
                       !LocalContext.IsMe(player) &&
                       player.PlayerCombatState != null &&
                       KotodamaEnergy.Instance.AppliesTo(player);
        int value = visible ? KotodamaEnergy.Get(player) : 0;
        visible = visible && value > 0;

        container.Visible = visible;
        Label? label = container.GetNodeOrNull<Label>(LabelName);
        if (label == null)
        {
            return;
        }

        label.Text = value.ToString();
        label.AddThemeColorOverride("font_color", value == 0 ? StsColors.red : StsColors.cream);
    }
}

using Godot;
using HarmonyLib;
using Manosaba.Characters.JogasakiNoah.Cards;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Manosaba.Patches
{
    [HarmonyPatch]
    public static class Patch_PaletteGapTargeting
    {
        private static readonly MethodInfo? CannotPlayThisCardFtueCheckMethod =
            AccessTools.Method(typeof(NCardPlay), "CannotPlayThisCardFtueCheck");
        private static readonly MethodInfo? TryShowEvokingOrbsMethod =
            AccessTools.Method(typeof(NCardPlay), "TryShowEvokingOrbs");
        private static readonly MethodInfo? CenterCardMethod =
            AccessTools.Method(typeof(NCardPlay), "CenterCard");
        private static readonly MethodInfo? TryPlayCardMethod =
            AccessTools.Method(typeof(NCardPlay), "TryPlayCard");

        private static readonly PropertyInfo? CardProp = AccessTools.Property(typeof(NCardPlay), "Card");
        private static readonly PropertyInfo? CardNodeProp = AccessTools.Property(typeof(NCardPlay), "CardNode");

        private static readonly FieldInfo? OrbManagerOrbsField = AccessTools.Field(typeof(NOrbManager), "_orbs");

        [HarmonyPatch(typeof(NMouseCardPlay), "TargetSelection")]
        [HarmonyPrefix]
        private static bool NMouseCardPlay_TargetSelection_Prefix(NMouseCardPlay __instance, TargetMode targetMode, ref Task __result)
        {
            PaletteGap? card = GetPaletteGapCard(__instance);
            if (card == null)
            {
                return true;
            }

            if (ShouldChannelLikePalette(card))
            {
                __result = NMouseCardPlay_PlayWithoutGapTargetSelection(__instance);
                return false;
            }

            __result = NMouseCardPlay_TargetSelectionPaletteGap(__instance, targetMode);
            return false;
        }

        [HarmonyPatch(typeof(NControllerCardPlay), nameof(NControllerCardPlay.Start))]
        [HarmonyPrefix]
        private static bool NControllerCardPlay_Start_Prefix(NControllerCardPlay __instance)
        {
            PaletteGap? card = GetPaletteGapCard(__instance);
            if (card == null)
            {
                return true;
            }

            NCard? cardNode = GetCardNode(__instance);
            if (cardNode == null)
            {
                return true;
            }

            NDebugAudioManager.Instance?.Play("card_select.mp3");
            NHoverTipSet.Remove(__instance.Holder);

            if (!card.CanPlay(out UnplayableReason _, out AbstractModel? _))
            {
                CannotPlayThisCardFtueCheckMethod?.Invoke(__instance, [card]);
                __instance.CancelPlayCard();
                return false;
            }

            if (ShouldChannelLikePalette(card))
            {
                TryPlayCardMethod?.Invoke(__instance, [null]);
                return false;
            }

            TryShowEvokingOrbsMethod?.Invoke(__instance, null);
            cardNode.CardHighlight.AnimFlash();
            CenterCardMethod?.Invoke(__instance, null);

            TaskHelper.RunSafely(NControllerCardPlay_TargetSelectionPaletteGap(__instance));
            return false;
        }

        private static Task NMouseCardPlay_PlayWithoutGapTargetSelection(NMouseCardPlay cardPlay)
        {
            TryPlayCardMethod?.Invoke(cardPlay, [null]);
            return Task.CompletedTask;
        }

        private static async Task NMouseCardPlay_TargetSelectionPaletteGap(NMouseCardPlay cardPlay, TargetMode targetMode)
        {
            PaletteGap? card = GetPaletteGapCard(cardPlay);
            NCard? cardNode = GetCardNode(cardPlay);
            if (card == null || cardNode == null)
            {
                return;
            }

            TryShowEvokingOrbsMethod?.Invoke(cardPlay, null);
            cardNode.CardHighlight.AnimFlash();

            int? insertIndex = await SelectInsertIndex(cardPlay, card, cardNode, targetMode);
            if (!insertIndex.HasValue)
            {
                cardPlay.CancelPlayCard();
                return;
            }

            card.PendingInsertIndex = insertIndex.Value;
            TryPlayCardMethod?.Invoke(cardPlay, [null]);
        }

        private static async Task NControllerCardPlay_TargetSelectionPaletteGap(NControllerCardPlay cardPlay)
        {
            PaletteGap? card = GetPaletteGapCard(cardPlay);
            NCard? cardNode = GetCardNode(cardPlay);
            if (card == null || cardNode == null)
            {
                return;
            }

            int? insertIndex = await SelectInsertIndex(cardPlay, card, cardNode, TargetMode.Controller);
            if (!GodotObject.IsInstanceValid(cardPlay))
            {
                return;
            }

            if (!insertIndex.HasValue)
            {
                cardPlay.CancelPlayCard();
                return;
            }

            card.PendingInsertIndex = insertIndex.Value;
            TryPlayCardMethod?.Invoke(cardPlay, [null]);
        }

        private static async Task<int?> SelectInsertIndex(NCardPlay cardPlay, PaletteGap card, NCard cardNode, TargetMode targetMode)
        {
            _ = card;
            NOrbManager? orbManager = NCombatRoom.Instance?.GetCreatureNode(card.Owner.Creature)?.OrbManager;
            if (orbManager == null || OrbManagerOrbsField?.GetValue(orbManager) is not List<NOrb> orbSlots || orbSlots.Count == 0)
            {
                return null;
            }

            int queueCount = card.Owner.PlayerCombatState?.OrbQueue?.Orbs?.Count ?? 0;
            int maxInsertIndex = Math.Min(queueCount, Math.Max(0, orbSlots.Count - 1));
            if (maxInsertIndex < 0)
            {
                return null;
            }

            Dictionary<Control, int> markerToIndex = [];
            bool includeFirstGap = queueCount == 0;
            List<Control> markers = CreateGapMarkers(orbSlots, maxInsertIndex, includeFirstGap, markerToIndex);
            if (markers.Count == 0)
            {
                return null;
            }

            NTargetManager targetManager = NTargetManager.Instance;
            bool usingController = targetMode == TargetMode.Controller;
            targetManager.StartTargeting(
                TargetType.TargetedNoCreature,
                cardNode,
                targetMode,
                () => !GodotObject.IsInstanceValid(cardPlay),
                node => node is Control c && markerToIndex.ContainsKey(c));

            if (usingController)
            {
                SetupControllerNavigation(markers);
            }

            try
            {
                Node? selectedNode = await targetManager.SelectionFinished();
                if (usingController)
                {
                    NCombatRoom.Instance?.EnableControllerNavigation();
                }

                if (selectedNode is Control control && markerToIndex.TryGetValue(control, out int idx))
                {
                    return idx;
                }

                return null;
            }
            finally
            {
                foreach (Control marker in markers)
                {
                    marker.QueueFreeSafely();
                }
            }
        }

        private static List<Control> CreateGapMarkers(
            IReadOnlyList<NOrb> orbSlots,
            int maxInsertIndex,
            bool includeFirstGap,
            Dictionary<Control, int> markerToIndex)
        {
            int startIndex = includeFirstGap ? 0 : 1;
            int markerCount = Math.Max(0, maxInsertIndex - startIndex + 1);
            List<Control> markers = new(markerCount);
            for (int i = startIndex; i <= maxInsertIndex; i++)
            {
                Vector2 position = GetGapPosition(orbSlots, maxInsertIndex, i);
                ColorRect marker = new()
                {
                    Name = $"PaletteGapTarget_{i}",
                    Color = new Color(1f, 1f, 1f, 0.16f),
                    CustomMinimumSize = new Vector2(44f, 44f),
                    Size = new Vector2(44f, 44f),
                    MouseFilter = Control.MouseFilterEnum.Stop,
                    FocusMode = Control.FocusModeEnum.All,
                    TopLevel = true,
                    GlobalPosition = position - new Vector2(22f, 22f),
                    ZIndex = 500,
                };

                marker.Connect(Control.SignalName.MouseEntered, Callable.From(() => NTargetManager.Instance.OnNodeHovered(marker)));
                marker.Connect(Control.SignalName.MouseExited, Callable.From(() => NTargetManager.Instance.OnNodeUnhovered(marker)));
                marker.Connect(Control.SignalName.FocusEntered, Callable.From(() => NTargetManager.Instance.OnNodeHovered(marker)));
                marker.Connect(Control.SignalName.FocusExited, Callable.From(() => NTargetManager.Instance.OnNodeUnhovered(marker)));

                NCombatRoom.Instance?.AddChild(marker);
                markers.Add(marker);
                markerToIndex[marker] = i;
            }

            return markers;
        }

        private static Vector2 GetGapPosition(IReadOnlyList<NOrb> orbSlots, int maxInsertIndex, int insertIndex)
        {
            if (maxInsertIndex == 0)
            {
                return GetSlotCenter(orbSlots[0]);
            }

            if (maxInsertIndex == 1)
            {
                Vector2 first = GetSlotCenter(orbSlots[0]);
                Vector2 second = GetSlotCenter(orbSlots[1]);
                Vector2 dir = (first - second).Normalized();
                return insertIndex == 0 ? first + dir * 28f : first - dir * 28f;
            }

            if (insertIndex == 0)
            {
                Vector2 first = GetSlotCenter(orbSlots[0]);
                Vector2 second = GetSlotCenter(orbSlots[1]);
                return first + (first - second).Normalized() * 28f;
            }

            if (insertIndex == maxInsertIndex)
            {
                Vector2 last = GetSlotCenter(orbSlots[maxInsertIndex - 1]);
                Vector2 prev = GetSlotCenter(orbSlots[Math.Max(0, maxInsertIndex - 2)]);
                return last + (last - prev).Normalized() * 28f;
            }

            Vector2 left = GetSlotCenter(orbSlots[insertIndex - 1]);
            Vector2 right = GetSlotCenter(orbSlots[insertIndex]);
            return (left + right) * 0.5f;
        }

        private static Vector2 GetSlotCenter(Control slot)
        {
            return slot.GlobalPosition + slot.Size * 0.5f;
        }

        private static void SetupControllerNavigation(IReadOnlyList<Control> markers)
        {
            if (markers.Count == 0)
            {
                return;
            }

            NCombatRoom.Instance?.RestrictControllerNavigation(markers);

            for (int i = 0; i < markers.Count; i++)
            {
                Control current = markers[i];
                Control prev = markers[(i - 1 + markers.Count) % markers.Count];
                Control next = markers[(i + 1) % markers.Count];
                current.FocusNeighborLeft = prev.GetPath();
                current.FocusNeighborRight = next.GetPath();
                current.FocusNeighborTop = current.GetPath();
                current.FocusNeighborBottom = current.GetPath();
            }

            markers[0].TryGrabFocus();
        }

        private static PaletteGap? GetPaletteGapCard(NCardPlay cardPlay)
        {
            return GetCard(cardPlay) as PaletteGap;
        }

        private static bool ShouldChannelLikePalette(PaletteGap card)
        {
            return (card.Owner.PlayerCombatState?.OrbQueue?.Capacity ?? 0) <= 1;
        }

        private static CardModel? GetCard(NCardPlay cardPlay)
        {
            return CardProp?.GetValue(cardPlay) as CardModel;
        }

        private static NCard? GetCardNode(NCardPlay cardPlay)
        {
            return CardNodeProp?.GetValue(cardPlay) as NCard;
        }
    }
}

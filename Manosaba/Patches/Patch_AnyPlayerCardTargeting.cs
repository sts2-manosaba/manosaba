using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Manosaba.Patches;

[HarmonyPatch]
public static class Patch_AnyPlayerCardTargeting
{
    private static readonly AccessTools.FieldRef<NCardPlay, bool> _isTryingToPlayCardRef =
        AccessTools.FieldRefAccess<NCardPlay, bool>("_isTryingToPlayCard");

    private static readonly MethodInfo? _cannotPlayThisCardFtueCheckMethod =
        AccessTools.Method(typeof(NCardPlay), "CannotPlayThisCardFtueCheck");
    private static readonly MethodInfo? _cleanupMethod =
        AccessTools.Method(typeof(NCardPlay), "Cleanup");
    private static readonly MethodInfo? _tryShowEvokingOrbsMethod =
        AccessTools.Method(typeof(NCardPlay), "TryShowEvokingOrbs");
    private static readonly MethodInfo? _centerCardMethod =
        AccessTools.Method(typeof(NCardPlay), "CenterCard");
    private static readonly MethodInfo? _tryPlayCardMethod =
        AccessTools.Method(typeof(NCardPlay), "TryPlayCard");
    private static readonly MethodInfo? _onCreatureHoverMethod =
        AccessTools.Method(typeof(NCardPlay), "OnCreatureHover");
    private static readonly MethodInfo? _onCreatureUnhoverMethod =
        AccessTools.Method(typeof(NCardPlay), "OnCreatureUnhover");
    private static readonly MethodInfo? _mouseSingleCreatureTargetingMethod =
        AccessTools.Method(typeof(NMouseCardPlay), "SingleCreatureTargeting");
    private static readonly MethodInfo? _controllerSingleCreatureTargetingMethod =
        AccessTools.Method(typeof(NControllerCardPlay), "SingleCreatureTargeting");

    private static readonly PropertyInfo? _cardProp = AccessTools.Property(typeof(NCardPlay), "Card");
    private static readonly PropertyInfo? _cardNodeProp = AccessTools.Property(typeof(NCardPlay), "CardNode");

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.IsValidTarget))]
    [HarmonyPrefix]
    private static bool CardModel_IsValidTarget_Prefix(CardModel __instance, Creature? target, ref bool __result)
    {
        if (__instance.TargetType != TargetType.AnyPlayer)
        {
            return true;
        }

        __result = target != null && target.IsAlive && target.IsPlayer;
        return false;
    }

    [HarmonyPatch(typeof(NCardPlay), "TryPlayCard")]
    [HarmonyPrefix]
    private static bool NCardPlay_TryPlayCard_Prefix(NCardPlay __instance, Creature? target)
    {
        CardModel? card = GetCard(__instance);
        if (card == null || card.TargetType != TargetType.AnyPlayer)
        {
            return true;
        }

        if (target == null)
        {
            __instance.CancelPlayCard();
            return false;
        }

        if (!__instance.Holder.CardModel.CanPlayTargeting(target))
        {
            _cannotPlayThisCardFtueCheckMethod?.Invoke(__instance, [__instance.Holder.CardModel]);
            __instance.CancelPlayCard();
            return false;
        }

        _isTryingToPlayCardRef(__instance) = true;
        bool played = card.TryManualPlay(target);
        _isTryingToPlayCardRef(__instance) = false;

        if (!played)
        {
            __instance.CancelPlayCard();
            return false;
        }

        if (__instance.Holder.IsInsideTree())
        {
            Vector2 size = __instance.GetViewport().GetVisibleRect().Size;
            __instance.Holder.SetTargetPosition(new Vector2(size.X / 2f, size.Y - __instance.Holder.Size.Y));
        }

        _cleanupMethod?.Invoke(__instance, [true]);
        NCombatRoom.Instance?.Ui.Hand.TryGrabFocus();
        return false;
    }

    [HarmonyPatch(typeof(NMouseCardPlay), "TargetSelection")]
    [HarmonyPrefix]
    private static bool NMouseCardPlay_TargetSelection_Prefix(NMouseCardPlay __instance, TargetMode targetMode, ref Task __result)
    {
        CardModel? card = GetCard(__instance);
        if (card == null || card.TargetType != TargetType.AnyPlayer)
        {
            return true;
        }

        __result = NMouseCardPlay_TargetSelectionAnyPlayer(__instance, targetMode);
        return false;
    }

    private static async Task NMouseCardPlay_TargetSelectionAnyPlayer(NMouseCardPlay cardPlay, TargetMode targetMode)
    {
        CardModel? card = GetCard(cardPlay);
        NCard? cardNode = GetCardNode(cardPlay);
        if (card == null || cardNode == null)
        {
            return;
        }

        _tryShowEvokingOrbsMethod?.Invoke(cardPlay, null);
        cardNode.CardHighlight.AnimFlash();

        if (_mouseSingleCreatureTargetingMethod?.Invoke(cardPlay, [targetMode, TargetType.AnyPlayer]) is Task task)
        {
            await task;
        }
    }

    [HarmonyPatch(typeof(NControllerCardPlay), nameof(NControllerCardPlay.Start))]
    [HarmonyPrefix]
    private static bool NControllerCardPlay_Start_Prefix(NControllerCardPlay __instance)
    {
        CardModel? card = GetCard(__instance);
        NCard? cardNode = GetCardNode(__instance);
        if (card == null || cardNode == null || card.TargetType != TargetType.AnyPlayer)
        {
            return true;
        }

        NDebugAudioManager.Instance?.Play("card_select.mp3");
        NHoverTipSet.Remove(__instance.Holder);

        if (!card.CanPlay(out UnplayableReason _, out AbstractModel? _))
        {
            _cannotPlayThisCardFtueCheckMethod?.Invoke(__instance, [card]);
            __instance.CancelPlayCard();
            return false;
        }

        _tryShowEvokingOrbsMethod?.Invoke(__instance, null);
        cardNode.CardHighlight.AnimFlash();
        _centerCardMethod?.Invoke(__instance, null);

        if (_controllerSingleCreatureTargetingMethod?.Invoke(__instance, [TargetType.AnyPlayer]) is Task targetingTask)
        {
            TaskHelper.RunSafely(targetingTask);
        }

        return false;
    }

    [HarmonyPatch(typeof(NControllerCardPlay), "SingleCreatureTargeting")]
    [HarmonyPrefix]
    private static bool NControllerCardPlay_SingleCreatureTargeting_Prefix(NControllerCardPlay __instance, TargetType targetType, ref Task __result)
    {
        if (targetType != TargetType.AnyPlayer)
        {
            return true;
        }

        __result = NControllerCardPlay_SingleCreatureTargetingAnyPlayer(__instance);
        return false;
    }

    private static async Task NControllerCardPlay_SingleCreatureTargetingAnyPlayer(NControllerCardPlay cardPlay)
    {
        CardModel? card = GetCard(cardPlay);
        NCard? cardNode = GetCardNode(cardPlay);
        if (card == null || cardNode == null)
        {
            return;
        }

        NTargetManager targetManager = NTargetManager.Instance;
        Callable hoverCallable = Callable.From<NCreature>(creature => _onCreatureHoverMethod?.Invoke(cardPlay, [creature]));
        Callable unhoverCallable = Callable.From<NCreature>(creature => _onCreatureUnhoverMethod?.Invoke(cardPlay, [creature]));
        targetManager.Connect(NTargetManager.SignalName.CreatureHovered, hoverCallable);
        targetManager.Connect(NTargetManager.SignalName.CreatureUnhovered, unhoverCallable);

        targetManager.StartTargeting(
            TargetType.AnyPlayer,
            cardNode,
            TargetMode.Controller,
            () => !GodotObject.IsInstanceValid(cardPlay) || !NControllerManager.Instance.IsUsingController,
            null);

        List<Creature> candidates = card.CombatState.PlayerCreatures
            .Where(creature => creature.IsHittable)
            .ToList();
        if (candidates.Count == 0)
        {
            targetManager.Disconnect(NTargetManager.SignalName.CreatureHovered, hoverCallable);
            targetManager.Disconnect(NTargetManager.SignalName.CreatureUnhovered, unhoverCallable);
            cardPlay.CancelPlayCard();
            return;
        }

        List<NCreature> candidateNodes = candidates
            .Select(creature => NCombatRoom.Instance.GetCreatureNode(creature))
            .OfType<NCreature>()
            .ToList();
        if (candidateNodes.Count == 0)
        {
            targetManager.Disconnect(NTargetManager.SignalName.CreatureHovered, hoverCallable);
            targetManager.Disconnect(NTargetManager.SignalName.CreatureUnhovered, unhoverCallable);
            cardPlay.CancelPlayCard();
            return;
        }

        NCombatRoom.Instance.RestrictControllerNavigation(candidateNodes.Select(node => node.Hitbox));
        candidateNodes.First().Hitbox.TryGrabFocus();

        Node? selectedNode = await targetManager.SelectionFinished();

        if (!GodotObject.IsInstanceValid(cardPlay))
        {
            return;
        }

        targetManager.Disconnect(NTargetManager.SignalName.CreatureHovered, hoverCallable);
        targetManager.Disconnect(NTargetManager.SignalName.CreatureUnhovered, unhoverCallable);

        Creature? target = selectedNode switch
        {
            NCreature creatureNode => creatureNode.Entity,
            NMultiplayerPlayerState playerState => playerState.Player.Creature,
            _ => null
        };

        if (target != null)
        {
            _tryPlayCardMethod?.Invoke(cardPlay, [target]);
        }
        else
        {
            cardPlay.CancelPlayCard();
        }
    }

    private static CardModel? GetCard(NCardPlay cardPlay)
    {
        return _cardProp?.GetValue(cardPlay) as CardModel;
    }

    private static NCard? GetCardNode(NCardPlay cardPlay)
    {
        return _cardNodeProp?.GetValue(cardPlay) as NCard;
    }
}

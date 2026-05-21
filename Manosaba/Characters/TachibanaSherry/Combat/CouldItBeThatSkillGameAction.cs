using Manosaba.Characters.TachibanaSherry.Cards;
using Manosaba.Characters.TachibanaSherry.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Manosaba.Characters.TachibanaSherry.Combat;

public sealed class CouldItBeThatSkillGameAction : GameAction
{
    public override ulong OwnerId => Player.NetId;

    public override GameActionType ActionType => GameActionType.CombatPlayPhaseOnly;

    public Player Player { get; }

    public CouldItBeThatSkillGameAction(Player player)
    {
        Player = player;
    }

    protected override async Task ExecuteAction()
    {
        if (Player.Creature?.CombatState is not { } combatState)
        {
            Cancel();
            return;
        }

        CouldItBeThatSkillPower? skillPower = Player.Creature.GetPower<CouldItBeThatSkillPower>();
        if (skillPower == null || !skillPower.IsReady)
        {
            Cancel();
            return;
        }

        List<CardModel> options = CouldItBeThatSkillTokens.PickRandomOptions(combatState, Player);
        if (options.Count == 0)
        {
            Cancel();
            return;
        }

        PlayerChoiceContext choiceContext = new GameActionPlayerChoiceContext(this);
        CardModel? selected = options.Count == 1
            ? options[0]
            : await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Player);

        if (selected == null)
        {
            Cancel();
            return;
        }

        skillPower.MarkUsed();
        await CardCmd.AutoPlay(choiceContext, selected, null);
    }

    protected override void CancelAction()
    {
        CouldItBeThatSkillActivation.ClearActivationPending(Player);
    }

    public override INetAction ToNetAction()
    {
        return new NetCouldItBeThatSkillGameAction();
    }

    public override string ToString()
    {
        return $"{nameof(CouldItBeThatSkillGameAction)} for player {Player.NetId}";
    }
}

public struct NetCouldItBeThatSkillGameAction : INetAction, IPacketSerializable
{
    public GameAction ToGameAction(Player player)
    {
        return new CouldItBeThatSkillGameAction(player);
    }

    public void Serialize(PacketWriter writer)
    {
    }

    public void Deserialize(PacketReader reader)
    {
    }

    public override string ToString()
    {
        return nameof(NetCouldItBeThatSkillGameAction);
    }
}

public static class CouldItBeThatSkillActivation
{
    private static ActionExecutor? _subscribedExecutor;
    private static ulong? _pendingPlayerNetId;

    public static bool IsActivationPending(Player player) =>
        _pendingPlayerNetId == player.NetId;

    public static bool CanActivate(Player player)
    {
        if (IsActivationPending(player))
        {
            return false;
        }

        if (player.Creature?.GetPower<CouldItBeThatSkillPower>() is not { IsReady: true })
        {
            return false;
        }

        if (!LocalContext.IsMe(player))
        {
            return false;
        }

        NCombatRoom? combatRoom = NCombatRoom.Instance;
        NCombatUi? combatUi = combatRoom?.Ui;
        if (combatUi == null || combatRoom == null || combatRoom.Mode != CombatRoomMode.ActiveCombat)
        {
            return false;
        }

        if (!ActiveScreenContext.Instance.IsCurrent(combatRoom))
        {
            return false;
        }

        if (combatUi.Hand.IsInCardSelection || combatUi.Hand.InCardPlay || combatUi.Hand.CurrentMode != NPlayerHand.Mode.Play)
        {
            return false;
        }

        if (player.Creature?.CombatState is not { CurrentSide: CombatSide.Player })
        {
            return false;
        }

        if (RunManager.Instance.ActionQueueSynchronizer.CombatState != ActionSynchronizerCombatState.PlayPhase
            || CombatManager.Instance.PlayerActionsDisabled)
        {
            return false;
        }

        if (CombatManager.Instance.PlayersTakingExtraTurn.Count > 0 &&
            !CombatManager.Instance.PlayersTakingExtraTurn.Contains(player))
        {
            return false;
        }

        if (CombatManager.Instance.IsPlayerReadyToEndTurn(player))
        {
            return false;
        }

        return player.Creature is { IsAlive: true };
    }

    public static void TryEnqueue(Player player)
    {
        if (!CanActivate(player))
        {
            return;
        }

        EnsurePendingTrackerSubscribed();
        _pendingPlayerNetId = player.NetId;
        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new CouldItBeThatSkillGameAction(player));
    }

    public static void ClearActivationPending(Player player)
    {
        if (_pendingPlayerNetId == player.NetId)
        {
            _pendingPlayerNetId = null;
        }
    }

    public static void ClearAllActivationPending()
    {
        _pendingPlayerNetId = null;
    }

    private static void EnsurePendingTrackerSubscribed()
    {
        ActionExecutor executor = RunManager.Instance.ActionExecutor;
        if (_subscribedExecutor == executor)
        {
            return;
        }

        if (_subscribedExecutor != null)
        {
            _subscribedExecutor.AfterActionExecuted -= OnAfterActionExecuted;
        }

        _subscribedExecutor = executor;
        _subscribedExecutor.AfterActionExecuted += OnAfterActionExecuted;
    }

    private static void OnAfterActionExecuted(GameAction action)
    {
        if (action is CouldItBeThatSkillGameAction && action.OwnerId == _pendingPlayerNetId)
        {
            _pendingPlayerNetId = null;
        }
    }
}

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
    public static bool CanActivate(Player player)
    {
        if (player.Creature?.GetPower<CouldItBeThatSkillPower>() is not { IsReady: true })
        {
            return false;
        }

        if (RunManager.Instance.ActionExecutor.CurrentlyRunningAction != null)
        {
            return false;
        }

        NCombatUi? combatUi = NCombatRoom.Instance?.Ui;
        if (combatUi == null || NCombatRoom.Instance?.Mode != CombatRoomMode.ActiveCombat)
        {
            return false;
        }

        if (combatUi.Hand.IsInCardSelection || combatUi.Hand.InCardPlay)
        {
            return false;
        }

        if (player.Creature?.CombatState is not { CurrentSide: CombatSide.Player })
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

        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new CouldItBeThatSkillGameAction(player));
    }
}

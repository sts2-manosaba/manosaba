using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Manosaba.Characters.JogasakiNoah.Powers;

public sealed class PetEnemyAiPower : PathCustomPowerModel
{
    private bool _skipFirstTrigger = false;
    private const int MaxMoveAdvanceAttempts = 8;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;


    public override bool ShouldPlayVfx => false;

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player)
        {
            return;
        }

        Creature owner = Owner;
        if (owner.IsDead || owner.Monster == null || owner.CombatState == null || !CombatManager.Instance.IsInProgress)
        {
            return;
        }

        if (_skipFirstTrigger)
        {
            _skipFirstTrigger = false;
            return;
        }

        Creature perspective = owner.PetOwner?.Creature ?? owner;
        List<Creature> targets = owner.CombatState.GetOpponentsOf(perspective)
            .Where(c => c != null && c.IsAlive)
            .ToList();
        if (targets.Count == 0)
        {
            return;
        }

        if (owner.Monster.NextMove.Id == "UNSET_MOVE")
        {
            PrepareForNextTurnWithTargets(owner, targets, rollNewMove: true);
        }

        if (!TryAdvanceToValidMove(owner, targets))
        {
            return;
        }

        MoveState move = owner.Monster.NextMove;
        await move.PerformMove(targets);
        owner.Monster.MoveStateMachine?.OnMovePerformed(move);
        PrepareForNextTurnWithTargets(owner, targets, rollNewMove: true);
    }

    public static bool TryAdvanceToValidMove(Creature owner, IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < MaxMoveAdvanceAttempts; i++)
        {
            MoveState currentMove = owner.Monster!.NextMove;
            bool isAllowedMove = IsAllowedMove(currentMove);
            if (isAllowedMove)
            {
                return true;
            }

            string oldMoveId = currentMove.Id;
            owner.Monster.MoveStateMachine?.OnMovePerformed(currentMove);
            PrepareForNextTurnWithTargets(owner, targets, rollNewMove: true);
            MoveState nextMove = owner.Monster.NextMove;

            bool nextIsAllowedMove = IsAllowedMove(nextMove);
            if (nextMove.Id == oldMoveId && !nextIsAllowedMove)
            {
                // Cannot advance away from this filtered move (e.g. forced/follow-up state).
                return false;
            }
        }

        return false;
    }

    private static bool IsAllowedMove(MoveState move)
    {
        return move.Intents.Count > 0 && move.Intents.All(intent => !IsFilteredIntentType(intent.IntentType));
    }

    private static bool IsFilteredIntentType(IntentType intentType)
    {
        return intentType is IntentType.StatusCard
            or IntentType.CardDebuff
            or IntentType.Summon
            or IntentType.Sleep;
    }

    private static void PrepareForNextTurnWithTargets(Creature owner, IReadOnlyList<Creature> targets, bool rollNewMove)
    {
        if (rollNewMove)
        {
            owner.Monster?.RollMove(targets.ToArray());
        }

        _ = NCombatRoom.Instance?.GetCreatureNode(owner)?.UpdateIntent(targets);
    }

    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (player != base.Owner.PetOwner)
        {
            return amount;
        }
        return amount - 1;
    }
}

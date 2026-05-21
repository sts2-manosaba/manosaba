using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.JogasakiNoah.Powers;

public sealed class PetEnemyAiPower : PathCustomPowerModel
{
    private bool _skipFirstTrigger = false;
    private const int MaxMoveAdvanceAttempts = 8;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;


    public override bool ShouldPlayVfx => false;

    public override Creature ModifyUnblockedDamageTarget(Creature target, decimal _, ValueProp props, Creature? __)
    {
        if (target != Owner.PetOwner?.Creature)
        {
            return target;
        }

        if (Owner.IsDead)
        {
            return target;
        }

        if (props.HasFlag(ValueProp.Unpowered))
        {
            return target;
        }

        return Owner;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> creatures)
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

        if (NCombatRoom.Instance is { } combatRoom && combatRoom.GetCreatureNode(owner) == null)
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

        if (owner.Monster.NextMove.Id == "UNSET_MOVE" && !TryPrepareForNextTurnWithTargets(owner, targets, rollNewMove: true))
        {
            return;
        }

        if (!TryAdvanceToValidMove(owner, targets))
        {
            return;
        }

        MoveState move = owner.Monster.NextMove;
        await move.PerformMove(targets);
        owner.Monster.MoveStateMachine?.OnMovePerformed(move);
        _ = TryPrepareForNextTurnWithTargets(owner, targets, rollNewMove: true);
    }

    public static bool TryAdvanceToValidMove(Creature owner, IReadOnlyList<Creature> targets)
    {
        if (owner.Monster == null)
        {
            return false;
        }

        for (int i = 0; i < MaxMoveAdvanceAttempts; i++)
        {
            MoveState currentMove = owner.Monster.NextMove;
            if (currentMove == null)
            {
                return false;
            }

            bool isAllowedMove = IsAllowedMove(currentMove);
            if (isAllowedMove)
            {
                return true;
            }

            string oldMoveId = currentMove.Id;
            owner.Monster.MoveStateMachine?.OnMovePerformed(currentMove);
            if (!TryPrepareForNextTurnWithTargets(owner, targets, rollNewMove: true))
            {
                return false;
            }
            MoveState nextMove = owner.Monster.NextMove;
            if (nextMove == null)
            {
                return false;
            }

            bool nextIsAllowedMove = IsAllowedMove(nextMove);
            if (nextMove.Id == oldMoveId && !nextIsAllowedMove)
            {
                // Cannot advance away from this filtered move (e.g. forced/follow-up state).
                return false;
            }
        }

        return false;
    }

    public static void AssignAiSlot(Creature pet, Creature? copiedFrom = null)
    {
        if (pet.SlotName != null)
        {
            return;
        }

        pet.SlotName = copiedFrom?.SlotName ?? "first";
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

    public static bool TryPrepareForNextTurnWithTargets(Creature owner, IReadOnlyList<Creature> targets, bool rollNewMove)
    {
        if (owner.Monster == null)
        {
            return false;
        }

        if (rollNewMove)
        {
            bool preparedMove;
            try
            {
                owner.Monster.RollMove(targets.ToArray());
                preparedMove = true;
            }
            catch (Exception ex) when (IsPetAiRollFailure(ex))
            {
                preparedMove = TryForceFallbackMove(owner);
            }

            if (!preparedMove)
            {
                return false;
            }
        }

        _ = NCombatRoom.Instance?.GetCreatureNode(owner)?.UpdateIntent(targets);
        return true;
    }

    private static bool IsPetAiRollFailure(Exception ex)
    {
        return ex is NullReferenceException or InvalidOperationException;
    }

    private static bool TryForceFallbackMove(Creature owner)
    {
        MonsterMoveStateMachine? moveStateMachine = owner.Monster?.MoveStateMachine;
        if (moveStateMachine == null)
        {
            return false;
        }

        MoveState? fallbackMove = moveStateMachine.States.Values
            .OfType<MoveState>()
            .FirstOrDefault(IsAllowedMove);
        if (fallbackMove == null)
        {
            return false;
        }

        owner.Monster!.SetMoveImmediate(fallbackMove, forceTransition: true);
        return true;
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

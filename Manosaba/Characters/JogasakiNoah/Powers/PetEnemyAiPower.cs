using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

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
            owner.PrepareForNextTurn(targets, rollNewMove: true);
        }

        if (!TryAdvanceToAttackOnlyMove(owner, targets))
        {
            return;
        }

        MoveState move = owner.Monster.NextMove;
        await move.PerformMove(targets);
        owner.Monster.MoveStateMachine?.OnMovePerformed(move);
        owner.PrepareForNextTurn(targets, rollNewMove: true);
    }

    public static bool TryAdvanceToAttackOnlyMove(Creature owner, IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < MaxMoveAdvanceAttempts; i++)
        {
            MoveState currentMove = owner.Monster!.NextMove;
            bool hasAnyIntents = currentMove.Intents.Count > 0;
            bool allIntentsAreAttack = hasAnyIntents && currentMove.Intents.All(intent => intent.IntentType == IntentType.Attack);
            if (allIntentsAreAttack)
            {
                return true;
            }

            string oldMoveId = currentMove.Id;
            owner.Monster.MoveStateMachine?.OnMovePerformed(currentMove);
            owner.PrepareForNextTurn(targets, rollNewMove: true);
            MoveState nextMove = owner.Monster.NextMove;

            bool nextHasAnyIntents = nextMove.Intents.Count > 0;
            bool nextAllIntentsAreAttack = nextHasAnyIntents && nextMove.Intents.All(intent => intent.IntentType == IntentType.Attack);
            if (nextMove.Id == oldMoveId && !nextAllIntentsAreAttack)
            {
                // Cannot advance away from this non-attack move (e.g. forced/follow-up state).
                return false;
            }
        }

        return false;
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

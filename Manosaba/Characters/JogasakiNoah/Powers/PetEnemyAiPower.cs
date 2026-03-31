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

        MoveState move = owner.Monster.NextMove;
        if (move.Intents.Any(i => i.IntentType == IntentType.Attack || i.IntentType == IntentType.Debuff))
            await move.PerformMove(targets);
        owner.Monster.MoveStateMachine?.OnMovePerformed(move);
        owner.PrepareForNextTurn(targets, rollNewMove: true);
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

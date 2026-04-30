using System;
using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;
using Manosaba.Characters.Common.Encounters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.Common.Monsters;

public sealed class BasementGuardian : MonsterModel, ICustomModel
{
    public override int MinInitialHp => InitialHp;
    public override int MaxInitialHp => InitialHp;
    public override bool IsHealthBarVisible => true;

    protected override string VisualsPath => "basement_guardian.tscn".EnemyScenePath();

    /// <summary>
    /// <see cref="MonsterModel.GenerateMoveStateMachine"/> is invoked during asset preload before <see cref="MonsterModel.Creature"/> is set;
    /// <see cref="MonsterModel.CombatState"/> reads <c>Creature</c> and throws. Use try/catch like vanilla preload-safe paths.
    /// </summary>
    private (int ActIndex, int ActFloor) ResolveActContext()
    {
        try
        {
            var rs = Creature.CombatState?.RunState;
            if (rs == null)
            {
                return (0, 0);
            }

            return (rs.CurrentActIndex, rs.ActFloor);
        }
        catch (InvalidOperationException)
        {
            return (0, 0);
        }
    }

    private int InitialHp
    {
        get
        {
            (int actIndex, int actFloor) = ResolveActContext();
            (int normalHp, int toughHp) hpByFloor = actIndex switch
            {
                0 => actFloor switch
                {
                    <= 5 => (52, 56),
                    <= 10 => (60, 65),
                    _ => (68, 74),
                },
                _ => (96, 104),
            };

            return AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, hpByFloor.toughHp, hpByFloor.normalHp);
        }
    }

    private int LightDamage
    {
        get
        {
            int actIndex = ResolveActContext().ActIndex;
            return actIndex switch
            {
                0 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9),
                1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11),
                _ => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11),
            };
        }
    }

    private int HeavyDamage
    {
        get
        {
            int actIndex = ResolveActContext().ActIndex;
            return actIndex switch
            {
                0 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 14),
                1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 16),
                _ => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 16),
            };
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState light1 = new("BASEMENT_GUARDIAN_LIGHT_1", LightAttack, new SingleAttackIntent(LightDamage));
        MoveState heavy1 = new("BASEMENT_GUARDIAN_HEAVY_1", HeavyAttack, new SingleAttackIntent(HeavyDamage));
        MoveState light2 = new("BASEMENT_GUARDIAN_LIGHT_2", LightAttack, new SingleAttackIntent(LightDamage));
        MoveState heavy2 = new("BASEMENT_GUARDIAN_HEAVY_2", HeavyAttack, new SingleAttackIntent(HeavyDamage));
        MoveState escape = new("BASEMENT_GUARDIAN_ESCAPE", EscapeMove, new EscapeIntent());

        light1.FollowUpState = heavy1;
        heavy1.FollowUpState = light2;
        light2.FollowUpState = heavy2;
        heavy2.FollowUpState = escape;
        escape.FollowUpState = escape;

        return new MonsterMoveStateMachine([light1, heavy1, light2, heavy2, escape], light1);
    }

    public override Task AfterAddedToRoom()
    {
        if (Creature.CombatState?.Encounter is not BasementGuardianEventEncounter)
        {
            return Task.CompletedTask;
        }

        // Mirror only for the basement event encounter.
        void FlipIfReady()
        {
            var body = NCombatRoom.Instance?.GetCreatureNode(Creature)?.Body;
            if (body != null)
            {
                body.Scale *= new Vector2(-1f, 1f);
            }
        }

        FlipIfReady();
        Callable.From(FlipIfReady).CallDeferred();
        return Task.CompletedTask;
    }

    private Task LightAttack(IReadOnlyList<Creature> targets)
    {
        return DamageCmd.Attack(LightDamage)
            .FromMonster(this)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
    }

    private Task HeavyAttack(IReadOnlyList<Creature> targets)
    {
        return DamageCmd.Attack(HeavyDamage)
            .FromMonster(this)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
    }

    private async Task EscapeMove(IReadOnlyList<Creature> targets)
    {
        _ = targets;
        if (Creature.CombatState?.Encounter is BasementGuardianEventEncounter encounter)
        {
            encounter.RanOutOfTime = true;
        }

        await CreatureCmd.Escape(Creature);
    }
}

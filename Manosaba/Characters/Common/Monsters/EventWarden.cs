using BaseLib.Abstracts;
using manosaba.Extensions;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace Manosaba.Characters.Common.Monsters;

/// <summary>「難道說？」事件 token 生成的敵方典獄長；數值比照小史萊姆。</summary>
public sealed class EventWarden : MonsterModel, ICustomModel
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 12, 11);

    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 16, 15);

    private int TackleDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Slime;

    protected override string VisualsPath => "event_warden.tscn".MonsterScenePath();

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack = new("TACKLE_MOVE", TackleMove, new SingleAttackIntent(TackleDamage));
        attack.FollowUpState = attack;
        return new MonsterMoveStateMachine([attack], attack);
    }

    private async Task TackleMove(IReadOnlyList<Creature> targets)
    {
        _ = targets;
        await DamageCmd.Attack(TackleDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.15f)
            .WithAttackerFx(null, AttackSfx)
            .WithHitFx("vfx/vfx_slime_impact")
            .Execute(null);
    }
}

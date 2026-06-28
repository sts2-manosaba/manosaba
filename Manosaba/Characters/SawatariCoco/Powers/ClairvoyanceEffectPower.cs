using BaseLib.Utils;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.SawatariCoco.Powers;

/// <summary>千里眼：回合結束時依敵人意圖獲得格擋或力量，並減少 1 層。</summary>
public sealed class ClairvoyanceEffectPower : PathCustomPowerModel
{
    private const decimal BlockGain = 5m;
    private const decimal StrengthGain = 1m;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != Owner.Side || !creatures.Contains(Owner) || Amount <= 0m)
        {
            return;
        }

        ICombatState? combatState = Owner.CombatState;
        if (combatState == null)
        {
            return;
        }

        bool anyEnemyIntendsAttack = combatState.GetOpponentsOf(Owner)
            .Any(enemy => enemy.IsAlive && enemy.Monster?.IntendsToAttack == true);

        if (anyEnemyIntendsAttack)
        {
            await CreatureCmd.GainBlock(Owner, BlockGain, ValueProp.Move, null);
        }
        else
        {
            await CommonActions.Apply<StrengthPower>(choiceContext, Owner, null, StrengthGain);
        }

        await CommonActions.Apply<ClairvoyanceEffectPower>(choiceContext, Owner, null, -1m);
    }
}

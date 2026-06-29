using BaseLib.Utils;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.SawatariCoco.Powers;

/// <summary>千里眼：獲得時與每回合開始依敵人意圖觸發效果，回合結束減少 1 層。</summary>
public sealed class ClairvoyanceEffectPower : PathCustomPowerModel
{
    private const decimal BlockGain = 5m;
    private const decimal StrengthGain = 1m;
    private const decimal VulnerableGain = 1m;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        _ = applier;
        _ = cardSource;

        await ApplyIntentEffectsAsync(new ThrowingPlayerChoiceContext());
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> creatures, ICombatState combatState)
    {
        _ = combatState;

        if (side != Owner.Side || Amount <= 0m || !creatures.Contains(Owner))
        {
            return;
        }

        await ApplyIntentEffectsAsync(new ThrowingPlayerChoiceContext());
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != Owner.Side || !creatures.Contains(Owner) || Amount <= 0m)
        {
            return;
        }

        await CommonActions.Apply<ClairvoyanceEffectPower>(choiceContext, Owner, null, -1m);
    }

    private async Task ApplyIntentEffectsAsync(PlayerChoiceContext choiceContext)
    {
        if (Amount <= 0m || Owner.CombatState is not { } combatState)
        {
            return;
        }

        decimal totalBlock = 0m;
        decimal totalStrength = 0m;

        foreach (Creature enemy in combatState.GetOpponentsOf(Owner).Where(creature => creature.IsAlive))
        {
            if (enemy.Monster is not { } monster)
            {
                continue;
            }

            foreach (AbstractIntent intent in monster.NextMove.Intents)
            {
                switch (intent.IntentType)
                {
                    case IntentType.Attack:
                    case IntentType.DeathBlow:
                        totalBlock += BlockGain;
                        break;
                    case IntentType.Debuff:
                    case IntentType.DebuffStrong:
                    case IntentType.CardDebuff:
                        await CommonActions.Apply<VulnerablePower>(choiceContext, enemy, null, VulnerableGain);
                        break;
                    default:
                        totalStrength += StrengthGain;
                        break;
                }
            }
        }

        if (totalBlock > 0m)
        {
            await CreatureCmd.GainBlock(Owner, totalBlock, ValueProp.Unpowered, null);
        }

        if (totalStrength > 0m)
        {
            await CommonActions.Apply<StrengthPower>(choiceContext, Owner, null, totalStrength);
        }
    }
}

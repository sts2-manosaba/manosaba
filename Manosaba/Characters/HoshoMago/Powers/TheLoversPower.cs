using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HoshoMago.Powers;

public sealed class TheLoversPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool IsInstanced => true;

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (target != Owner)
            return;

        if (!props.HasFlag(ValueProp.Move))
            return;

        if (result.UnblockedDamage <= 0)
            return;

        if (dealer != Applier || !Applier.IsAlive)
            return;

        decimal healAmount = result.UnblockedDamage * Amount / 100m;
        if (healAmount <= 0)
            return;

        await CreatureCmd.Heal(Applier, healAmount);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }
}

using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.Common.Powers;

public sealed class WallPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner)
            return;

        if (!props.HasFlag(ValueProp.Move))
            return;

        int attackedAmount = result.BlockedDamage + result.UnblockedDamage;
        if (attackedAmount <= 0)
            return;

        await PowerCmd.Apply<VigorPower>(Owner, attackedAmount, Owner, cardSource);
    }
}

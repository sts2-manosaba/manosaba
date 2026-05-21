using BaseLib.Utils;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HasumiLeia.Powers;

public sealed class CrowningPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        _ = choiceContext;
        _ = result;

        if (dealer != Owner)
        {
            return;
        }

        if (!props.IsPoweredAttack())
        {
            return;
        }

        if (target.Side == Owner.Side || !target.IsAlive)
        {
            return;
        }

        await CommonActions.Apply<TemporaryStrengthDownPower>(choiceContext, target, cardSource, Amount);
    }
}

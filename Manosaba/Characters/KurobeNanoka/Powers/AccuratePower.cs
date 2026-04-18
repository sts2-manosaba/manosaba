using Manosaba.Characters.KurobeNanoka.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.KurobeNanoka.Powers;

public sealed class AccuratePower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => Amount;
    public override bool AllowNegative => false;

    private static bool ShouldAffectDamage(ValueProp props)
    {
        return props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered);
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        _ = target;
        _ = amount;

        if (!ShouldAffectDamage(props))
        {
            return 1m;
        }

        if (dealer != Owner)
        {
            return 1m;
        }

        if (cardSource is not GunBase || cardSource.Type != CardType.Attack)
        {
            return 1m;
        }

        if (Amount < 1m)
        {
            return 1m;
        }

        return 1m + Amount;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player)
        {
            return;
        }

        if (cardPlay.Card is not GunBase || cardPlay.Card.Type != CardType.Attack)
        {
            return;
        }

        await PowerCmd.Remove(this);
    }
}

using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Manosaba.Characters.HasumiLeia.Powers;

public sealed class CrowningPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        _ = context;

        if (Owner?.Player == null || cardPlay.Card.Owner != Owner.Player)
        {
            return;
        }

        if (cardPlay.Card.Type != CardType.Attack)
        {
            return;
        }

        if (cardPlay.Target is not { } target || target.Side == Owner.Side || !target.IsAlive)
        {
            return;
        }

        await PowerCmd.Apply<TemporaryStrengthDownPower>(target, Amount, Owner, cardPlay.Card);
    }
}

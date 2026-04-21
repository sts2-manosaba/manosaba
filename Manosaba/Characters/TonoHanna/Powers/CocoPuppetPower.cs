using Manosaba.Characters.Common.Overrides;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TonoHanna.Powers;

public class CocoPuppetPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player)
            return;

        if (!cardPlay.Card.Tags.Contains(ManosabaCardTags.Puppet))
            return;

        if (Owner.CombatState == null)
            return;

        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move | ValueProp.Unpowered, cardPlay);
    }
}

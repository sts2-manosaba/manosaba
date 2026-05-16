using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.TonoHanna.Powers;

public sealed class LittleMatchGirlPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        _ = fromHandDraw;

        if (card.Owner != Owner.Player || card.Type != CardType.Status || Amount <= 0m)
        {
            return;
        }

        Flash();
        await CreatureCmd.Heal(Owner, Amount);
    }
}

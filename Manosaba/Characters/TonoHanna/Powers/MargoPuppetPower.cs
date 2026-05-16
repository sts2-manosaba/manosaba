using System.Linq;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.TonoHanna.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.TonoHanna.Powers
{
    public class MargoPuppetPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
        {
            if (side != Owner.Side || Owner.Player == null)
                return;

            for (int stack = 0; stack < (int)Amount; stack++)
            {
                CardPile pile = PileType.Discard.GetPile(Owner.Player);
                CardModel? card = pile.Cards
                    .Where(c => c.Tags.Contains(ManosabaCardTags.Puppet))
                    .ToList()
                    .UnstableShuffle(Owner.Player.RunState.Rng.CombatCardSelection)
                    .FirstOrDefault();

                if (card == null)
                    return;

                await CardPileCmd.Add(card, PileType.Hand);

                if (card is MeruruPuppet)
                {
                    await PowerCmd.Apply<MajokaPower>(Owner, 10m, Owner, null);
                }
            }
        }
    }
}

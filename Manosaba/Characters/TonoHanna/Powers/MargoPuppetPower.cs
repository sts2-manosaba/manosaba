using BaseLib.Utils;
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

using MegaCrit.Sts2.Core.Entities.Creatures;
namespace Manosaba.Characters.TonoHanna.Powers
{
    public class MargoPuppetPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> creatures, ICombatState combatState)
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
                    await CommonActions.Apply<MajokaPower>(choiceContext, Owner, null, 10m);
                }
            }
        }
    }
}

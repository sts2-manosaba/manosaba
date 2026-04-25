using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Manosaba.Characters.NikaidoHiro.Powers
{
    public class LaboursOfHiroPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;


        public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (cardPlay.Card.Type == CardType.Attack)
            {
                if (Owner.Player?.Creature is not { } ownerCreature)
                {
                    return;
                }

                await PowerCmd.Apply<MajokaPower>(ownerCreature, 10 * cardPlay.Card.EnergyCost.GetResolved(), ownerCreature, null);
            }
        }
    }
}

using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.NikaidoHiro.Powers
{
    public class LaboursOfHiroPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(3m)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

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

        public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
        {
            _ = choiceContext;
            _ = deathAnimLength;

            if (wasRemovalPrevented || Owner is not { IsAlive: true } || Owner.CombatState == null)
            {
                return;
            }

            if (creature.CombatState != Owner.CombatState || creature.Side != Owner.Side || creature == Owner)
            {
                return;
            }

            Flash();
            await PowerCmd.Apply<StrengthPower>(Owner, DynamicVars["StrengthPower"].BaseValue, Owner, null);
        }
    }
}

using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.Common.Powers
{
    public class MurderousImpulsePower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override bool AllowNegative => false;

        public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (base.Amount >= 1 && cardPlay.Card.Owner == base.Owner.Player && cardPlay.Card.Type == CardType.Attack && cardPlay.Card.DynamicVars.Damage.BaseValue > 0)
            {
                await Cmd.CustomScaledWait(0.1f, 0.2f);
                Creature creature = base.Owner.Player.RunState.Rng.CombatTargets.NextItem(base.Owner.CombatState.Allies);

                if (creature != null && creature != base.Owner.Player.Creature)
                {
                    await CreatureCmd.Damage(context, creature, cardPlay.Card.DynamicVars.Damage.BaseValue * base.Amount * 0.25m, ValueProp.Unpowered, Owner.Player.Creature);
                }
            }
        }
    }
}

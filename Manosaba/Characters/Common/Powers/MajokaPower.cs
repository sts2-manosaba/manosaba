using BaseLib.Abstracts;
using BaseLib.Extensions;
using manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.Common.Powers
{
    public class MajokaPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override bool AllowNegative => false;

        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (base.Owner != dealer)
            {
                return 1m;
            }

            return 1m + base.Amount / 100m;
        }

        public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (base.Amount >= 100 && cardPlay.Card.Owner == base.Owner.Player && cardPlay.Card.Type == CardType.Attack && cardPlay.Card.DynamicVars.Damage.BaseValue > 0)
            {
                await Cmd.CustomScaledWait(0.1f, 0.2f);
                Creature creature = base.Owner.Player.RunState.Rng.CombatTargets.NextItem(base.Owner.CombatState.Allies);

                if (creature != null)
                {
                    await CreatureCmd.Damage(context, creature, cardPlay.Card.DynamicVars.Damage, Owner.Player.Creature);
                }
            }

        }

        public override string CustomPackedIconPath => "Majoka.png".PowerImagePath();
        public override string CustomBigIconPath => "Majoka.png".PowerImagePath();
        public override string CustomBigBetaIconPath => "Majoka.png".PowerImagePath();
    }
}

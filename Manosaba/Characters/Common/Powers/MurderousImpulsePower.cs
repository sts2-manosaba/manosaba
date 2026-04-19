using Manosaba.Characters.HikamiMeruru.Powers;
using Manosaba.Multiplayer;
using Manosaba.Extensions;
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
    public class MurderousImpulsePower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override bool AllowNegative => false;

        public override async Task AfterAttack(AttackCommand command)
        {
            if (base.Amount < 1 || Owner.GetPowerAmount<InhibitionPower>() >= 1)
            {
                return;
            }

            if (command.Attacker != Owner || command.ModelSource is not CardModel card || card.Owner != Owner.Player || card.Type != CardType.Attack)
            {
                return;
            }

            int totalDamage = command.Results.Sum(result => result.TotalDamage);
            if (totalDamage <= 0)
            {
                return;
            }

            Creature[] validAllies = Owner.CombatState.Allies
                .Where(c => c != Owner && !c.IsDead)
                .ToArray();
            if (validAllies.Length == 0)
            {
                return;
            }

            decimal perStackMultiplier = ManosabaLobbyDifficultyState.GetMurderousImpulseAllyDamageMultiplierForGameplay();
            decimal allyDamage = totalDamage * base.Amount * perStackMultiplier;
            if (allyDamage <= 0m)
            {
                return;
            }

            await Cmd.CustomScaledWait(0.1f, 0.2f);
            Creature ally = base.Owner.Player.RunState.Rng.CombatTargets.NextItem(validAllies);
            await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), ally, allyDamage, ValueProp.Unpowered, Owner.Player.Creature);
        }
    }
}

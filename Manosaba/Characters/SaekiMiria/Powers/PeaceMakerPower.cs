using BaseLib.Utils;
using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.SaekiMiria.Cards;
using Manosaba.Characters.SaekiMiria.Helper;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using System;

namespace Manosaba.Characters.SaekiMiria.Powers
{
    public sealed class PeacemakerPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;


        /*public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
        {
            if (player.Creature != Owner)
                return;
            
            List<Creature> enemies = combatState.GetOpponentsOf(player.Creature).ToList();
            foreach (Creature creature in enemies)
            {
                await PowerCmd.Apply<StrengthPower>(creature, -Amount, player.Creature, null);
            }

            
        }*/

        public override decimal ModifyPowerAmountGiven(
            PowerModel power,
            Creature giver,
            decimal amount,
            Creature? target,
            CardModel? cardSource)
        {
            if (power is StrengthPower && target != null && target.IsMonster && amount > 0)
            {
                return Math.Max(0, amount - Amount);
            }

            return amount;
        }
    }
}
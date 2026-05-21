using Godot;
using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HasumiLeia.Powers
{
    public class UnlimitedSpearWorkPower : PathCustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override bool AllowNegative => false;



        //After player turn start, gain 1 Forge Spear in hand
        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player != Owner.Player || player.Creature?.CombatState is not { } combatState)
                return;
            List<ForgeSpear> list = new List<ForgeSpear>();
            for (int i = 0; i < Amount; i++)
            {
                var card = combatState.CreateCard<ForgeSpear>(player);
                card.AddKeyword(CardKeyword.Ethereal);
                list.Add(card);
            }

            await CardPileCmd.AddGeneratedCardsToCombat(list, PileType.Hand, Owner.Player);
        }
    }
}

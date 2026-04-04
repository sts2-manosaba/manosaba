using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace Manosaba.Characters.SaekiMiria.Helper
{
    internal class CardHelperService
    {
        public static IEnumerable<CardModel> GetAvailableCards(Player player, IEnumerable<CardModel> cards, int count, Rng rng)
        {
            Player player2 = player;
            List<CardModel> list = TestRngInjector.ConsumeCombatCardGenerationOverride();
            if (list != null)
            {
                return list;
            }

            return from c in cards.TakeRandom(count, rng)
                   select player2.Creature.CombatState.CreateCard(c, player2);
        }
    }
}

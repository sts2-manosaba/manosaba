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
            bool isMultiplayer = player.RunState.Players.Count > 1;
            IEnumerable<CardModel> availableCards = isMultiplayer
                ? cards
                : cards.Where(c => c.MultiplayerConstraint != CardMultiplayerConstraint.MultiplayerOnly);

            return from c in availableCards.TakeRandom(count, rng)
                   select player.Creature.CombatState.CreateCard(c, player);
        }
    }
}

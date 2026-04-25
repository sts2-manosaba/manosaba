using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace Manosaba.Characters.SaekiMiria.Helper
{
    internal class CardHelperService
    {
        public static IEnumerable<CardModel> GetAvailableCards(Player player, IEnumerable<CardModel> cards, int count, Rng rng)
        {
            if (player.Creature?.CombatState is not { } combatState)
            {
                return [];
            }

            bool isMultiplayer = player.RunState.Players.Count > 1;
            IEnumerable<CardModel> availableCards = cards
                .Where(c => !MiriaConstants.IsIgnoredCard(c));

            if (!isMultiplayer)
            {
                availableCards = availableCards
                    .Where(c => c.MultiplayerConstraint != CardMultiplayerConstraint.MultiplayerOnly);
            }

            return from c in availableCards.TakeRandom(count, rng)
                   select combatState.CreateCard(c, player);
        }
    }
}

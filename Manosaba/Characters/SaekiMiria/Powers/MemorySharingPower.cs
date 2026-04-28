using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.SaekiMiria.Helper;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.SaekiMiria.Powers
{
    public sealed class MemorySharingPower : PathCustomPowerModel
    {
        private Creature? _applier;
        private bool _upgraded;

        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override bool IsInstanced => true;

        public void SetApplier(Creature applier)
        {
            _applier = applier;
        }

        public void SetUpgraded(bool upgraded)
        {
            _upgraded = upgraded;
        }

        public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
        {
            if (player.Creature != Owner)
                return;

            if (base.Owner.Player is not { } ownerPlayer)
            {
                return;
            }

            Creature? applier = _applier;
            if (applier == null)
            {
                return;
            }

            decimal majoka = applier.GetPowerAmount<MajokaPower>();
            Console.WriteLine($"Majoka amount: {majoka}");

            List<CardModel> pool = new List<CardModel>();

            IEnumerable<Player> enumerable = from c in base.CombatState.GetTeammatesOf(base.Owner)
                                             where c != null && c.IsAlive && c.IsPlayer && c.Player != null
                                             select c.Player;

            foreach (Player item in enumerable)
            {
                var playerPool = item.Character.CardPool.AllCards;
                if (majoka < 100)
                {
                    var playerDeck = CardPile.GetCards(item, PileType.Deck);
                    var deckIds = playerDeck
                        .Select(c => c.Id)
                        .ToHashSet();
                    playerPool = playerPool.Where(c => deckIds.Contains(c.Id)).ToList();
                }
                pool.AddRange(playerPool);
            }

            HashSet<Type> IgnoredCards = MiriaConstants.IgnoredCards;
            //filter out quest and status cards, and token rarity cards
            pool = pool
                .Where(c => c.Type != CardType.Quest && c.Type != CardType.Status)
                .Where(c => c.Rarity != CardRarity.Ancient && c.Rarity != CardRarity.Token)
                .Where(c => !IgnoredCards.Contains(c.GetType()))
                .ToList();

            if (majoka < 30)
            {
                pool = pool.Where(c => c.Rarity == CardRarity.Basic || c.Rarity == CardRarity.Common).ToList();
            }
            else if (majoka < 70)
            {
                pool = pool.Where(c => c.Rarity == CardRarity.Basic || c.Rarity == CardRarity.Common || c.Rarity == CardRarity.Uncommon).ToList();
            }
            else
            {
                pool = pool.Where(c => c.Rarity == CardRarity.Basic || c.Rarity == CardRarity.Common || c.Rarity == CardRarity.Uncommon || c.Rarity == CardRarity.Rare).ToList();
            }


            //List<CardModel> cards = CardFactory.GetDistinctForCombat(player, pool, 1, player.RunState.Rng.CombatCardGeneration).ToList();


            var generatedList = CardHelperService
                .GetAvailableCards(ownerPlayer, pool, 1, ownerPlayer.RunState.Rng.CombatCardGeneration)
                .ToList();

            var card = generatedList.FirstOrDefault();

            if (card != null)
            {
                if (_upgraded)
                {
                    CardCmd.Upgrade(card);
                }
                var copy = card.CreateClone();

                if (majoka < 150)
                {
                    copy.AddKeyword(CardKeyword.Exhaust);

                }
                if (majoka < 200)
                {
                    copy.AddKeyword(CardKeyword.Ethereal);
                }
                copy.AddKeyword(ManosabaKeywords.Shared);

                await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, addedByPlayer: true);

            }


        }
    }
}

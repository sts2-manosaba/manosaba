using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.NikaidoHiro.Powers;
using Manosaba.Characters.SaekiMiria.Helper;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.SaekiMiria.Cards
{
    [Pool(typeof(SaekiMiriaCardPool))]
    public class Exchange : PathCustomCardModel
    {
        private const int energyCost = 0;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Basic;
        private const TargetType targetType = TargetType.AnyAlly;
        private const bool shouldShowInCardLibrary = true;
        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust), HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [];
        public Exchange() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            /*if (cardPlay.Target.Player == base.Owner) {

                await CardPileCmd.Draw(choiceContext, 1, base.Owner);
                return;
            }*/

            if (cardPlay.Target?.Player is not { } target)
            {
                return;
            }

            HashSet<Type> IgnoredCards = MiriaConstants.IgnoredCards;

            var pool = target.Character.CardPool.AllCards;
            var deck = CardPile.GetCards(target, PileType.Deck);


            var deckIds = deck
                .Select(c => c.Id)
                .ToHashSet();

            
            var cards = pool
                .Where(c => c.Type != CardType.Quest && c.Type != CardType.Status)
                .Where(c => c.Rarity != CardRarity.Ancient && c.Rarity != CardRarity.Token)
                .Where(c => deckIds.Contains(c.Id))
                .Where(c => !IgnoredCards.Contains(c.GetType()))
                .ToList();


            if (!base.IsUpgraded)
            {
                cards = cards
                    .Where(c => c.Rarity != CardRarity.Rare)
                    .ToList();
            }


            var generatedList = CardHelperService
                .GetAvailableCards(base.Owner, cards, 1, base.Owner.RunState.Rng.CombatCardGeneration)
                .ToList();

            var card = generatedList.FirstOrDefault();
            
            if (card != null)
            {
                var copy = card.CreateClone(); 

                //copy.SetToFreeThisTurn();
                copy.AddKeyword(CardKeyword.Exhaust);
                copy.AddKeyword(CardKeyword.Ethereal);

                await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, addedByPlayer: true);
            }


            var pool2 = base.Owner.Character.CardPool.AllCards;
            var deck2 = CardPile.GetCards(Owner, PileType.Deck);

            var deckIds2 = deck2
                .Select(c => c.Id)
                .ToHashSet();

            var cards2 = pool2
                .Where(c => c.Type != CardType.Quest && c.Type != CardType.Status)
                .Where(c => c.Rarity != CardRarity.Ancient && c.Rarity != CardRarity.Token)
                .Where(c => deckIds2.Contains(c.Id))
                .Where(c => !IgnoredCards.Contains(c.GetType()))
                .ToList();

            if (!base.IsUpgraded)
            {
                cards2 = cards2
                    .Where(c => c.Rarity != CardRarity.Rare)
                    .ToList();
            }

            var generatedList2 = CardHelperService
                .GetAvailableCards(target, cards2, 1, target.RunState.Rng.CombatCardGeneration)
                .ToList();

            var card2 = generatedList2.FirstOrDefault(); 
            if (card2 != null)
            {
                var copy2 = card2.CreateClone(); 

                //copy2.SetToFreeThisTurn();
                copy2.AddKeyword(CardKeyword.Exhaust);
                copy2.AddKeyword(CardKeyword.Ethereal);

                await CardPileCmd.AddGeneratedCardToCombat(copy2, PileType.Hand, addedByPlayer: true);
            }
        }


        protected override void OnUpgrade()
        {
            
        }
    }
}

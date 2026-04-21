using BaseLib.Utils;
using Manosaba.Characters.SaekiMiria.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using System;
using System.Reflection;
using Manosaba.Characters.SaekiMiria.Powers;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.SaekiMiria.Relics
{

    [Pool(typeof(SaekiMiriaRelicPool))]
    public sealed class CabinetKey : LevelingPathCustomRelicModel
    {
        private const int MAX_MOVIE_CARD_GENERATED = 10;
        private const int SMALL_PAPER_CHANCE = 1000;

        public override RelicRarity Rarity => RelicRarity.Starter;
        protected override int MaxRelicLevel => 5;

        private int blockPerMovieCard = 8;

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new DynamicVar("BlockPerMovie", 8m),
            new DynamicVar("MaxMovieCards", MAX_MOVIE_CARD_GENERATED),
        ];

        private static readonly IReadOnlyList<Func<Player, CombatState, MovieBase>> MovieFactories =
        [
            static (player, combatState) => combatState.CreateCard<HorrorMovie>(player),
            static (player, combatState) => combatState.CreateCard<ComedyMovie>(player),
            static (player, combatState) => combatState.CreateCard<CassetteShapedRock>(player),
            static (player, combatState) => combatState.CreateCard<FantasyMovie>(player),
            static (player, combatState) => combatState.CreateCard<ActionMovie>(player),
            static (player, combatState) => combatState.CreateCard<RomanticMovie>(player),
            static (player, combatState) => combatState.CreateCard<SpyMovie>(player),
        ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            .. base.ExtraHoverTips,
            HoverTipFactory.FromCard<HorrorMovie>(),
            HoverTipFactory.FromCard<ComedyMovie>(),
            HoverTipFactory.FromCard<CassetteShapedRock>(),
            HoverTipFactory.FromCard<FantasyMovie>(),
            HoverTipFactory.FromCard<ActionMovie>(),
            HoverTipFactory.FromCard<RomanticMovie>(),
            HoverTipFactory.FromCard<SpyMovie>()
        ];

        public override Task AfterObtained()
        {
            ApplyRelicLevelEffects();
            RemoveExchangeFromSinglePlayerStartingDeck();

            return Task.CompletedTask;
        }

        public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
        {
            if (side != CombatSide.Enemy)
                return;

            if (!Owner.Creature.IsAlive)
                return;

            CombatState? combatState = Owner.Creature.CombatState;
            if (combatState == null)
                return;

            int requiredBlock = Math.Max(1, blockPerMovieCard);
            decimal blockRemaining = GetCreatureBlock(Owner.Creature);
            if (blockRemaining <= 0m)
                return;
            int moviesToAdd = (int)decimal.Ceiling(blockRemaining / requiredBlock);
            moviesToAdd = Math.Min(moviesToAdd, MAX_MOVIE_CARD_GENERATED);
            if (moviesToAdd <= 0)
                return;

            Flash();
            var rng = combatState.RunState.Rng.CombatCardSelection;
            List<CardModel> generatedCards = new(moviesToAdd);
            for (int i = 0; i < moviesToAdd; i++)
            {
                generatedCards.Add(CreateRelicGeneratedCard(Owner, combatState, rng));
            }

            Player? invitedPlayer = Owner.Creature.GetPower<MovieInvitationPower>()?.ConsumeInvitedPlayerForRelicMovies();
            if (invitedPlayer != null)
            {
                List<CardModel> invitedCards = new(generatedCards.Count);
                foreach (CardModel card in generatedCards)
                {
                    invitedCards.Add(CreateRelicGeneratedCardCopyForPlayer(card, invitedPlayer, combatState));
                }

                IReadOnlyList<CardPileAddResult> invitedResults = await CardPileCmd.AddGeneratedCardsToCombat(
                    invitedCards,
                    PileType.Draw,
                    addedByPlayer: true,
                    CardPilePosition.Random);
                if (LocalContext.IsMe(invitedPlayer.Creature))
                {
                    CardCmd.PreviewCardPileAdd(invitedResults);
                }
            }

            IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(
                generatedCards,
                PileType.Draw,
                addedByPlayer: true,
                CardPilePosition.Random);
            CardCmd.PreviewCardPileAdd(results);
        }



        protected override void OnRelicLevelChanged(int oldLevel, int newLevel)
        {
            ApplyRelicLevelEffects();
        }

        private void ApplyRelicLevelEffects()
        {
            // Lv1: 8 Block per card
            // Lv2: 7
            // Lv3: 6
            // Lv4: 5
            // Lv5: 4
            blockPerMovieCard = Math.Max(1, 8 - RelicLevel);
            DynamicVars["BlockPerMovie"].BaseValue = blockPerMovieCard;
        }

        private static CardModel CreateRelicGeneratedCard(Player player, CombatState combatState, MegaCrit.Sts2.Core.Random.Rng rng)
        {
            // Very small chance to generate the secret card Small Paper 
            if (rng.NextInt(SMALL_PAPER_CHANCE) == 0)
            {
                return combatState.CreateCard<SmallPaper>(player);
            }

            return rng.NextItem(MovieFactories)(player, combatState);
        }

        private static CardModel CreateRelicGeneratedCardCopyForPlayer(CardModel card, Player player, CombatState combatState) =>
            card switch
            {
                HorrorMovie => combatState.CreateCard<HorrorMovie>(player),
                ComedyMovie => combatState.CreateCard<ComedyMovie>(player),
                CassetteShapedRock => combatState.CreateCard<CassetteShapedRock>(player),
                FantasyMovie => combatState.CreateCard<FantasyMovie>(player),
                ActionMovie => combatState.CreateCard<ActionMovie>(player),
                RomanticMovie => combatState.CreateCard<RomanticMovie>(player),
                SpyMovie => combatState.CreateCard<SpyMovie>(player),
                SmallPaper => combatState.CreateCard<SmallPaper>(player),
                _ => throw new InvalidOperationException($"Unsupported Cabinet Key copy: {card.GetType().Name}")
            };

        private static decimal GetCreatureBlock(Creature creature)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type type = creature.GetType();

            foreach (string name in new[] { "Block", "CurrentBlock", "BlockAmount" })
            {
                PropertyInfo? prop = type.GetProperty(name, flags);
                if (prop != null)
                {
                    object? value = prop.GetValue(creature);
                    return value == null ? 0m : Convert.ToDecimal(value);
                }

                FieldInfo? field = type.GetField(name, flags);
                if (field != null)
                {
                    object? value = field.GetValue(creature);
                    return value == null ? 0m : Convert.ToDecimal(value);
                }
            }

            return 0m;
        }

        private void RemoveExchangeFromSinglePlayerStartingDeck()
        {
            if (Owner.RunState.Players.Count > 1 || Owner.Deck?.Cards == null)
                return;

            List<CardModel> exchangeCards = Owner.Deck.Cards
                .Where(card => card is Exchange)
                .ToList();

            foreach (CardModel exchangeCard in exchangeCards)
            {
                Owner.Deck.RemoveInternal(exchangeCard);
            }
        }
    }
}

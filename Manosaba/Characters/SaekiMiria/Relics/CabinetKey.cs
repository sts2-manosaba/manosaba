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
using Manosaba.Characters.SaekiMiria.Powers;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models;
using Manosaba.Characters.SaekiMiria.Helper;

namespace manosaba.Characters.SaekiMiria.Relics
{

    [Pool(typeof(SaekiMiriaRelicPool))]
    public sealed class CabinetKey : LevelingPathCustomRelicModel
    {
        private const int MAX_MOVIE_CARD_GENERATED = 10;
        private const int SMALL_PAPER_CHANCE = 100;
        private const string BlockPerMovieVar = "BlockPerMovie";
        private const string MaxMovieCardsVar = "MaxMovieCards";
        private const string MoviesPerTurnVar = "MoviesPerTurn";
        private const string CassetteChanceVar = "CassetteChance";

        public override RelicRarity Rarity => RelicRarity.Starter;
        protected override int MaxRelicLevel => 5;

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new DynamicVar(BlockPerMovieVar, 8m),
            new DynamicVar(MaxMovieCardsVar, MAX_MOVIE_CARD_GENERATED),
            new DynamicVar(MoviesPerTurnVar, 1m),
            new DynamicVar(CassetteChanceVar, 0m),
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
            AdjustStartingDeckForRunMode();

            return Task.CompletedTask;
        }

        public override Task BeforeCombatStart()
        {
            ApplyRelicLevelEffects();
            return Task.CompletedTask;
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (Owner != player || !Owner.Creature.IsAlive)
                return;

            ICombatState? combatState = Owner.Creature.CombatState;
            if (combatState == null)
                return;

            int moviesToAdd = DynamicVars[MoviesPerTurnVar].IntValue;
            moviesToAdd += (int)(Owner.Creature.GetPower<CabinetExpansionPower>()?.Amount ?? 0m);
            if (moviesToAdd <= 0)
                return;

            Flash();
            var rng = combatState.RunState.Rng.CombatCardSelection;
            List<CardModel> generatedCards = new(moviesToAdd);
            for (int i = 0; i < moviesToAdd; i++)
            {
                generatedCards.Add(await CreateRelicGeneratedCard(Owner, combatState, rng, ShouldUpgradeMovieCards()));
            }

            if (RelicLevel >= 3 && rng.NextInt(100) < DynamicVars[CassetteChanceVar].IntValue)
            {
                CardModel cassette = combatState.CreateCard<CassetteShapedRock>(Owner);
                await MovieCardGenerator.TryApplyAbsoluteCinemaAsync(Owner, cassette);
                generatedCards.Add(cassette);
            }

            Player? invitedPlayer = Owner.Creature.GetPower<MovieInvitationPower>()?.ConsumeInvitedPlayerForRelicMovies();
            if (invitedPlayer != null)
            {
                List<CardModel> invitedCards = new(generatedCards.Count);
                foreach (CardModel card in generatedCards)
                {
                    invitedCards.Add(await CreateRelicGeneratedCardCopyForPlayerAsync(card, invitedPlayer, combatState));
                }

                IReadOnlyList<CardPileAddResult> invitedResults = await CardPileCmd.AddGeneratedCardsToCombat(invitedCards, PileType.Draw, invitedPlayer, CardPilePosition.Random);
                if (LocalContext.IsMe(invitedPlayer.Creature))
                {
                    CardCmd.PreviewCardPileAdd(invitedResults);
                }
            }

            IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(generatedCards, PileType.Draw, Owner, CardPilePosition.Random);
            CardCmd.PreviewCardPileAdd(results);
        }

        [Obsolete("Deprecated block-scaling Cabinet Key behavior retained for future reference.")]
        private async Task DeprecatedBeforeTurnEndBlockScalingEffect(PlayerChoiceContext choiceContext, CombatSide side)
        {
            if (side != CombatSide.Enemy)
                return;

            if (Owner.Creature is not { } ownerCreature || !ownerCreature.IsAlive)
                return;

            ICombatState? combatState = ownerCreature.CombatState;
            if (combatState == null)
                return;

            int requiredBlock = GetDeprecatedBlockPerMovieCard();
            int blockRemaining = ownerCreature.Block;
            if (blockRemaining <= 0)
            {
                return;
            }
            int moviesToAdd = (int)decimal.Ceiling((decimal)blockRemaining / requiredBlock);
            moviesToAdd = Math.Min(moviesToAdd, DynamicVars[MaxMovieCardsVar].IntValue);
            if (moviesToAdd <= 0)
                return;

            Flash();
            var rng = combatState.RunState.Rng.CombatCardSelection;
            List<CardModel> generatedCards = new(moviesToAdd);
            for (int i = 0; i < moviesToAdd; i++)
            {
                generatedCards.Add(await CreateRelicGeneratedCard(Owner, combatState, rng, ShouldUpgradeMovieCards()));
            }

            if (RelicLevel >= 3 && rng.NextInt(100) < DynamicVars[CassetteChanceVar].IntValue)
            {
                CardModel cassette = combatState.CreateCard<CassetteShapedRock>(Owner);
                await MovieCardGenerator.TryApplyAbsoluteCinemaAsync(Owner, cassette);
                generatedCards.Add(cassette);
            }

            Player? invitedPlayer = Owner.Creature.GetPower<MovieInvitationPower>()?.ConsumeInvitedPlayerForRelicMovies();
            if (invitedPlayer != null)
            {
                List<CardModel> invitedCards = new(generatedCards.Count);
                foreach (CardModel card in generatedCards)
                {
                    invitedCards.Add(await CreateRelicGeneratedCardCopyForPlayerAsync(card, invitedPlayer, combatState));
                }

                IReadOnlyList<CardPileAddResult> invitedResults = await CardPileCmd.AddGeneratedCardsToCombat(invitedCards, PileType.Draw, invitedPlayer, CardPilePosition.Random);
                if (invitedPlayer.Creature != null && LocalContext.IsMe(invitedPlayer.Creature))
                {
                    CardCmd.PreviewCardPileAdd(invitedResults);
                }
            }

            IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(generatedCards, PileType.Draw, Owner, CardPilePosition.Random);
            CardCmd.PreviewCardPileAdd(results);
        }

        protected override void OnRelicLevelChanged(int oldLevel, int newLevel)
        {
            ApplyRelicLevelEffects();
        }

        private void ApplyRelicLevelEffects()
        {
            DynamicVars[BlockPerMovieVar].BaseValue = GetDeprecatedBlockPerMovieCard();
            DynamicVars[MaxMovieCardsVar].BaseValue = MAX_MOVIE_CARD_GENERATED;
            DynamicVars[MoviesPerTurnVar].BaseValue = GetMoviesPerTurn();
            DynamicVars[CassetteChanceVar].BaseValue = GetCassetteChancePercent();
        }

        private int GetDeprecatedBlockPerMovieCard()
        {
            return Math.Max(1, 8 - RelicLevel);
        }

        private int GetMoviesPerTurn()
        {
            return RelicLevel >= 5 ? 2 : 1;
        }

        private int GetCassetteChancePercent()
        {
            return RelicLevel >= 3 ? 50 : 0;
        }

        private bool ShouldUpgradeMovieCards()
        {
            return RelicLevel >= 4;
        }

        private static Task<CardModel> CreateRelicGeneratedCard(Player player, ICombatState combatState, MegaCrit.Sts2.Core.Random.Rng rng, bool upgradeMovies)
            => MovieCardGenerator.CreateCabinetKeyMovieAsync(
                player,
                combatState,
                rng,
                smallPaperChance: SMALL_PAPER_CHANCE,
                upgradeMovie: upgradeMovies,
                applyAbsoluteCinema: true);

        private static async Task<CardModel> CreateRelicGeneratedCardCopyForPlayerAsync(CardModel card, Player player, ICombatState combatState)
        {
            CardModel copy = card switch
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

            if (card.IsUpgraded)
            {
                CardCmd.Upgrade(copy);
            }

            await MovieCardGenerator.TryApplyAbsoluteCinemaAsync(player, copy);
            return copy;
        }

        private void AdjustStartingDeckForRunMode()
        {
            if (Owner.Deck?.Cards == null)
                return;

            if (Owner.RunState.Players.Count <= 1)
            {
                foreach (CardModel exchangeCard in Owner.Deck.Cards.Where(card => card is Exchange).ToList())
                {
                    Owner.Deck.RemoveInternal(exchangeCard);
                }

                foreach (CardModel socialCard in Owner.Deck.Cards.Where(card => card is Social).ToList())
                {
                    Owner.Deck.RemoveInternal(socialCard);
                }
                return;
            }

            CardModel? defendCard = Owner.Deck.Cards.FirstOrDefault(card => card is DefendSaekiMiria);
            if (defendCard != null)
            {
                Owner.Deck.RemoveInternal(defendCard);
            }

            CardModel? mindShardCard = Owner.Deck.Cards.FirstOrDefault(card => card is MindShard);
            if (mindShardCard != null)
            {
                Owner.Deck.RemoveInternal(mindShardCard);
            }
        }
    }
}

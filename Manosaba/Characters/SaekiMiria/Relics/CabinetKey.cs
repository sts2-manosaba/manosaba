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
using MegaCrit.Sts2.Core.Models;
using System;
using System.Reflection;

namespace manosaba.Characters.SaekiMiria.Relics
{

    [Pool(typeof(SaekiMiriaRelicPool))]
    public sealed class CabinetKey : LevelingPathCustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Starter;
        protected override int MaxRelicLevel => 5;

        private int blockPerMovieCard = 9;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("BlockPerMovie", 9m)];

        private static readonly IReadOnlyList<Func<Player, CombatState, MovieBase>> MovieFactories =
        [
            static (player, combatState) => combatState.CreateCard<HorrorMovie>(player),
            static (player, combatState) => combatState.CreateCard<ComedyMovie>(player),
            static (player, combatState) => combatState.CreateCard<FantasyMovie>(player),
            static (player, combatState) => combatState.CreateCard<ActionMovie>(player),
            static (player, combatState) => combatState.CreateCard<RomanticMovie>(player),
            static (player, combatState) => combatState.CreateCard<SpyMovie>(player),
        ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromCard<HorrorMovie>(),
            HoverTipFactory.FromCard<ComedyMovie>(),
            HoverTipFactory.FromCard<FantasyMovie>(),
            HoverTipFactory.FromCard<ActionMovie>(),
            HoverTipFactory.FromCard<RomanticMovie>(),
            HoverTipFactory.FromCard<SpyMovie>(),
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
            if (moviesToAdd <= 0)
                return;

            Flash();
            var rng = Owner.RunState.Rng.CombatCardSelection;
            List<MovieBase> movies = new(moviesToAdd);
            for (int i = 0; i < moviesToAdd; i++)
            {
                movies.Add(rng.NextItem(MovieFactories)(Owner, combatState));
            }

            IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(
                movies,
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
            // Lv1: 9 Block per card
            // Lv2: 8
            // Lv3: 7
            // Lv4: 6
            // Lv5: 5
            blockPerMovieCard = Math.Max(1, 10 - RelicLevel);
            DynamicVars["BlockPerMovie"].BaseValue = blockPerMovieCard;
        }

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

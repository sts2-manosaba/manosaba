using System;
using System.Collections.Generic;
using System.Linq;
using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.TonoHanna.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Random;

namespace Manosaba.Characters.TonoHanna.Cards
{
    [Pool(typeof(TonoHannaCardPool))]
    public class MiriaPuppet : PathCustomCardModel
    {
        protected override HashSet<CardTag> CanonicalTags => [ManosabaCardTags.Puppet];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Transform)];

        private const int energyCost = 0;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

        public MiriaPuppet() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (Owner?.Creature is not { } ownerCreature)
            {
                return;
            }

            await CommonActions.Apply<MiriaPuppetCollectionPower>(choiceContext, ownerCreature, this, 1m);

            int handCount = PileType.Hand.GetPile(Owner).Cards.Count;
            int maxSelect = Math.Min(DynamicVars.Cards.IntValue, handCount);
            if (maxSelect <= 0)
            {
                return;
            }

            IEnumerable<CardModel> selected = await CardSelectCmd.FromHand(
                prefs: new CardSelectorPrefs(SelectionScreenPrompt, 0, maxSelect),
                context: choiceContext,
                player: Owner,
                filter: null,
                source: this);

            Rng rng = Owner.RunState.Rng.CombatCardSelection;
            foreach (CardModel card in selected.ToList())
            {
                CardModel? transformed = null;

                if (!IsEligibleSourceTypeForPuppetTransform(card))
                {
                    transformed = (await CardCmd.TransformToRandom(card, rng)).cardAdded;
                }
                else
                {
                    List<CardModel> puppetPool = BuildMiriaPuppetTransformPool(card);

                    if (puppetPool.Count == 0)
                    {
                        transformed = (await CardCmd.TransformToRandom(card, rng)).cardAdded;
                    }
                    else
                    {
                        try
                        {
                            CardTransformation transformation = new(card, puppetPool);
                            transformed = (await CardCmd.Transform(transformation.Yield(), rng, CardPreviewStyle.HorizontalLayout))
                                .FirstOrDefault()
                                .cardAdded;
                        }
                        catch (InvalidOperationException)
                        {
                            transformed = (await CardCmd.TransformToRandom(card, rng)).cardAdded;
                        }
                    }
                }

                if (IsUpgraded && transformed != null)
                {
                    CardCmd.Upgrade(transformed);
                }
            }
        }

        /// <summary>Only main-deck card types may be forced toward puppet outcomes; curses/status/etc. use vanilla transform.</summary>
        private static bool IsEligibleSourceTypeForPuppetTransform(CardModel card) =>
            card.Type is CardType.Attack or CardType.Skill or CardType.Power;

        /// <summary>
        /// Puppets tagged in <see cref="CardFactory.GetDefaultTransformationOptions"/> plus Hanna-pool fallback, merged by id.
        /// Vanilla transform can throw for some Hosho <see cref="CardRarity.Basic"/> sources; fallback keeps outcomes deterministic for multiplayer.
        /// </summary>
        private static List<CardModel> BuildMiriaPuppetTransformPool(CardModel original)
        {
            List<CardModel> fromDefault = [];
            try
            {
                fromDefault = CardFactory.GetDefaultTransformationOptions(original, isInCombat: true)
                    .Where(c => c.Tags.Contains(ManosabaCardTags.Puppet))
                    .ToList();
            }
            catch (InvalidOperationException)
            {
                // e.g. Hosho Basic + empty vanilla filter after ManosabaTransformHelper tarot branch misses
            }

            return fromDefault
                .Concat(BuildFallbackPuppetTransformPool(original))
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .ToList();
        }

        /// <summary>
        /// Default transform options often omit puppets; Hanna puppet cards always come from
        /// <see cref="TonoHannaCardPool"/> so prismatic / non-Hanna players can still roll a puppet outcome.
        /// </summary>
        private static List<CardModel> BuildFallbackPuppetTransformPool(CardModel original)
        {
            Player? player = original.Owner;
            if (player == null || original.RunState == null)
            {
                return [];
            }

            // Puppets are defined on Hanna's pool; prismatic / other characters still need this list for MiriaPuppet.
            IEnumerable<CardModel> q = ModelDb.CardPool<TonoHannaCardPool>()
                .GetUnlockedCards(player.UnlockState, original.RunState.CardMultiplayerConstraint)
                .Where(c => c.Tags.Contains(ManosabaCardTags.Puppet))
                .Where(c => c.Id != original.Id)
                .Where(c => original.RunState.Players.Count > 1
                    ? c.MultiplayerConstraint != CardMultiplayerConstraint.SingleplayerOnly
                    : c.MultiplayerConstraint != CardMultiplayerConstraint.MultiplayerOnly);

            return q.GroupBy(c => c.Id).Select(g => g.First()).ToList();
        }

        protected override void OnUpgrade()
        {
        }
    }
}

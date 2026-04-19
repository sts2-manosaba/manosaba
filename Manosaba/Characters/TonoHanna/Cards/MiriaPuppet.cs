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

        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

        public MiriaPuppet() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<MiriaPuppetCollectionPower>(Owner.Creature, 1m, Owner.Creature, this);
            var selected = await CardSelectCmd.FromHand(
                prefs: new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, DynamicVars.Cards.IntValue),
                context: choiceContext,
                player: Owner,
                filter: null,
                source: this);

            Rng rng = Owner.RunState.Rng.CombatCardSelection;
            foreach (CardModel card in selected.ToList())
            {
                if (!IsEligibleSourceTypeForPuppetTransform(card))
                {
                    await CardCmd.TransformToRandom(card, rng);
                    continue;
                }

                List<CardModel> puppetPool = CardFactory.GetDefaultTransformationOptions(card, isInCombat: true)
                    .Where(c => c.Tags.Contains(ManosabaCardTags.Puppet))
                    .ToList();
                if (puppetPool.Count == 0)
                {
                    puppetPool = BuildFallbackPuppetTransformPool(card);
                }

                if (puppetPool.Count == 0)
                {
                    await CardCmd.TransformToRandom(card, rng);
                }
                else
                {
                    try
                    {
                        CardTransformation transformation = new(card, puppetPool);
                        await CardCmd.Transform(transformation.Yield(), rng, CardPreviewStyle.HorizontalLayout);
                    }
                    catch (InvalidOperationException)
                    {
                        await CardCmd.TransformToRandom(card, rng);
                    }
                }
            }
        }

        /// <summary>Only main-deck card types may be forced toward puppet outcomes; curses/status/etc. use vanilla transform.</summary>
        private static bool IsEligibleSourceTypeForPuppetTransform(CardModel card) =>
            card.Type is CardType.Attack or CardType.Skill or CardType.Power;

        /// <summary>
        /// Default transform options often omit puppets; Hanna puppet cards always come from
        /// <see cref="TonoHannaCardPool"/> so prismatic / non-Hanna players can still roll a puppet outcome.
        /// </summary>
        private static List<CardModel> BuildFallbackPuppetTransformPool(CardModel original)
        {
            Player? player = original.Owner;
            if (player == null)
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
            DynamicVars.Cards.UpgradeValueBy(1);
        }
    }
}

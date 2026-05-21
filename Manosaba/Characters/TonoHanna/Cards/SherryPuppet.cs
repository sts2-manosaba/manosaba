using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.TonoHanna.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Manosaba.Characters.Common.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.TonoHanna.Cards
{
    [Pool(typeof(TonoHannaCardPool))]
    public class SherryPuppet : PathCustomCardModel
    {
        protected override HashSet<CardTag> CanonicalTags => [ManosabaCardTags.Puppet];

        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Attacks", 1m)];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromCard<HannaPuppet>(),
            HoverTipFactory.FromCard<Boulders>(),
        ];

        protected override bool ShouldGlowGoldInternal =>
            Owner?.Creature is { } ownerCreature
            && PuppetCollectionHelper.HasUsedInCombat<HannaPuppetCollectionPower>(ownerCreature);

        public SherryPuppet() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (Owner?.Creature is not { } ownerCreature)
            {
                return;
            }

            if (PuppetCollectionHelper.HasUsedInCombat<HannaPuppetCollectionPower>(ownerCreature)
                && CombatState is { } combatState
                && Owner is { } player)
            {
                CardModel boulder = combatState.CreateCard(ModelDb.Card<Boulders>(), player);
                CardCmd.ApplyKeyword(boulder, CardKeyword.Exhaust);
                boulder.EnergyCost.SetThisTurnOrUntilPlayed(0);
                await CardPileCmd.AddGeneratedCardToCombat(boulder, PileType.Hand, Owner);
            }

            await CommonActions.Apply<SherryPuppetCollectionPower>(choiceContext, ownerCreature, this, 1m);
            await CommonActions.Apply<SherryPuppetPower>(choiceContext, ownerCreature, this, DynamicVars["Attacks"].BaseValue);
        }

        protected override void OnUpgrade()
        {
            AddKeyword(CardKeyword.Retain);
        }
    }
}

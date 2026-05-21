using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.TonoHanna.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.TonoHanna.Cards
{
    [Pool(typeof(TonoHannaCardPool))]
    public class HannaPuppet : PathCustomCardModel
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
        protected override HashSet<CardTag> CanonicalTags => [ManosabaCardTags.Puppet];

        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<HannaPuppetCollectionPower>(1),
            new PowerVar<HannaPuppetPower>(1),
        ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromPower<SoarPower>(),
            HoverTipFactory.FromCard<SherryPuppet>(),
            HoverTipFactory.FromPower<StrengthPower>(),
        ];

        protected override bool ShouldGlowGoldInternal =>
            Owner?.Creature is { } ownerCreature
            && PuppetCollectionHelper.HasUsedInCombat<SherryPuppetCollectionPower>(ownerCreature);

        public HannaPuppet() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (Owner?.Creature is not { } ownerCreature)
            {
                return;
            }

            if (PuppetCollectionHelper.HasUsedInCombat<SherryPuppetCollectionPower>(ownerCreature))
            {
                await CommonActions.Apply<StrengthPower>(choiceContext, ownerCreature, this, 2m);
            }

            await CommonActions.Apply<HannaPuppetCollectionPower>(choiceContext, ownerCreature, this, DynamicVars["HannaPuppetCollectionPower"].BaseValue);
            await CommonActions.Apply<HannaPuppetPower>(choiceContext, ownerCreature, this, DynamicVars["HannaPuppetPower"].BaseValue);
        }

        protected override void OnUpgrade()
        {
            RemoveKeyword(CardKeyword.Exhaust);
        }
    }
}

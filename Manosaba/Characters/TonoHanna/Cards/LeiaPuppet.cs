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

namespace Manosaba.Characters.TonoHanna.Cards
{
    [Pool(typeof(TonoHannaCardPool))]
    public class LeiaPuppet : PathCustomCardModel
    {
        protected override HashSet<CardTag> CanonicalTags => [ManosabaCardTags.Puppet];

        private const int energyCost = 1;
        private const CardType type = CardType.Power;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new GoldVar(10)];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromCard<MiriaPuppet>(),
            HoverTipFactory.FromCard<CocoPuppet>(),
        ];

        protected override bool ShouldGlowGoldInternal =>
            Owner?.Creature is { } ownerCreature
            && PuppetCollectionHelper.HasUsedInCombat<MiriaPuppetCollectionPower>(ownerCreature)
            && PuppetCollectionHelper.HasUsedInCombat<CocoPuppetCollectionPower>(ownerCreature);

        public LeiaPuppet() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (Owner?.Creature is not { } ownerCreature)
            {
                return;
            }

            decimal goldAmount = DynamicVars.Gold.BaseValue;
            if (PuppetCollectionHelper.HasUsedInCombat<MiriaPuppetCollectionPower>(ownerCreature)
                && PuppetCollectionHelper.HasUsedInCombat<CocoPuppetCollectionPower>(ownerCreature))
            {
                goldAmount += 5m;
            }

            await CommonActions.Apply<LeiaPuppetCollectionPower>(choiceContext, ownerCreature, this, 1m);
            await CommonActions.Apply<LeiaPuppetPower>(choiceContext, ownerCreature, this, goldAmount);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Gold.UpgradeValueBy(5m);
        }
    }
}

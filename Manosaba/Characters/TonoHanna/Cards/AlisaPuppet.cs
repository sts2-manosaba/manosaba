using System;
using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
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
    public class AlisaPuppet : PathCustomCardModel
    {
        protected override HashSet<CardTag> CanonicalTags => [ManosabaCardTags.Puppet];
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromPower<BurnPower>(),
            HoverTipFactory.FromCard<EmaPuppet>(),
        ];

        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.AnyEnemy;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<BurnPower>(10m)];

        protected override bool ShouldGlowGoldInternal =>
            Owner?.Creature is { } ownerCreature
            && PuppetCollectionHelper.HasUsedInCombat<EmaPuppetCollectionPower>(ownerCreature);

        public AlisaPuppet() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (Owner.Creature is not { } ownerCreature || cardPlay.Target is not { } target)
            {
                return;
            }

            decimal burnAmount = DynamicVars["BurnPower"].BaseValue;
            if (PuppetCollectionHelper.HasUsedInCombat<EmaPuppetCollectionPower>(ownerCreature))
            {
                burnAmount += 3m;
            }

            await PowerCmd.Apply<AlisaPuppetCollectionPower>(ownerCreature, 1m, ownerCreature, this);
            await PowerCmd.Apply<BurnPower>(target, burnAmount, ownerCreature, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["BurnPower"].UpgradeValueBy(5m);
        }
    }
}

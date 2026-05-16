using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.TonoHanna.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TonoHanna.Cards
{
    [Pool(typeof(TonoHannaCardPool))]
    public class AnAnPuppet : PathCustomCardModel
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
        protected override HashSet<CardTag> CanonicalTags => [ManosabaCardTags.Puppet];

        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Common;
        private const TargetType targetType = TargetType.AllEnemies;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("StrengthLoss", 6m)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromPower<StrengthPower>(),
            HoverTipFactory.FromCard<NoahPuppet>(),
            HoverTipFactory.Static(StaticHoverTip.Block),
        ];

        protected override bool ShouldGlowGoldInternal =>
            Owner?.Creature is { } ownerCreature
            && PuppetCollectionHelper.HasUsedInCombat<NoahPuppetCollectionPower>(ownerCreature);

        public AnAnPuppet() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (Owner?.Creature is not { } ownerCreature || CombatState == null)
            {
                return;
            }

            if (PuppetCollectionHelper.HasUsedInCombat<NoahPuppetCollectionPower>(ownerCreature))
            {
                await CreatureCmd.GainBlock(ownerCreature, 3m, ValueProp.Move, cardPlay);
            }

            await PowerCmd.Apply<AnAnPuppetCollectionPower>(ownerCreature, 1m, ownerCreature, this);
            foreach (Creature enemy in CombatState.HittableEnemies)
            {
                await PowerCmd.Apply<AnAnPuppetPower>(enemy, DynamicVars["StrengthLoss"].BaseValue, ownerCreature, this);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars["StrengthLoss"].UpgradeValueBy(2m);
        }
    }
}

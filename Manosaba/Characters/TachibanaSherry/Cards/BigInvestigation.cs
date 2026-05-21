using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

using Manosaba.Characters.TachibanaSherry.Powers;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class BigInvestigation : PathCustomCardModel
    {
        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag>();
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Common;
        private const TargetType targetType = TargetType.AnyEnemy;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromPower<VulnerablePower>(),
            HoverTipFactory.FromPower<CluePower>(),
            HoverTipFactory.FromPower<SherryDetectiveRewardPower>(),
            HoverTipFactory.FromPower<StrengthPower>(),
            HoverTipFactory.Static(StaticHoverTip.Block),
        ];

        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VulnerablePower>(1m), new PowerVar<CluePower>(1m)];

        public BigInvestigation() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target is not { } target || Owner?.Creature is not { } ownerCreature)
            {
                return;
            }

            await CommonActions.Apply<VulnerablePower>(choiceContext, target, this, DynamicVars["VulnerablePower"].BaseValue);
            await CommonActions.Apply<CluePower>(choiceContext, ownerCreature, this, DynamicVars["CluePower"].BaseValue);
            if (ownerCreature.GetPowerAmount<SherryDetectiveRewardPower>() > 0m)
            {
                await PowerCmd.Apply<StrengthPower>(choiceContext, ownerCreature, 2m, ownerCreature, this);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars["VulnerablePower"].UpgradeValueBy(1m);
        }
    }
}

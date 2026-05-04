using BaseLib.Utils;
using manosaba.Characters.Common;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.Common.Cards
{
    [Pool(typeof(CommonCardPool))]
    public class SleepingPill : PathCustomCardModel
    {
        private const int energyCost = 0;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Common;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SusPower>()];

        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new PowerVar<SusPower>(1)];

        public SleepingPill() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
            await PowerCmd.Apply<SusPower>(base.Owner.Creature, -base.DynamicVars["SusPower"].BaseValue, base.Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            RemoveKeyword(CardKeyword.Exhaust);
        }
    }
}

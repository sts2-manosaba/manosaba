using BaseLib.Utils;
using manosaba.Characters.Common;
using manosaba.Characters.SaekiMiria;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.SaekiMiria.Cards
{
    [Pool(typeof(SaekiMiriaCardPool))]
    public class UncleWriting : PathCustomCardModel
    {

        private const int energyCost = 0;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust),EnergyHoverTip, HoverTipFactory.FromPower<SusPower>()];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1), new CardsVar(2), new PowerVar<SusPower>(1)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        public UncleWriting() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
            await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
            await PowerCmd.Apply<SusPower>(base.Owner.Creature, DynamicVars["SusPower"].BaseValue, base.Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            base.AddKeyword(CardKeyword.Innate);
        }
    }
}

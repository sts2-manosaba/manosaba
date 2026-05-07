using BaseLib.Utils;
using manosaba.Characters.NikaidoHiro;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.NikaidoHiro.Cards
{
    [Pool(typeof(NikaidoHiroCardPool))]
    public class Perjury : PathCustomCardModel
    {
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<SusPower>(2), new PowerVar<StrengthPower>(2)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SusPower>(), HoverTipFactory.FromPower<StrengthPower>()];
        public Perjury() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<SusPower>(base.Owner.Creature, -DynamicVars["SusPower"].BaseValue, base.Owner.Creature, this);
            await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, DynamicVars["StrengthPower"].BaseValue, base.Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["SusPower"].UpgradeValueBy(1);
            DynamicVars["StrengthPower"].UpgradeValueBy(1);
        }
    }
}

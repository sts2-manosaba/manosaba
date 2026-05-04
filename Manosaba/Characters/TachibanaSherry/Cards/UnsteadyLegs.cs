using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class UnsteadyLegs : PathCustomCardModel
    {
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SusPower>(), HoverTipFactory.FromPower<StrengthPower>()];

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Unpowered), new PowerVar<SusPower>(2), new PowerVar<StrengthPower>(1)];

        public UnsteadyLegs() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.Damage(choiceContext, base.Owner.Creature, DynamicVars.Damage.BaseValue, ValueProp.Unpowered, base.Owner.Creature);
            await PowerCmd.Apply<SusPower>(base.Owner.Creature, -DynamicVars["SusPower"].BaseValue, base.Owner.Creature, this);
            await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, DynamicVars.Strength.BaseValue, base.Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Strength.UpgradeValueBy(1m);
        }
    }
}

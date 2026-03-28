using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
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
    public class DirtyEhe : PathCustomCardModel
    {
        private const int energyCost = 3;
        private const CardType type = CardType.Power;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ThornsPower>(), HoverTipFactory.FromPower<VulnerablePower>()];

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Unpowered), new PowerVar<ThornsPower>(10), new PowerVar<VulnerablePower>(5)];

        public DirtyEhe() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.Damage(choiceContext, base.Owner.Creature, DynamicVars.Damage.BaseValue, ValueProp.Unpowered, base.Owner.Creature);
            await PowerCmd.Apply<ThornsPower>(base.Owner.Creature, DynamicVars["ThornsPower"].BaseValue, base.Owner.Creature, this);
            await PowerCmd.Apply<VulnerablePower>(base.Owner.Creature, DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars["ThornsPower"].UpgradeValueBy(5m);
        }
    }
}

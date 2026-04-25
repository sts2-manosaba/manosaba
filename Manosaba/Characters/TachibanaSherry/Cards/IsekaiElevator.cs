using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class IsekaiElevator : PathCustomCardModel
    {
        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.AnyAlly;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];

        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MajokaPower>(20m), new DamageVar(5m, ValueProp.Unpowered)];

        public IsekaiElevator() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (base.Owner.Creature is not { } ownerCreature || cardPlay.Target is not { } target)
            {
                return;
            }

            await PowerCmd.Apply<MajokaPower>(target, base.DynamicVars["MajokaPower"].BaseValue, ownerCreature, this);
            await PowerCmd.Apply<MajokaPower>(ownerCreature, base.DynamicVars["MajokaPower"].BaseValue, ownerCreature, this);
            await CreatureCmd.Damage(choiceContext, ownerCreature, base.DynamicVars.Damage.BaseValue, ValueProp.Unpowered, ownerCreature);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars["MajokaPower"].UpgradeValueBy(10m);
        }
    }
}

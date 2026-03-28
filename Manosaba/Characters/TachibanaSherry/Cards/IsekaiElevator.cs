using BaseLib.Utils;
using System;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class IsekaiElevator : PathCustomCardModel
    {
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.AnyAlly;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MajokaPower>(20m), new DamageVar(5m, ValueProp.Unpowered)];

        public IsekaiElevator() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

            await PowerCmd.Apply<MajokaPower>(cardPlay.Target, base.DynamicVars["MajokaPower"].BaseValue, base.Owner.Creature, this);
            await PowerCmd.Apply<MajokaPower>(base.Owner.Creature, base.DynamicVars["MajokaPower"].BaseValue, base.Owner.Creature, this);
            await CreatureCmd.Damage(choiceContext, base.Owner.Creature, base.DynamicVars.Damage.BaseValue, ValueProp.Unpowered, base.Owner.Creature);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars["MajokaPower"].UpgradeValueBy(10m);
        }
    }
}

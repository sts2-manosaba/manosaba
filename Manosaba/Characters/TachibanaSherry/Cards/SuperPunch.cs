using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class SuperPunch : PathCustomCardModel
    {
        private const int energyCost = 0;
        private const CardType type = CardType.Attack;
        private const CardRarity rarity = CardRarity.Ancient;
        private const TargetType targetType = TargetType.AllEnemies;
        private const bool shouldShowInCardLibrary = true;

        public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Mahou, CardKeyword.Eternal];

        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new CardsVar(1),
            new DamageVar(33m, ValueProp.Move),
            new PowerVar<StrengthPower>(5m)
        ];

        protected override bool ShouldGlowGoldInternal => base.Owner.Creature.GetPowerAmount<MajokaPower>() >= 100;

        public SuperPunch() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);

            if (base.CombatState == null)
                return;

            if (base.Owner.Creature.GetPowerAmount<MajokaPower>() < 100)
                return;

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .TargetingAllOpponents(base.CombatState)
                .Execute(choiceContext);
            await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, base.DynamicVars["StrengthPower"].BaseValue, base.Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            AddKeyword(CardKeyword.Retain);
        }
    }
}

using BaseLib.Utils;
using manosaba.Characters.NikaidoHiro;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.NikaidoHiro.Cards
{
    [Pool(typeof(NikaidoHiroCardPool))]
    public class WitchesSabbath : PathCustomCardModel
    {
        protected override bool ShouldGlowGoldInternal => IsPlayable;
        private const int energyCost = 0;
        private const CardType type = CardType.Attack;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.AllEnemies;
        private const bool shouldShowInCardLibrary = true;

        protected override bool IsPlayable => base.Owner.Creature.GetPowerAmount<MajokaPower>() >= 100;
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new CalculationBaseVar(50m),
            new ExtraDamageVar(1m),
            new CalculatedDamageVar(ValueProp.Unpowered).WithMultiplier(delegate(CardModel card, Creature? _){
                int voteAmount = card.Owner.Creature.GetPowerAmount<VotePower>();
                int majokaAmount = card.Owner.Creature.GetPowerAmount<MajokaPower>();

                return (50m + voteAmount * 3 + majokaAmount / 25) * (1 + 0.01m * majokaAmount) - 50m;
            }),
            new PowerVar<MajokaPower>(100)
            ];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>(), HoverTipFactory.FromPower<VotePower>()];
        public WitchesSabbath() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var combatState = base.CombatState;
            if (combatState == null)
                return;

            int majokaAmount = base.Owner.Creature.GetPowerAmount<MajokaPower>();
            int voteAmount = base.Owner.Creature.GetPowerAmount<VotePower>();
            await PowerCmd.Apply<MajokaPower>(base.Owner.Creature, -majokaAmount, base.Owner.Creature, this);
            await PowerCmd.Apply<VotePower>(base.Owner.Creature, -voteAmount, base.Owner.Creature, this);

            decimal damage = (DynamicVars.CalculationBase.BaseValue + voteAmount * 3 + majokaAmount / 25) * (1 + 0.01m * majokaAmount);

            await DamageCmd.Attack(damage)
                .FromCard(this)
                .TargetingAllOpponents(combatState)
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.CalculationBase.UpgradeValueBy(20m);
        }
    }
}

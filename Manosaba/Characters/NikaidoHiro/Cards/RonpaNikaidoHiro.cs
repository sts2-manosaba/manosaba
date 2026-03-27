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
    public class RonpaNikaidoHiro : PathCustomCardModel
    {
        private const int energyCost = 1;
        private const CardType type = CardType.Attack;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.AnyEnemy;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new CalculationBaseVar(8m),
            new ExtraDamageVar(4m),
            new CalculatedDamageVar(ValueProp.Move).WithMultiplier(delegate(CardModel card, Creature? _){
                if (card.Owner.Creature.GetPowerAmount<VotePower>() > 1)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }),
            new DynamicVar("VotePowerCost", 1)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VotePower>()];
        public RonpaNikaidoHiro() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var target = cardPlay.Target;
            if (target == null)
                return;

            if (base.Owner.Creature.GetPowerAmount<VotePower>() > 1)
            {
                await DamageCmd.Attack(DynamicVars.CalculatedDamage)
                 .FromCard(this)
                 .Targeting(target)
                 .Execute(choiceContext);
                await PowerCmd.Apply<VotePower>(base.Owner.Creature, -DynamicVars["VotePowerCost"].BaseValue, base.Owner.Creature, this);
            }
            else
            {
                await DamageCmd.Attack(DynamicVars.CalculationBase.BaseValue)
                    .FromCard(this)
                    .Targeting(target)
                    .Execute(choiceContext);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.CalculationBase.UpgradeValueBy(2);
            DynamicVars.ExtraDamage.UpgradeValueBy(3);
        }
    }
}

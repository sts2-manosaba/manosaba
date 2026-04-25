using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.JogasakiNoah.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoahCard.Cards
{
    [Pool(typeof(JogasakiNoahCardPool))]
    public class LiquidManipulation : PathCustomCardModel
    {
        private const int energyCost = 3;
        private const CardType type = CardType.Power;
        private const CardRarity rarity = CardRarity.Ancient;
        private const TargetType targetType = TargetType.AllAllies;
        private const bool shouldShowInCardLibrary = true;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<LiquidManipulationPower>(), HoverTipFactory.FromPower<MajokaPower>()];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Mahou, CardKeyword.Eternal];
        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new CalculationBaseVar(0m),
            new CalculationExtraVar(20m),
            new CalculatedVar("LiquidManipulationPower").WithMultiplier(GetMajokaFactor)
        ];
        public LiquidManipulation() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (base.CombatState is not { } combatState || base.Owner.Creature is not { } ownerCreature)
            {
                return;
            }

            IEnumerable<Creature> enumerable = from c in combatState.GetTeammatesOf(ownerCreature)
                                               where c != null && c.IsAlive && c.IsPlayer
                                               select c;
            foreach (Creature item in enumerable)
            {
                decimal amount = ((CalculatedVar)DynamicVars["LiquidManipulationPower"]).Calculate(null);
                await PowerCmd.Apply<LiquidManipulationPower>(item, amount, ownerCreature, this);
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }

        private static decimal GetMajokaFactor(CardModel card, Creature? _)
            => Math.Min(card.Owner?.Creature?.GetPowerAmount<MajokaPower>() / 100m ?? 0m, 1m);
    }
}

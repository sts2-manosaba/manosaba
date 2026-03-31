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
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<LiquidManipulationPower>()];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Mahou, CardKeyword.Eternal];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<LiquidManipulationPower>(20)];
        public LiquidManipulation() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            IEnumerable<Creature> enumerable = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
                                               where c != null && c.IsAlive && c.IsPlayer
                                               select c;
            foreach (Creature item in enumerable)
            {
                await PowerCmd.Apply<LiquidManipulationPower>(item, DynamicVars["LiquidManipulationPower"].BaseValue * Math.Min(Owner.Creature.GetPowerAmount<MajokaPower>() / 100m, 1m), Owner.Creature, this);
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }
    }
}

using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.HoverTips;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class BrokenCrossbow : PathCustomCardModel
    {
        private const int energyCost = 0;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Token;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = false;
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VotePower>()];

        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VotePower>(2), new EnergyVar(2)];

        public BrokenCrossbow() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        public static IEnumerable<BrokenCrossbow> Create(Player owner, int amount, CombatState combatState)
        {
            List<BrokenCrossbow> list = new List<BrokenCrossbow>();
            for (int i = 0; i < amount; i++)
            {
                list.Add(combatState.CreateCard<BrokenCrossbow>(owner));
            }

            return list;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<VotePower>(base.Owner.Creature, base.DynamicVars["VotePower"].BaseValue, base.Owner.Creature, this);
            await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Energy.UpgradeValueBy(1m);
        }
    }
}

using BaseLib.Utils;
using System.Linq;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class MasterKey : PathCustomCardModel
    {
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Common;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;
        public override IEnumerable<CardKeyword> CanonicalKeywords => [];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<BrokenLock>(base.IsUpgraded)];

        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(1)];

        public MasterKey() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (base.CombatState == null)
                return;

            List<BrokenLock> cards = BrokenLock.Create(base.Owner, 1, base.CombatState).ToList();
            await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, addedByPlayer: true, CardPilePosition.Random);
            if (base.IsUpgraded)
            {
                foreach (CardModel c in cards)
                {
                    CardCmd.Upgrade(c);
                }
            }
            await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, base.DynamicVars.Strength.BaseValue, base.Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            // Upgrade now affects generated Broken Lock card level.
        }
    }
}

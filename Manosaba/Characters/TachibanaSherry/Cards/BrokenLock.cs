using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class BrokenLock : PathCustomCardModel
    {
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Token;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = false;
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

        public BrokenLock() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        public static IEnumerable<BrokenLock> Create(Player owner, int amount, CombatState combatState)
        {
            List<BrokenLock> list = new List<BrokenLock>();
            for (int i = 0; i < amount; i++)
            {
                list.Add(combatState.CreateCard<BrokenLock>(owner));
            }

            return list;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}

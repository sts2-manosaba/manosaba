using BaseLib.Utils;
using manosaba.Characters.Common;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.Common.Cards
{
    [Pool(typeof(CommonCardPool))]
    public class SSArrow : PathCustomCardModel
    {

        private const int energyCost = 0;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Token;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = false;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<SimpleSpear>()];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Retain];

        public SSArrow() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
        {
            if (side == CombatSide.Player)
            {
                CardModel token1, token2, token3, token4;

                if ((token1 = PileType.Hand.GetPile(Owner).Cards.FirstOrDefault(c => c is SSArrow)) == null)
                    return;
                if ((token2 = PileType.Hand.GetPile(Owner).Cards.FirstOrDefault(c => c is SSBroom)) == null)
                    return;
                if ((token3 = PileType.Hand.GetPile(Owner).Cards.FirstOrDefault(c => c is SSRibbon)) == null)
                    return;
                if ((token4 = PileType.Hand.GetPile(Owner).Cards.FirstOrDefault(c => c is SSRapier)) == null)
                    return;
                await CardPileCmd.RemoveFromCombat(token1);
                await CardPileCmd.RemoveFromCombat(token2);
                await CardPileCmd.RemoveFromCombat(token3);
                await CardPileCmd.RemoveFromCombat(token4);
                List<SimpleSpear> cards = SimpleSpear.Create(base.Owner, 1, combatState).ToList();
                IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, addedByPlayer: true, CardPilePosition.Random);
                CardCmd.PreviewCardPileAdd(results);
            }
        }

        public static IEnumerable<SSArrow> Create(Player owner, int amount, CombatState combatState)
        {
            List<SSArrow> list = new List<SSArrow>();
            for (int i = 0; i < amount; i++)
            {
                list.Add(combatState.CreateCard<SSArrow>(owner));
            }

            return list;
        }
    }
}

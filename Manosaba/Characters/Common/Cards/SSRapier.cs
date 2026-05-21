using BaseLib.Utils;
using manosaba.Characters.Common;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Manosaba.Characters.Common.Cards
{
    [Pool(typeof(TokenCardPool))]
    public class SSRapier : PathCustomCardModel
    {

        private const int energyCost = -1;
        private const CardType type = CardType.Quest;
        private const CardRarity rarity = CardRarity.Token;
        private const TargetType targetType = TargetType.None;
        private const bool shouldShowInCardLibrary = true;

        public override bool CanBeGeneratedInCombat => false;
        public override bool CanBeGeneratedByModifiers => false;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<SimpleSpear>()];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Retain];

        public SSRapier() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        public static IEnumerable<SSRapier> Create(Player owner, int amount, ICombatState combatState)
        {
            List<SSRapier> list = new List<SSRapier>();
            for (int i = 0; i < amount; i++)
            {
                list.Add(combatState.CreateCard<SSRapier>(owner));
            }

            return list;
        }
    }
}

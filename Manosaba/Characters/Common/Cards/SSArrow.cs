using BaseLib.Utils;
using manosaba.Characters.Common;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.HasumiLeia.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.Common.Cards
{
    [Pool(typeof(CommonCardPool))]
    public class SSArrow : PathCustomCardModel
    {

        private const int energyCost = -1;
        private const CardType type = CardType.Quest;
        private const CardRarity rarity = CardRarity.Token;
        private const TargetType targetType = TargetType.None;
        private const bool shouldShowInCardLibrary = false;

        public override bool CanBeGeneratedInCombat => false;
        public override bool CanBeGeneratedByModifiers => false;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<SimpleSpear>()];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Retain];

        public SSArrow() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
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
            await CardPileCmd.AddToCombatAndPreview<SimpleSpear>(Owner.Creature, PileType.Hand, 1, true);
            await CheckForAdditionalEffect();
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

        public async Task CheckForAdditionalEffect()
        {
            if (Owner?.Creature == null)
                return;


            int portableFletchingStationAmount = (int)Owner.Creature.GetPowerAmount<PortableFletchingStationPower>();
            if (portableFletchingStationAmount <= 0)
                return;

            for (int i = 0; i < portableFletchingStationAmount; i++)
            {
                if (Owner.RunState.Rng.CombatCardGeneration.NextInt(5) != 0)
                    continue;

                await Owner.Creature.GetPower<PortableFletchingStationPower>()?.TriggerFlash();
                CardModel bonusSpear = Owner.Creature.CombatState.CreateCard<SimpleSpear>(Owner);
                await CardPileCmd.AddGeneratedCardToCombat(bonusSpear, PileType.Hand, addedByPlayer: true);
            }
        }
    }
}

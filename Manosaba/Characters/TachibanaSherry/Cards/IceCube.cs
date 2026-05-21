using BaseLib.Utils;
using System.Linq;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Extensions;
using Manosaba.Characters.TachibanaSherry.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class IceCube : PathCustomCardModel
    {
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromCard<IceBall>(),
            HoverTipFactory.FromPower<SherryDetectiveRewardPower>(),
            HoverTipFactory.FromPower<StrengthPower>(),
            HoverTipFactory.Static(StaticHoverTip.Block),
            HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        ];

        public IceCube() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (base.CombatState == null)
                return;

            int count = base.DynamicVars.Cards.IntValue;
            List<IceBall> cards = IceBall.Create(base.Owner, count, base.CombatState).ToList();
            await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, Owner, CardPilePosition.Random);
            if (base.Owner.Creature.GetPowerAmount<SherryDetectiveRewardPower>() > 0m)
            {
                await CommonActions.Apply<MegaCrit.Sts2.Core.Models.Powers.StrengthPower>(choiceContext, base.Owner.Creature, this, 2m);
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}

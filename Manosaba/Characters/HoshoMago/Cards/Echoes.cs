using BaseLib.Utils;
using manosaba.Characters.HoshoMago;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.HoshoMago.Cards
{
    [Pool(typeof(HoshoMagoCardPool))]
    public class Echoes : PathCustomCardModel
    {
        private const int energyCost = 2;
        private const CardType type = CardType.Power;
        private const CardRarity rarity = CardRarity.Ancient;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>(), HoverTipFactory.Static(StaticHoverTip.ReplayStatic)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Mahou, CardKeyword.Eternal];
        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new CalculationBaseVar(1m),
            new CalculationExtraVar(4m),
            new CalculatedVar("Replay").WithMultiplier(GetMajokaFactor)
        ];
        public Echoes() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            List<CardModel> list = PileType.Draw.GetPile(base.Owner).Cards.ToList();
            if (list.Count == 0)
            {
                return;
            }

            List<CardModel> list2 = list.Where(delegate (CardModel c)
            {
                bool flag = !c.Keywords.Contains(CardKeyword.Unplayable);
                bool flag2 = flag;
                if (flag2)
                {
                    CardType type = c.Type;
                    bool flag3 = (uint)(type - 5) <= 1u;
                    flag2 = !flag3;
                }

                return flag2 && c.GetEnchantedReplayCount() < 1;
            }).ToList();
            List<CardModel> list3 = list2.Where(delegate (CardModel c)
            {
                CardType type = c.Type;
                return (uint)(type - 1) <= 2u;
            }).ToList();
            IEnumerable<CardModel> items = ((list3.Count == 0) ? list2 : list3);
            CardModel cardModel = base.Owner.RunState.Rng.CombatCardSelection.NextItem(items);
            if (cardModel != null)
            {
                cardModel.BaseReplayCount += (int)((CalculatedVar)DynamicVars["Replay"]).Calculate(null);
                CardCmd.Preview(cardModel);
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }

        private static decimal GetMajokaFactor(CardModel card, Creature? _)
            => Math.Min(card.Owner.Creature.GetPowerAmount<MajokaPower>() / 100m, 1m);
    }
}

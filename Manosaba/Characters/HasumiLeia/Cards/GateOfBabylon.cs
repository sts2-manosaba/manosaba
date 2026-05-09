using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using Manosaba.Characters.Common.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using System.Linq;

namespace Manosaba.Characters.HasumiLeia.Cards
{
    [Pool(typeof(HasumiLeiaCardPool))]
    public sealed class GateOfBabylon : PathCustomCardModel
    {
        private const string _calculatedShivsKey = "CalculatedShivs";

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<SimpleSpear>(base.IsUpgraded)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new CalculationBaseVar(0m),
            new CalculationExtraVar(1m),
            new CalculatedVar(_calculatedShivsKey).WithMultiplier((CardModel card, Creature? _) =>
                PileType.Exhaust.GetPile(card.Owner).Cards.Count(c => c is SimpleSpear)),
        ];

        public GateOfBabylon()
            : base(3, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy,true)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
            Creature target = cardPlay.Target;

            List<CardModel> enumerable = PileType.Exhaust.GetPile(base.Owner).Cards
                .Where(c => c is SimpleSpear)
                .ToList();
            bool flag = true;
            foreach (CardModel item in enumerable)
            {
                if (base.IsUpgraded)
                {
                    CardCmd.Upgrade(item, CardPreviewStyle.None);
                }

                await CardCmd.AutoPlay(choiceContext, item, target, AutoPlayType.Default, skipXCapture: false, !flag);
                flag = false;
            }
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}

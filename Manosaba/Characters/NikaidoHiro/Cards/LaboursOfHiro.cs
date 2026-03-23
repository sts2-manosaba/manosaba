using BaseLib.Abstracts;
using BaseLib.Utils;
using manosaba.Characters.NikaidoHiro;
using Manosaba.Characters.NikaidoHiro.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.NikaidoHiro.Cards
{
    [Pool(typeof(NikaidoHiroCardPool))]
    public class LaboursOfHiro : PathCustomCardModel
    {
        private const int energyCost = 2;
        private const CardType type = CardType.Power;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<LaboursOfHiroPower>()];
        public LaboursOfHiro() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<LaboursOfHiroPower>(Owner.Creature, 1m, Owner.Creature, this);
        }

        // 升级后的效果逻辑
        protected override void OnUpgrade()
        {
            AddKeyword(CardKeyword.Innate);
        }
    }
}

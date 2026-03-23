using BaseLib.Abstracts;
using BaseLib.Utils;
using manosaba.Characters.Common;
using Manosaba.Extensions;
using Manosaba.Characters.NikaidoHiro.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.Common.Cards
{
    [Pool(typeof(CommonCardPool))]
    public class PicnicTime : PathCustomCardModel
    {
        // 基础耗能
        private const int energyCost = 1;
        // 卡牌类型
        private const CardType type = CardType.Skill;
        // 卡牌稀有度
        private const CardRarity rarity = CardRarity.Uncommon;
        // 目标类型（AnyEnemy表示任意敌人）
        private const TargetType targetType = TargetType.Self;
        // 是否在卡牌图鉴中显示
        private const bool shouldShowInCardLibrary = true;
        // 卡牌的基础属性
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [base.EnergyHoverTip];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1), new CardsVar(2)];

        public PicnicTime() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        // 打出时的效果逻辑
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
            await PowerCmd.Apply<EnergyNextTurnPower>(base.Owner.Creature, base.DynamicVars.Energy.IntValue, base.Owner.Creature, this);
        }

        // 升级后的效果逻辑
        protected override void OnUpgrade()
        {
            base.DynamicVars.Cards.UpgradeValueBy(1m);
            base.DynamicVars.Energy.UpgradeValueBy(1m);
        }
    }
}

using BaseLib.Abstracts;
using BaseLib.Utils;
using manosaba.Characters.NikaidoHiro;
using Manosaba.Characters.Common.Powers;
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
    public class GoodSleep : PathCustomCardModel
    {
        public override bool GainsBlock => true;
        private const string _majokaKey = "MajokaPower";
        // 基础耗能
        private const int energyCost = 1;
        // 卡牌类型
        private const CardType type = CardType.Skill;
        // 卡牌稀有度
        private const CardRarity rarity = CardRarity.Common;
        // 目标类型（AnyEnemy表示任意敌人）
        private const TargetType targetType = TargetType.Self;
        // 是否在卡牌图鉴中显示
        private const bool shouldShowInCardLibrary = true;

        // 卡牌的基础属性
        protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7, ValueProp.Move), new DynamicVar(_majokaKey, 5m)];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];

        public GoodSleep() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        // 打出时的效果逻辑
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<MajokaPower>(Owner.Creature, DynamicVars[_majokaKey].BaseValue, Owner.Creature, this);
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }

        // 升级后的效果逻辑
        protected override void OnUpgrade()
        {
            DynamicVars[_majokaKey].UpgradeValueBy(2);
            DynamicVars.Block.UpgradeValueBy(3);
        }
    }
}

using BaseLib.Abstracts;
using BaseLib.Utils;
using manosaba.Characters.NikaidoHiro;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.NikaidoHiro.Cards
{
    [Pool(typeof(NikaidoHiroCardPool))]
    public class StrikeNikaidoHiro : PathCustomCardModel
    {
        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };
        // 基础耗能
        private const int energyCost = 1;
        // 卡牌类型
        private const CardType type = CardType.Attack;
        // 卡牌稀有度
        private const CardRarity rarity = CardRarity.Basic;
        // 目标类型（AnyEnemy表示任意敌人）
        private const TargetType targetType = TargetType.AnyEnemy;
        // 是否在卡牌图鉴中显示
        private const bool shouldShowInCardLibrary = true;

        // 卡牌的基础属性
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

        public StrikeNikaidoHiro() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        // 打出时的效果逻辑
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue) // 造成伤害，数值来源于卡牌的基础伤害属性
                .FromCard(this) // 伤害来源于这张卡牌
                .Targeting(cardPlay.Target) // 伤害目标是玩家选择的目标
                .Execute(choiceContext);
        }

        // 升级后的效果逻辑
        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(3); // 升级后增加3点伤害
        }
    }
}

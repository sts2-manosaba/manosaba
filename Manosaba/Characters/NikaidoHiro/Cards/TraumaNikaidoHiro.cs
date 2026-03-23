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
    public class TraumaNikaidoHiro : PathCustomCardModel
    {
        private const string _majokaKey = "MajokaPower";
        private const int energyCost = 0;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Common;
        private const TargetType targetType = TargetType.Self; 
        private const bool shouldShowInCardLibrary = true;
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(_majokaKey, 10m)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];
        public TraumaNikaidoHiro() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        // 打出时的效果逻辑
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<MajokaPower>(Owner.Creature, DynamicVars[_majokaKey].BaseValue, Owner.Creature, this);
        }

        // 升级后的效果逻辑
        protected override void OnUpgrade()
        {
            DynamicVars[_majokaKey].UpgradeValueBy(5);
        }
    }
}

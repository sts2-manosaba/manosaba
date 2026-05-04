using BaseLib.Utils;
using manosaba.Characters.NikaidoHiro;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.NikaidoHiro.Cards
{
    [Pool(typeof(NikaidoHiroCardPool))]
    public class Perjury : PathCustomCardModel
    {
        public override bool GainsBlock => true;
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Common;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7, ValueProp.Move), new DynamicVar("ExtraBlock", 3)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SusPower>()];
        public Perjury() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int currentSus = base.Owner.Creature.GetPowerAmount<SusPower>();
            if (currentSus > 0)
            {
                int susPowerCost = (currentSus + 1) / 2;
                decimal extraBlock = DynamicVars["ExtraBlock"].BaseValue * susPowerCost;
                await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(extraBlock, ValueProp.Move), cardPlay);
                await PowerCmd.Apply<SusPower>(base.Owner.Creature, -susPowerCost, base.Owner.Creature, this);
            }
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Block.UpgradeValueBy(2);
            DynamicVars["ExtraBlock"].UpgradeValueBy(3);
        }
    }
}

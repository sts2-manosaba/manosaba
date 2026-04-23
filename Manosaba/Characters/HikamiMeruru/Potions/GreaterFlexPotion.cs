using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.HikamiMeruru.Potions
{
    [Pool(typeof(HikamiMeruruPotionPool))]
    public class GreaterFlexPotion : PathCustomPotionModel
    {
        public override PotionUsage Usage => PotionUsage.CombatOnly;
        public override PotionRarity Rarity => PotionRarity.Token;
        public override TargetType TargetType => TargetType.AnyPlayer;

        public override bool CanBeGeneratedInCombat => false;
        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(5m)];
        public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

        protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
        {
            PotionModel.AssertValidForTargetedPotion(target);
            await PowerCmd.Apply<StrengthPower>(target, base.DynamicVars.Strength.BaseValue, base.Owner.Creature, null);
        }
    }
}

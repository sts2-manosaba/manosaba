using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.HikamiMeruru.Potions
{
    [Pool(typeof(HikamiMeruruPotionPool))]
    public class VitalPotion : PathCustomPotionModel
    {
        private const decimal MaxHpGain = 5m;

        public override PotionUsage Usage => PotionUsage.CombatOnly;
        public override PotionRarity Rarity => PotionRarity.Token;
        public override TargetType TargetType => TargetType.AnyPlayer;

        public override bool CanBeGeneratedInCombat => false;

        protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
        {
            PotionModel.AssertValidForTargetedPotion(target);
            await CreatureCmd.GainMaxHp(target, MaxHpGain);
        }
    }
}

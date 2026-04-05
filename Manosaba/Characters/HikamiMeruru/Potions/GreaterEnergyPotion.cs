using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.HikamiMeruru.Potions
{
    [Pool(typeof(HikamiMeruruPotionPool))]
    public class GreaterEnergyPotion : PathCustomPotionModel
    {
        public override PotionUsage Usage => PotionUsage.CombatOnly;
        public override PotionRarity Rarity => PotionRarity.Token;
        public override TargetType TargetType => TargetType.AnyPlayer;

        public override bool CanBeGeneratedInCombat => false;
        protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(5)];

        protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
        {
            await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, target.Player);
        }
    }
}

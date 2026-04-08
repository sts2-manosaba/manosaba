using BaseLib.Utils;
using Manosaba.Characters.Common.Monsters;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace manosaba.Characters.SaekiMiria.Relics
{

    [Pool(typeof(SaekiMiriaRelicPool))]
    public sealed class CabinetKey : LevelingPathCustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Starter;
        protected override int MaxRelicLevel => 5;

        protected decimal basePercentage = 0.1m;

        protected decimal dmgTakenThisTurn = 0m;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

        public override Task AfterObtained()
        {
            ApplyRelicLevelEffects();
            return Task.CompletedTask;
        }

        public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            ApplyRelicLevelEffects();
            if (base.Owner.Creature == target)
            {
                if(result.UnblockedDamage > 0)
                {
                    dmgTakenThisTurn += result.UnblockedDamage;
                }
            }
            return Task.CompletedTask;
        }

        public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            int energyToGain = (int)decimal.Ceiling(dmgTakenThisTurn * basePercentage);
            PlayerCmd.GainEnergy(energyToGain, base.Owner);
            dmgTakenThisTurn = 0m;
            return Task.CompletedTask;
        }



        protected override void OnRelicLevelChanged(int oldLevel, int newLevel)
        {
            ApplyRelicLevelEffects();
        }

        private void ApplyRelicLevelEffects()
        {
            // Pen of Hiro level effects are defined on this relic:
            // Lv1: 20% 
            // Lv2: 25% 
            // Lv3: 30% 
            //  Lv4: 35% 
            //  Lv5: 40% 
            int level = RelicLevel;
            basePercentage = 0.2m + (level - 1) * 0.05m; // Increases by 5% each level, starting at 20% at level 1
        }
    }
}

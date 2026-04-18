using System;
using BaseLib.Utils;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace manosaba.Characters.KurobeNanoka.Relics
{
    [Pool(typeof(KurobeNanokaRelicPool))]
    public sealed class MagicalGun : PathCustomRelicModel
    {
        private const string BulletsVarName = "Bullets";
        private const int BaseCombatStartBullets = 1;
        private const int MaxBullets = 6;

        public override RelicRarity Rarity => RelicRarity.Starter;
        public override bool ShowCounter => true;
        public override int DisplayAmount => CurrentBullets;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(BulletsVarName, 0m)];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

        private int CurrentBullets
        {
            get => DynamicVars[BulletsVarName].IntValue;
            set => DynamicVars[BulletsVarName].BaseValue = Math.Clamp(value, 0, MaxBullets);
        }

        public override Task AfterObtained()
        {
            CurrentBullets = 0;
            InvokeDisplayAmountChanged();
            return Task.CompletedTask;
        }

        public override Task BeforeCombatStart()
        {
            int ribbonBonus = Owner.GetRelic<Ribbon>()?.CombatStartBulletBonus ?? 0;
            CurrentBullets = BaseCombatStartBullets + ribbonBonus;
            InvokeDisplayAmountChanged();
            return Task.CompletedTask;
        }

        public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player != Owner)
                return Task.CompletedTask;

            int bulletsToGain = Owner.Creature.GetPowerAmount<MajokaPower>() >= 100m ? 2 : 1;
            CurrentBullets += bulletsToGain;
            InvokeDisplayAmountChanged();
            return Task.CompletedTask;
        }

        public override Task AfterCombatEnd(CombatRoom room)
        {
            CurrentBullets = 0;
            InvokeDisplayAmountChanged();
            return Task.CompletedTask;
        }

        public bool HasEnoughBullets(int requiredBullets)
        {
            return CurrentBullets >= requiredBullets;
        }

        public bool ConsumeBullets(int bulletsToConsume)
        {
            if (!HasEnoughBullets(bulletsToConsume))
                return false;

            CurrentBullets -= bulletsToConsume;
            InvokeDisplayAmountChanged();
            return true;
        }
    }
}

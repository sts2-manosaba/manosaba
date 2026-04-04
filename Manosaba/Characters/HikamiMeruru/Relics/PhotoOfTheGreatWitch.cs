using BaseLib.Utils;
using Manosaba.Characters.HikamiMeruru.Potions;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace manosaba.Characters.HikamiMeruru.Relics
{

    [Pool(typeof(HikamiMeruruRelicPool))]
    public sealed class PhotoOfTheGreatWitch : LevelingPathCustomRelicModel
    {
        private bool _grantingBonusCatalyst;
        private int _appliedPotionSlotBonus;

        public override RelicRarity Rarity => RelicRarity.Starter;
        protected override int MaxRelicLevel => 5;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("PotionSlots", 3m)];

        [SavedProperty]
        public int AppliedPotionSlotBonus
        {
            get => _appliedPotionSlotBonus;
            set
            {
                AssertMutable();
                _appliedPotionSlotBonus = value < 0 ? 0 : value;
            }
        }

        public override async Task AfterObtained()
        {
            await SyncPotionSlotsWithLevel();
        }

        public override async Task BeforeCombatStart()
        {
            await SyncPotionSlotsWithLevel();
        }

        public override async Task AfterCombatVictory(CombatRoom room)
        {
            await base.AfterCombatVictory(room);
            await SyncPotionSlotsWithLevel();
        }

        public override async Task AfterPotionProcured(PotionModel potion)
        {
            if (potion.Owner != base.Owner)
            {
                return;
            }

            int catalystChancePercent = GetBonusCatalystChancePercent();
            if (catalystChancePercent <= 0)
            {
                return;
            }

            if (_grantingBonusCatalyst)
            {
                return;
            }

            if (!base.Owner.HasOpenPotionSlots)
            {
                return;
            }

            if (base.Owner.RunState.Rng.CombatPotionGeneration.NextInt(100) >= catalystChancePercent)
            {
                return;
            }

            _grantingBonusCatalyst = true;
            try
            {
                var result = await PotionCmd.TryToProcure<Catalyst>(base.Owner);
                if (result.success)
                {
                    Flash();
                }
            }
            finally
            {
                _grantingBonusCatalyst = false;
            }
        }

        private int GetTargetPotionSlotBonus()
        {
            // Base +4 slots, then +1 every 2 levels (Lv3, Lv5).
            return 4 + (RelicLevel - 1) / 2;
        }

        private int GetBonusCatalystChancePercent()
        {
            return RelicLevel switch
            {
                >= 4 => 20,
                >= 2 => 10,
                _ => 0
            };
        }

        private async Task SyncPotionSlotsWithLevel()
        {
            int targetSlots = GetTargetPotionSlotBonus();
            int delta = targetSlots - AppliedPotionSlotBonus;
            if (delta == 0)
            {
                return;
            }

            base.DynamicVars["PotionSlots"].BaseValue = targetSlots;
            await PlayerCmd.GainMaxPotionCount(delta, base.Owner);
            AppliedPotionSlotBonus = targetSlots;
        }
    }
}

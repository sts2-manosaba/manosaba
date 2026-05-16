using BaseLib.Utils;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace manosaba.Characters.HikamiMeruru.Relics
{

    [Pool(typeof(HikamiMeruruRelicPool))]
    public sealed class PhotoOfTheGreatWitch : LevelingPathCustomRelicModel
    {
        private int _appliedPotionSlotBonus;

        public override RelicRarity Rarity => RelicRarity.Starter;
        protected override int MaxRelicLevel => 5;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("PotionSlots", 1m)];

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

        private int GetTargetPotionSlotBonus()
        {
            return RelicLevel switch
            {
                >= 5 => 10,
                >= 4 => 6,
                >= 3 => 4,
                >= 2 => 2,
                _ => 1
            };
        }

        private async Task SyncPotionSlotsWithLevel()
        {
            int targetSlots = GetTargetPotionSlotBonus();
            base.DynamicVars["PotionSlots"].BaseValue = targetSlots;

            int delta = targetSlots - AppliedPotionSlotBonus;
            if (delta == 0)
            {
                return;
            }

            await PlayerCmd.GainMaxPotionCount(delta, base.Owner);
            AppliedPotionSlotBonus = targetSlots;
        }
    }
}

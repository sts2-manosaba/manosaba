using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Manosaba.Extensions
{
    public abstract class LevelingPathCustomRelicModel : PathCustomRelicModel
    {
        private int _manosaba_relicExp;

        protected virtual int MaxRelicLevel => RelicLevelExpTable.MaxLevel;
        protected virtual int GetCombatVictoryExpGain(CombatRoom room)
        {
            return room.RoomType switch
            {
                RoomType.Elite => 2,
                RoomType.Boss => 3,
                _ => 1
            };
        }

        public int RelicLevel => RelicLevelExpTable.GetLevelForExp(_manosaba_relicExp, MaxRelicLevel);

        public override bool ShowCounter => true;
        public override int DisplayAmount => RelicLevel;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<RelicLevelingHoverTipPower>()];

        [SavedProperty]
        public int RelicExp
        {
            get => _manosaba_relicExp;
            set
            {
                AssertMutable();

                int oldLevel = RelicLevel;
                int cappedExp = value < 0 ? 0 : value;
                int maxExp = RelicLevelExpTable.GetMaxExpForLevel(MaxRelicLevel);
                _manosaba_relicExp = cappedExp > maxExp ? maxExp : cappedExp;

                int newLevel = RelicLevel;
                if (newLevel != oldLevel)
                {
                    OnRelicLevelChanged(oldLevel, newLevel);
                    InvokeDisplayAmountChanged();
                }
            }
        }

        public override async Task AfterCombatVictory(CombatRoom room)
        {
            int expGain = GetCombatVictoryExpGain(room);
            if (expGain <= 0 || RelicLevel >= MaxRelicLevel)
            {
                return;
            }

            int oldLevel = RelicLevel;
            RelicExp += expGain;
            int newLevel = RelicLevel;
            if (newLevel > oldLevel)
            {
                Flash();
                await AfterRelicLevelChanged(oldLevel, newLevel);
            }
        }

        /// <summary>
        /// Sets relic exp to an absolute value and runs async level-up side effects
        /// for all gained levels in one pass.
        /// </summary>
        public async Task SetRelicExpAndProcessLevelUpsAsync(int exp)
        {
            int oldLevel = RelicLevel;
            RelicExp = exp;
            int newLevel = RelicLevel;
            if (newLevel > oldLevel)
            {
                await AfterRelicLevelChanged(oldLevel, newLevel);
            }
        }

        protected virtual void OnRelicLevelChanged(int oldLevel, int newLevel)
        {
        }

        protected virtual Task AfterRelicLevelChanged(int oldLevel, int newLevel)
        {
            return Task.CompletedTask;
        }
    }
}

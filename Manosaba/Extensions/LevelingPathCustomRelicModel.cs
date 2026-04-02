using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Manosaba.Extensions
{
    public abstract class LevelingPathCustomRelicModel : PathCustomRelicModel
    {
        private int _relicExp;

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

        public int RelicLevel => RelicLevelExpTable.GetLevelForExp(_relicExp, MaxRelicLevel);

        public override bool ShowCounter => true;
        public override int DisplayAmount => RelicLevel;

        [SavedProperty]
        public int RelicExp
        {
            get => _relicExp;
            set
            {
                AssertMutable();

                int oldLevel = RelicLevel;
                int cappedExp = value < 0 ? 0 : value;
                int maxExp = RelicLevelExpTable.GetMaxExpForLevel(MaxRelicLevel);
                _relicExp = cappedExp > maxExp ? maxExp : cappedExp;

                int newLevel = RelicLevel;
                if (newLevel != oldLevel)
                {
                    OnRelicLevelChanged(oldLevel, newLevel);
                    InvokeDisplayAmountChanged();
                }
            }
        }

        public override Task AfterCombatVictory(CombatRoom room)
        {
            int expGain = GetCombatVictoryExpGain(room);
            if (expGain <= 0 || RelicLevel >= MaxRelicLevel)
            {
                return Task.CompletedTask;
            }

            int oldLevel = RelicLevel;
            RelicExp += expGain;
            if (RelicLevel > oldLevel)
            {
                Flash();
            }

            return Task.CompletedTask;
        }

        protected virtual void OnRelicLevelChanged(int oldLevel, int newLevel)
        {
        }
    }
}

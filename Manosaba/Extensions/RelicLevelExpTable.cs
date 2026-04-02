namespace Manosaba.Extensions
{
    public static class RelicLevelExpTable
    {
        // Cumulative exp needed to reach each level (1-indexed levels).
        // Index 0 => Level 1, index 1 => Level 2, etc.
        private static readonly int[] CumulativeExpPerLevel = [0, 2, 5, 9, 14, 20];

        public static int MaxLevel => CumulativeExpPerLevel.Length;

        public static int GetLevelForExp(int exp, int maxLevel)
        {
            int clampedMaxLevel = ClampMaxLevel(maxLevel);
            int safeExp = exp < 0 ? 0 : exp;

            int level = 1;
            for (int i = 1; i < clampedMaxLevel; i++)
            {
                if (safeExp >= CumulativeExpPerLevel[i])
                {
                    level = i + 1;
                }
                else
                {
                    break;
                }
            }

            return level;
        }

        public static int GetMaxExpForLevel(int maxLevel)
        {
            int clampedMaxLevel = ClampMaxLevel(maxLevel);
            return CumulativeExpPerLevel[clampedMaxLevel - 1];
        }

        public static int GetExpToNextLevel(int exp, int maxLevel)
        {
            int clampedMaxLevel = ClampMaxLevel(maxLevel);
            int level = GetLevelForExp(exp, clampedMaxLevel);
            if (level >= clampedMaxLevel)
            {
                return 0;
            }

            int nextLevelThreshold = CumulativeExpPerLevel[level];
            return nextLevelThreshold - (exp < 0 ? 0 : exp);
        }

        private static int ClampMaxLevel(int maxLevel)
        {
            if (maxLevel < 1)
            {
                return 1;
            }

            if (maxLevel > MaxLevel)
            {
                return MaxLevel;
            }

            return maxLevel;
        }
    }
}

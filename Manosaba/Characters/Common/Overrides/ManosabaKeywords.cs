using BaseLib.Patches.Content;
using Manosaba.Characters.NikaidoHiro.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.Common.Overrides
{
    public class ManosabaKeywords
    {
        [CustomEnum("unique")]
        [KeywordProperties(AutoKeywordPosition.Before)]
        public static CardKeyword Unique;

        [CustomEnum("mahou")]
        [KeywordProperties(AutoKeywordPosition.Before)]
        public static CardKeyword Mahou;

        [CustomEnum("high_stance")]
        [KeywordProperties(AutoKeywordPosition.Before)]
        public static CardKeyword HighStance;

        [CustomEnum("mid_stance")]
        [KeywordProperties(AutoKeywordPosition.Before)]
        public static CardKeyword MidStance;

        [CustomEnum("low_stance")]
        [KeywordProperties(AutoKeywordPosition.Before)]
        public static CardKeyword LowStance;

        [CustomEnum("gun_shot")]
        [KeywordProperties(AutoKeywordPosition.Before)]
        public static CardKeyword GunShot;

        [CustomEnum("combust")]
        [KeywordProperties(AutoKeywordPosition.After)]
        public static CardKeyword Combust;

        [CustomEnum("combust_ignite")]
        [KeywordProperties(AutoKeywordPosition.After)]
        public static CardKeyword CombustIgnite;

        [CustomEnum("shared")]
        [KeywordProperties(AutoKeywordPosition.After)]
        public static CardKeyword Shared;

        [CustomEnum("beta")]
        [KeywordProperties(AutoKeywordPosition.Before)]
        public static CardKeyword Beta;

        [CustomEnum("sword_technique")]
        [KeywordProperties(AutoKeywordPosition.Before)]
        public static CardKeyword SwordTechnique;

        public readonly record struct StanceBonus(decimal BonusDamage, decimal BonusBlock);

        public static async Task<StanceBonus> ResolveHighStance(Creature owner, Creature? applier, CardModel? source)
        {
            if (!owner.HasPower<HighStancePower>())
            {
                await PowerCmd.Apply<HighStancePower>(owner, 1m, applier ?? owner, source);
            }
            return default;
        }

        public static async Task<StanceBonus> ResolveMidStance(Creature owner, Creature? applier, CardModel? source)
        {
            if (!owner.HasPower<MidStancePower>())
            {
                await PowerCmd.Apply<MidStancePower>(owner, 1m, applier ?? owner, source);
            }
            return default;
        }

        public static async Task<StanceBonus> ResolveLowStance(Creature owner, Creature? applier, CardModel? source)
        {
            if (!owner.HasPower<LowStancePower>())
            {
                await PowerCmd.Apply<LowStancePower>(owner, 1m, applier ?? owner, source);
            }
            return default;
        }
    }
}

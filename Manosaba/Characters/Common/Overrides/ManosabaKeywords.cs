using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

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
    }
}

using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.HasumiLeia.Powers;
using Manosaba.Characters.JogasakiNoah.Powers;
using Manosaba.Characters.KurobeNanoka.Cards;
using Manosaba.Characters.NikaidoHiro.Powers;
using Manosaba.Characters.SaekiMiria.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System;
using System.Collections.Generic;

namespace Manosaba.Characters.SaekiMiria.Helper
{
    public class MiriaConstants
    {
        public static HashSet<Type> IgnoredCards = new()
            {
                typeof(Exchange),
                typeof(EmaDogAttack)
            };

        public static HashSet<Type> IgnoredCardBaseTypes = new()
            {
                typeof(GunBase)
            };

        public static HashSet<CardKeyword> IgnoredCardKeywords = new()
            {
                ManosabaKeywords.GunShot
            };

        public static HashSet<Type> IgnoredPowers = new()
            {
                typeof(MajokaPower),
                typeof(VotePower),
                typeof(CoveredPower),
                typeof(InterceptPower),
                typeof(DeathLoopPower),
                typeof(GazeGuidingPower),
                typeof(StandOutPower),
                typeof(TankPower),
                typeof(LiquidManipulationPower),
            };

        public static bool IsIgnoredCard(CardModel card)
        {
            Type cardType = card.GetType();

            if (IgnoredCards.Contains(cardType))
            {
                return true;
            }

            foreach (Type ignoredBaseType in IgnoredCardBaseTypes)
            {
                if (ignoredBaseType.IsAssignableFrom(cardType))
                {
                    return true;
                }
            }

            foreach (CardKeyword ignoredKeyword in IgnoredCardKeywords)
            {
                if (card.CanonicalKeywords.Contains(ignoredKeyword))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

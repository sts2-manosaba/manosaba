using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.HasumiLeia.Powers;
using Manosaba.Characters.JogasakiNoah.Powers;
using Manosaba.Characters.KurobeNanoka.Cards;
using Manosaba.Characters.KurobeNanoka.Powers;
using Manosaba.Characters.NikaidoHiro.Powers;
using Manosaba.Characters.SaekiMiria.Cards;
using Manosaba.Characters.SaekiMiria.Powers;
using Manosaba.Characters.ShitoAlisa.Powers;
using Manosaba.Characters.TachibanaSherry.Powers;
using Manosaba.Characters.TonoHanna.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

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

        //For mind sharing
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
                typeof(PuppetCollectionSummaryPower),
                typeof(MovieInvitationPower),

                typeof(StrengthPower),
                typeof(SherryPuppetPower),
                typeof(AccuracyPower),
                typeof(LaboursOfHiroPower),
                typeof(SteadyShotPower),
                typeof(MusouKenPower),
                typeof(QuickWitPower),
                typeof(HouseKeepingPower),
                typeof(FlexPotionPower),
                typeof(ReptileTrinketPower),
                typeof(SetupStrikePower),
                typeof(CoordinatePower),
                typeof(FeedingFrenzyPower),
                typeof(GigantificationPower),
                typeof(RitualPower),
                typeof(DoubleDamagePower),
                typeof(VigorPower),
                typeof(DemonFormPower),

                typeof(ShowOffPower),

                typeof(ScaldingTouchPower),
                typeof(FireballSwarmPower),
                typeof(FinalCountdownPower),
                typeof(ThisIsFinePower),
                typeof(BeaconOfHopePower),

                typeof(RelicSearchPower),
            };

        public static HashSet<Type> AllowedLuckTransferPowers = new()
            {
                typeof(VulnerablePower),
                typeof(WeakPower),
                typeof(PoisonPower),
                typeof(BurnPower),
                typeof(DoomPower),
                typeof(DemisePower),
                typeof(DisintegrationPower),
                typeof(ConstrictPower),
                typeof(MagicBombPower),
                typeof(UnluckyPower),
                typeof(ShrinkPower)
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

        public static bool IsAllowedLuckTransferPower(PowerModel power)
        {
            return power != null && AllowedLuckTransferPowers.Contains(power.GetType());
        }
    }
}

using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.HasumiLeia.Powers;
using Manosaba.Characters.JogasakiNoah.Powers;
using Manosaba.Characters.NikaidoHiro.Powers;
using Manosaba.Characters.SaekiMiria.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manosaba.Characters.SaekiMiria.Helper
{
    public class MiriaConstants
    {
        public static HashSet<Type> IgnoredCards = new()
            {
                typeof(Exchange),
                typeof(EmaDogAttack)
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
    }
}

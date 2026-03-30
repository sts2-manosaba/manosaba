using Manosaba.Characters.JogasakiNoa.Orbs;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoah
{
    public class JogasakiNoahOrbPool
    {
        public static IReadOnlyList<OrbModel> AllOrbs => [
            ModelDb.Orb<RedPaintOrb>(),
            ModelDb.Orb<YellowPaintOrb>(),
            ModelDb.Orb<BluePaintOrb>(),
            ];
    }
}

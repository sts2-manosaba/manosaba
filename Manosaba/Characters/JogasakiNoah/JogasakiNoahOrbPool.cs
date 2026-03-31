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
            ModelDb.Orb<OrangePaintOrb>(),
            ModelDb.Orb<GreenPaintOrb>(),
            ModelDb.Orb<PurplePaintOrb>(),
            ModelDb.Orb<BlackPaintOrb>(),
            ModelDb.Orb<WhitePaintOrb>(),
            ];
    }
}

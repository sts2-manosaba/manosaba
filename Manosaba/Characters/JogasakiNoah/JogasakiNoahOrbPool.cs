using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Characters.JogasakiNoah.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoah
{
    public class JogasakiNoahOrbPool
    {
        public static IReadOnlyList<OrbModel> ZumaPrimaryOrbs => [
            ModelDb.Orb<RedPaintOrb>(),
            ModelDb.Orb<BluePaintOrb>(),
            ModelDb.Orb<YellowPaintOrb>(),
            ];

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

        public static OrbModel RollRandomGeneratedOrb(Player player)
        {
            if (player.Creature.HasPower<SekirinyakudouPower>())
            {
                return ModelDb.Orb<BloodOrb>();
            }

            IReadOnlyList<OrbModel> orbs = player.Creature.HasPower<ZumaPower>() ? ZumaPrimaryOrbs : AllOrbs;
            int idx = player.RunState.Rng.CombatOrbGeneration.NextInt(orbs.Count);
            return orbs[idx];
        }
    }
}

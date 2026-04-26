using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace Manosaba.Characters.ShitoAlisa.Visuals;

public static class FireballOrbitVisuals
{
    public static void Sync(Creature owner, int fireballCount)
    {
        if (TestMode.IsOn)
            return;

        NCreature? creature = NCombatRoom.Instance?.GetCreatureNode(owner);
        if (creature == null)
            return;

        FireballOrbitRing ring = FireballOrbitRing.GetOrCreate(creature);
        ring.SetVisibleCount(fireballCount);
    }
}

using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace Manosaba.Characters.TonoHanna.Visuals;

public static class PuppetCollectionVisuals
{
    public static void SetSlot(Creature owner, int slotIndex, bool active, string textureResourcePath)
    {
        if (TestMode.IsOn)
            return;

        NCreature? creature = NCombatRoom.Instance?.GetCreatureNode(owner);
        if (creature == null)
            return;

        PuppetCollectionRing ring = PuppetCollectionRing.GetOrCreate(creature);
        ring.SetSlotActive(slotIndex, active, textureResourcePath);
    }
}

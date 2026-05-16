using Manosaba.Characters.TonoHanna.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Manosaba.Characters.TonoHanna;

internal static class PuppetCollectionHelper
{
    public static bool HasUsedInCombat<T>(Creature owner) where T : PuppetCollectionPowerBase =>
        owner.GetPower<T>() is { Amount: > 0 };
}

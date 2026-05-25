using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.TonoHanna.Powers;

/// <summary>Same energy retention as vanilla <see cref="MegaCrit.Sts2.Core.Models.Relics.IceCream"/>.</summary>
public sealed class EnergyInFridgePower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override bool ShouldPlayerResetEnergy(Player player)
    {
        if (player.Creature.CombatState?.RoundNumber == 1)
        {
            return true;
        }

        if (player.Creature != Owner)
        {
            return true;
        }

        return false;
    }
}

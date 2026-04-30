using BaseLib.Utils;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Manosaba.Characters.Common.Relics;

[Pool(typeof(EventRelicPool))]
public sealed class RitualSwordBloodied : PathCustomRelicModel
{
    private const int MajokaAmount = 100;

    [SavedProperty]
    public bool Triggered { get; set; }

    public override RelicRarity Rarity => RelicRarity.Event;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        _ = combatState;
        if (Triggered || Owner?.Creature == null || side != Owner.Creature.Side)
        {
            return;
        }

        Triggered = true;
        Flash();
        await PowerCmd.Apply<MajokaPower>(Owner.Creature, MajokaAmount, Owner.Creature, null);
    }
}

using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace manosaba.Characters.TonoHanna.Relics;

[Pool(typeof(TonoHannaRelicPool))]
public sealed class Frugal : PathCustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("GoldPerEnergy", 5m)];

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Creature.Side)
            return;

        if (!Owner.Creature.IsAlive)
            return;

        if (Owner.PlayerCombatState == null)
            return;

        int energyLeft = Owner.PlayerCombatState.Energy;
        if (energyLeft <= 0)
            return;

        int gold = energyLeft * DynamicVars["GoldPerEnergy"].IntValue;
        if (gold <= 0)
            return;

        await PlayerCmd.GainGold(gold, Owner);
    }
}

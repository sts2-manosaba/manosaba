using System;
using BaseLib.Utils;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.ShitoAlisa.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.ShitoAlisa;

[Pool(typeof(ShitoAlisaRelicPool))]
public sealed class LegIrons : LevelingPathCustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override int MaxRelicLevel => 5;

    public override async Task BeforeCombatStart()
    {
        if (Owner.Creature == null)
            return;

        decimal stacks = Math.Max(1, RelicLevel);
        await PowerCmd.Apply<FireballSwarmPower>(Owner.Creature, stacks, Owner.Creature, null);
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (RelicLevel < 2 || Owner.Creature == null)
            return;
        if (power is not FireballSwarmPower || power.Owner != Owner.Creature || amount <= 0m)
            return;

        await PowerCmd.Apply<MajokaPower>(Owner.Creature, amount * 5m, Owner.Creature, cardSource);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (RelicLevel < 4 || Owner.Creature == null || side != Owner.Creature.Side)
            return;

        decimal fireballStacks = Owner.Creature.GetPowerAmount<FireballSwarmPower>();
        if (fireballStacks <= 0m)
            return;

        await CreatureCmd.Heal(Owner.Creature, fireballStacks);
    }
}

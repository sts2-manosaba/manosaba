using System;
using Manosaba.Characters.Common.Monsters;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.KurobeNanoka.Powers;

public sealed class JailerRegenerationPower : PathCustomPowerModel
{
    private const string HealVar = "Heal";

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(HealVar, 0m)];

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        _ = applier;
        _ = cardSource;
        RefreshDerivedValues();
        return Task.CompletedTask;
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await base.AfterPowerAmountChanged(power, amount, applier, cardSource);

        if (power != this)
        {
            return;
        }

        RefreshDerivedValues();
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || player.PlayerCombatState == null || player.Creature.CombatState == null)
        {
            return;
        }

        Creature? jailer = player.PlayerCombatState.GetPet<Jailer>()
            ?? player.Creature.CombatState.Allies.FirstOrDefault(c => c.Monster is Jailer && c.PetOwner == player);
        if (jailer is not { IsAlive: true })
        {
            return;
        }

        decimal missingHp = jailer.MaxHp - jailer.CurrentHp;
        if (missingHp <= 0m)
        {
            return;
        }

        decimal healAmount = Math.Min(Amount, missingHp);
        if (healAmount <= 0m)
        {
            return;
        }

        await CreatureCmd.Heal(jailer, healAmount);
    }

    private void RefreshDerivedValues()
    {
        DynamicVars[HealVar].BaseValue = Amount;
        InvokeDisplayAmountChanged();
    }
}

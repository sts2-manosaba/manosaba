using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.TonoHanna.Powers;

/// <summary>Ice-cream-style energy retention plus per-stack energy at turn start (stacks from <see cref="Cards.EnergyInFridge"/>).</summary>
public sealed class EnergyInFridgePower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

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

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _ = choiceContext;

        if (player.Creature != Owner || Owner.Player == null || Amount <= 0m)
        {
            return;
        }

        await PlayerCmd.GainEnergy(Amount, Owner.Player);
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await base.AfterPowerAmountChanged(power, amount, applier, cardSource);

        if (power == this)
        {
            DynamicVars.Energy.BaseValue = Amount;
        }
    }
}

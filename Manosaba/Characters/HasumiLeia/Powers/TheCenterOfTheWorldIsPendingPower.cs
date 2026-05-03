using System;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.HasumiLeia.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.HasumiLeia.Powers;

/// <summary>
/// Delays TheCenterOfTheWorldIs' multiplayer selection prompt until end of the current turn.
/// Hidden/internal power.
/// </summary>
public sealed class TheCenterOfTheWorldIsPendingPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;
    public override bool ShouldPlayVfx => false;
    protected override bool IsVisibleInternal => false;

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)
        {
            return;
        }

        CombatState? combatState = Owner.CombatState;
        if (combatState?.Players == null || Owner.Player == null)
        {
            await PowerCmd.Remove(this);
            return;
        }

        bool anyChoseLeia = false;
        foreach (Player player in combatState.Players)
        {
            if (player == Owner.Player)
            {
                continue;
            }

            if (player?.Creature is not { } creature || !creature.IsAlive)
            {
                continue;
            }

            List<CardModel> options =
            [
                combatState.CreateCard<TheCenterOfTheWorldIs_LeiaChoice>(Owner.Player),
                combatState.CreateCard<TheCenterOfTheWorldIs_IgnoreChoice>(Owner.Player),
            ];

            CardModel? selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, player, canSkip: true);
            if (selected is TheCenterOfTheWorldIs_LeiaChoice)
            {
                anyChoseLeia = true;
            }
        }

        if (anyChoseLeia)
        {
            await PowerCmd.Apply<StrengthPower>(Owner, 4m, Owner, cardSource: null);
        }
        else
        {
            decimal currentMajoka = Owner.GetPowerAmount<MajokaPower>();
            decimal toApply = Math.Max(0m, 100m - currentMajoka);
            if (toApply > 0m)
            {
                await PowerCmd.Apply<MajokaPower>(Owner, toApply, Owner, cardSource: null);
            }
        }

        await PowerCmd.Remove(this);
    }
}

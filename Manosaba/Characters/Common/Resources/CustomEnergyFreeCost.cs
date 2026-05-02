using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.Common.Resources;

public static class CustomEnergyFreeCost
{
    private sealed class FreeCostState
    {
        public int UntilPlayed { get; set; }
        public int ThisTurn { get; set; }
        public int ThisTurnOrUntilPlayed { get; set; }
        public int ThisCombat { get; set; }

        public bool IsFree => UntilPlayed > 0 || ThisTurn > 0 || ThisTurnOrUntilPlayed > 0 || ThisCombat > 0;
    }

    private static readonly ConditionalWeakTable<CardModel, FreeCostState> States = new();

    public static bool IsFree(CardModel card)
    {
        return States.TryGetValue(card, out FreeCostState? state) && state.IsFree;
    }

    public static void MarkFree(CardModel card, LocalCostModifierExpiration expiration)
    {
        FreeCostState state = States.GetOrCreateValue(card);

        if (expiration == (LocalCostModifierExpiration.EndOfTurn | LocalCostModifierExpiration.WhenPlayed))
        {
            state.ThisTurnOrUntilPlayed++;
        }
        else if (expiration == LocalCostModifierExpiration.EndOfTurn)
        {
            state.ThisTurn++;
        }
        else if (expiration == LocalCostModifierExpiration.WhenPlayed)
        {
            state.UntilPlayed++;
        }
        else if (expiration == LocalCostModifierExpiration.EndOfCombat)
        {
            state.ThisCombat++;
        }

        card.InvokeEnergyCostChanged();
    }

    public static void ClearEndOfTurn(CardModel card)
    {
        if (!States.TryGetValue(card, out FreeCostState? state))
        {
            return;
        }

        bool wasFree = state.IsFree;
        state.ThisTurn = 0;
        state.ThisTurnOrUntilPlayed = 0;
        RemoveIfEmpty(card, state);
        if (wasFree)
        {
            card.InvokeEnergyCostChanged();
        }
    }

    public static void ClearAfterPlayed(CardModel card)
    {
        if (!States.TryGetValue(card, out FreeCostState? state))
        {
            return;
        }

        bool wasFree = state.IsFree;
        state.UntilPlayed = 0;
        state.ThisTurnOrUntilPlayed = 0;
        RemoveIfEmpty(card, state);
        if (wasFree)
        {
            card.InvokeEnergyCostChanged();
        }
    }

    private static void RemoveIfEmpty(CardModel card, FreeCostState state)
    {
        if (!state.IsFree)
        {
            States.Remove(card);
        }
    }
}

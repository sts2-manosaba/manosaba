using BaseLib.Utils;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.ShitoAlisa.Powers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System.Runtime.CompilerServices;

namespace manosaba.Characters.ShitoAlisa;

public static class ShitoCombustOperations
{
    private sealed class ExternalCombustState
    {
        public int Max;
        public int Current;
    }

    private static readonly ConditionalWeakTable<CardModel, ExternalCombustState> ExternalCombust = new();

    public static bool TryGetCombustVar(CardModel card, out ShitoCombustDynamicVar? var)
    {
        var = null;
        if (!card.DynamicVars.ContainsKey("Combust"))
            return false;
        var = card.DynamicVars["Combust"] as ShitoCombustDynamicVar;
        return var != null;
    }

    public static bool CanAttachCombust(CardModel card)
    {
        if (card.Keywords.Contains(ManosabaKeywords.CombustIgnite))
            return false;

        if (TryGetCombustVar(card, out ShitoCombustDynamicVar? v) && v != null)
        {
            if (card.Keywords.Contains(ManosabaKeywords.Combust))
                return false;
            return v.MaxCombust <= 0m;
        }

        // Non-Shito cards do not have the Combust dynamic var; allow external combust state.
        return !card.Keywords.Contains(ManosabaKeywords.Combust);
    }

    public static void AttachCombust(CardModel card, int amount)
    {
        if (TryGetCombustVar(card, out ShitoCombustDynamicVar? v) && v != null)
        {
            v.SetCombustFromExternal(amount);
        }
        else
        {
            ExternalCombustState state = ExternalCombust.GetOrCreateValue(card);
            state.Max = amount;
            state.Current = amount;
        }

        card.AddKeyword(ManosabaKeywords.Combust);
    }

    public static bool HasActiveCombust(CardModel card)
        => TryGetCombustState(card, out int current, out _) && current > 0;

    public static bool TryGetCombustState(CardModel card, out int current, out int max)
    {
        current = 0;
        max = 0;

        if (TryGetCombustVar(card, out ShitoCombustDynamicVar? v) && v != null)
        {
            current = (int)v.CurrentCombust;
            max = (int)v.MaxCombust;
            return max > 0;
        }

        if (ExternalCombust.TryGetValue(card, out ExternalCombustState? state) && state.Max > 0)
        {
            current = state.Current;
            max = state.Max;
            return true;
        }

        return false;
    }

    public static async Task ApplyIgniteToCard(PlayerChoiceContext choiceContext, CardModel target, decimal amount)
    {
        int before;
        int after;
        int max;

        if (TryGetCombustVar(target, out ShitoCombustDynamicVar? v) && v != null && v.MaxCombust > 0m)
        {
            before = (int)v.CurrentCombust;
            v.ApplyIgnite(amount);
            after = (int)v.CurrentCombust;
            max = (int)v.MaxCombust;
            RefreshCombustVisual(target);
            Log.Debug($"[Manosaba Combust] Ignite card={target.Id.Entry} -{amount} {before}->{after} (max={max})");
            if (v.CurrentCombust > 0m)
                return;
            await FinalCountdownPower.ReduceFromCombust(target.Owner.Creature, target);
            await GrantFireballSwarmOnCombustZero(target);
            v.ResetCurrentToMax();
            RefreshCombustVisual(target);
            await CardCmd.AutoPlay(choiceContext, target, null);
            return;
        }

        if (!ExternalCombust.TryGetValue(target, out ExternalCombustState? state) || state.Max <= 0)
            return;

        before = state.Current;
        state.Current = Math.Max(0, state.Current - (int)amount);
        after = state.Current;
        max = state.Max;
        RefreshCombustVisual(target);
        Log.Debug($"[Manosaba Combust] Ignite card={target.Id.Entry} -{amount} {before}->{after} (max={max})");
        if (state.Current > 0)
            return;
        await FinalCountdownPower.ReduceFromCombust(target.Owner.Creature, target);
        await GrantFireballSwarmOnCombustZero(target);
        state.Current = state.Max;
        RefreshCombustVisual(target);
        await CardCmd.AutoPlay(choiceContext, target, null);
    }

    public static async Task AfterCardDrawn(CardModel self, PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != self)
            return;

        if (TryGetCombustVar(self, out ShitoCombustDynamicVar? v) && v != null && v.MaxCombust > 0m)
        {
            int before = (int)v.CurrentCombust;
            v.TickOnDraw();
            int after = (int)v.CurrentCombust;
            RefreshCombustVisual(self);
            Log.Debug($"[Manosaba Combust] Draw tick card={self.Id.Entry} fromHandDraw={fromHandDraw} {before}->{after} (max={(int)v.MaxCombust})");
            if (v.CurrentCombust > 0m)
                return;

            await FinalCountdownPower.ReduceFromCombust(self.Owner.Creature, self);
            await GrantFireballSwarmOnCombustZero(self);
            v.ResetCurrentToMax();
            RefreshCombustVisual(self);
            Log.Debug($"[Manosaba Combust] AutoPlay triggered card={self.Id.Entry}; reset to {(int)v.CurrentCombust}");
            await CardCmd.AutoPlay(choiceContext, self, null);
            return;
        }

        if (!ExternalCombust.TryGetValue(self, out ExternalCombustState? state) || state.Max <= 0)
            return;

        int beforeExternal = state.Current;
        state.Current = Math.Max(0, state.Current - 1);
        int afterExternal = state.Current;
        RefreshCombustVisual(self);
        Log.Debug($"[Manosaba Combust] Draw tick card={self.Id.Entry} fromHandDraw={fromHandDraw} {beforeExternal}->{afterExternal} (max={state.Max})");
        if (state.Current > 0)
            return;

        await FinalCountdownPower.ReduceFromCombust(self.Owner.Creature, self);
        await GrantFireballSwarmOnCombustZero(self);
        state.Current = state.Max;
        RefreshCombustVisual(self);
        Log.Debug($"[Manosaba Combust] AutoPlay triggered card={self.Id.Entry}; reset to {state.Current}");
        await CardCmd.AutoPlay(choiceContext, self, null);
    }

    private static async Task GrantFireballSwarmOnCombustZero(CardModel card)
    {
        if (card.Owner?.Creature is not { } ownerCreature || !ownerCreature.IsAlive)
            return;

        await PowerCmd.Apply<FireballSwarmPower>(ownerCreature, 1m, ownerCreature, null);
    }

    private static void RefreshCombustVisual(CardModel card)
    {
        if (!card.Keywords.Contains(ManosabaKeywords.Combust))
            return;

        // NCard does not subscribe to dynamic-var changes; poke keyword changed event to force Reload().
        card.RemoveKeyword(ManosabaKeywords.Combust);
        card.AddKeyword(ManosabaKeywords.Combust);
    }
}

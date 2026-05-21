using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Utils;

internal static class CombatStateEncounterRef
{
    private static readonly VariableReference<EncounterModel?> Encounter;

    static CombatStateEncounterRef()
    {
        Type? combatStateType = Type.GetType("MegaCrit.Sts2.Core.Combat.ICombatState, sts2")
            ?? Type.GetType("MegaCrit.Sts2.Core.Combat.CombatState, sts2");
        if (combatStateType == null)
        {
            throw new InvalidOperationException("Failed to resolve combat state type for Encounter access.");
        }

        Encounter = new VariableReference<EncounterModel?>(combatStateType, "Encounter");
    }

    public static EncounterModel? Get(object? combatState) =>
        combatState == null ? null : Encounter.Get(combatState);
}

/// <summary>
/// Thin wrappers around <see cref="BetaMainCompatibility"/> for main/beta dual-branch combat APIs.
/// </summary>
public static class ManosabaCombatCompat
{
    public static CombatStateWrapper? From(CardModel card) =>
        BetaMainCompatibility.CardModel_.WrappedCombatState(card);

    public static CombatStateWrapper? From(Creature? creature) =>
        creature == null ? null : BetaMainCompatibility.Creature_.WrappedCombatState(creature);

    public static object? RawFrom(CardModel card) =>
        BetaMainCompatibility.CardModel_.CombatState.Get(card);

    public static object? RawFrom(Creature? creature) =>
        creature == null ? null : BetaMainCompatibility.Creature_.CombatState.Get(creature);

    /// <summary>Hand preview: card combat state is often null; fall back to owner's creature.</summary>
    public static CombatStateWrapper? FromCardOrOwner(CardModel card) =>
        From(card) ?? From(card.Owner?.Creature);

    public static object? RawFromCardOrOwner(CardModel card) =>
        RawFrom(card) ?? RawFrom(card.Owner?.Creature);

    public static bool HasCombatState(Creature creature) =>
        BetaMainCompatibility.Creature_.CombatState.Get(creature) != null;

    public static EncounterModel? GetEncounter(Creature creature) =>
        CombatStateEncounterRef.Get(RawFrom(creature));

    /// <summary>Cross-branch replacement for <see cref="AttackCommand.TargetingAllOpponents"/>.</summary>
    public static AttackCommand TargetingAllOpponentsCompat(this AttackCommand cmd, object? combatState)
    {
        if (combatState != null)
        {
            BetaMainCompatibility.AttackCommand_.TargetingAllOpponents.Invoke(cmd, combatState);
        }

        return cmd;
    }

    public static AttackCommand TargetingAllOpponentsCompat(this AttackCommand cmd, CardModel card)
    {
        return cmd.TargetingAllOpponentsCompat(RawFrom(card) ?? RawFrom(card.Owner?.Creature));
    }
}

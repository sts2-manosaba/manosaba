using Manosaba.Characters.JogasakiNoa.Orbs;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Manosaba.Characters.JogasakiNoah.Cards;

internal static class SekketsusoujitsuHelper
{
    public readonly record struct BloodOrbEvokeResult(decimal PassiveValue, decimal Layers);

    public static decimal BloodOrbDamageBonus(CardModel card)
    {
        return GetHighestPassiveBloodOrb(card.Owner)?.PassiveVal ?? 0m;
    }

    public static bool HasBloodOrb(Player? player)
    {
        return player?.PlayerCombatState?.OrbQueue?.Orbs.Any(orb => orb is BloodOrb) == true;
    }

    public static async Task<decimal> EvokeNextBloodOrb(PlayerChoiceContext choiceContext, Player owner)
    {
        BloodOrbEvokeResult? result = await EvokeHighestPassiveBloodOrb(choiceContext, owner);
        return result?.PassiveValue ?? 0m;
    }

    public static async Task<BloodOrbEvokeResult?> EvokeHighestPassiveBloodOrb(PlayerChoiceContext choiceContext, Player owner)
    {
        OrbQueue? queue = owner.PlayerCombatState?.OrbQueue;
        if (queue == null || owner.Creature?.CombatState == null)
        {
            return null;
        }

        BloodOrb? bloodOrb = GetHighestPassiveBloodOrb(owner);
        if (bloodOrb == null || !queue.Orbs.Contains(bloodOrb))
        {
            return null;
        }

        BloodOrbEvokeResult result = new(bloodOrb.PassiveVal, bloodOrb.Layers);
        bool removed = queue.Remove(bloodOrb);
        NCombatRoom.Instance?.GetCreatureNode(owner.Creature)?.OrbManager?.EvokeOrbAnim(bloodOrb);

        IEnumerable<Creature> targets;
        choiceContext.PushModel(bloodOrb);
        try
        {
            targets = await bloodOrb.Evoke(choiceContext);
        }
        finally
        {
            choiceContext.PopModel(bloodOrb);
        }

        await Hook.AfterOrbEvoked(choiceContext, owner.Creature.CombatState, bloodOrb, targets);
        if (removed)
        {
            bloodOrb.RemoveInternal();
        }

        return result;
    }

    private static BloodOrb? GetHighestPassiveBloodOrb(Player? owner)
    {
        return owner?.PlayerCombatState?.OrbQueue?.Orbs
            .OfType<BloodOrb>()
            .OrderByDescending(orb => orb.PassiveVal)
            .ThenByDescending(orb => orb.Layers)
            .FirstOrDefault();
    }

}

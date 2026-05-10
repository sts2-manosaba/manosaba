using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Characters.JogasakiNoah.Powers;
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
    public static decimal RedScaleBonusMultiplier(CardModel card)
    {
        return card.Owner?.Creature?.HasPower<SekirinyakudouPower>() == true ? 1m : 0m;
    }

    public static decimal RedScaleDamageBonus(CardModel card, decimal baseDamage)
    {
        return RedScaleBonusMultiplier(card) * Math.Floor(baseDamage * 0.5m);
    }

    public static decimal BloodOrbDamageBonus(CardModel card)
    {
        return card.Owner?.PlayerCombatState?.OrbQueue?.Orbs
            .OfType<BloodOrb>()
            .FirstOrDefault()?.PassiveVal ?? 0m;
    }

    public static bool HasBloodOrb(Player? player)
    {
        return player?.PlayerCombatState?.OrbQueue?.Orbs.Any(orb => orb is BloodOrb) == true;
    }

    public static async Task<decimal> EvokeNextBloodOrb(PlayerChoiceContext choiceContext, Player owner)
    {
        OrbQueue? queue = owner.PlayerCombatState?.OrbQueue;
        if (queue == null || owner.Creature?.CombatState == null)
        {
            return 0m;
        }

        BloodOrb? bloodOrb = queue.Orbs.OfType<BloodOrb>().FirstOrDefault();
        if (bloodOrb == null || !queue.Orbs.Contains(bloodOrb))
        {
            return 0m;
        }

        decimal damageBonus = bloodOrb.PassiveVal;
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

        return damageBonus;
    }

    public static void AddLayersToBloodOrbs(Player? owner, decimal amount)
    {
        foreach (BloodOrb bloodOrb in owner?.PlayerCombatState?.OrbQueue?.Orbs.OfType<BloodOrb>() ?? [])
        {
            bloodOrb.AddLayers(amount);
        }
    }
}

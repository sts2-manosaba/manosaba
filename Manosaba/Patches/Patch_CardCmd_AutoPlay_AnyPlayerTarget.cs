using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(CardCmd), nameof(CardCmd.AutoPlay))]
public static class Patch_CardCmd_AutoPlay_AnyPlayerTarget
{
    [HarmonyPrefix]
    private static void Prefix(CardModel card, ref Creature? target)
    {
        if (target != null || card.TargetType != TargetType.AnyPlayer)
        {
            return;
        }

        if (card.Owner is not { } owner)
        {
            return;
        }

        if ((card.CombatState ?? owner.Creature?.CombatState) is not { } combatState)
        {
            return;
        }

        List<Player> players = combatState.Players
            .Where(player => player?.Creature?.IsAlive == true)
            .ToList();
        target = players.Count == 0
            ? null
            : owner.RunState.Rng.CombatTargets.NextItem(players)?.Creature;
    }
}

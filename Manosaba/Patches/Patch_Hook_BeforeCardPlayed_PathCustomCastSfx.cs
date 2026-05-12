using HarmonyLib;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Patches;

/// <summary>
/// Plays owner cast animation/sound once for all <see cref="PathCustomCardModel"/> Skill, Power, and Quest cards
/// (BaseLib <c>CustomCastSfx</c> on <c>card.Owner.Character</c>), including <see cref="manosaba.Characters.Common.CommonCardPool"/> cards:
/// whoever owns/plays the card supplies the cast SFX, same as character-specific pools.
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCardPlayed))]
public static class Patch_Hook_BeforeCardPlayed_PathCustomCastSfx
{
    [HarmonyPostfix]
    private static Task BeforeCardPlayedPostfix(Task __result, CombatState combatState, CardPlay cardPlay)
        => BeforeCardPlayedWrapped(__result, cardPlay);

    private static async Task BeforeCardPlayedWrapped(Task originalTask, CardPlay cardPlay)
    {
        await originalTask;
        if (cardPlay.Card is not PathCustomCardModel pathCard)
            return;
        if (pathCard.Type != CardType.Skill && pathCard.Type != CardType.Power && pathCard.Type != CardType.Quest)
            return;
        await pathCard.PlayOwnerCastAnimAsync();
    }
}

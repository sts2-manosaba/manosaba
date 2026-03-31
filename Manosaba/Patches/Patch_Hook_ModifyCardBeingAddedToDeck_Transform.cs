using HarmonyLib;
using Manosaba;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyCardBeingAddedToDeck))]
public static class Patch_Hook_ModifyCardBeingAddedToDeck_Transform
{
    public static void Postfix(IRunState runState, CardModel card, ref CardModel __result)
    {
        if (!ManosabaFeatureFlags.AprilFoolsModeEnabled)
            return;

        if (__result == null || !__result.IsTransformable)
            return;

        Rng? rng = runState.CurrentRoom switch
        {
            MerchantRoom => card.Owner.PlayerRng.Shops,
            CombatRoom => card.Owner.PlayerRng.Rewards,
            _ => null
        };

        if (rng == null)
            return;

        var owner = card.Owner;
        var ownerPoolCards = owner.Character.CardPool
            .GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint);

        __result = CardFactory.CreateRandomCardForTransform(__result, ownerPoolCards, isInCombat: false, rng);
    }
}

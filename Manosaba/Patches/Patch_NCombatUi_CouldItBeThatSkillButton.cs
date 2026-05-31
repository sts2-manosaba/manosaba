using HarmonyLib;
using Manosaba.Characters.TachibanaSherry.Combat;
using Manosaba.Characters.TachibanaSherry.Visuals;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(NCombatUi))]
public static class Patch_NCombatUi_CouldItBeThatSkillButton
{
    [HarmonyPatch(nameof(NCombatUi.Activate))]
    [HarmonyPostfix]
    private static void Activate_Postfix(NCombatUi __instance, CombatState state)
    {
        Player? me = TryResolveLocalPlayer(state);
        if (me == null)
        {
            CouldItBeThatSkillButtonUi.RemoveFrom(__instance);
            return;
        }

        CouldItBeThatSkillButtonUi.RefreshIfPresent(__instance, state);
    }

    [HarmonyPatch(nameof(NCombatUi.Deactivate))]
    [HarmonyPostfix]
    private static void Deactivate_Postfix(NCombatUi __instance)
    {
        CouldItBeThatSkillButtonUi.RemoveFrom(__instance);
        CouldItBeThatSkillActivation.ClearAllActivationPending();
    }

    private static Player? TryResolveLocalPlayer(CombatState state)
    {
        try
        {
            return LocalContext.GetMe(state);
        }
        catch (Exception)
        {
            return state.Players.Count == 1 ? state.Players[0] : null;
        }
    }
}

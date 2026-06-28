using HarmonyLib;
using manosaba.Characters.SawatariCoco.Combat;
using manosaba.Characters.SawatariCoco.Visuals;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(NCombatUi))]
public static class Patch_NCombatUi_FanServiceSkillButton
{
    [HarmonyPatch(nameof(NCombatUi.Activate))]
    [HarmonyPostfix]
    private static void Activate_Postfix(NCombatUi __instance, ICombatState state)
    {
        Player? me = TryResolveLocalPlayer(state);
        if (me == null)
        {
            FanServiceSkillButtonUi.RemoveFrom(__instance);
            return;
        }

        FanServiceSkillButtonUi.RefreshIfPresent(__instance, state);
    }

    [HarmonyPatch(nameof(NCombatUi.Deactivate))]
    [HarmonyPostfix]
    private static void Deactivate_Postfix(NCombatUi __instance)
    {
        FanServiceSkillButtonUi.RemoveFrom(__instance);
        FanServiceSkillActivation.ClearAllActivationPending();
    }

    private static Player? TryResolveLocalPlayer(ICombatState state)
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

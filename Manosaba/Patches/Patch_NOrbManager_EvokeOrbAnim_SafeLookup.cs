using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Orbs;

namespace Manosaba.Patches;

/// <summary>
/// Vanilla <see cref="NOrbManager.EvokeOrbAnim"/> uses <c>Last</c> and throws when no matching visual exists
/// (e.g. after a failed <see cref="NOrb.UpdateVisuals"/>). Skip the animation instead of crashing combat.
/// </summary>
[HarmonyPatch(typeof(NOrbManager), nameof(NOrbManager.EvokeOrbAnim))]
public static class Patch_NOrbManager_EvokeOrbAnim_SafeLookup
{
    private static readonly AccessTools.FieldRef<NOrbManager, List<NOrb>> OrbsRef =
        AccessTools.FieldRefAccess<NOrbManager, List<NOrb>>("_orbs");

    [HarmonyPrefix]
    private static bool Prefix(NOrbManager __instance, OrbModel orb)
    {
        List<NOrb> orbs = OrbsRef(__instance);
        return orbs.Any(node => node.Model == orb);
    }
}

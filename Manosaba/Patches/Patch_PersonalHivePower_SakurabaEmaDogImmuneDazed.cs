using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Threading.Tasks;

[HarmonyPatch(typeof(PersonalHivePower), nameof(PersonalHivePower.AfterDamageReceived))]
public static class Patch_PersonalHivePower_SakurabaEmaDogImmuneDazed
{
    private static bool Prefix(PersonalHivePower __instance, Creature target, Creature? dealer, ref Task __result)
    {
        // Pets do not have a player card pile, so Personal Hive cannot add Dazed to them safely.
        if (target != __instance.Owner || dealer?.PetOwner == null)
        {
            return true;
        }

        __result = Task.CompletedTask;
        return false;
    }
}

using HarmonyLib;
using Manosaba.Characters.Common.Monsters;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

[HarmonyPatch(typeof(PersonalHivePower), nameof(PersonalHivePower.AfterDamageReceived))]
public static class Patch_PersonalHivePower_SakurabaEmaDogImmuneDazed
{
    private static bool Prefix(PersonalHivePower __instance, Creature target, ValueProp props, Creature? dealer)
    {
        // Keep default behavior unless this is exactly Personal Hive retaliating to Sakuraba Ema Dog's powered attack.
        if (target == __instance.Owner && dealer?.Monster is SakurabaEmaDog)
        {
            return false;
        }

        return true;
    }
}

using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Manosaba.Patches;

internal static class EntomancerPersonalHiveFilter
{
    private static readonly AsyncLocal<int> PotionThrowDamageDepth = new();

    public static bool IsPotionThrowDamage => PotionThrowDamageDepth.Value > 0;

    public static IDisposable BeginPotionThrowDamageScope()
    {
        PotionThrowDamageDepth.Value++;
        return new PotionThrowDamageScope();
    }

    private sealed class PotionThrowDamageScope : IDisposable
    {
        public void Dispose()
        {
            PotionThrowDamageDepth.Value = Math.Max(0, PotionThrowDamageDepth.Value - 1);
        }
    }
}

[HarmonyPatch(typeof(PersonalHivePower), nameof(PersonalHivePower.AfterDamageReceived))]
public static class Patch_PersonalHivePower_FilterDazed
{
    private static bool Prefix(PersonalHivePower __instance, Creature target, Creature? dealer, ref Task __result)
    {
        if (target == __instance.Owner && EntomancerPersonalHiveFilter.IsPotionThrowDamage)
        {
            __result = Task.CompletedTask;
            return false;
        }

        // Pets do not have a player card pile, so Personal Hive cannot add Dazed to them safely.
        if (target != __instance.Owner || dealer?.PetOwner == null)
        {
            return true;
        }

        __result = Task.CompletedTask;
        return false;
    }
}

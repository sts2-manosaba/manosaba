using HarmonyLib;
using Manosaba.Multiplayer;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Runs;
using System.Runtime.CompilerServices;

namespace Manosaba.Patches;

public static class Patch_Difficulties
{
    private static ConditionalWeakTable<Creature, object> _hpAppliedAfterAdded = new();
    private static readonly object _hpAppliedMarker = new();

    private static bool ShouldApplyEnemyHpMultiplier(Creature creature)
    {
        return creature.IsEnemy && !creature.IsDead && ManosabaLobbyDifficultyState.GetEnableEnemyHpMultiplierForGameplay();
    }

    private static decimal ScaleEnemyHp(decimal rawHp)
    {
        decimal multiplier = ManosabaLobbyDifficultyState.GetEnemyHpMultiplierForGameplay();
        return Math.Max(1m, Math.Round(rawHp * multiplier, MidpointRounding.AwayFromZero));
    }

    [HarmonyPatch(typeof(Creature), nameof(Creature.AfterAddedToRoom))]
    private static class Patch_Creature_AfterAddedToRoom_HpMultiplier
    {
        private static void Postfix(Creature __instance)
        {
            if (!ShouldApplyEnemyHpMultiplier(__instance))
            {
                return;
            }

            if (_hpAppliedAfterAdded.TryGetValue(__instance, out _))
            {
                return;
            }
            _hpAppliedAfterAdded.Add(__instance, _hpAppliedMarker);

            int newMaxHp = (int)ScaleEnemyHp(__instance.MaxHp);
            __instance.SetMaxHpInternal(newMaxHp);
            __instance.SetCurrentHpInternal(newMaxHp);
        }
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.SetMaxHp))]
    private static class Patch_CreatureCmd_SetMaxHp_HpMultiplier
    {
        private static void Prefix(Creature creature, ref decimal amount)
        {
            if (!ShouldApplyEnemyHpMultiplier(creature))
            {
                return;
            }

            amount = ScaleEnemyHp(amount);
        }
    }

    [HarmonyPatch(typeof(RunState), nameof(RunState.CreateForNewRun))]
    private static class Patch_RunState_CreateForNewRun_ResetEnemyHpMultiplierCache
    {
        private static void Prefix()
        {
            ManosabaLobbyDifficultyState.ClearRunSnapshot();
        }

        private static void Postfix()
        {
            _hpAppliedAfterAdded = new ConditionalWeakTable<Creature, object>();
            ManosabaLobbyDifficultyState.FreezeForRun();
        }
    }
}

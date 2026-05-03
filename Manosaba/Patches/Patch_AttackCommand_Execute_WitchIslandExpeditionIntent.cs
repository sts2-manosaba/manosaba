using HarmonyLib;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.Common.Visuals;
using Manosaba.Input;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using System.Runtime.CompilerServices;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.Execute))]
public static class Patch_AttackCommand_Execute_WitchIslandExpeditionIntent
{
    private const string AttackIntentSfx = "event:/Manosaba/audio/SFX/attack_intent.wav";
    private const float IntentLeadSeconds = 0.3f;
    private const float MultiplayerParrySyncTimeoutSeconds = 2f;
    private const double MaxGuardWindowSeconds = 0.5d;

    private static readonly ConditionalWeakTable<AttackCommand, object> WrappedCommands = new();
    private static readonly object WrappedMarker = new();

    private static void Prefix(AttackCommand __instance)
    {
        if (WrappedCommands.TryGetValue(__instance, out _))
        {
            return;
        }

        WrappedCommands.Add(__instance, WrappedMarker);

        Traverse traverse = Traverse.Create(__instance);
        Func<Task>? existingBeforeDamage = traverse.Field("_beforeDamage").GetValue<Func<Task>>();
        Func<Task>? existingAfterAttackerAnim = traverse.Field("_afterAttackerAnim").GetValue<Func<Task>>();

        traverse.Field("_afterAttackerAnim").SetValue(async () =>
        {
            if (existingAfterAttackerAnim != null)
            {
                await existingAfterAttackerAnim();
            }

            IReadOnlyList<Creature> expeditionTargets = GetExpeditionTargets(__instance);
            if (expeditionTargets.Count > 0)
            {
                if (expeditionTargets.Any(LocalContext.IsMe))
                {
                    SfxCmd.Play(AttackIntentSfx);
                    WitchIslandExpeditionParryCueVisuals.PlayForLocalTarget(expeditionTargets, IntentLeadSeconds);
                }

                PerfectGuardInputTracker.OpenPerfectGuardWindow(
                    MaxGuardWindowSeconds,
                    expeditionTargets.Select(creature => creature.Player!.NetId));
                await Cmd.Wait(IntentLeadSeconds);
            }
        });

        traverse.Field("_beforeDamage").SetValue(async () =>
        {
            bool hasExpeditionTargets = GetExpeditionTargets(__instance).Count > 0;
            if (hasExpeditionTargets)
            {
                PerfectGuardInputTracker.ClosePerfectGuardWindow();
                if (RunManager.Instance.NetService?.Type.IsMultiplayer() == true)
                {
                    await PerfectGuardInputTracker.ResolveMultiplayerParriesAsync(MultiplayerParrySyncTimeoutSeconds);
                }
            }

            if (existingBeforeDamage != null)
            {
                await existingBeforeDamage();
            }

            PerfectGuardInputTracker.BeginDamageResolution(__instance, hasExpeditionTargets);
        });
    }

    private static void Postfix(AttackCommand __instance, ref Task<AttackCommand> __result)
    {
        __result = EndAttackWhenComplete(__instance, __result);
    }

    private static async Task<AttackCommand> EndAttackWhenComplete(AttackCommand command, Task<AttackCommand> result)
    {
        try
        {
            return await result;
        }
        finally
        {
            PerfectGuardInputTracker.EndAttack(command);
        }
    }

    private static IReadOnlyList<Creature> GetExpeditionTargets(AttackCommand command)
    {
        Creature? attacker = command.Attacker;
        if (attacker == null || !attacker.IsEnemy || !attacker.IsMonster)
        {
            return [];
        }

        ValueProp props = command.DamageProps;
        if (!props.HasFlag(ValueProp.Move) || props.HasFlag(ValueProp.Unpowered))
        {
            return [];
        }

        return GetPossibleTargets(command).Where(IsExpeditionHolder).ToArray();
    }

    private static IEnumerable<Creature> GetPossibleTargets(AttackCommand command)
    {
        Traverse traverse = Traverse.Create(command);

        Creature? singleTarget = traverse.Field("_singleTarget").GetValue<Creature?>();
        if (singleTarget != null)
        {
            return [singleTarget];
        }

        CombatState? combatState = traverse.Field("_combatState").GetValue<CombatState?>();
        return combatState?.PlayerCreatures ?? [];
    }

    private static bool IsExpeditionHolder(Creature creature)
    {
        return creature.IsAlive
            && creature.Player != null
            && creature.HasPower<WitchIslandExpeditionPower>();
    }
}

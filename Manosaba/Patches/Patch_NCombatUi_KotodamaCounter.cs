using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using Manosaba.Characters.NatsumeAnan.Visuals;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(NCombatUi))]
public static class Patch_NCombatUi_KotodamaCounter
{
    private const string CounterNodeName = "KotodamaCounterInjected";
    private const string CounterScenePath = "res://Manosaba/scenes/natsume_anan/kotodama_counter.tscn";
    private const string CounterScenePathAlt = "res://Manosaba/scenes/natsume_anan/kotodama_energy_counter.tscn";
    private const string StarCounterScenePath = "res://scenes/combat/energy_counters/star_counter.tscn";
    private static readonly Vector2 CounterOffsetFromEnergyCounter = new(108f, 92f);

    [HarmonyPatch(nameof(NCombatUi._Ready))]
    [HarmonyPrefix]
    private static void Ready_Prefix(NCombatUi __instance)
    {
        Node? starByUniqueName = __instance.GetNodeOrNull<Node>("%StarCounter");
        if (starByUniqueName is NStarCounter)
        {
            return;
        }

        if (starByUniqueName != null)
        {
            // Prevent wrong node type from being resolved by %StarCounter.
            starByUniqueName.UniqueNameInOwner = false;
            if (starByUniqueName.Name == "StarCounter")
            {
                starByUniqueName.Name = "StarCounterWrongBinding";
            }
        }

        NStarCounter? actual = FindDescendantByType<NStarCounter>(__instance);
        if (actual == null)
        {
            PackedScene? packed = ResourceLoader.Load<PackedScene>(StarCounterScenePath);
            if (packed == null)
            {
                GD.PushWarning("[Manosaba] Could not load core star counter scene.");
                return;
            }

            actual = packed.Instantiate<NStarCounter>(PackedScene.GenEditState.Disabled);
            Control? energyContainer = __instance.GetNodeOrNull<Control>("%EnergyCounterContainer")
                ?? FindDescendantByName(__instance, "EnergyCounterContainer") as Control;
            if (energyContainer != null)
            {
                energyContainer.AddChild(actual);
            }
            else
            {
                __instance.AddChild(actual);
            }
        }

        actual.Name = "StarCounter";
        actual.Owner = __instance;
        actual.UniqueNameInOwner = true;
    }

    [HarmonyPatch(nameof(NCombatUi.Activate))]
    [HarmonyPostfix]
    private static void Activate_Postfix(NCombatUi __instance, CombatState state)
    {
        try
        {
            if (state == null)
            {
                RemoveCounters(__instance);
                return;
            }

            Player? me = ResolveTargetPlayer(state);
            if (me == null)
            {
                RemoveCounters(__instance);
                return;
            }

            RemoveCounters(__instance);

            PackedScene? packed = LoadCounterScene();
            if (packed == null)
            {
                GD.PushWarning("[Manosaba] Kotodama counter scene not found.");
                return;
            }

            NKotodamaCounter node = packed.Instantiate<NKotodamaCounter>(PackedScene.GenEditState.Disabled);
            node.Name = CounterNodeName;
            node.Initialize(me);

            Control? energyCounterContainer = __instance.GetNodeOrNull<Control>("%EnergyCounterContainer")
                ?? FindDescendantByName(__instance, "EnergyCounterContainer") as Control;
            if (energyCounterContainer != null)
            {
                if (energyCounterContainer.GetParent() is Node parent)
                {
                    parent.AddChild(node);
                }
                else
                {
                    __instance.AddChild(node);
                }

                node.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
                node.ZIndex = 0;
                node.GlobalPosition = energyCounterContainer.GlobalPosition + CounterOffsetFromEnergyCounter;
                node.AnimIn();
            }
            else
            {
                __instance.AddChild(node);
                node.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
                node.ZIndex = 0;
                node.OffsetLeft = 232f;
                node.OffsetTop = -212f;
                node.OffsetRight = 360f;
                node.OffsetBottom = -84f;
                node.AnimIn();
            }
        }
        catch (Exception ex)
        {
            GD.PushWarning($"[Manosaba] Kotodama counter Activate patch skipped due to exception: {ex.Message}");
        }
    }

    [HarmonyPatch(nameof(NCombatUi.Deactivate))]
    [HarmonyPostfix]
    private static void Deactivate_Postfix(NCombatUi __instance)
    {
        RemoveCounters(__instance);
    }

    [HarmonyPatch(nameof(NCombatUi.AnimOut))]
    [HarmonyPostfix]
    private static void AnimOut_Postfix(NCombatUi __instance)
    {
        foreach (NKotodamaCounter counter in FindDescendantsByType<NKotodamaCounter>(__instance))
        {
            if (!counter.IsQueuedForDeletion())
            {
                counter.AnimOut();
            }
        }
    }

    private static void RemoveCounters(NCombatUi combatUi)
    {
        Node? node = FindDescendantByName(combatUi, CounterNodeName);
        node?.QueueFree();

        foreach (NKotodamaCounter counter in FindDescendantsByType<NKotodamaCounter>(combatUi))
        {
            if (!counter.IsQueuedForDeletion())
            {
                counter.QueueFree();
            }
        }
    }

    private static PackedScene? LoadCounterScene()
    {
        PackedScene? primary = ResourceLoader.Load<PackedScene>(CounterScenePath);
        if (primary != null)
        {
            return primary;
        }

        return ResourceLoader.Load<PackedScene>(CounterScenePathAlt);
    }

    private static Player? ResolveTargetPlayer(CombatState state)
    {
        try
        {
            Player? me = LocalContext.GetMe(state);
            if (me != null)
            {
                return me;
            }
        }
        catch (Exception)
        {
            // Fallback below.
        }

        // Single player fallback when LocalContext has not fully initialized yet.
        if (state.Players.Count == 1)
        {
            return state.Players[0];
        }

        return null;
    }

    private static Node? FindDescendantByName(Node root, string targetName)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child.Name == targetName)
            {
                return child;
            }

            Node? nested = FindDescendantByName(child, targetName);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private static T? FindDescendantByType<T>(Node root) where T : Node
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is T match)
            {
                return match;
            }

            T? nested = FindDescendantByType<T>(child);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private static List<T> FindDescendantsByType<T>(Node root) where T : Node
    {
        List<T> results = [];
        CollectDescendantsByType(root, results);
        return results;
    }

    private static void CollectDescendantsByType<T>(Node root, List<T> results) where T : Node
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is T match)
            {
                results.Add(match);
            }

            CollectDescendantsByType(child, results);
        }
    }
}

using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Manosaba.Characters.JogasakiNoah.Cards
{
    internal static class OrbGapChannelHelper
    {
        private static readonly FieldInfo? OrbQueueOrbsField = AccessTools.Field(typeof(OrbQueue), "_orbs");
        private static readonly FieldInfo? OrbManagerOrbsField = AccessTools.Field(typeof(NOrbManager), "_orbs");

        public static async Task ChannelAt(PlayerChoiceContext choiceContext, OrbModel orb, Player player, int insertIndex)
        {
            if (CombatManager.Instance.IsOverOrEnding)
            {
                return;
            }

            CombatState combatState = player.Creature.CombatState;
            OrbQueue orbQueue = player.PlayerCombatState.OrbQueue;
            if (player.Character.BaseOrbSlotCount == 0 && orbQueue.Capacity == 0)
            {
                await OrbCmd.AddSlots(player, 1);
            }

            orb.AssertMutable();
            orb.Owner = player;

            if (orbQueue.Orbs.Count >= orbQueue.Capacity)
            {
                await OrbCmd.EvokeNext(choiceContext, player);
            }

            if (!await orbQueue.TryEnqueue(orb))
            {
                return;
            }

            insertIndex = Math.Clamp(insertIndex, 0, orbQueue.Orbs.Count - 1);
            MoveLastOrbToInsertIndex(orbQueue, insertIndex);

            CombatManager.Instance.History.OrbChanneled(combatState, orb);
            orb.PlayChannelSfx();
            NCombatRoom.Instance?.GetCreatureNode(player.Creature)?.OrbManager?.AddOrbAnim();
            SyncOrbVisualOrder(player);
            await Hook.AfterOrbChanneled(combatState, choiceContext, player, orb);
        }

        private static void MoveLastOrbToInsertIndex(OrbQueue queue, int insertIndex)
        {
            if (OrbQueueOrbsField?.GetValue(queue) is not List<OrbModel> mutableOrbs || mutableOrbs.Count <= 1)
            {
                return;
            }

            int lastIndex = mutableOrbs.Count - 1;
            if (insertIndex >= lastIndex)
            {
                return;
            }

            OrbModel insertedOrb = mutableOrbs[lastIndex];
            mutableOrbs.RemoveAt(lastIndex);
            mutableOrbs.Insert(insertIndex, insertedOrb);
        }

        private static void SyncOrbVisualOrder(Player player)
        {
            NOrbManager? orbManager = NCombatRoom.Instance?.GetCreatureNode(player.Creature)?.OrbManager;
            if (orbManager == null)
            {
                return;
            }

            if (OrbManagerOrbsField?.GetValue(orbManager) is not List<NOrb> orbNodes)
            {
                return;
            }

            IReadOnlyList<OrbModel> queueOrbs = player.PlayerCombatState.OrbQueue.Orbs;
            int count = Math.Min(queueOrbs.Count, orbNodes.Count);
            for (int i = 0; i < count; i++)
            {
                OrbModel desired = queueOrbs[i];
                NOrb node = orbNodes[i];
                if (node.Model != desired)
                {
                    node.ReplaceOrb(desired);
                }
            }
        }
    }
}

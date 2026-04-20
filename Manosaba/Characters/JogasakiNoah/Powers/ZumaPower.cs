using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Manosaba.Characters.JogasakiNoah.Powers
{
    public class ZumaPower : PathCustomPowerModel
    {
        private static readonly AsyncLocal<int> ResolveDepth = new();
        private static readonly AsyncLocal<decimal> ActiveEvokeMultiplier = new();

        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override bool AllowNegative => false;

        public static decimal GetCurrentEvokeMultiplier()
        {
            decimal multiplier = ActiveEvokeMultiplier.Value;
            return multiplier > 0m ? multiplier : 1m;
        }

        public override async Task AfterOrbChanneled(PlayerChoiceContext choiceContext, Player player, OrbModel orb)
        {
            _ = orb;
            if (player != Owner.Player)
            {
                return;
            }

            await ResolveTriplesAfterOrbQueueChanged(choiceContext, player);
        }

        public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
        {
            _ = targets;
            Player? player = Owner.Player;
            if (player == null || orb.Owner != player)
            {
                return;
            }

            await ResolveTriplesAfterOrbQueueChanged(choiceContext, player);
        }

        private async Task ResolveTriplesAfterOrbQueueChanged(PlayerChoiceContext choiceContext, Player player)
        {
            if (player.Creature == null || ResolveDepth.Value > 0 || Amount < 1m)
            {
                return;
            }

            ResolveDepth.Value++;
            try
            {
                decimal multiplier = 2m;
                int safety = 0;
                while (safety++ < 24)
                {
                    IReadOnlyList<OrbModel>? orbs = player.PlayerCombatState?.OrbQueue?.Orbs;
                    if (orbs == null)
                    {
                        break;
                    }

                    int tripleStart = FindTripleStartIndex(orbs);
                    if (tripleStart < 0)
                    {
                        break;
                    }

                    decimal previous = ActiveEvokeMultiplier.Value;
                    ActiveEvokeMultiplier.Value = multiplier;
                    try
                    {
                        await EvokeTripleAt(choiceContext, player, tripleStart);
                    }
                    finally
                    {
                        ActiveEvokeMultiplier.Value = previous;
                    }

                    multiplier *= 2m;
                }
            }
            finally
            {
                ResolveDepth.Value--;
            }
        }

        private static async Task EvokeTripleAt(PlayerChoiceContext choiceContext, Player player, int startIndex)
        {
            OrbQueue? queue = player.PlayerCombatState?.OrbQueue;
            if (queue == null || queue.Orbs.Count < startIndex + 3 || player.Creature.CombatState == null)
            {
                return;
            }

            List<OrbModel> matchedOrbs = queue.Orbs.Skip(startIndex).Take(3).ToList();
            foreach (OrbModel matchedOrb in matchedOrbs)
            {
                if (!queue.Orbs.Contains(matchedOrb))
                {
                    continue;
                }

                bool removed = queue.Remove(matchedOrb);
                NCombatRoom.Instance?.GetCreatureNode(player.Creature)?.OrbManager?.EvokeOrbAnim(matchedOrb);

                IEnumerable<Creature> targets;
                choiceContext.PushModel(matchedOrb);
                try
                {
                    targets = await matchedOrb.Evoke(choiceContext);
                }
                finally
                {
                    choiceContext.PopModel(matchedOrb);
                }

                await Hook.AfterOrbEvoked(choiceContext, player.Creature.CombatState, matchedOrb, targets);

                if (removed)
                {
                    matchedOrb.RemoveInternal();
                }
            }
        }

        private static int FindTripleStartIndex(IReadOnlyList<OrbModel> orbs)
        {
            if (orbs.Count < 3)
            {
                return -1;
            }

            for (int i = 0; i <= orbs.Count - 3; i++)
            {
                OrbModel first = orbs[i];
                if (!IsPaintOrb(first))
                {
                    continue;
                }

                Type firstType = first.GetType();
                if (orbs[i + 1].GetType() == firstType && orbs[i + 2].GetType() == firstType)
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool IsPaintOrb(OrbModel orb) =>
            orb is RedPaintOrb
            or BluePaintOrb
            or YellowPaintOrb
            or OrangePaintOrb
            or GreenPaintOrb
            or PurplePaintOrb
            or BlackPaintOrb
            or WhitePaintOrb;
    }
}

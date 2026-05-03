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

namespace Manosaba.Characters.JogasakiNoah.Powers
{
    public class ZumaPower : PathCustomPowerModel
    {
        private static readonly AsyncLocal<int> ResolveDepth = new();
        private static readonly AsyncLocal<decimal> ActiveEvokeMultiplier = new();

        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;
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
                int combo = 1;
                int safety = 0;
                while (safety++ < 24)
                {
                    IReadOnlyList<OrbModel>? orbs = player.PlayerCombatState?.OrbQueue?.Orbs;
                    if (orbs == null)
                    {
                        break;
                    }

                    List<OrbModel> matchingRun = FindMatchingRun(orbs);
                    if (matchingRun.Count < 3)
                    {
                        break;
                    }

                    decimal previous = ActiveEvokeMultiplier.Value;
                    ActiveEvokeMultiplier.Value = GetComboMultiplier(combo);
                    try
                    {
                        await EvokeMatchingRun(choiceContext, player, matchingRun);
                    }
                    finally
                    {
                        ActiveEvokeMultiplier.Value = previous;
                    }

                    combo++;
                }
            }
            finally
            {
                ResolveDepth.Value--;
            }
        }

        private static decimal GetComboMultiplier(int combo)
        {
            return combo switch
            {
                <= 1 => 2m,
                2 => 4m,
                _ => 8m,
            };
        }

        private static async Task EvokeMatchingRun(PlayerChoiceContext choiceContext, Player player, IReadOnlyList<OrbModel> matchedOrbs)
        {
            OrbQueue? queue = player.PlayerCombatState?.OrbQueue;
            if (queue == null || matchedOrbs.Count < 3 || player.Creature.CombatState == null)
            {
                return;
            }

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

        private static List<OrbModel> FindMatchingRun(IReadOnlyList<OrbModel> orbs)
        {
            if (orbs.Count < 3)
            {
                return [];
            }

            int start = 0;
            while (start < orbs.Count)
            {
                OrbModel first = orbs[start];
                if (!IsPaintOrb(first))
                {
                    start++;
                    continue;
                }

                Type firstType = first.GetType();
                int end = start + 1;
                while (end < orbs.Count && orbs[end].GetType() == firstType)
                {
                    end++;
                }

                if (end - start >= 3)
                {
                    return orbs.Skip(start).Take(end - start).ToList();
                }

                start = end;
            }

            return [];
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

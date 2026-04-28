using Manosaba.Characters.KurobeNanoka.Helpers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace Manosaba.Characters.KurobeNanoka.Powers;

public sealed class RelicSearchPower : PathCustomPowerModel
{
    private const int RelicChancePercentPerRoll = 50;
    private int _resolvedRollCount;
    private bool _rewardQueuedForCombatEnd;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => Amount;
    public override bool AllowNegative => false;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        _ = applier;
        _ = cardSource;
        return ResolvePendingRolls("apply");
    }

    public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        _ = applier;
        _ = cardSource;

        if (power != this || amount <= 0m)
        {
            return Task.CompletedTask;
        }

        return ResolvePendingRolls("stack");
    }

    private Task ResolvePendingRolls(string source)
    {
        if (Owner.Player == null || Amount <= 0m)
        {
            return Task.CompletedTask;
        }

        int totalRolls = Math.Max(0, Amount);
        int newRolls = Math.Max(0, totalRolls - _resolvedRollCount);
        if (newRolls <= 0 || _rewardQueuedForCombatEnd)
        {
            return Task.CompletedTask;
        }

        Console.WriteLine($"[RelicSearch] {source}: player={Owner.Player.NetId}, amount={Amount}, resolved={_resolvedRollCount}, newRolls={newRolls}, chancePerRoll={RelicChancePercentPerRoll}%");

        for (int i = 0; i < newRolls; i++)
        {
            int roll = Owner.Player.RunState.Rng.Niche.NextInt(100);
            Console.WriteLine($"[RelicSearch] {source} roll {i + 1}/{newRolls}: {roll}");
            if (roll < RelicChancePercentPerRoll)
            {
                _rewardQueuedForCombatEnd = true;
                Console.WriteLine($"[RelicSearch] {source} success -> queued relic reward");
                break;
            }
        }

        _resolvedRollCount = totalRolls;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner.Player == null)
        {
            return Task.CompletedTask;
        }

        bool awarded = _rewardQueuedForCombatEnd;
        _resolvedRollCount = 0;
        _rewardQueuedForCombatEnd = false;

        if (!awarded)
        {
            NanokaHelper.PlayRollFailSfx();
            Console.WriteLine("[RelicSearch] combat end: no queued reward");
            return Task.CompletedTask;
        }

        // At most 1 relic reward per combat.
        NanokaHelper.PlayRewardSfx();
        Console.WriteLine("[RelicSearch] combat end success -> adding queued relic reward");
        room.AddExtraReward(Owner.Player, new RelicReward(Owner.Player));

        return Task.CompletedTask;
    }
}

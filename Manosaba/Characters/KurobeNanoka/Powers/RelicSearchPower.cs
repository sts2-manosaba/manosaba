using Manosaba.Characters.KurobeNanoka.Helpers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace Manosaba.Characters.KurobeNanoka.Powers;

public sealed class RelicSearchPower : PathCustomPowerModel
{
    private const int RelicChancePercentPerRoll = 50;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => Amount;
    public override bool AllowNegative => false;

    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner.Player == null || Amount <= 0m)
        {
            return Task.CompletedTask;
        }

        // Fixed 30% chance per roll. Amount = number of rolls.
        int totalRolls = Math.Max(0, Amount);
        if (totalRolls == 0)
        {
            return Task.CompletedTask;
        }

        Console.WriteLine($"[RelicSearch] combat end: player={Owner.Player.NetId}, amount={Amount}, totalRolls={totalRolls}, chancePerRoll={RelicChancePercentPerRoll}%");

        bool awarded = false;
        for (int i = 0; i < totalRolls; i++)
        {
            int roll = Owner.Player.RunState.Rng.Niche.NextInt(100);
            Console.WriteLine($"[RelicSearch] roll {i + 1}/{totalRolls}: {roll}");
            if (roll < RelicChancePercentPerRoll)
            {
                awarded = true;
                break;
            }
        }

        if (awarded)
        {
            // At most 1 relic reward per combat.
            NanokaHelper.PlayRewardSfx();
            Console.WriteLine("[RelicSearch] success -> adding relic reward");
            room.AddExtraReward(Owner.Player, new RelicReward(Owner.Player));
        }
        else
        {
            NanokaHelper.PlayRollFailSfx();
            Console.WriteLine("[RelicSearch] failed -> no relic reward");
        }

        return Task.CompletedTask;
    }
}

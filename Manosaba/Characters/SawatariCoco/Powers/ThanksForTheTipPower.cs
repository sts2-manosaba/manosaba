using manosaba.Characters.SawatariCoco.Helper;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace Manosaba.Characters.SawatariCoco.Powers;

/// <summary>感謝打賞：打出時鎖定當下粉絲數（與升級加成）為 Amount，戰鬥結束時獲得該金幣。</summary>
public sealed class ThanksForTheTipPower : PathCustomPowerModel
{
    public const int UpgradedBonusGold = 10;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public static int GetGoldForApplication(Player player, bool upgraded)
    {
        int fanCount = SawatariCocoHelper.GetTotalFanCount(player);
        return fanCount + (upgraded ? UpgradedBonusGold : 0);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner.Player is not { } player || Amount <= 0)
        {
            return Task.CompletedTask;
        }

        room.AddExtraReward(player, new GoldReward(Amount, player));
        return Task.CompletedTask;
    }
}

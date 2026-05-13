using Manosaba.Characters.TachibanaSherry.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TachibanaSherry.Powers;

/// <summary>神探當賞：每次獲得力量時額外 1 點格檔（單次結算只觸發一次）；回合結束失去 1 層。</summary>
public sealed class SherryDetectiveRewardPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await base.AfterPowerAmountChanged(power, amount, applier, cardSource);

        if (power == this)
        {
            KyuMiraclePartners.RefreshCostsForOwner(Owner.Player);
        }

        if (power is not StrengthPower || power.Owner != Owner || amount <= 0m || Amount <= 0)
            return;

        await CreatureCmd.GainBlock(Owner, 1m, ValueProp.Unpowered, null);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side || Amount <= 0)
            return;

        await PowerCmd.Apply<SherryDetectiveRewardPower>(Owner, -1m, Owner, null);
    }
}

using Manosaba.Characters.TachibanaSherry.Cards;
using BaseLib.Utils;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.TachibanaSherry.Powers;

/// <summary>本回合已獲得的疑點層數（供三！減費）。回合結束歸零。</summary>
public sealed class CluesGainedThisTurnPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>僅供減費等邏輯計數，不顯示在角色下方 Power 列。</summary>
    protected override bool IsVisibleInternal => false;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != Owner.Side || Amount <= 0m)
        {
            return;
        }

        await CommonActions.Apply<CluesGainedThisTurnPower>(choiceContext, Owner, null, -Amount);
        SanFindsClue.RefreshCostsForOwner(Owner.Player);
    }
}

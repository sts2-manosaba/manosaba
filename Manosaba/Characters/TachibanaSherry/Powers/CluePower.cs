using Manosaba.Characters.TachibanaSherry.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.TachibanaSherry.Powers;

/// <summary>疑點：每累積10層獲得1點力量與1層調查時刻（與回合開始+1同路徑、不扣疑點）；並供調查時刻滿5時結算傷害。</summary>
public sealed class CluePower : PathCustomPowerModel
{
    private const int CluePerInvestigationTick = 10;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await base.AfterPowerAmountChanged(power, amount, applier, cardSource);

        if (power != this || amount <= 0m)
            return;

        await PowerCmd.Apply<CluesGainedThisTurnPower>(Owner, (int)amount, Owner, cardSource);
        SanFindsClue.RefreshCostsForOwner(Owner.Player);

        InvestigationMomentPower? investigation = Owner.GetPower<InvestigationMomentPower>();
        if (investigation == null)
            return;

        int delta = (int)amount;
        int newAmt = Amount;
        int oldAmt = System.Math.Max(0, newAmt - delta);

        int newBlocks = newAmt / CluePerInvestigationTick;
        int oldBlocks = oldAmt / CluePerInvestigationTick;
        int gained = newBlocks - oldBlocks;
        if (gained <= 0)
            return;

        var ctx = new ThrowingPlayerChoiceContext();
        for (int i = 0; i < gained; i++)
        {
            await PowerCmd.Apply<StrengthPower>(Owner, 1m, Owner, cardSource);
            await investigation.AdvanceOneLayerCreditAsync(ctx);
        }
    }
}

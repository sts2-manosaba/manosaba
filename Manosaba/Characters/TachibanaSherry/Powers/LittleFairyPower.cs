using System.Collections.Generic;
using System.Linq;
using Manosaba.Characters.TachibanaSherry.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.TachibanaSherry.Powers;

/// <summary>小仙子：每累積獲得 5 點力量，依各次打出時記錄的能量值獲得能量。</summary>
public sealed class LittleFairyPower : PathCustomPowerModel
{
    private sealed class Data
    {
        public int StrengthCredit;
        public readonly List<decimal> EnergyPerStackLayer = [];
    }

    private const int StrengthThreshold = 5;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => GetInternalData<Data>().StrengthCredit;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override object InitInternalData() => new Data();

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await base.AfterPowerAmountChanged(power, amount, applier, cardSource);

        Data data = GetInternalData<Data>();
        if (power == this)
        {
            List<decimal> layers = data.EnergyPerStackLayer;
            if (amount > 0m && cardSource is LittleFairy lf)
            {
                decimal energyPerStack = lf.DynamicVars.Energy.BaseValue;
                for (int i = 0; i < (int)amount; i++)
                {
                    layers.Add(energyPerStack);
                }
            }
            else if (amount < 0m)
            {
                int remove = (int)(-amount);
                for (int r = 0; r < remove && layers.Count > 0; r++)
                {
                    layers.RemoveAt(layers.Count - 1);
                }
            }

            DynamicVars.Energy.BaseValue = TotalEnergyPerTrigger();
        }

        if (power is not StrengthPower || power.Owner != Owner || amount <= 0m)
        {
            return;
        }

        data.StrengthCredit += (int)amount;
        while (data.StrengthCredit >= StrengthThreshold)
        {
            data.StrengthCredit -= StrengthThreshold;
            InvokeDisplayAmountChanged();
            Flash();
            int energy = (int)TotalEnergyPerTrigger();
            if (energy > 0 && Owner.Player != null)
            {
                await PlayerCmd.GainEnergy(energy, Owner.Player);
            }
        }

        InvokeDisplayAmountChanged();
    }

    private decimal TotalEnergyPerTrigger()
    {
        List<decimal> layers = GetInternalData<Data>().EnergyPerStackLayer;
        return layers.Count > 0 ? layers.Sum() : 1m;
    }
}

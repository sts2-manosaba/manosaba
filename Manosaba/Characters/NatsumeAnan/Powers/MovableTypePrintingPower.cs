using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.NatsumeAnan.Powers;

public sealed class MovableTypePrintingPower : PathCustomPowerModel
{
    private sealed class Data
    {
        public int EnergySpent;
        public int TriggerCount;
    }

    private const int EnergyIncrement = 3;
    private const int KotodamaGain = 1;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => EnergyIncrement - GetInternalData<Data>().EnergySpent % EnergyIncrement;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(EnergyIncrement),
        new DynamicVar("KotodamaGain", KotodamaGain),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task AfterEnergySpent(CardModel card, int amount)
    {
        if (amount <= 0 || card.Owner.Creature != Owner || Owner.Player == null)
            return Task.CompletedTask;

        Data data = GetInternalData<Data>();
        data.EnergySpent += amount;

        int triggers = data.EnergySpent / EnergyIncrement - data.TriggerCount;
        if (triggers <= 0)
        {
            InvokeDisplayAmountChanged();
            return Task.CompletedTask;
        }

        int gain = KotodamaGain * triggers;
        if (gain > 0)
        {
            KotodamaEnergy.Gain(Owner.Player, gain);
            Flash();
        }

        data.TriggerCount += triggers;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }
}

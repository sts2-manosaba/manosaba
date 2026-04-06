using Manosaba.Characters.TonoHanna.Visuals;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;

namespace Manosaba.Characters.TonoHanna.Powers;

public abstract class PuppetCollectionPowerBase : PathCustomPowerModel
{
    protected abstract int CompanionSlotIndex { get; }
    protected abstract string CompanionTexturePath { get; }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>Per-character collection buffs only drive the ring visuals; the shared <see cref="PuppetCollectionSummaryPower"/> is shown in the power row.</summary>
    protected override bool IsVisibleInternal => false;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (TestMode.IsOn || Owner == null)
            return Task.CompletedTask;

        PuppetCollectionVisuals.SetSlot(Owner, CompanionSlotIndex, true, CompanionTexturePath);
        return PuppetCollectionSummaryPower.SyncToRosterAsync(Owner);
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await base.AfterPowerAmountChanged(power, amount, applier, cardSource);
        if (power != this || power.Owner == null || TestMode.IsOn)
            return;

        PuppetCollectionVisuals.SetSlot(power.Owner, CompanionSlotIndex, power.Amount > 0, CompanionTexturePath);
        await PuppetCollectionSummaryPower.SyncToRosterAsync(power.Owner);
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        if (!TestMode.IsOn)
            PuppetCollectionVisuals.SetSlot(oldOwner, CompanionSlotIndex, false, CompanionTexturePath);
        return PuppetCollectionSummaryPower.SyncToRosterAsync(oldOwner);
    }
}

public sealed class AlisaPuppetCollectionPower : PuppetCollectionPowerBase
{
    protected override int CompanionSlotIndex => PuppetCompanionLayout.SlotAlisa;
    protected override string CompanionTexturePath => PuppetTexturePaths.AlisaPuppetCard;
}

public sealed class AnAnPuppetCollectionPower : PuppetCollectionPowerBase
{
    protected override int CompanionSlotIndex => PuppetCompanionLayout.SlotAnAn;
    protected override string CompanionTexturePath => PuppetTexturePaths.AnAnPuppetCard;
}

public sealed class CocoPuppetCollectionPower : PuppetCollectionPowerBase
{
    protected override int CompanionSlotIndex => PuppetCompanionLayout.SlotCoco;
    protected override string CompanionTexturePath => PuppetTexturePaths.CocoPuppetCard;
}

public sealed class EmaPuppetCollectionPower : PuppetCollectionPowerBase
{
    protected override int CompanionSlotIndex => PuppetCompanionLayout.SlotEma;
    protected override string CompanionTexturePath => PuppetTexturePaths.EmaPuppetCard;
}

public sealed class HannaPuppetCollectionPower : PuppetCollectionPowerBase
{
    protected override int CompanionSlotIndex => PuppetCompanionLayout.SlotHanna;
    protected override string CompanionTexturePath => PuppetTexturePaths.HannaPuppetCard;
}

public sealed class HiroPuppetCollectionPower : PuppetCollectionPowerBase
{
    protected override int CompanionSlotIndex => PuppetCompanionLayout.SlotHiro;
    protected override string CompanionTexturePath => PuppetTexturePaths.HiroPuppetCard;
}

public sealed class LeiaPuppetCollectionPower : PuppetCollectionPowerBase
{
    protected override int CompanionSlotIndex => PuppetCompanionLayout.SlotLeia;
    protected override string CompanionTexturePath => PuppetTexturePaths.LeiaPuppetCard;
}

public sealed class MargoPuppetCollectionPower : PuppetCollectionPowerBase
{
    protected override int CompanionSlotIndex => PuppetCompanionLayout.SlotMargo;
    protected override string CompanionTexturePath => PuppetTexturePaths.MargoPuppetCard;
}

public sealed class MeruruPuppetCollectionPower : PuppetCollectionPowerBase
{
    protected override int CompanionSlotIndex => PuppetCompanionLayout.SlotMeruru;
    protected override string CompanionTexturePath => PuppetTexturePaths.MeruruPuppetCard;
}

public sealed class MiriaPuppetCollectionPower : PuppetCollectionPowerBase
{
    protected override int CompanionSlotIndex => PuppetCompanionLayout.SlotMiria;
    protected override string CompanionTexturePath => PuppetTexturePaths.MiriaPuppetCard;
}

public sealed class NanokaPuppetCollectionPower : PuppetCollectionPowerBase
{
    protected override int CompanionSlotIndex => PuppetCompanionLayout.SlotNanoka;
    protected override string CompanionTexturePath => PuppetTexturePaths.NanokaPuppetCard;
}

public sealed class NoahPuppetCollectionPower : PuppetCollectionPowerBase
{
    protected override int CompanionSlotIndex => PuppetCompanionLayout.SlotNoah;
    protected override string CompanionTexturePath => PuppetTexturePaths.NoahPuppetCard;
}

public sealed class SherryPuppetCollectionPower : PuppetCollectionPowerBase
{
    protected override int CompanionSlotIndex => PuppetCompanionLayout.SlotSherry;
    protected override string CompanionTexturePath => PuppetTexturePaths.SherryPuppetCard;
}

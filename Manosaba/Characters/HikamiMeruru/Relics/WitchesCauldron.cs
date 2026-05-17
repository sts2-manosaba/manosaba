using BaseLib.Utils;
using Manosaba.Characters.HikamiMeruru.PotionCraft;
using Manosaba.Characters.HikamiMeruru.Potions;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace manosaba.Characters.HikamiMeruru.Relics;

[Pool(typeof(HikamiMeruruRelicPool))]
public sealed class WitchesCauldron : PathCustomRelicModel
{
    public const int MaxFirepower = 100;

    private const int BaseFirepower = 20;
    private const string FirepowerVar = "Firepower";

    private bool _grantingCatalyst;
    private int _firepower = BaseFirepower;

    public override RelicRarity Rarity => RelicRarity.Uncommon;
    public override bool ShowCounter => true;
    public override int DisplayAmount => Firepower;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(FirepowerVar, BaseFirepower)];

    [SavedProperty]
    public int Firepower
    {
        get => _firepower;
        set
        {
            AssertMutable();
            _firepower = Math.Clamp(value, 0, MaxFirepower);
            SyncFirepowerDynamicVar();
            InvokeDisplayAmountChanged();
        }
    }

    public void AddFirepower(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Firepower += amount;
        Flash();
    }

    public void MaximizeFirepower()
    {
        Firepower = MaxFirepower;
        Flash();
    }

    public override Task BeforeCombatStart()
    {
        ResetFirepower();
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _ = room;
        ResetFirepower();
        return Task.CompletedTask;
    }

    public override async Task AfterPotionDiscarded(PotionModel potion)
    {
        await TryGrantCatalyst(potion, suppressCraftDiscard: true);
    }

    public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
    {
        _ = target;
        await TryGrantCatalyst(potion, suppressCraftDiscard: false);
    }

    private async Task TryGrantCatalyst(PotionModel potion, bool suppressCraftDiscard)
    {
        if (_grantingCatalyst || potion.Owner != Owner || potion is Catalyst)
            return;

        if ((suppressCraftDiscard && PotionCraftService.IsCraftDiscardSuppressed) || !Owner.HasOpenPotionSlots)
            return;

        int procChance = Math.Min(Firepower, 100);
        if (procChance <= 0 || Owner.RunState.Rng.CombatPotionGeneration.NextInt(100) >= procChance)
        {
            return;
        }

        _grantingCatalyst = true;
        try
        {
            var result = await PotionCmd.TryToProcure<Catalyst>(Owner);
            if (result.success)
            {
                Flash();
            }
        }
        finally
        {
            _grantingCatalyst = false;
        }
    }

    private void ResetFirepower()
    {
        Firepower = BaseFirepower;
    }

    private void SyncFirepowerDynamicVar()
    {
        DynamicVars[FirepowerVar].BaseValue = Firepower;
    }
}

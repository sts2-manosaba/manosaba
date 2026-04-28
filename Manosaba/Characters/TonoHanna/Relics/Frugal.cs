using System.Collections.Generic;
using System.Linq;
using BaseLib.Utils;
using manosaba.Characters.TonoHanna;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace manosaba.Characters.TonoHanna.Relics;

[Pool(typeof(TonoHannaRelicPool))]
public sealed class FeatherFan : LevelingPathCustomRelicModel
{
    private const int MaxUnspentEnergyForGoldPerCombat = 5;
    private const decimal BaseGoldPerEnergy = 10m;
    private const int Lv4GoldPerBonusEnergy = 100;

    /// <summary>Unused energy points already converted to gold this combat (capped at <see cref="MaxUnspentEnergyForGoldPerCombat"/>).</summary>
    private int _energyChargesConvertedThisCombat;

    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("GoldPerEnergy", BaseGoldPerEnergy),
        new DynamicVar("CombatUnspentEnergyGold", 0m),
        new DynamicVar("MaxUnspentEnergyCombat", MaxUnspentEnergyForGoldPerCombat),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => featherFanCombatExtraHoverTips();

    private IEnumerable<IHoverTip> featherFanCombatExtraHoverTips()
    {
        foreach (IHoverTip tip in base.ExtraHoverTips)
            yield return tip;

        if (!ShouldShowCombatEnergyTrackLine())
            yield break;

        LocString line = new LocString("relics", $"{Id.Entry}.combatEnergyTrack");
        DynamicVars.AddTo(line);
        yield return new HoverTip(line);
    }

    /// <summary>Combat-only. Canonical / library instances must not touch <see cref="RelicModel.Owner"/> (asserts mutable).</summary>
    private bool ShouldShowCombatEnergyTrackLine()
        => IsMutable && Owner?.PlayerCombatState != null;

    public override Task AfterObtained() => Task.CompletedTask;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (RelicLevel < 4 || Owner?.Creature == null)
        {
            return;
        }

        if (side != Owner.Creature.Side || combatState.RoundNumber > 1)
        {
            return;
        }

        if (Owner.PlayerCombatState == null)
        {
            return;
        }

        int bonusEnergy = Owner.Gold / Lv4GoldPerBonusEnergy;
        if (bonusEnergy <= 0)
        {
            return;
        }

        Flash();
        await PlayerCmd.GainEnergy(bonusEnergy, Owner);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _ = room;
        _energyChargesConvertedThisCombat = 0;
        SyncCombatGoldTrackingDynamicVars(0);
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    protected override Task AfterRelicLevelChanged(int oldLevel, int newLevel)
    {
        return newLevel > oldLevel
            ? OnRelicLevelIncreasedAsync(oldLevel, newLevel)
            : Task.CompletedTask;
    }

    private async Task OnRelicLevelIncreasedAsync(int oldLevel, int newLevel)
    {
        if (Owner?.Deck == null || Owner.RunState == null)
        {
            return;
        }

        int levelsGained = newLevel - oldLevel;
        for (int i = 0; i < levelsGained; i++)
        {
            await TryAddRandomPuppetCardAsync();
        }
    }

    private async Task TryAddRandomPuppetCardAsync()
    {
        if (Owner?.Deck == null || Owner.RunState == null)
        {
            return;
        }

        List<CardModel> puppetPool = ModelDb.CardPool<TonoHannaCardPool>()
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Tags.Contains(ManosabaCardTags.Puppet))
            .ToList();

        if (puppetPool.Count <= 0)
        {
            return;
        }

        CardModel? canonical = Owner.RunState.Rng.Niche.NextItem(puppetPool);
        if (canonical == null)
        {
            return;
        }

        CardModel card = Owner.RunState.CreateCard(canonical, Owner);
        CardPileAddResult result = await CardPileCmd.Add(card, PileType.Deck);
        CardCmd.PreviewCardPileAdd(result, 1.2f, CardPreviewStyle.GridLayout);
    }

    /// <summary>Called when <see cref="Manosaba.Characters.TonoHanna.Powers.PuppetCollectionSummaryPower"/> increases (a new distinct puppet appears in the collection).</summary>
    public static async Task OnPuppetCollectionIncreasedAsync(Creature owner, decimal positiveDelta)
    {
        if (positiveDelta <= 0m || TestMode.IsOn)
        {
            return;
        }

        Player? player = owner.Player;
        FeatherFan? featherFan = player?.GetRelic<FeatherFan>();
        if (featherFan == null || featherFan.RelicLevel < 2)
        {
            return;
        }

        decimal majoka = 10m * positiveDelta;
        await PowerCmd.Apply<MajokaPower>(owner, majoka, owner, null);
    }

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (Owner?.Creature is not { } ownerCreature)
        {
            return;
        }

        if (side != ownerCreature.Side)
        {
            return;
        }

        if (!ownerCreature.IsAlive)
        {
            return;
        }

        if (Owner.PlayerCombatState == null)
        {
            return;
        }

        int energyLeft = Owner.PlayerCombatState.Energy;
        if (energyLeft <= 0)
        {
            return;
        }

        int capRemaining = MaxUnspentEnergyForGoldPerCombat - _energyChargesConvertedThisCombat;
        if (capRemaining <= 0)
        {
            return;
        }

        int toConvert = Math.Min(energyLeft, capRemaining);
        _energyChargesConvertedThisCombat += toConvert;

        SyncCombatGoldTrackingDynamicVars(_energyChargesConvertedThisCombat);
        InvokeDisplayAmountChanged();

        int gold = toConvert * DynamicVars["GoldPerEnergy"].IntValue;
        if (gold <= 0)
        {
            return;
        }

        await PlayerCmd.GainGold(gold, Owner);
    }

    private void SyncCombatGoldTrackingDynamicVars(int combatEnergyConverted)
    {
        DynamicVars["CombatUnspentEnergyGold"].BaseValue = combatEnergyConverted;
    }
}

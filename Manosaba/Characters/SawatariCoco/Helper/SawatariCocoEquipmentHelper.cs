using BaseLib.Utils;
using manosaba.Characters.SawatariCoco.Cards;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.SawatariCoco.Equipment;
using Manosaba.Characters.SawatariCoco.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace manosaba.Characters.SawatariCoco.Helper;

public static class SawatariCocoEquipmentHelper
{
    /// <summary>Vanilla <see cref="PowerCmd.Apply"/> ignores zero amounts and never creates the power instance.</summary>
    private const decimal InitialPowerAmount = 1m;

    public static async Task<bool> TryEquipFromCardAsync(
        PlayerChoiceContext choiceContext,
        Player player,
        EquipmentCardModel source)
    {
        if (player.Creature?.CombatState is not { } combatState || player.Creature is not { } creature)
        {
            return false;
        }

        if (!EquipmentPieceTokenRegistry.TryGetTokenType(source.GetType(), out Type newTokenType))
        {
            return false;
        }

        EquipmentSlot slot = source.EquipSlot;
        EquipmentSeries series = source.EquipSeries;
        string pieceName = source.EquipPieceDisplayName;
        int score = source.EquipScore;

        if (ModelDb.GetById<CardModel>(ModelDb.GetId(newTokenType)) is not CardModel newCanonical)
        {
            return false;
        }

        CardModel? newOption = combatState.CreateCard(newCanonical, player);
        if (newOption == null)
        {
            return false;
        }

        List<CardModel> options = [newOption];

        if (GetSlotSeries(creature, slot) is var equippedSeries
            && equippedSeries != EquipmentSeries.None
            && GetSlotScore(creature, slot) > 0
            && EquipmentPieceTokenRegistry.TryGetTokenType(equippedSeries, slot, out Type oldTokenType)
            && ModelDb.GetById<CardModel>(ModelDb.GetId(oldTokenType)) is CardModel oldCanonical)
        {
            CardModel? oldOption = combatState.CreateCard(oldCanonical, player);
            if (oldOption != null)
            {
                options.Add(oldOption);
            }
        }

        CardModel? selected;
        try
        {
            selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, player, canSkip: true);
        }
        catch (Exception)
        {
            return false;
        }

        if (selected == null || selected.GetType() != newTokenType)
        {
            return false;
        }

        await EquipPieceAsync(choiceContext, creature, slot, series, pieceName, score, source);
        return true;
    }

    public static async Task EquipPieceAsync(
        PlayerChoiceContext choiceContext,
        Creature creature,
        EquipmentSlot slot,
        EquipmentSeries series,
        string pieceName,
        int score,
        CardModel? source)
    {
        EquipmentSlotPowerBase? slotPower = await GetOrCreateSlotPowerAsync(choiceContext, creature, slot);
        if (slotPower == null)
        {
            return;
        }

        slotPower.SetEquippedPiece(series, pieceName, score);

        int totalScore = GetTotalEquipmentScore(creature);
        if (await GetOrCreateScorePowerAsync(choiceContext, creature) is { } scorePower)
        {
            await scorePower.UpdateTotalScoreAsync(choiceContext, totalScore, source);
        }

        await TryTriggerSetBonusAsync(choiceContext, creature, series, source);
    }

    public static int GetTotalEquipmentScore(Creature creature)
        => GetSlotScore<EquipmentHeadwearSlotPower>(creature)
           + GetSlotScore<EquipmentTopSlotPower>(creature)
           + GetSlotScore<EquipmentSkirtSlotPower>(creature)
           + GetSlotScore<EquipmentShoesSlotPower>(creature);

    public static EquipmentSeries? GetFullSetSeries(Creature creature)
    {
        EquipmentSeries? series = null;
        foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
        {
            EquipmentSeries slotSeries = GetSlotSeries(creature, slot);
            if (slotSeries == EquipmentSeries.None)
            {
                return null;
            }

            series = series is null ? slotSeries : series;
            if (series != slotSeries)
            {
                return null;
            }
        }

        return series;
    }

    public static string GetVisualTexturePath(EquipmentSlot slot, EquipmentSeries series, bool isMajoka)
    {
        if (series == EquipmentSeries.None)
        {
            return string.Empty;
        }

        string seriesKey = series switch
        {
            EquipmentSeries.PunkCat => "punk_cat",
            EquipmentSeries.Cybercat => "cybercat",
            EquipmentSeries.MysteriousCat => "mysterious_cat",
            EquipmentSeries.CutieCats => "cutie_cats",
            _ => string.Empty,
        };

        if (string.IsNullOrEmpty(seriesKey))
        {
            return string.Empty;
        }

        string majokaSuffix = isMajoka ? "_majoka" : string.Empty;
        return $"res://Manosaba/images/characters/sawatari_coco/equipment/{seriesKey}_{slot.ToString().ToLowerInvariant()}{majokaSuffix}.png";
    }

    private static async Task TryTriggerSetBonusAsync(
        PlayerChoiceContext choiceContext,
        Creature creature,
        EquipmentSeries equippedSeries,
        CardModel? source)
    {
        if (equippedSeries == EquipmentSeries.None)
        {
            return;
        }

        if (GetFullSetSeries(creature) != equippedSeries)
        {
            return;
        }

        if (await EnsureSetBonusPowerAsync(choiceContext, creature) is not { } tracker || tracker.HasTriggered(equippedSeries))
        {
            return;
        }

        tracker.MarkTriggered(equippedSeries);

        switch (equippedSeries)
        {
            case EquipmentSeries.PunkCat:
                await CommonActions.Apply<StrengthPower>(choiceContext, creature, source, 5m);
                break;
            case EquipmentSeries.Cybercat:
                await CommonActions.Apply<DexterityPower>(choiceContext, creature, source, 5m);
                break;
            case EquipmentSeries.MysteriousCat:
                await CommonActions.Apply<MysteriousCatSetEnergyPower>(choiceContext, creature, source, 2m);
                break;
            case EquipmentSeries.CutieCats:
                if (creature.CombatState is not { } combatState)
                {
                    break;
                }

                foreach (Creature enemy in combatState.GetOpponentsOf(creature).Where(e => e.IsAlive && e.CanReceivePowers))
                {
                    await CommonActions.Apply<PoisonPower>(choiceContext, enemy, source, 10m);
                    await CommonActions.Apply<BurnPower>(choiceContext, enemy, source, 10m);
                }

                break;
        }
    }

    private static int GetSlotScore(Creature creature, EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Headwear => GetSlotScore<EquipmentHeadwearSlotPower>(creature),
        EquipmentSlot.Top => GetSlotScore<EquipmentTopSlotPower>(creature),
        EquipmentSlot.Skirt => GetSlotScore<EquipmentSkirtSlotPower>(creature),
        EquipmentSlot.Shoes => GetSlotScore<EquipmentShoesSlotPower>(creature),
        _ => 0,
    };

    private static int GetSlotScore<T>(Creature creature) where T : EquipmentSlotPowerBase
        => creature.GetPower<T>()?.SlotScore ?? 0;

    private static EquipmentSeries GetSlotSeries(Creature creature, EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Headwear => creature.GetPower<EquipmentHeadwearSlotPower>()?.EquippedSeries ?? EquipmentSeries.None,
        EquipmentSlot.Top => creature.GetPower<EquipmentTopSlotPower>()?.EquippedSeries ?? EquipmentSeries.None,
        EquipmentSlot.Skirt => creature.GetPower<EquipmentSkirtSlotPower>()?.EquippedSeries ?? EquipmentSeries.None,
        EquipmentSlot.Shoes => creature.GetPower<EquipmentShoesSlotPower>()?.EquippedSeries ?? EquipmentSeries.None,
        _ => EquipmentSeries.None,
    };

    private static async Task<EquipmentScorePower?> GetOrCreateScorePowerAsync(PlayerChoiceContext choiceContext, Creature creature)
    {
        if (creature.GetPower<EquipmentScorePower>() is { } existing)
        {
            return existing;
        }

        await CommonActions.Apply<EquipmentScorePower>(choiceContext, creature, null, InitialPowerAmount, silent: true);
        return creature.GetPower<EquipmentScorePower>();
    }

    private static async Task<EquipmentSetBonusPower?> EnsureSetBonusPowerAsync(PlayerChoiceContext choiceContext, Creature creature)
    {
        if (creature.GetPower<EquipmentSetBonusPower>() is { } existing)
        {
            return existing;
        }

        await CommonActions.Apply<EquipmentSetBonusPower>(choiceContext, creature, null, InitialPowerAmount, silent: true);
        return creature.GetPower<EquipmentSetBonusPower>();
    }

    private static async Task<EquipmentSlotPowerBase?> GetOrCreateSlotPowerAsync(
        PlayerChoiceContext choiceContext,
        Creature creature,
        EquipmentSlot slot)
    {
        EquipmentSlotPowerBase? existing = slot switch
        {
            EquipmentSlot.Headwear => creature.GetPower<EquipmentHeadwearSlotPower>(),
            EquipmentSlot.Top => creature.GetPower<EquipmentTopSlotPower>(),
            EquipmentSlot.Skirt => creature.GetPower<EquipmentSkirtSlotPower>(),
            EquipmentSlot.Shoes => creature.GetPower<EquipmentShoesSlotPower>(),
            _ => null,
        };

        if (existing != null)
        {
            return existing;
        }

        switch (slot)
        {
            case EquipmentSlot.Headwear:
                await CommonActions.Apply<EquipmentHeadwearSlotPower>(choiceContext, creature, null, InitialPowerAmount, silent: true);
                return creature.GetPower<EquipmentHeadwearSlotPower>();
            case EquipmentSlot.Top:
                await CommonActions.Apply<EquipmentTopSlotPower>(choiceContext, creature, null, InitialPowerAmount, silent: true);
                return creature.GetPower<EquipmentTopSlotPower>();
            case EquipmentSlot.Skirt:
                await CommonActions.Apply<EquipmentSkirtSlotPower>(choiceContext, creature, null, InitialPowerAmount, silent: true);
                return creature.GetPower<EquipmentSkirtSlotPower>();
            case EquipmentSlot.Shoes:
                await CommonActions.Apply<EquipmentShoesSlotPower>(choiceContext, creature, null, InitialPowerAmount, silent: true);
                return creature.GetPower<EquipmentShoesSlotPower>();
            default:
                return null;
        }
    }
}

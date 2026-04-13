using HarmonyLib;
using manosaba.Characters.HoshoMago;
using Manosaba.Characters.HoshoMago.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using System.Reflection;

namespace Manosaba.Patches;

[HarmonyPatch]
public static class Patch_Hosho_Mago_Card_Factory
{
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        MethodInfo? transform = AccessTools.Method(typeof(CardFactory), nameof(CardFactory.CreateRandomCardForTransform), [typeof(CardModel), typeof(bool), typeof(Rng)]);
        if (transform != null)
        {
            yield return transform;
        }

        foreach (MethodInfo method in AccessTools.GetDeclaredMethods(typeof(CardFactory)).Where(m => m.Name == nameof(CardFactory.GetDistinctForCombat)))
        {
            yield return method;
        }
    }

    [HarmonyPrefix]
    private static bool Prefix(MethodBase __originalMethod, object[] __args, ref object? __result)
    {
        if (__originalMethod.Name == nameof(CardFactory.CreateRandomCardForTransform))
        {
            return PrefixCreateRandomCardForTransform(__args, ref __result);
        }

        if (__originalMethod.Name == nameof(CardFactory.GetDistinctForCombat))
        {
            return PrefixGetDistinctForCombat(__args, ref __result);
        }

        return true;
    }

    private static bool PrefixCreateRandomCardForTransform(object[] args, ref object? result)
    {
        if (args.Length < 3 || args[0] is not CardModel original || args[1] is not bool isInCombat || args[2] is not Rng rng)
        {
            return true;
        }

        if (original.Pool is not HoshoMagoCardPool)
        {
            return true;
        }

        IEnumerable<CardModel> unlocked = original.Pool.GetUnlockedCards(original.Owner.UnlockState, original.RunState.CardMultiplayerConstraint);

        bool usesVanillaRarityBand = original.Rarity != CardRarity.Ancient && original.Rarity != CardRarity.Token;
        IEnumerable<CardModel> vanillaCandidates = unlocked
            .Where(c => c.Id != original.Id)
            .Where(c => !isInCombat || c.CanBeGeneratedInCombat);
        if (usesVanillaRarityBand)
        {
            vanillaCandidates = vanillaCandidates.Where(c => c.Rarity == CardRarity.Common || c.Rarity == CardRarity.Uncommon || c.Rarity == CardRarity.Rare);
        }

        if (vanillaCandidates.Any())
        {
            return true;
        }

        List<CardModel> fallbackCandidates = BuildHoshoFallbackPool(unlocked, isInCombat, includeBasic: false, excludeCardId: original.Id);
        if (fallbackCandidates.Count == 0)
        {
            return true;
        }

        result = original.CardScope.CreateCard(rng.NextItem(fallbackCandidates), original.Owner);
        return false;
    }

    private static bool PrefixGetDistinctForCombat(object[] args, ref object? result)
    {
        if (args.Length < 4)
        {
            return true;
        }

        Player? player = args.OfType<Player>().FirstOrDefault();
        Rng? rng = args.OfType<Rng>().FirstOrDefault();
        int? count = args.OfType<int>().Select(v => (int?)v).FirstOrDefault();
        object? poolArg = args.FirstOrDefault(a => a != null && a is not Player && a is not Rng && a is not int && a is not bool);
        if (player == null || rng == null || count == null || poolArg == null)
        {
            return true;
        }

        if (!TryGetPoolCards(poolArg, player, out List<CardModel>? poolCards))
        {
            return true;
        }

        if (!IsHoshoPool(poolArg, poolCards))
        {
            return true;
        }

        List<CardModel> available = BuildHoshoFallbackPool(poolCards, isInCombat: true, includeBasic: true, excludeCardId: null)
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .ToList();

        if (available.Count == 0)
        {
            result = Enumerable.Empty<CardModel>();
            return false;
        }

        int targetCount = Math.Clamp(count.Value, 0, available.Count);
        List<CardModel> picked = [];
        for (int i = 0; i < targetCount; i++)
        {
            CardModel canonical = rng.NextItem(available);
            CardModel card = player.Creature?.CombatState != null
                ? player.Creature.CombatState.CreateCard(canonical, player)
                : player.RunState.CreateCard(canonical, player);
            picked.Add(card);
            available.Remove(canonical);
        }

        result = picked;
        return false;
    }

    private static bool TryGetPoolCards(object poolArg, Player player, out List<CardModel>? cards)
    {
        if (poolArg is IEnumerable<CardModel> directEnumerable)
        {
            cards = directEnumerable.ToList();
            return true;
        }

        MethodInfo? getUnlockedCards = poolArg
            .GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == "GetUnlockedCards" && m.GetParameters().Length == 2);
        if (getUnlockedCards == null)
        {
            cards = null;
            return false;
        }

        object? unlocked = getUnlockedCards.Invoke(poolArg, [player.UnlockState, player.RunState.CardMultiplayerConstraint]);
        if (unlocked is IEnumerable<CardModel> unlockedEnumerable)
        {
            cards = unlockedEnumerable.ToList();
            return true;
        }

        cards = null;
        return false;
    }

    private static bool IsHoshoPool(object poolArg, List<CardModel> cards)
    {
        if (poolArg is HoshoMagoCardPool)
        {
            return true;
        }

        return cards.Count > 0 && cards.All(c => c.Pool is HoshoMagoCardPool);
    }

    private static List<CardModel> BuildHoshoFallbackPool(IEnumerable<CardModel> source, bool isInCombat, bool includeBasic, ModelId? excludeCardId = null)
    {
        IEnumerable<CardModel> query = source
            .Where(c => excludeCardId == null || c.Id != excludeCardId)
            .Where(c => c.Rarity != CardRarity.Token)
            .Where(c => includeBasic || c.Rarity != CardRarity.Basic)
            .Where(c => c is not TheWorld && c is not Echoes)
            .Where(c => !isInCombat || c.CanBeGeneratedInCombat);

        return query.ToList();
    }
}

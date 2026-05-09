using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Manosaba.Characters.Common;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.HoshoMago.Cards;
using manosaba.Characters.Common;
using manosaba.Characters.HoshoMago;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace Manosaba.Patches;

internal static class ManosabaTransformHelper
{
    /// <summary>
    /// Prefix body for <see cref="CardFactory.CreateRandomCardForTransform"/> overload that takes an explicit card pool (all characters).
    /// </summary>
    internal static void PrefixCustomPoolTransformOptions(CardModel original, bool isInCombat, ref IEnumerable<CardModel> options)
    {
        if (original.Owner is not Player player)
        {
            return;
        }

        CardModel[] arr = options.ToArray();
        if (arr.Length == 0)
        {
            return;
        }

        CardModel[] after = FilterUniqueAlreadyInDeck(player, arr);
        if (after.Length != arr.Length)
        {
            Log.Debug(
                $"[Manosaba Transform] CustomPool_UniqueDeckFilter original={original.Id.Entry} inCombat={isInCombat} beforeCount={arr.Length} afterCount={after.Length} playerNetId={player.NetId}");
        }

        options = after;
    }

    /// <summary>
    /// Removes Unique templates already in the player's deck. If that would empty the pool, returns <paramref name="candidates"/> unchanged.
    /// </summary>
    internal static CardModel[] FilterUniqueAlreadyInDeck(Player player, CardModel[] candidates)
    {
        CardModel[] filtered = candidates
            .Where(c => !ManosabaUniqueCardEligibility.IsBlockedForPlayerOffer(player, c))
            .ToArray();

        if (filtered.Length == 0 && candidates.Length > 0)
        {
            Log.Debug($"[Manosaba Transform] FilterUniqueAlreadyInDeck fallback: all {candidates.Length} candidates blocked for player {player.NetId}; restoring unfiltered pool (may allow duplicate Unique).");
        }

        return filtered.Length > 0 ? filtered : candidates;
    }

    /// <summary>
    /// True when transform uses the Manosaba merged pool (character unlocks + <see cref="CommonCardPool"/>) and should apply Unique-in-deck filtering.
    /// Hosho tarot path and Manosaba mod tokens are excluded here (handled earlier).
    /// Only <see cref="CardType.Attack"/> / <see cref="CardType.Skill"/> / <see cref="CardType.Power"/> use the merged path so curse/status/quest keep vanilla pools (see <c>MiriaPuppet.IsEligibleSourceTypeForPuppetTransform</c>).
    /// </summary>
    internal static bool ShouldApplyCommonPathUniqueDeckFilter(CardModel original)
    {
        return GetCommonPathUniqueDeckFilterSkipReason(original) == null;
    }

    /// <summary>
    /// When non-null, <see cref="ShouldApplyCommonPathUniqueDeckFilter"/> is false for this reason (Owner must still be checked separately in the patch).
    /// </summary>
    internal static string? GetCommonPathUniqueDeckFilterSkipReason(CardModel original)
    {
        if (original.Owner?.Character?.CardPool == null || original.RunState == null)
        {
            return "Owner.Character.CardPool or RunState null";
        }

        if (original.Pool is HoshoMagoCardPool)
        {
            return "original.Pool is HoshoMagoCardPool";
        }

        if (IsManosabaModToken(original))
        {
            return "IsManosabaModToken";
        }

        if (original.Type != CardType.Attack && original.Type != CardType.Skill && original.Type != CardType.Power)
        {
            return "original.Type not Attack/Skill/Power";
        }

        if (!ModelDb.CardPool<CommonCardPool>().AllCardIds.Contains(original.Id))
        {
            if (original.Owner is not Player p || !ManosabaPlayerHelper.IsManosabaPlayer(p))
            {
                return "original.Id not in CommonCardPool.AllCardIds";
            }
        }

        return null;
    }

    internal static void LogTransformCandidatePool(string phase, CardModel original, bool isInCombat, IEnumerable<CardModel> pool, Player? player)
    {
        CardModel[] arr = pool as CardModel[] ?? pool.ToArray();
        string ids = string.Join(',', arr.Select(c => c.Id.Entry));
        IEnumerable<string> uniqueInfos = arr
            .Where(ManosabaUniqueCardEligibility.IsUniqueTemplate)
            .Select(c =>
                $"{c.Id.Entry}:blocked={(player != null && ManosabaUniqueCardEligibility.IsBlockedForPlayerOffer(player, c))}");
        string uniquePart = uniqueInfos.Any() ? $" manosabaUnique=[{string.Join(';', uniqueInfos)}]" : string.Empty;
        Log.Debug($"[Manosaba Transform] {phase} original={original.Id.Entry} inCombat={isInCombat} count={arr.Length} playerNetId={player?.NetId}{uniquePart} ids=[{ids}]");
    }

    public static bool TryGetTransformOptions(CardModel original, bool isInCombat, out IEnumerable<CardModel> options)
    {
        options = Enumerable.Empty<CardModel>();

        if (original.Owner?.Character?.CardPool == null || original.RunState == null)
        {
            return false;
        }

        if (original.Pool is HoshoMagoCardPool)
        {
            CardModel[] tarotCandidates = ApplyHoshoTransformFilters(original, GetHoshoTarotSource(original), isInCombat);
            if (tarotCandidates.Length == 0)
            {
                return false;
            }

            options = tarotCandidates;
            return true;
        }

        CommonCardPool commonPool = ModelDb.CardPool<CommonCardPool>();

        // Manosaba-only tokens: same transform source as CommonCardPool cards (character pool + CommonCardPool),
        // and restrict targets to Common/Uncommon/Rare like normal commons (vanilla skips that band when original is Token).
        if (IsManosabaModToken(original))
        {
            IEnumerable<CardModel> tokenSource = GetSourcePoolForCommonCard(original, commonPool);
            CardModel[] tokenCandidates = ApplyVanillaTransformFilters(original, tokenSource, isInCombat, forcePlayableRarityTargetBand: true);
            if (tokenCandidates.Length == 0)
            {
                return false;
            }

            options = tokenCandidates;
            return true;
        }

        // MiriaPuppet / vanilla: curse, status, quest, etc. must use vanilla transform (e.g. TransformToRandom), not merged character+common.
        if (original.Type != CardType.Attack && original.Type != CardType.Skill && original.Type != CardType.Power)
        {
            return false;
        }

        // Historically gated on CommonCardPool.AllCardIds so only [Pool(CommonCardPool)] sources used the merged path.
        // Character-only ids (e.g. Nikaido Hiro) were excluded and fell through to vanilla — skipping Unique-in-deck filtering.
        // Manosaba players still use the same merged source (GetSourcePoolForCommonCard); Hosho and mod tokens return above.
        if (!commonPool.AllCardIds.Contains(original.Id))
        {
            if (original.Owner is not Player p || !ManosabaPlayerHelper.IsManosabaPlayer(p))
            {
                return false;
            }
        }

        IEnumerable<CardModel> source = GetSourcePoolForCommonCard(original, commonPool);
        CardModel[] candidates = ApplyVanillaTransformFilters(original, source, isInCombat);
        if (candidates.Length == 0)
        {
            return false;
        }

        options = candidates;
        LogTransformCandidatePool("TryGet_CommonPath_candidates", original, isInCombat, candidates, original.Owner as Player);
        return true;
    }

    public static bool TryCreateTransformResult(CardModel original, bool isInCombat, MegaCrit.Sts2.Core.Random.Rng rng, out CardModel result)
    {
        result = null!;

        if (!TryGetTransformOptions(original, isInCombat, out IEnumerable<CardModel> options))
        {
            return false;
        }

        CardModel[] filtered = options.ToArray();
        if (filtered.Length == 0)
        {
            return false;
        }

        if (original.Owner is Player player)
        {
            filtered = FilterUniqueAlreadyInDeck(player, filtered);
        }

        CardModel? selected = rng.NextItem(filtered);
        if (selected == null || original.CardScope == null || original.Owner == null)
        {
            return false;
        }

        result = original.CardScope.CreateCard(selected, original.Owner);
        return true;
    }

    private static IEnumerable<CardModel> GetSourcePoolForCommonCard(CardModel original, CommonCardPool commonPool)
    {
        if (original.Owner?.Character?.CardPool == null || original.RunState == null)
        {
            return [];
        }

        return original.Owner.Character.CardPool
            .GetUnlockedCards(original.Owner.UnlockState, original.RunState.CardMultiplayerConstraint)
            .Concat(commonPool.GetUnlockedCards(original.Owner.UnlockState, original.RunState.CardMultiplayerConstraint))
            .GroupBy(card => card.Id)
            .Select(group => group.First());
    }

    private static IEnumerable<CardModel> GetHoshoTarotSource(CardModel original)
    {
        if (original.Owner?.Character?.CardPool == null || original.RunState == null)
        {
            return [];
        }

        return original.Owner.Character.CardPool
            .GetUnlockedCards(original.Owner.UnlockState, original.RunState.CardMultiplayerConstraint)
            .Where(card => card.Tags.Contains(ManosabaCardTags.Tarot) && card is not TheWorld);
    }

    private static bool IsManosabaModToken(CardModel original)
    {
        if (original.Rarity != CardRarity.Token)
        {
            return false;
        }

        Assembly modAssembly = typeof(ManosabaTransformHelper).Assembly;
        return original.GetType().Assembly == modAssembly;
    }

    private static CardModel[] ApplyVanillaTransformFilters(
        CardModel original,
        IEnumerable<CardModel> source,
        bool isInCombat,
        bool forcePlayableRarityTargetBand = false)
    {
        IEnumerable<CardModel> query = source;

        bool narrowTargetRarities = forcePlayableRarityTargetBand
            || (original.Rarity != CardRarity.Ancient && original.Rarity != CardRarity.Token);
        if (narrowTargetRarities)
        {
            query = query.Where(card =>
                card.Rarity == CardRarity.Common ||
                card.Rarity == CardRarity.Uncommon ||
                card.Rarity == CardRarity.Rare);
        }

        if (isInCombat)
        {
            query = query.Where(card => card.CanBeGeneratedInCombat);
        }

        query = query
            .Where(card => card.Id != original.Id)
            .Where(card => original.Owner.RunState.Players.Count > 1
                ? card.MultiplayerConstraint != CardMultiplayerConstraint.SingleplayerOnly
                : card.MultiplayerConstraint != CardMultiplayerConstraint.MultiplayerOnly);

        return query.ToArray();
    }

    private static CardModel[] ApplyHoshoTransformFilters(CardModel original, IEnumerable<CardModel> source, bool isInCombat)
    {
        IEnumerable<CardModel> query = source;

        if (isInCombat)
        {
            query = query.Where(card => card.CanBeGeneratedInCombat);
        }

        query = query
            .Where(card => card.Id != original.Id)
            .Where(card => original.Owner.RunState.Players.Count > 1
                ? card.MultiplayerConstraint != CardMultiplayerConstraint.SingleplayerOnly
                : card.MultiplayerConstraint != CardMultiplayerConstraint.MultiplayerOnly);

        return query.ToArray();
    }

    /// <summary>
    /// Harmony entry for <see cref="CardFactory.CreateRandomCardForTransform"/> with explicit pool; nested here so it shares the same compilation context as transform helpers.
    /// </summary>
    [HarmonyPatch]
    public static class CardFactoryCustomPoolTransformUniquePatch
    {
        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            MethodInfo? m = AccessTools.Method(
                typeof(CardFactory),
                nameof(CardFactory.CreateRandomCardForTransform),
                [typeof(CardModel), typeof(IEnumerable<CardModel>), typeof(bool), typeof(Rng)]);
            if (m != null)
            {
                yield return m;
            }
        }

        [HarmonyPrefix]
        private static void Prefix(CardModel original, ref IEnumerable<CardModel> options, bool isInCombat, Rng rng)
        {
            PrefixCustomPoolTransformOptions(original, isInCombat, ref options);
        }
    }
}

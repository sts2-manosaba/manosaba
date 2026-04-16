using System.Collections.Generic;
using System.Linq;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.HoshoMago.Cards;
using manosaba.Characters.Common;
using manosaba.Characters.HoshoMago;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Patches;

internal static class ManosabaTransformHelper
{
    public static bool TryGetTransformOptions(CardModel original, bool isInCombat, out IEnumerable<CardModel> options)
    {
        options = Enumerable.Empty<CardModel>();

        if (original.Owner?.Character?.CardPool == null)
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
        if (!commonPool.AllCardIds.Contains(original.Id))
        {
            return false;
        }

        IEnumerable<CardModel> source = GetSourcePoolForCommonCard(original, commonPool);
        CardModel[] candidates = ApplyVanillaTransformFilters(original, source, isInCombat);
        if (candidates.Length == 0)
        {
            return false;
        }

        options = candidates;
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

        result = original.CardScope.CreateCard(rng.NextItem(filtered), original.Owner);
        return true;
    }

    private static IEnumerable<CardModel> GetSourcePoolForCommonCard(CardModel original, CommonCardPool commonPool)
    {
        return original.Owner.Character.CardPool
            .GetUnlockedCards(original.Owner.UnlockState, original.RunState.CardMultiplayerConstraint)
            .Concat(commonPool.GetUnlockedCards(original.Owner.UnlockState, original.RunState.CardMultiplayerConstraint))
            .GroupBy(card => card.Id)
            .Select(group => group.First());
    }

    private static IEnumerable<CardModel> GetHoshoTarotSource(CardModel original)
    {
        return original.Owner.Character.CardPool
            .GetUnlockedCards(original.Owner.UnlockState, original.RunState.CardMultiplayerConstraint)
            .Where(card => card.Tags.Contains(ManosabaCardTags.Tarot) && card is not TheWorld);
    }

    private static CardModel[] ApplyVanillaTransformFilters(CardModel original, IEnumerable<CardModel> source, bool isInCombat)
    {
        IEnumerable<CardModel> query = source;

        if (original.Rarity != CardRarity.Ancient && original.Rarity != CardRarity.Token)
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
}

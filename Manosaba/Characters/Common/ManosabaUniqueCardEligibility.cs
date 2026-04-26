using System.Collections.Generic;
using System.Linq;
using Manosaba.Characters.Common.Overrides;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace Manosaba.Characters.Common;

/// <summary>Rules for <see cref="ManosabaKeywords.Unique"/>: per-player deck only; offer filtering for rewards/shop.</summary>
public static class ManosabaUniqueCardEligibility
{
    public static bool IsUniqueTemplate(CardModel card)
        => card.CanonicalKeywords.Contains(ManosabaKeywords.Unique);

    public static bool PlayerDeckContainsCardId(Player player, ModelId cardId)
    {
        if (player.Deck?.Cards == null)
            return false;

        return player.Deck.Cards.Any(c => c.Id == cardId);
    }

    /// <summary>True if this template should not be offered to <paramref name="player"/> (already in their deck).</summary>
    public static bool IsBlockedForPlayerOffer(Player player, CardModel templateOrInstance)
    {
        if (!IsUniqueTemplate(templateOrInstance))
            return false;

        return PlayerDeckContainsCardId(player, templateOrInstance.Id);
    }

    /// <summary>
    /// Merchant rolls cards from this pool before <see cref="Hook.ModifyMerchantCardCreationResults"/>; vanilla never copies the hook list back into <c>MerchantCardEntry.CreationResult</c>, so filtering must happen here.
    /// </summary>
    public static IEnumerable<CardModel> FilterMerchantCardPool(Player player, IEnumerable<CardModel>? options)
    {
        if (options == null)
            return options!;

        return options.Where(c => !IsBlockedForPlayerOffer(player, c));
    }

    public static CardCreationOptions FilterCardCreationOptions(Player player, CardCreationOptions options)
    {
        if (options.Flags.HasFlag(CardCreationFlags.NoCardPoolModifications))
            return options;

        Func<CardModel, bool>? previous = options.CardPoolFilter;

        bool Combined(CardModel c)
        {
            if (previous != null && !previous(c))
                return false;
            if (IsBlockedForPlayerOffer(player, c))
                return false;
            return true;
        }

        if (options.CustomCardPool != null)
        {
            CardModel[] filtered = options.GetPossibleCards(player).Where(Combined).ToArray();
            if (filtered.Length == 0)
                return options;
            return options.WithCustomPool(filtered, options.RarityOdds);
        }

        // WithCardPools clears _cardPools before AddRange; CardPools returns that same list — snapshot first.
        List<CardPoolModel> poolSnapshot = options.CardPools.ToList();
        return options.WithCardPools(poolSnapshot, Combined);
    }

    public static void FilterCardCreationResults(Player player, List<CardCreationResult> results)
    {
        for (int i = results.Count - 1; i >= 0; i--)
        {
            CardModel card = results[i].Card;
            if (IsBlockedForPlayerOffer(player, card))
                results.RemoveAt(i);
        }
    }
}

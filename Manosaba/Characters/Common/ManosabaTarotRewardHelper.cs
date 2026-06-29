using manosaba.Characters.HoshoMago;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.HoshoMago.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.Common;

internal static class ManosabaTarotRewardHelper
{
    internal static bool UsesTarotPoolInsteadOfStandardRewards(CardPoolModel pool)
        => pool is HoshoMagoCardPool;

    internal static List<CardModel> GetOfferableTarotPool(Player player)
    {
        List<CardModel> tarotPool = ModelDb.CardPool<HoshoMagoCardPool>()
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .Where(card => card.Tags.Contains(ManosabaCardTags.Tarot) && card is not TheWorld)
            .Where(card => !ManosabaUniqueCardEligibility.IsBlockedForPlayerOffer(player, card))
            .ToList();

        if (tarotPool.Count > 0)
        {
            return tarotPool;
        }

        return ModelDb.CardPool<HoshoMagoCardPool>()
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .Where(card => card.Tags.Contains(ManosabaCardTags.Tarot) && card is not TheWorld)
            .ToList();
    }

    internal static CardModel? PickRandomTarot(Player player, List<CardModel> tarotPool, HashSet<ModelId>? excludeIds = null)
    {
        if (tarotPool.Count == 0)
        {
            return null;
        }

        List<CardModel> available = excludeIds is { Count: > 0 }
            ? tarotPool.Where(card => !excludeIds.Contains(card.Id)).ToList()
            : tarotPool.ToList();

        if (available.Count == 0)
        {
            available = tarotPool.ToList();
        }

        CardModel? canonical = player.RunState.Rng.Niche.NextItem(available);
        if (canonical == null)
        {
            return null;
        }

        return player.RunState.CreateCard(canonical, player);
    }

    internal static CardModel? PickRandomTarotOrLog(Player player, List<CardModel> tarotPool, string context)
    {
        CardModel? card = PickRandomTarot(player, tarotPool);
        if (card == null)
        {
            Log.Error($"{context}: tarot pool was empty.");
        }

        return card;
    }
}

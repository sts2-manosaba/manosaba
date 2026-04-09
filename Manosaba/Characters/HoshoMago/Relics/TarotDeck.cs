using BaseLib.Utils;
using manosaba.Characters.Common;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.HoshoMago.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace manosaba.Characters.HoshoMago.Relics;

[Pool(typeof(HoshoMagoRelicPool))]
public sealed class TarotDeck : LevelingPathCustomRelicModel
{
    private const int TarotKindsExcludingWorld = 21;

    public override RelicRarity Rarity => RelicRarity.Starter;
    protected override int MaxRelicLevel => 5;

    public override async Task AfterObtained()
    {
        if (Owner?.RunState == null)
        {
            return;
        }

        List<CardModel> tarotPool = GetEligibleTarotCards(Owner);
        if (tarotPool.Count > 0)
        {
            CardModel randomTarot = Owner.RunState.Rng.Niche.NextItem(tarotPool);
            await AddCardToDeckWithPreview(randomTarot);
        }

        await TryAddWorldWhenCollectionCompleted();
    }

    public override async Task AfterRewardTaken(Player player, Reward reward)
    {
        _ = reward;

        if (Owner == player)
        {
            await TryAddWorldWhenCollectionCompleted();
        }
    }

    public override CardCreationOptions ModifyCardRewardCreationOptions(Player player, CardCreationOptions options)
    {
        if (Owner != player)
        {
            return options;
        }
        CardCreationOptions cardCreationOptions = new CardCreationOptions(options.CardPools, options.Source, options.RarityOdds);
        cardCreationOptions.WithCardPools([ModelDb.CardPool<CommonCardPool>()]);
        return cardCreationOptions;
    }

    public override bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
    {
        _ = creationOptions;

        if (Owner != player)
        {
            return false;
        }

        List<CardModel> tarotPool = GetEligibleTarotCards(player);
        if (tarotPool.Count <= 0 || cardRewardOptions.Count <= 0)
        {
            return false;
        }

        List<CardCreationResult> rerolled = [];
        List<CardModel> available = tarotPool.ToList();
        for (int i = 0; i < cardRewardOptions.Count; i++)
        {
            if (available.Count <= 0)
            {
                available = tarotPool.ToList();
            }

            CardModel canonical = player.RunState.Rng.Niche.NextItem(available);
            available.Remove(canonical);

            CardModel card = player.RunState.CreateCard(canonical, player);
            rerolled.Add(new CardCreationResult(card));
        }

        cardRewardOptions.Clear();
        cardRewardOptions.AddRange(rerolled);
        return true;
    }

    public override IEnumerable<CardModel> ModifyMerchantCardPool(Player player, IEnumerable<CardModel> options)
    {
        if (Owner != player)
        {
            return options;
        }
        return GetEligibleTarotCards(player);
    }

    public override CardRarity ModifyMerchantCardRarity(Player player, CardRarity rarity)
    {
        if (Owner != player)
        {
            return rarity;
        }
        return CardRarity.Ancient;
    }

    public override void ModifyMerchantCardCreationResults(Player player, List<CardCreationResult> cards)
    {
        if (Owner != player || cards.Count <= 0)
        {
            return;
        }

        List<CardModel> tarotPool = GetEligibleTarotCards(player);
        if (tarotPool.Count <= 0)
        {
            return;
        }

        List<CardModel> orderedTarotPool = tarotPool.OrderBy(card => card.Id).ToList();
        HashSet<ModelId> pickedTarotIds = [];

        for (int i = 0; i < cards.Count; i++)
        {
            CardCreationResult creationResult = cards[i];
            if (creationResult.Card.Tags.Contains(ManosabaCardTags.Tarot))
            {
                pickedTarotIds.Add(creationResult.Card.Id);
                continue;
            }

            CardModel canonical = PickStableMerchantTarot(orderedTarotPool, pickedTarotIds, creationResult.Card.Id, i);
            CardModel card = player.RunState.CreateCard(canonical, player);
            creationResult.ModifyCard(card, this);
            pickedTarotIds.Add(canonical.Id);
        }
    }

    public override decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal cost)
    {
        if (Owner != player)
        {
            return cost;
        }

        if (entry is not MerchantCardEntry cardEntry)
        {
            return cost;
        }

        if (cardEntry.CreationResult?.Card.Tags.Contains(ManosabaCardTags.Tarot) != true)
        {
            return cost;
        }

        decimal basePrice = cardEntry.IsOnSale ? 100m : 200m;

        // MerchantCardEntry uses base 50 with a 0.95~1.05 roll (halved if on sale).
        decimal sourceCenter = cardEntry.IsOnSale ? 25m : 50m;
        decimal sourceHalfRange = sourceCenter * 0.05m;
        decimal normalized = sourceHalfRange <= 0m ? 0m : (cost - sourceCenter) / sourceHalfRange;
        normalized = Math.Clamp(normalized, -1m, 1m);

        decimal target = basePrice + normalized * (basePrice * 0.15m);
        return decimal.Round(target, 0, MidpointRounding.AwayFromZero);
    }

    protected override void OnRelicLevelChanged(int oldLevel, int newLevel)
    {
        TaskHelper.RunSafely(OnRelicLevelChangedAsync(oldLevel, newLevel));
    }

    private async Task OnRelicLevelChangedAsync(int oldLevel, int newLevel)
    {
        if (Owner?.Deck == null || Owner?.RunState == null || newLevel <= oldLevel)
        {
            return;
        }

        int cardsToAdd = newLevel - oldLevel;
        List<CardModel> tarotPool = GetEligibleTarotCards(Owner);
        if (tarotPool.Count <= 0)
        {
            return;
        }

        var ownedCardIds = Owner.Deck.Cards.Select(card => card.Id).ToHashSet();

        for (int i = 0; i < cardsToAdd; i++)
        {
            List<CardModel> unownedTarot = tarotPool.Where(card => !ownedCardIds.Contains(card.Id)).ToList();
            if (unownedTarot.Count <= 0)
            {
                break;
            }

            CardModel canonical = Owner.RunState.Rng.Niche.NextItem(unownedTarot);
            CardModel? addedCard = await AddCardToDeckWithPreview(canonical);
            if (addedCard != null)
            {
                ownedCardIds.Add(addedCard.Id);
            }
        }

        await TryAddWorldWhenCollectionCompleted();
    }

    private static List<CardModel> GetEligibleTarotCards(Player player)
    {
        return ModelDb.CardPool<HoshoMagoCardPool>()
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .Where(card => card.Tags.Contains(ManosabaCardTags.Tarot) && card is not TheWorld)
            .ToList();
    }

    private async Task TryAddWorldWhenCollectionCompleted()
    {
        if (Owner?.Deck == null || Owner?.RunState == null)
        {
            return;
        }

        if (Owner.Deck.Cards.Any(card => card is TheWorld))
        {
            return;
        }

        int ownedTarotKinds = Owner.Deck.Cards
            .Where(card => card.Tags.Contains(ManosabaCardTags.Tarot) && card is not TheWorld)
            .Select(card => card.Id)
            .Distinct()
            .Count();

        if (ownedTarotKinds < TarotKindsExcludingWorld)
        {
            return;
        }

        await AddCardToDeckWithPreview(ModelDb.Card<TheWorld>());
    }

    private async Task<CardModel?> AddCardToDeckWithPreview(CardModel canonicalCard)
    {
        if (Owner?.RunState == null)
        {
            return null;
        }

        CardModel card = Owner.RunState.CreateCard(canonicalCard, Owner);
        CardPileAddResult result = await CardPileCmd.Add(card, PileType.Deck);
        CardCmd.PreviewCardPileAdd(result, 1.2f, CardPreviewStyle.GridLayout);
        return result.success ? result.cardAdded : null;
    }

    private static CardModel PickStableMerchantTarot(List<CardModel> orderedTarotPool, HashSet<ModelId> pickedTarotIds, ModelId sourceCardId, int slotIndex)
    {
        if (orderedTarotPool.Count <= 1)
        {
            return orderedTarotPool[0];
        }

        uint hash = StableHash(sourceCardId, slotIndex);
        int start = (int)(hash % (uint)orderedTarotPool.Count);

        for (int offset = 0; offset < orderedTarotPool.Count; offset++)
        {
            CardModel candidate = orderedTarotPool[(start + offset) % orderedTarotPool.Count];
            if (!pickedTarotIds.Contains(candidate.Id))
            {
                return candidate;
            }
        }

        return orderedTarotPool[start];
    }

    private static uint StableHash(ModelId cardId, int slotIndex)
    {
        const uint FnvOffset = 2166136261;
        const uint FnvPrime = 16777619;

        string idText = cardId.ToString();
        uint hash = FnvOffset;
        for (int i = 0; i < idText.Length; i++)
        {
            hash ^= idText[i];
            hash *= FnvPrime;
        }

        hash ^= (uint)slotIndex;
        hash *= FnvPrime;
        return hash;
    }

}

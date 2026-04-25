using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Modifiers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using System.Globalization;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(SealedDeck), nameof(SealedDeck.GenerateNeowOption))]
public static class Patch_SealedDeck_ManosabaRewardCount
{
    private const int RequestedRewardCount = 30;
    private const int RequiredSelectionCount = 10;

    [HarmonyPrefix]
    private static bool Prefix(EventModel eventModel, ref Func<Task> __result)
    {
        if (eventModel.Owner is not Player player || !IsManosabaPlayer(player))
        {
            return true;
        }

        __result = () => ChooseCards(player);
        return false;
    }

    private static async Task ChooseCards(Player player)
    {
        CardCreationOptions options = new CardCreationOptions(
                [player.Character.CardPool],
                CardCreationSource.Other,
                CardRarityOddsType.RegularEncounter)
            .WithFlags(CardCreationFlags.NoUpgradeRoll | CardCreationFlags.ForceRarityOddsChange);

        List<CardCreationResult> rewards = CreateRewards(player, options);
        CardSelectorPrefs prefs = new(new LocString("modifiers", "SEALED_DECK.selectionPrompt"), RequiredSelectionCount)
        {
            Cancelable = false,
            RequireManualConfirmation = true,
            Comparison = CompareCards
        };

        List<CardModel> cards = (await CardSelectCmd.FromSimpleGridForRewards(
            new BlockingPlayerChoiceContext(),
            rewards,
            player,
            prefs)).ToList();

        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(cards, PileType.Deck), 1.2f, CardPreviewStyle.GridLayout);
        foreach (Player runPlayer in player.RunState.Players)
        {
            runPlayer.RelicGrabBag.Remove<PandorasBox>();
        }

        player.RunState.SharedRelicGrabBag.Remove<PandorasBox>();
    }

    private static List<CardCreationResult> CreateRewards(Player player, CardCreationOptions options)
    {
        List<CardModel> rewardCandidates = GetRewardCandidates(player, options);
        int rewardCount = Math.Min(RequestedRewardCount, rewardCandidates.Count);

        if (rewardCount >= RequiredSelectionCount)
        {
            try
            {
                return CardFactory.CreateForReward(player, rewardCount, options).ToList();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("valid rarity", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("valid card", StringComparison.OrdinalIgnoreCase))
            {
            }
        }

        return CreateManualRewards(player, rewardCandidates);
    }

    private static List<CardModel> GetRewardCandidates(Player player, CardCreationOptions options)
    {
        return FilterForPlayerCount(player, options.GetPossibleCards(player))
            .Where(IsRewardRarity)
            .GroupBy(card => card.Id)
            .Select(group => group.First())
            .ToList();
    }

    private static List<CardCreationResult> CreateManualRewards(Player player, List<CardModel> rewardCandidates)
    {
        List<CardModel> candidates = rewardCandidates.Count > 0
            ? rewardCandidates
            : GetFallbackCandidates(player);

        List<CardCreationResult> rewards = [];
        if (candidates.Count == 0)
        {
            return rewards;
        }

        Rng rng = player.PlayerRng.Rewards;
        List<CardModel> available = candidates.ToList();
        int uniqueCount = Math.Min(RequestedRewardCount, available.Count);
        for (int i = 0; i < uniqueCount; i++)
        {
            CardModel? canonical = rng.NextItem(available);
            if (canonical == null)
            {
                break;
            }

            rewards.Add(new CardCreationResult(player.RunState.CreateCard(canonical, player)));
            available.Remove(canonical);
        }

        while (rewards.Count < RequiredSelectionCount)
        {
            CardModel? canonical = rng.NextItem(candidates);
            if (canonical == null)
            {
                break;
            }

            rewards.Add(new CardCreationResult(player.RunState.CreateCard(canonical, player)));
        }

        return rewards;
    }

    private static List<CardModel> GetFallbackCandidates(Player player)
    {
        return FilterForPlayerCount(player, player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint))
            .Where(card => card.Rarity is not CardRarity.Token and not CardRarity.Status and not CardRarity.Curse and not CardRarity.Quest)
            .GroupBy(card => card.Id)
            .Select(group => group.First())
            .ToList();
    }

    private static IEnumerable<CardModel> FilterForPlayerCount(Player player, IEnumerable<CardModel> options)
    {
        return player.RunState.Players.Count > 1
            ? options.Where(card => card.MultiplayerConstraint != CardMultiplayerConstraint.SingleplayerOnly)
            : options.Where(card => card.MultiplayerConstraint != CardMultiplayerConstraint.MultiplayerOnly);
    }

    private static bool IsRewardRarity(CardModel card)
    {
        return card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare;
    }

    private static bool IsManosabaPlayer(Player player)
    {
        string? poolNamespace = player.Character?.CardPool.GetType().Namespace;
        return poolNamespace != null && poolNamespace.StartsWith("manosaba.", StringComparison.OrdinalIgnoreCase);
    }

    private static int CompareCards(CardModel card1, CardModel card2)
    {
        if (card1.Rarity != card2.Rarity)
        {
            return card1.Rarity.CompareTo(card2.Rarity);
        }

        return string.Compare(card1.Title, card2.Title, LocManager.Instance.CultureInfo, CompareOptions.None);
    }
}

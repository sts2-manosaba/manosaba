using System.Reflection;
using HarmonyLib;
using manosaba.Characters.HoshoMago;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;

namespace Manosaba.Characters.Common;

internal static class ManosabaSeaGlassHelper
{
    private static readonly MethodInfo AncientDoneMethod =
        AccessTools.Method(typeof(AncientEventModel), "Done")
        ?? throw new InvalidOperationException("AncientEventModel.Done not found.");

    internal static bool UsesTarotRewardPool(SeaGlass seaGlass)
    {
        if (seaGlass.Owner == null)
        {
            return false;
        }

        if (IsHoshoMagoPlayer(seaGlass.Owner))
        {
            return true;
        }

        if (seaGlass.CharacterId == null)
        {
            return false;
        }

        CharacterModel? character = ModelDb.GetById<CharacterModel>(seaGlass.CharacterId);
        if (character?.CardPool is HoshoMagoCardPool)
        {
            return true;
        }

        return character != null && !HasStandardRewardCards(character, seaGlass.Owner);
    }

    private static bool IsHoshoMagoPlayer(Player player)
        => player.Character?.CardPool is HoshoMagoCardPool;

    internal static void TryFinishAncientEventForPlayer(Player player)
    {
        if (RunManager.Instance?.EventSynchronizer?.GetEventForPlayer(player) is not AncientEventModel ancient
            || ancient.IsFinished)
        {
            return;
        }

        try
        {
            AncientDoneMethod.Invoke(ancient, null);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to finish ancient event for player {player.NetId} after Sea Glass: {ex}");
        }
    }

    internal static void EnsureCurrentMapPointPlayerEntry(Player player)
    {
        MapPointHistoryEntry? entry = player.RunState.CurrentMapPointHistoryEntry;
        if (entry == null)
        {
            return;
        }

        if (entry.PlayerStats.Any(stat => stat.PlayerId == player.NetId))
        {
            return;
        }

        entry.PlayerStats.Add(new PlayerMapPointHistoryEntry
        {
            PlayerId = player.NetId
        });
    }

    internal static bool TryHandleAfterObtained(SeaGlass seaGlass, ref Task __result)
    {
        if (!UsesTarotRewardPool(seaGlass))
        {
            return false;
        }

        __result = RunTarotAfterObtained(seaGlass);
        return true;
    }

    private static bool HasStandardRewardCards(CharacterModel character, Player player)
    {
        IEnumerable<CardModel> pool = character.CardPool.GetUnlockedCards(
            player.UnlockState,
            player.RunState.CardMultiplayerConstraint);

        return pool.Any(card => card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare);
    }

    private static async Task RunTarotAfterObtained(SeaGlass seaGlass)
    {
        Player player = seaGlass.Owner;
        if (seaGlass.CharacterId == null)
        {
            Log.Error("Sea Glass was obtained without a character ID assigned! Defaulting to Ironclad");
            seaGlass.CharacterId = ModelDb.Character<Ironclad>().Id;
        }

        EnsureCurrentMapPointPlayerEntry(player);

        int totalCards = seaGlass.DynamicVars.Cards.IntValue;
        List<CardModel> tarotPool = ManosabaTarotRewardHelper.GetOfferableTarotPool(player);
        if (tarotPool.Count == 0)
        {
            Log.Error("Sea Glass tarot pool was empty.");
            return;
        }

        List<CardCreationResult> options = [];
        HashSet<ModelId> offeredIds = [];
        for (int i = 0; i < totalCards; i++)
        {
            CardModel? card = ManosabaTarotRewardHelper.PickRandomTarot(player, tarotPool, offeredIds);
            if (card == null)
            {
                break;
            }

            offeredIds.Add(card.Id);
            options.Add(new CardCreationResult(card));
        }

        if (options.Count == 0)
        {
            Log.Error("Sea Glass failed to generate tarot card offers.");
            return;
        }

        try
        {
            IReadOnlyList<CardModel> selected = (await CardSelectCmd.FromSimpleGridForRewards(
                context: new BlockingPlayerChoiceContext(),
                cards: options,
                player: player,
                prefs: new CardSelectorPrefs(
                    new LocString("relics", seaGlass.Id.Entry + ".selectionScreenPrompt"),
                    0,
                    options.Count))).ToList();

            foreach (CardModel item in selected)
            {
                try
                {
                    CardPileAddResult result = await CardPileCmd.Add(
                        item,
                        PileType.Deck,
                        skipVisuals: true);
                    if (result.success)
                    {
                        CardCmd.PreviewCardPileAdd(result);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Sea Glass failed to add {item.Id}: {ex}");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Sea Glass tarot card selection failed: {ex}");
            throw;
        }
    }
}

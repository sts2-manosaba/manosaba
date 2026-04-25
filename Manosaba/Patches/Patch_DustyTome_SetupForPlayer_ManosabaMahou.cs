using HarmonyLib;
using Manosaba.Characters.Common.Overrides;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(DustyTome), nameof(DustyTome.SetupForPlayer))]
public static class Patch_DustyTome_SetupForPlayer_ManosabaMahou
{
    [HarmonyPrefix]
    private static bool Prefix(DustyTome __instance, Player player)
    {
        if (!IsManosabaCharacter(player))
        {
            return true;
        }

        List<CardModel> mahouCards = player.Character.CardPool
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .Where(c => c.Keywords.Contains(ManosabaKeywords.Mahou))
            .ToList();

        if (mahouCards.Count == 0)
        {
            return true;
        }

        CardModel? selected = player.PlayerRng.Rewards.NextItem(mahouCards);
        if (selected == null)
        {
            return true;
        }

        __instance.AncientCard = selected.Id;
        return false;
    }

    private static bool IsManosabaCharacter(Player player)
    {
        string? characterNamespace = player.Character?.GetType().Namespace;
        return characterNamespace != null &&
               characterNamespace.StartsWith("manosaba.Characters", StringComparison.OrdinalIgnoreCase);
    }
}

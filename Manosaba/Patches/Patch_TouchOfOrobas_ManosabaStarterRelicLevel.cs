using HarmonyLib;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(TouchOfOrobas), nameof(TouchOfOrobas.AfterObtained))]
public static class Patch_TouchOfOrobas_ManosabaStarterRelicLevel
{
    [HarmonyPrefix]
    private static bool Prefix(TouchOfOrobas __instance, ref Task __result)
    {
        Player owner = __instance.Owner;
        if (!IsManosabaCharacter(owner))
        {
            return true;
        }

        RelicModel starterRelic = ResolveStarterRelic(__instance, owner);
        if (starterRelic is LevelingPathCustomRelicModel levelingRelic)
        {
            levelingRelic.RelicExp = RelicLevelExpTable.GetMaxExpForLevel(5);
        }

        __result = Task.CompletedTask;
        return false;
    }

    private static RelicModel ResolveStarterRelic(TouchOfOrobas relic, Player owner)
    {
        ModelId starterRelicId = relic.StarterRelic ?? owner.Relics.First((RelicModel r) => r.Rarity == RelicRarity.Starter).Id;
        return owner.GetRelicById(starterRelicId);
    }

    private static bool IsManosabaCharacter(Player owner)
    {
        string? characterNamespace = owner.Character?.GetType().Namespace;
        return characterNamespace != null &&
               characterNamespace.StartsWith("manosaba.Characters", StringComparison.OrdinalIgnoreCase);
    }
}

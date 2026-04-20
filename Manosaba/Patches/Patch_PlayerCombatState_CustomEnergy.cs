using HarmonyLib;
using Manosaba.Characters.Common.Resources;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace Manosaba.Patches;

[HarmonyPatch]
public static class Patch_PlayerCombatState_CustomEnergy
{
    private static readonly AccessTools.FieldRef<PlayerCombatState, Player> PlayerRef =
        AccessTools.FieldRefAccess<PlayerCombatState, Player>("_player");

    [HarmonyPatch(typeof(PlayerCombatState), nameof(PlayerCombatState.AfterCombatEnd))]
    [HarmonyPostfix]
    private static void PlayerCombatState_AfterCombatEnd_Postfix(PlayerCombatState __instance)
    {
        CharacterCustomEnergyService.OnCombatEnd(PlayerRef(__instance));
    }

    [HarmonyPatch(typeof(RunState), nameof(RunState.CreateForNewRun))]
    [HarmonyPrefix]
    private static void RunState_CreateForNewRun_Prefix()
    {
        CharacterCustomEnergyService.ClearAll();
    }
}

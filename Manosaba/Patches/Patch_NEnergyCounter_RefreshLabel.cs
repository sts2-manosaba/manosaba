using HarmonyLib;
using manosaba.Characters.Common;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

[HarmonyPatch(typeof(NEnergyCounter), "RefreshLabel")]
public static class Patch_NEnergyCounter_RefreshLabel
{
    private static readonly AccessTools.FieldRef<NEnergyCounter, Player> _playerRef =
        AccessTools.FieldRefAccess<NEnergyCounter, Player>("_player");

    static void Postfix(NEnergyCounter __instance)
    {
        if (__instance is not ManosabaEnergyCounter) return;

        var label = __instance.GetNodeOrNull<ManosabaLabel>("Label");
        if (label == null) return;

        Player? player = _playerRef(__instance);
        if (player?.PlayerCombatState == null) return;

        int energy = player.PlayerCombatState.Energy;
        label.RestoreStyleKeepingZeroEnergyColor(energy == 0);
    }
}

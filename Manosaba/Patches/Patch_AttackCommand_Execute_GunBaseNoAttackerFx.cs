using HarmonyLib;
using Manosaba.Characters.KurobeNanoka.Cards;
using MegaCrit.Sts2.Core.Commands.Builders;

[HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.Execute))]
public static class Patch_AttackCommand_Execute_GunBaseNoAttackerFx
{
    private static void Prefix(AttackCommand __instance)
    {
        object? modelSource = Traverse.Create(__instance).Property("ModelSource").GetValue();
        if (modelSource is not GunBase)
        {
            return;
        }

        Traverse traverse = Traverse.Create(__instance);
        traverse.Field("_attackerVfx").SetValue(null);
        traverse.Field("_attackerSfx").SetValue(null);
        traverse.Field("_attackerAnimName").SetValue(null);
        traverse.Field("_attackerAnimDelay").SetValue(0f);
    }
}

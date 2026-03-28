using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

[HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.FromOsty))]
public static class Patch_AttackCommand_FromOsty_AnyCreatureNoCard
{
    static bool Prefix(
        AttackCommand __instance,
        Creature osty,
        CardModel card, // 會被忽略
        ref AttackCommand __result)
    {
        // Attacker
        Traverse.Create(__instance).Property("Attacker").SetValue(osty);

        // ModelSource = null (忽略 card)
        Traverse.Create(__instance).Property("ModelSource").SetValue(null);

        // _sourceType = Card (避免走 Monster 邏輯)
        var sourceTypeEnum = AccessTools.Inner(typeof(AttackCommand), "SourceType");
        var cardValue = System.Enum.Parse(sourceTypeEnum, "Card");
        Traverse.Create(__instance).Field("_sourceType").SetValue(cardValue);

        // 設定攻擊動畫欄位，讓 WithAttackerAnim 可用
        Traverse.Create(__instance).Field("_attackerAnimName").SetValue("Attack");
        Traverse.Create(__instance).Field("_attackerAnimDelay").SetValue(0.3f);

        // 可選：沿用原本 Osty sfx
        __instance.WithAttackerFx(null, "event:/sfx/characters/osty/osty_attack");

        __result = __instance;
        return false; // 跳過原本 FromOsty
    }
}

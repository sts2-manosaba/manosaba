using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Models;
using System.Reflection;

[HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.FromMonster))]
public static class Patch_AttackCommand_FromMonster_PetTargeting
{
    private const int SourceTypeCardValue = 1;

    public static void Postfix(AttackCommand __instance, MonsterModel monster)
    {
        if (monster?.Creature == null || !monster.Creature.IsPet)
        {
            return;
        }

        // AttackCommand.GetPossibleTargets hardcodes PlayerCreatures when _sourceType == Monster.
        // For pets on player side, switch _sourceType to Card so targeting uses opponents of attacker.
        FieldInfo? sourceTypeField = AccessTools.Field(typeof(AttackCommand), "_sourceType");
        if (sourceTypeField == null)
        {
            return;
        }

        object enumValue = Enum.ToObject(sourceTypeField.FieldType, SourceTypeCardValue);
        sourceTypeField.SetValue(__instance, enumValue);
    }
}

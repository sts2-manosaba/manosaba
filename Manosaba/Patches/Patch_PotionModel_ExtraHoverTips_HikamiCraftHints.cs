using HarmonyLib;
using Manosaba.Characters.HikamiMeruru.PotionCraft;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Manosaba.Patches
{
    [HarmonyPatch]
    public static class Patch_PotionModel_ExtraHoverTips_HikamiCraftHints
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            MethodInfo? baseGetter = AccessTools.PropertyGetter(typeof(PotionModel), nameof(PotionModel.ExtraHoverTips));
            if (baseGetter is null)
            {
                yield break;
            }

            yield return baseGetter;

            foreach (Type type in AccessTools.AllTypes())
            {
                if (!typeof(PotionModel).IsAssignableFrom(type) || type.IsAbstract || type == typeof(PotionModel))
                {
                    continue;
                }

                MethodInfo? getter = type.GetProperty(
                    nameof(PotionModel.ExtraHoverTips),
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?.GetMethod;

                if (getter is null || getter == baseGetter || getter.DeclaringType != type)
                {
                    continue;
                }

                yield return getter;
            }
        }

        [HarmonyPostfix]
        private static void Postfix(PotionModel __instance, ref IEnumerable<IHoverTip> __result)
        {
            List<PotionRecipe> matchedRecipes = PotionRecipeTable.Recipes
                .Where(recipe => recipe.Ingredients.Keys.Any(ingredientType => IsIngredientMatch(ingredientType, __instance)))
                .ToList();

            if (matchedRecipes.Count == 0)
            {
                return;
            }

            List<IHoverTip> tips = __result?.ToList() ?? [];
            string formulaText = BuildFormulaText(matchedRecipes);
            tips.Add(new HoverTip(
                new MegaCrit.Sts2.Core.Localization.LocString("static_hover_tips", "MANOSABA-POTION_CRAFT_FORMULA.title"),
                formulaText));
            __result = tips;
        }

        private static PotionModel ResolvePotionModel(Type potionType)
        {
            return ModelDb.GetById<PotionModel>(ModelDb.GetId(potionType));
        }

        private static bool IsIngredientMatch(Type ingredientType, PotionModel potion)
        {
            Type actualType = potion.GetType();
            if (ingredientType == actualType || ingredientType.IsAssignableFrom(actualType))
            {
                return true;
            }

            return ModelDb.GetId(ingredientType) == potion.Id;
        }

        private static string BuildFormulaText(IEnumerable<PotionRecipe> recipes)
        {
            return string.Join("\n", recipes.Select(BuildSingleFormulaLine));
        }

        private static string BuildSingleFormulaLine(PotionRecipe recipe)
        {
            string left = string.Join(" + ", recipe.Ingredients.Select(FormatIngredient));
            string right = recipe.ResultPotionType.Title.GetFormattedText();
            return $"{left} = {right}";
        }

        private static string FormatIngredient(KeyValuePair<Type, int> ingredient)
        {
            PotionModel ingredientModel = ResolvePotionModel(ingredient.Key);
            string title = ingredientModel.Title.GetFormattedText();
            return ingredient.Value > 1 ? $"{ingredient.Value}x {title}" : title;
        }
    }
}

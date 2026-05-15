using System;
using System.Threading;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.HikamiMeruru.PotionCraft
{
    public static class PotionCraftService
    {
        private static readonly AsyncLocal<int> CraftDiscardDepth = new();

        public static bool IsCraftDiscardSuppressed => CraftDiscardDepth.Value > 0;

        public static PotionRecipe? FindFirstCraftableRecipe(IEnumerable<PotionModel?> potionSlots)
        {
            return PotionRecipeTable.Recipes.FirstOrDefault(r => r.CanCraft(potionSlots));
        }

        public static PotionRecipe? FindRecipeForPair(PotionModel? first, PotionModel? second)
        {
            if (first == null || second == null || ReferenceEquals(first, second))
                return null;

            Dictionary<Type, int> pairCounts = new[] { first.GetType(), second.GetType() }
                .GroupBy(t => t)
                .ToDictionary(g => g.Key, g => g.Count());

            return PotionRecipeTable.Recipes.FirstOrDefault(recipe =>
                recipe.Ingredients.Count == pairCounts.Count &&
                recipe.Ingredients.All(ingredient =>
                    pairCounts.TryGetValue(ingredient.Key, out int count) && count == ingredient.Value));
        }

        public static async Task<bool> TryCraftPair(Player owner, PotionModel first, PotionModel second)
        {
            if (owner == null || first == null || second == null || ReferenceEquals(first, second))
                return false;

            if (first.Owner != owner || second.Owner != owner)
                return false;

            if (!owner.PotionSlots.Contains(first) || !owner.PotionSlots.Contains(second))
                return false;

            PotionRecipe? recipe = FindRecipeForPair(first, second);
            if (recipe == null)
                return false;

            using (BeginCraftDiscardScope())
            {
                await PotionCmd.Discard(first);
                await PotionCmd.Discard(second);
            }

            await PotionCmd.TryToProcure(recipe.ResultPotionType.ToMutable(), owner);
            return true;
        }

        public static async Task<bool> TryCraft(Player owner, IEnumerable<PotionModel?> potionSlots, PotionRecipe? recipe)
        {
            if (owner == null || potionSlots == null || recipe == null)
                return false;

            var potions = potionSlots
                .OfType<PotionModel>()
                .ToList();

            using (BeginCraftDiscardScope())
            {
                foreach (var ingredient in recipe.Ingredients)
                {
                    for (int i = 0; i < ingredient.Value; i++)
                    {
                        var potion = potions.FirstOrDefault(p => p.GetType() == ingredient.Key);
                        if (potion == null)
                            return false;

                        potions.Remove(potion);
                        await PotionCmd.Discard(potion);
                    }
                }
            }

            await PotionCmd.TryToProcure(recipe.ResultPotionType.ToMutable(), owner);
            return true;
        }

        private static IDisposable BeginCraftDiscardScope()
        {
            CraftDiscardDepth.Value++;
            return new ScopeGuard();
        }

        private sealed class ScopeGuard : IDisposable
        {
            public void Dispose()
            {
                CraftDiscardDepth.Value = Math.Max(0, CraftDiscardDepth.Value - 1);
            }
        }
    }
}

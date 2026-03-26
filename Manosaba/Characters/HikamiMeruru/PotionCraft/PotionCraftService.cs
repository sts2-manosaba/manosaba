using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.HikamiMeruru.PotionCraft
{
    public static class PotionCraftService
    {
        public static PotionRecipe FindFirstCraftableRecipe(IEnumerable<PotionModel> potionSlots)
        {
            return PotionRecipeTable.Recipes.FirstOrDefault(r => r.CanCraft(potionSlots), null);
        }

        public static async Task<bool> TryCraft(Player owner, IEnumerable<PotionModel> potionSlots, PotionRecipe recipe)
        {
            if (owner == null || potionSlots == null || recipe == null)
                return false;

            var potions = potionSlots
                .Where(p => p != null)
                .ToList();

            foreach (var ingredient in recipe.Ingredients)
            {
                for (int i = 0; i < ingredient.Value; i++)
                {
                    var potion = potions.FirstOrDefault(p => p != null && p.GetType() == ingredient.Key);
                    if (potion == null)
                        return false;

                    potions.Remove(potion);
                    await PotionCmd.Discard(potion);
                }
            }
            await PotionCmd.TryToProcure(recipe.ResultPotionType.ToMutable(), owner);
            return true;
        }
    }
}
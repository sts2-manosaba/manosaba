using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.HikamiMeruru.PotionCraft
{
    public class PotionRecipe
    {
        public IReadOnlyDictionary<Type, int> Ingredients { get; }
        public PotionModel ResultPotionType { get; }

        public PotionRecipe(Dictionary<Type, int> ingredients, PotionModel resultPotionType)
        {
            Ingredients = ingredients;
            ResultPotionType = resultPotionType;
        }

        public bool CanCraft(IEnumerable<PotionModel> potionSlots)
        {
            var counts = potionSlots
                .Where(p => p != null)
                .GroupBy(p => p.GetType())
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var ingredient in Ingredients)
            {
                if (!counts.TryGetValue(ingredient.Key, out int ownedCount) || ownedCount < ingredient.Value)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
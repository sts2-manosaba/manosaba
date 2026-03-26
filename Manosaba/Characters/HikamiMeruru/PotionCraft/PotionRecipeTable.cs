using Manosaba.Characters.HikamiMeruru.Potions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;

namespace Manosaba.Characters.HikamiMeruru.PotionCraft
{
    public static class PotionRecipeTable
    {
        public static readonly List<PotionRecipe> Recipes = new()
        {
            new PotionRecipe(
                new Dictionary<Type, int>
                {
                    { typeof(LesserPainKillerPotion), 2 }
                },
                ModelDb.Potion<PainKillerPotion>()
            ),
            new PotionRecipe(
                new Dictionary<Type, int>
                {
                    { typeof(PainKillerPotion), 1 },
                    { typeof(Catalyst), 1 }
                },
                ModelDb.Potion<GreaterPainKillerPotion>()
            ),
             new PotionRecipe(
                new Dictionary<Type, int>
                {
                    { typeof(LesserBlockPotion), 2 }
                },
                ModelDb.Potion<BlockPotion>()
            ),
              new PotionRecipe(
                new Dictionary<Type, int>
                {
                    { typeof(BlockPotion), 1 },
                    { typeof(Catalyst), 1 }
                },
                ModelDb.Potion<GreaterBlockPotion>()
            ),
               new PotionRecipe(
                new Dictionary<Type, int>
                {
                    { typeof(LesserStrengthPotion), 2 }
                },
                ModelDb.Potion<FlexPotion>()
            ),
               new PotionRecipe(
                new Dictionary<Type, int>
                {
                    { typeof(BeetleJuice), 1 },
                    { typeof(Catalyst), 1  }
                },
                ModelDb.Potion<GreaterBeetleJuice>()
            ),
            // new PotionRecipe(
            //     new Dictionary<Type, int>
            //     {
            //         { typeof(FirePotion), 1 },
            //         { typeof(WaterPotion), 1 }
            //     },
            //     typeof(SteamPotion)
            // )
        };
    }
}
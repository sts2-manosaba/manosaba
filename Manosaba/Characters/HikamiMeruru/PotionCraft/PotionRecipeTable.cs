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
                    { typeof(GreaterPainKillerPotion), 1 },
                    { typeof(Tredecim), 1 }
                },
                ModelDb.Potion<VitalPotion>()
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
                    { typeof(LesserFlexPotion), 2 }
                },
                ModelDb.Potion<FlexPotion>()
            ),
               new PotionRecipe(
                new Dictionary<Type, int>
                {
                    { typeof(FlexPotion), 1 },
                    { typeof(Catalyst), 1  }
                },
                ModelDb.Potion<GreaterFlexPotion>()
            ),
               new PotionRecipe(
                new Dictionary<Type, int>
                {
                    { typeof(EnergyPotion), 1 },
                    { typeof(Catalyst), 1  }
                },
                ModelDb.Potion<GreaterEnergyPotion>()
            ),
               new PotionRecipe(
                new Dictionary<Type, int>
                {
                    { typeof(WeakPotion), 1 },
                    { typeof(Catalyst), 1  }
                },
                ModelDb.Potion<GreaterWeakPotion>()
            ),
               new PotionRecipe(
                new Dictionary<Type, int>
                {
                    { typeof(BeetleJuice), 1 },
                    { typeof(Catalyst), 1  }
                },
                ModelDb.Potion<GreaterBeetleJuice>()
            ),
               new PotionRecipe(
                new Dictionary<Type, int>
                {
                    { typeof(GreaterBlockPotion), 1 },
                    { typeof(Tredecim), 1  }
                },
                ModelDb.Potion<Fortifier>()
            ),
               new PotionRecipe(
                new Dictionary<Type, int>
                {
                    { typeof(GreaterFlexPotion), 1 },
                    { typeof(Tredecim), 1  }
                },
                ModelDb.Potion<MazalethsGift>()
            ),
               new PotionRecipe(
                new Dictionary<Type, int>
                {
                    { typeof(GreaterWeakPotion), 1 },
                    { typeof(Tredecim), 1  }
                },
                ModelDb.Potion<ShacklingPotion>()
            ),
               new PotionRecipe(
                new Dictionary<Type, int>
                {
                    { typeof(GreaterEnergyPotion), 1 },
                    { typeof(Tredecim), 1  }
                },
                ModelDb.Potion<OrobicAcid>()
            ),
               new PotionRecipe(
                new Dictionary<Type, int>
                {
                    { typeof(GreaterBeetleJuice), 1 },
                    { typeof(Tredecim), 1  }
                },
                ModelDb.Potion<LuckyTonic>()
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

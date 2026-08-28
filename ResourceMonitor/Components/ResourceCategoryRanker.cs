using System.Collections.Generic;

namespace ResourceMonitor.Components
{
    /**
    * Ranks TechTypes into the buckets the resource monitor sorts by: raw resources, then basic
    * materials, advanced materials, electronics, food, and finally everything else (fully crafted
    * items). Categories are read from the game's own CraftData.groups so the ranking matches what
    * players already see on the PDA's Blueprints tab. The game doesn't distinguish "raw" from
    * "basic material" as separate categories (Titanium and Titanium Ingot are both BasicMaterials),
    * so within that category, anything without a crafting recipe is treated as raw.
    */
    public static class ResourceCategoryRanker
    {
        public const int RAW_RESOURCES = 0;
        public const int BASIC_MATERIALS = 1;
        public const int ADVANCED_MATERIALS = 2;
        public const int ELECTRONICS = 3;
        public const int FOOD = 4;
        public const int OTHER = 5;

        private static Dictionary<TechType, TechCategory> categoryByTechType;

        public static int GetRank(TechType type)
        {
            switch (GetCategory(type))
            {
                case TechCategory.BasicMaterials:
                    return HasRecipe(type) ? BASIC_MATERIALS : RAW_RESOURCES;
                case TechCategory.AdvancedMaterials:
                    return ADVANCED_MATERIALS;
                case TechCategory.Electronics:
                    return ELECTRONICS;
#if SUBNAUTICA
                case TechCategory.CookedFood:
                case TechCategory.CuredFood:
                case TechCategory.Water:
                    return FOOD;
#elif BELOWZERO
                case TechCategory.FoodAndDrinks:
                    return FOOD;
#endif
                default:
                    return OTHER;
            }
        }

        public static bool IsTrackableMaterial(TechType type)
        {
            var rank = GetRank(type);
            return rank == RAW_RESOURCES || rank == BASIC_MATERIALS || rank == ADVANCED_MATERIALS || rank == ELECTRONICS;
        }

        private static bool HasRecipe(TechType type)
        {
            // TechData.entries only holds recipes mods have explicitly registered; vanilla recipes
            // aren't in there. CraftDataHandler.GetRecipeData covers both modded and vanilla items.
            var recipe = Nautilus.Handlers.CraftDataHandler.GetRecipeData(type);
            return recipe != null && recipe.ingredientCount > 0;
        }

        private static TechCategory GetCategory(TechType type)
        {
            if (categoryByTechType == null)
            {
                BuildCategoryLookup();
            }

            return categoryByTechType.TryGetValue(type, out var category) ? category : TechCategory.Misc;
        }

        private static void BuildCategoryLookup()
        {
            categoryByTechType = new Dictionary<TechType, TechCategory>();
            foreach (var group in CraftData.groups)
            {
                foreach (var category in group.Value)
                {
                    foreach (var type in category.Value)
                    {
                        categoryByTechType[type] = category.Key;
                    }
                }
            }
        }
    }
}

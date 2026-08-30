using System;
using System.Collections.Generic;

namespace ResourceMonitor.Components
{
    /**
    * Ranks TechTypes into the buckets the resource monitor sorts by, in display order: raw
    * materials, basic materials, electronics, advanced materials, water, raw fish, cooked food,
    * cured food, deployables, creature eggs, vehicle upgrades, scanner room upgrades, seeds,
    * tools, equipment, and finally everything else (misc).
    *
    * Almost none of this can be derived from the game's own data - TechType doesn't distinguish
    * "raw ore" from "creature egg" from "seed" from "misc junk": they're all just "not registered
    * in any Fabricator menu." So TIER below is a hand-audited table (built from a full TechType
    * CSV dump - see TechTypeDump - reviewed item-by-item) rather than a formula. PRIORITY pins the
    * exact intra-tier order for the first four tiers (raw/basic/electronics/advanced) from that
    * same audit; every other tier sorts alphabetically.
    *
    * Anything not in TIER (almost always a modded item) falls back to best-effort classification
    * from the game's TechCategory/TechGroup data and the Cooked/Cured naming convention, then
    * sorts alphabetically after the audited items in whatever tier it lands in.
    */
    public static class ResourceCategoryRanker
    {
        public const int RAW_MATERIALS = 0;
        public const int BASIC_MATERIALS = 1;
        public const int ELECTRONICS = 2;
        public const int ADVANCED_MATERIALS = 3;
        public const int WATER = 4;
        public const int RAW_FISH = 5;
        public const int COOKED_FOOD = 6;
        public const int CURED_FOOD = 7;
        public const int DEPLOYABLES = 8;
        public const int CREATURE_EGG = 9;
        public const int VEHICLE_UPGRADE = 10;
        public const int SCANNER_ROOM_UPGRADE = 11;
        public const int SEED = 12;
        public const int TOOLS = 13;
        public const int EQUIPMENT = 14;
        public const int MISC = 15;

        private static readonly Dictionary<TechType, int> TIER = new Dictionary<TechType, int>
        {
            // 1. Raw Materials
            [TechType.AcidMushroom] = RAW_MATERIALS,
            [TechType.AluminumOxide] = RAW_MATERIALS,
            [TechType.BloodOil] = RAW_MATERIALS,
            [TechType.Copper] = RAW_MATERIALS,
            [TechType.CoralChunk] = RAW_MATERIALS,
            [TechType.CrashPowder] = RAW_MATERIALS,
            [TechType.CreepvinePiece] = RAW_MATERIALS,
            [TechType.CreepvineSeedCluster] = RAW_MATERIALS,
            [TechType.Diamond] = RAW_MATERIALS,
            [TechType.Gold] = RAW_MATERIALS,
            [TechType.JellyPlant] = RAW_MATERIALS,
            [TechType.JeweledDiskPiece] = RAW_MATERIALS,
            [TechType.Kyanite] = RAW_MATERIALS,
            [TechType.Lead] = RAW_MATERIALS,
            [TechType.Lithium] = RAW_MATERIALS,
            [TechType.Magnetite] = RAW_MATERIALS,
            [TechType.Nickel] = RAW_MATERIALS,
            [TechType.Quartz] = RAW_MATERIALS,
            [TechType.Salt] = RAW_MATERIALS,
            [TechType.ScrapMetal] = RAW_MATERIALS,
            [TechType.Silver] = RAW_MATERIALS,
            [TechType.StalkerTooth] = RAW_MATERIALS,
            [TechType.Sulphur] = RAW_MATERIALS,
            [TechType.Titanium] = RAW_MATERIALS,
            [TechType.UraniniteCrystal] = RAW_MATERIALS,
            [TechType.WhiteMushroom] = RAW_MATERIALS,
            // 2. Basic Materials
            [TechType.Bleach] = BASIC_MATERIALS,
            [TechType.EnameledGlass] = BASIC_MATERIALS,
            [TechType.FiberMesh] = BASIC_MATERIALS,
            [TechType.Glass] = BASIC_MATERIALS,
            [TechType.Lubricant] = BASIC_MATERIALS,
            [TechType.PlasteelIngot] = BASIC_MATERIALS,
            [TechType.Silicone] = BASIC_MATERIALS,
            [TechType.TitaniumIngot] = BASIC_MATERIALS,
            // 3. Electronics
            [TechType.AdvancedWiringKit] = ELECTRONICS,
            [TechType.Battery] = ELECTRONICS,
            [TechType.ComputerChip] = ELECTRONICS,
            [TechType.CopperWire] = ELECTRONICS,
            [TechType.PowerCell] = ELECTRONICS,
            [TechType.ReactorRod] = ELECTRONICS,
            [TechType.WiringKit] = ELECTRONICS,
            // 4. Advanced Materials
            [TechType.Aerogel] = ADVANCED_MATERIALS,
            [TechType.AramidFibers] = ADVANCED_MATERIALS,
            [TechType.Benzene] = ADVANCED_MATERIALS,
            [TechType.HatchingEnzymes] = ADVANCED_MATERIALS,
            [TechType.HydrochloricAcid] = ADVANCED_MATERIALS,
            [TechType.Polyaniline] = ADVANCED_MATERIALS,
            // 5. Water
            [TechType.BigFilteredWater] = WATER,
            [TechType.DisinfectedWater] = WATER,
            [TechType.FilteredWater] = WATER,
            // 6. Raw Fish
            [TechType.Bladderfish] = RAW_FISH,
            [TechType.Boomerang] = RAW_FISH,
            [TechType.Eyeye] = RAW_FISH,
            [TechType.Floater] = RAW_FISH,
            [TechType.GarryFish] = RAW_FISH,
            [TechType.HoleFish] = RAW_FISH,
            [TechType.Hoopfish] = RAW_FISH,
            [TechType.Hoverfish] = RAW_FISH,
            [TechType.LavaBoomerang] = RAW_FISH,
            [TechType.LavaEyeye] = RAW_FISH,
            [TechType.Oculus] = RAW_FISH,
            [TechType.Peeper] = RAW_FISH,
            [TechType.Reginald] = RAW_FISH,
            [TechType.Spadefish] = RAW_FISH,
            [TechType.Spinefish] = RAW_FISH,
            // 7. Cooked Food
            [TechType.CookedBladderfish] = COOKED_FOOD,
            [TechType.CookedBoomerang] = COOKED_FOOD,
            [TechType.CookedEyeye] = COOKED_FOOD,
            [TechType.CookedGarryFish] = COOKED_FOOD,
            [TechType.CookedHoleFish] = COOKED_FOOD,
            [TechType.CookedHoopfish] = COOKED_FOOD,
            [TechType.CookedHoverfish] = COOKED_FOOD,
            [TechType.CookedLavaBoomerang] = COOKED_FOOD,
            [TechType.CookedLavaEyeye] = COOKED_FOOD,
            [TechType.CookedOculus] = COOKED_FOOD,
            [TechType.CookedPeeper] = COOKED_FOOD,
            [TechType.CookedReginald] = COOKED_FOOD,
            [TechType.CookedSpadefish] = COOKED_FOOD,
            [TechType.CookedSpinefish] = COOKED_FOOD,
            // 8. Cured Food
            [TechType.CuredBladderfish] = CURED_FOOD,
            [TechType.CuredBoomerang] = CURED_FOOD,
            [TechType.CuredEyeye] = CURED_FOOD,
            [TechType.CuredGarryFish] = CURED_FOOD,
            [TechType.CuredHoleFish] = CURED_FOOD,
            [TechType.CuredHoopfish] = CURED_FOOD,
            [TechType.CuredHoverfish] = CURED_FOOD,
            [TechType.CuredLavaBoomerang] = CURED_FOOD,
            [TechType.CuredLavaEyeye] = CURED_FOOD,
            [TechType.CuredOculus] = CURED_FOOD,
            [TechType.CuredPeeper] = CURED_FOOD,
            [TechType.CuredReginald] = CURED_FOOD,
            [TechType.CuredSpadefish] = CURED_FOOD,
            [TechType.CuredSpinefish] = CURED_FOOD,
            [TechType.NutrientBlock] = CURED_FOOD,
            [TechType.Snack1] = CURED_FOOD,
            [TechType.Snack2] = CURED_FOOD,
            [TechType.Snack3] = CURED_FOOD,
            // 9. Deployables
            [TechType.Beacon] = DEPLOYABLES,
            [TechType.Constructor] = DEPLOYABLES,
            [TechType.CyclopsDecoy] = DEPLOYABLES,
            [TechType.Gravsphere] = DEPLOYABLES,
            [TechType.Seaglide] = DEPLOYABLES,
            [TechType.SmallStorage] = DEPLOYABLES,
            // 10. Creature Egg
            [TechType.BonesharkEgg] = CREATURE_EGG,
            [TechType.BonesharkEggUndiscovered] = CREATURE_EGG,
            [TechType.CrabsnakeEgg] = CREATURE_EGG,
            [TechType.CrabsnakeEggUndiscovered] = CREATURE_EGG,
            [TechType.CrabsquidEgg] = CREATURE_EGG,
            [TechType.CrabsquidEggUndiscovered] = CREATURE_EGG,
            [TechType.CrashEgg] = CREATURE_EGG,
            [TechType.CrashEggUndiscovered] = CREATURE_EGG,
            [TechType.CutefishEgg] = CREATURE_EGG,
            [TechType.CutefishEggUndiscovered] = CREATURE_EGG,
            [TechType.GasopodEgg] = CREATURE_EGG,
            [TechType.GasopodEggUndiscovered] = CREATURE_EGG,
            [TechType.GenericEgg] = CREATURE_EGG,
            [TechType.GrandReefsEgg] = CREATURE_EGG,
            [TechType.GrassyPlateausEgg] = CREATURE_EGG,
            [TechType.RabbitrayEgg] = CREATURE_EGG,
            [TechType.RabbitrayEggUndiscovered] = CREATURE_EGG,
            [TechType.ReefbackEgg] = CREATURE_EGG,
            [TechType.ReefbackEggUndiscovered] = CREATURE_EGG,
            [TechType.SafeShallowsEgg] = CREATURE_EGG,
            [TechType.SandsharkEgg] = CREATURE_EGG,
            [TechType.SandsharkEggUndiscovered] = CREATURE_EGG,
            [TechType.ShockerEgg] = CREATURE_EGG,
            [TechType.ShockerEggUndiscovered] = CREATURE_EGG,
            [TechType.SpadefishEgg] = CREATURE_EGG,
            [TechType.SpadefishEggUndiscovered] = CREATURE_EGG,
            [TechType.Stalker] = CREATURE_EGG,
            [TechType.StalkerEgg] = CREATURE_EGG,
            [TechType.TwistyBridgesEgg] = CREATURE_EGG,
            // 11. Vehicle Upgrade
            [TechType.CyclopsDecoyModule] = VEHICLE_UPGRADE,
            [TechType.CyclopsFireSuppressionModule] = VEHICLE_UPGRADE,
            [TechType.CyclopsHullModule1] = VEHICLE_UPGRADE,
            [TechType.CyclopsHullModule2] = VEHICLE_UPGRADE,
            [TechType.CyclopsHullModule3] = VEHICLE_UPGRADE,
            [TechType.CyclopsSeamothRepairModule] = VEHICLE_UPGRADE,
            [TechType.CyclopsShieldModule] = VEHICLE_UPGRADE,
            [TechType.CyclopsSonarModule] = VEHICLE_UPGRADE,
            [TechType.CyclopsThermalReactorModule] = VEHICLE_UPGRADE,
            [TechType.ExoHullModule1] = VEHICLE_UPGRADE,
            [TechType.ExoHullModule2] = VEHICLE_UPGRADE,
            [TechType.ExosuitClawArmModule] = VEHICLE_UPGRADE,
            [TechType.ExosuitDrillArmModule] = VEHICLE_UPGRADE,
            [TechType.ExosuitGrapplingArmModule] = VEHICLE_UPGRADE,
            [TechType.ExosuitJetUpgradeModule] = VEHICLE_UPGRADE,
            [TechType.ExosuitPropulsionArmModule] = VEHICLE_UPGRADE,
            [TechType.ExosuitThermalReactorModule] = VEHICLE_UPGRADE,
            [TechType.ExosuitTorpedoArmModule] = VEHICLE_UPGRADE,
            [TechType.GasTorpedo] = VEHICLE_UPGRADE,
            [TechType.HullReinforcementModule] = VEHICLE_UPGRADE,
            [TechType.HullReinforcementModule2] = VEHICLE_UPGRADE,
            [TechType.PowerUpgradeModule] = VEHICLE_UPGRADE,
            [TechType.SeamothElectricalDefense] = VEHICLE_UPGRADE,
            [TechType.SeamothReinforcementModule] = VEHICLE_UPGRADE,
            [TechType.SeamothSolarCharge] = VEHICLE_UPGRADE,
            [TechType.SeamothSonarModule] = VEHICLE_UPGRADE,
            [TechType.SeamothTorpedoModule] = VEHICLE_UPGRADE,
            [TechType.VehicleArmorPlating] = VEHICLE_UPGRADE,
            [TechType.VehicleHullModule1] = VEHICLE_UPGRADE,
            [TechType.VehicleHullModule2] = VEHICLE_UPGRADE,
            [TechType.VehicleHullModule3] = VEHICLE_UPGRADE,
            [TechType.VehiclePowerUpgradeModule] = VEHICLE_UPGRADE,
            [TechType.VehicleStorageModule] = VEHICLE_UPGRADE,
            [TechType.WhirlpoolTorpedo] = VEHICLE_UPGRADE,
            // 12. Scanner Room Upgrades
            [TechType.MapRoomCamera] = SCANNER_ROOM_UPGRADE,
            [TechType.MapRoomHUDChip] = SCANNER_ROOM_UPGRADE,
            [TechType.MapRoomUpgradeScanRange] = SCANNER_ROOM_UPGRADE,
            [TechType.MapRoomUpgradeScanSpeed] = SCANNER_ROOM_UPGRADE,
            // 13. Seed
            [TechType.AcidMushroomSpore] = SEED,
            [TechType.BluePalmSeed] = SEED,
            [TechType.EyesPlantSeed] = SEED,
            [TechType.FernPalmSeed] = SEED,
            [TechType.GabeSFeatherSeed] = SEED,
            [TechType.KooshChunk] = SEED,
            [TechType.PinkFlowerSeed] = SEED,
            [TechType.PinkMushroomSpore] = SEED,
            [TechType.PurpleBranchesSeed] = SEED,
            [TechType.PurpleStalkSeed] = SEED,
            [TechType.PurpleTentacleSeed] = SEED,
            [TechType.PurpleVasePlantSeed] = SEED,
            [TechType.RedBasketPlantSeed] = SEED,
            [TechType.RedBushSeed] = SEED,
            [TechType.RedConePlantSeed] = SEED,
            [TechType.RedGreenTentacleSeed] = SEED,
            [TechType.RedRollPlantSeed] = SEED,
            [TechType.SeaCrownSeed] = SEED,
            [TechType.ShellGrassSeed] = SEED,
            [TechType.SmallMelon] = SEED,
            [TechType.SnakeMushroomSpore] = SEED,
            [TechType.SpikePlantSeed] = SEED,
            [TechType.SpottedLeavesPlantSeed] = SEED,
            [TechType.TreeMushroomPiece] = SEED,
            // 14. Tools
            [TechType.AirBladder] = TOOLS,
            [TechType.Builder] = TOOLS,
            [TechType.DiveReel] = TOOLS,
            [TechType.Flare] = TOOLS,
            [TechType.Flashlight] = TOOLS,
            [TechType.HeatBlade] = TOOLS,
            [TechType.Knife] = TOOLS,
            [TechType.LaserCutter] = TOOLS,
            [TechType.LEDLight] = TOOLS,
            [TechType.PropulsionCannon] = TOOLS,
            [TechType.RepulsionCannon] = TOOLS,
            [TechType.Scanner] = TOOLS,
            [TechType.StasisRifle] = TOOLS,
            [TechType.Terraformer] = TOOLS,
            [TechType.Transfuser] = TOOLS,
            [TechType.Welder] = TOOLS,
            // 15. Equipment
            [TechType.Compass] = EQUIPMENT,
            [TechType.DoubleTank] = EQUIPMENT,
            [TechType.Fins] = EQUIPMENT,
            [TechType.FireExtinguisher] = EQUIPMENT,
            [TechType.FirstAidKit] = EQUIPMENT,
            [TechType.HighCapacityTank] = EQUIPMENT,
            [TechType.Pipe] = EQUIPMENT,
            [TechType.PipeSurfaceFloater] = EQUIPMENT,
            [TechType.PlasteelTank] = EQUIPMENT,
            [TechType.PrecursorKey_Blue] = EQUIPMENT,
            [TechType.PrecursorKey_Orange] = EQUIPMENT,
            [TechType.PrecursorKey_Purple] = EQUIPMENT,
            [TechType.RadiationGloves] = EQUIPMENT,
            [TechType.RadiationSuit] = EQUIPMENT,
            [TechType.Rebreather] = EQUIPMENT,
            [TechType.ReinforcedDiveSuit] = EQUIPMENT,
            [TechType.ReinforcedGloves] = EQUIPMENT,
            [TechType.SwimChargeFins] = EQUIPMENT,
            [TechType.Tank] = EQUIPMENT,
            [TechType.Thermometer] = EQUIPMENT,
            [TechType.UltraGlideFins] = EQUIPMENT,
            [TechType.WaterFiltrationSuit] = EQUIPMENT,
            // 16. Misc
            [TechType.ArcadeGorgetoy] = MISC,
            [TechType.Cap1] = MISC,
            [TechType.Cap2] = MISC,
            [TechType.DepletedReactorRod] = MISC,
            [TechType.GasPod] = MISC,
            [TechType.ReefbackDNA] = MISC,
            [TechType.SeaTreaderPoop] = MISC,
        };

        // Pins the exact intra-tier order within RAW_MATERIALS/BASIC_MATERIALS/ELECTRONICS/
        // ADVANCED_MATERIALS. Anything in one of those tiers but not listed here (e.g. a modded
        // item) sorts alphabetically after all of these - see GetPriority.
        private static readonly Dictionary<TechType, int> PRIORITY = new Dictionary<TechType, int>
        {
            [TechType.Titanium] = 1,
            [TechType.ScrapMetal] = 2,
            [TechType.Quartz] = 3,
            [TechType.Copper] = 4,
            [TechType.Lead] = 5,
            [TechType.Silver] = 6,
            [TechType.Gold] = 7,
            [TechType.AcidMushroom] = 8,
            [TechType.CreepvineSeedCluster] = 9,
            [TechType.CreepvinePiece] = 10,
            [TechType.JeweledDiskPiece] = 11,
            [TechType.CrashPowder] = 12,
            [TechType.StalkerTooth] = 13,
            [TechType.CoralChunk] = 14,
            [TechType.Salt] = 15,
            [TechType.Lithium] = 16,
            [TechType.Diamond] = 17,
            [TechType.Magnetite] = 18,
            [TechType.AluminumOxide] = 19,
            [TechType.UraniniteCrystal] = 20,
            [TechType.Sulphur] = 21,
            [TechType.Nickel] = 22,
            [TechType.Kyanite] = 23,
            [TechType.WhiteMushroom] = 24,
            [TechType.BloodOil] = 25,
            [TechType.JellyPlant] = 26,
            [TechType.Glass] = 27,
            [TechType.TitaniumIngot] = 28,
            [TechType.Silicone] = 29,
            [TechType.FiberMesh] = 30,
            [TechType.Lubricant] = 31,
            [TechType.EnameledGlass] = 32,
            [TechType.PlasteelIngot] = 33,
            [TechType.Bleach] = 34,
            [TechType.CopperWire] = 35,
            [TechType.ComputerChip] = 36,
            [TechType.WiringKit] = 37,
            [TechType.AdvancedWiringKit] = 38,
            [TechType.Battery] = 39,
            [TechType.PowerCell] = 40,
            [TechType.ReactorRod] = 41,
            [TechType.Aerogel] = 42,
            [TechType.HydrochloricAcid] = 43,
            [TechType.Polyaniline] = 44,
            [TechType.Benzene] = 45,
            [TechType.AramidFibers] = 46,
            [TechType.HatchingEnzymes] = 47,
        };

        private static readonly HashSet<TechCategory> DEPLOYABLE_CATEGORIES = new HashSet<TechCategory>
        {
            TechCategory.Machines,
            TechCategory.Constructor,
            TechCategory.Workbench,
            TechCategory.BasePiece,
            TechCategory.ExteriorModule,
            TechCategory.InteriorPiece,
            TechCategory.InteriorModule,
            TechCategory.MiscHullplates,
#if SUBNAUTICA
            TechCategory.BaseRoom,
            TechCategory.BaseWall,
            TechCategory.ExteriorLight,
            TechCategory.ExteriorOther,
            TechCategory.InteriorRoom,
#elif BELOWZERO
            TechCategory.PrecursorBodyParts,
#endif
        };

        private static readonly HashSet<TechCategory> VEHICLE_UPGRADE_CATEGORIES = new HashSet<TechCategory>
        {
            TechCategory.VehicleUpgrades,
#if SUBNAUTICA
            TechCategory.Cyclops,
            TechCategory.CyclopsUpgrades,
#endif
        };

        private static Dictionary<TechType, TechCategory> categoryByTechType;
        private static HashSet<TechType> rawFishTechTypes;
        private static Dictionary<TechType, int> recipeUsageCountByTechType;

        public static int GetRank(TechType type)
        {
            return TIER.TryGetValue(type, out var tier) ? tier : FallbackRank(type);
        }

        /**
        * Explicit sort position within RAW_MATERIALS/BASIC_MATERIALS/ELECTRONICS/ADVANCED_MATERIALS,
        * from the hand-audited PRIORITY table. Anything not in that table (a modded item) sorts
        * after every audited item in its tier - see ResourceMonitorLogic.GetSortedTrackedResources.
        */
        public static int GetPriority(TechType type)
        {
            return PRIORITY.TryGetValue(type, out var priority) ? priority : int.MaxValue;
        }

        private static int FallbackRank(TechType type)
        {
            var name = type.ToString();
            if (name.StartsWith("Cooked")) return COOKED_FOOD;
            if (name.StartsWith("Cured")) return CURED_FOOD;
            if (IsRawFish(type)) return RAW_FISH;

            if (!TryGetCategory(type, out var category))
            {
                // Not registered in any Fabricator menu, and TechCategory can't tell raw
                // materials, fish, eggs, seeds and equipment apart - they're all just "not
                // found." These signals (found via a full audit of the vanilla TechType list)
                // narrow it down: raw fish are the only not-found items with EquipmentType.Hand;
                // a worn/vehicle EquipmentType still means equipment or a vehicle upgrade even
                // when unregistered; eggs always have "Egg" in the name; seeds/spores/samples
                // (plus the melon) always match one of those words.
                switch (TechData.GetEquipmentType(type))
                {
                    case EquipmentType.Hand:
                        return RAW_FISH;
                    case EquipmentType.Head:
                    case EquipmentType.Body:
                    case EquipmentType.Gloves:
                    case EquipmentType.Foots:
                    case EquipmentType.Tank:
                    case EquipmentType.Chip:
                        return EQUIPMENT;
                    case EquipmentType.CyclopsModule:
                    case EquipmentType.SeamothModule:
                    case EquipmentType.ExosuitModule:
                    case EquipmentType.ExosuitArm:
                    case EquipmentType.VehicleModule:
                        return VEHICLE_UPGRADE;
                }

                if (name.IndexOf("Egg", StringComparison.OrdinalIgnoreCase) >= 0) return CREATURE_EGG;
                if (LooksLikeSeed(name)) return SEED;
                return RAW_MATERIALS;
            }

            switch (category)
            {
                case TechCategory.BasicMaterials:
                    return HasRecipe(type) ? BASIC_MATERIALS : RAW_MATERIALS;
                case TechCategory.AdvancedMaterials:
                    return ADVANCED_MATERIALS;
                case TechCategory.Electronics:
                    return ELECTRONICS;
#if SUBNAUTICA
                case TechCategory.Water:
#elif BELOWZERO
                case TechCategory.FoodAndDrinks:
#endif
                    return WATER;
                case TechCategory.Equipment:
                    return EQUIPMENT;
                case TechCategory.Tools:
                    return TOOLS;
                case TechCategory.MapRoomUpgrades:
                    return SCANNER_ROOM_UPGRADE;
                default:
                    if (VEHICLE_UPGRADE_CATEGORIES.Contains(category)) return VEHICLE_UPGRADE;
                    return DEPLOYABLE_CATEGORIES.Contains(category) ? DEPLOYABLES : MISC;
            }
        }

        // A modded item is "seed-like" if its name mentions seed, spore or sample - or is the
        // one vanilla exception that doesn't (the small marblemelon).
        private static bool LooksLikeSeed(string name)
        {
            return name.IndexOf("Seed", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Spore", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Sample", StringComparison.OrdinalIgnoreCase) >= 0
                || name == "SmallMelon";
        }

        /**
        * How many registered recipes (vanilla or modded) use this TechType as an ingredient. Used
        * as a tiebreaker for modded items that land in a tier but have no explicit PRIORITY entry.
        */
        public static int GetRecipeUsageCount(TechType type)
        {
            if (recipeUsageCountByTechType == null)
            {
                BuildRecipeUsageCounts();
            }

            return recipeUsageCountByTechType.TryGetValue(type, out var count) ? count : 0;
        }

        private static bool HasRecipe(TechType type)
        {
            // TechData.entries only holds recipes mods have explicitly registered; vanilla recipes
            // aren't in there. CraftDataHandler.GetRecipeData covers both modded and vanilla items.
            var recipe = Nautilus.Handlers.CraftDataHandler.GetRecipeData(type);
            return recipe != null && recipe.ingredientCount > 0;
        }

        private static bool IsRawFish(TechType type)
        {
            if (rawFishTechTypes == null)
            {
                BuildRawFishLookup();
            }

            return rawFishTechTypes.Contains(type);
        }

        private static bool TryGetCategory(TechType type, out TechCategory category)
        {
            if (categoryByTechType == null)
            {
                BuildCategoryLookup();
            }

            return categoryByTechType.TryGetValue(type, out category);
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

        private static void BuildRawFishLookup()
        {
            rawFishTechTypes = new HashSet<TechType>();
            foreach (TechType candidate in Enum.GetValues(typeof(TechType)))
            {
                var name = candidate.ToString();
                string rawName = null;
                if (name.StartsWith("Cooked")) rawName = name.Substring("Cooked".Length);
                else if (name.StartsWith("Cured")) rawName = name.Substring("Cured".Length);

                if (rawName != null && Enum.TryParse(rawName, out TechType rawType))
                {
                    rawFishTechTypes.Add(rawType);
                }
            }
        }

        private static void BuildRecipeUsageCounts()
        {
            recipeUsageCountByTechType = new Dictionary<TechType, int>();
            foreach (TechType candidate in Enum.GetValues(typeof(TechType)))
            {
                var recipe = Nautilus.Handlers.CraftDataHandler.GetRecipeData(candidate);
                if (recipe?.Ingredients == null) continue;

                foreach (var ingredient in recipe.Ingredients)
                {
                    recipeUsageCountByTechType.TryGetValue(ingredient.techType, out var count);
                    recipeUsageCountByTechType[ingredient.techType] = count + 1;
                }
            }
        }
    }
}

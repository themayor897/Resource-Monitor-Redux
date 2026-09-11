using System.Collections.Generic;
using System.IO;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Utility;
using UnityEngine;

namespace ResourceMonitor.Game_Items
{
    /**
    * Shared registration logic for both sizes of the resource monitor screen.
    */
    public static class ResourceMonitorScreenGeneric
    {
        public static void Register(string classId, string friendlyName, string description, string iconFileName, int numberOfIngredientsRequired, Vector3? scale, bool isLarge)
        {
            var info = PrefabInfo.WithTechType(classId, friendlyName, description)
                .WithIcon(ImageUtils.LoadSpriteFromFile(Path.Combine(EntryPoint.ASSETS_FOLDER_LOCATION, iconFileName)));

            var prefab = new CustomPrefab(info);
            prefab.SetGameObject(() => BuildGameObject(info, scale, isLarge));
            prefab.SetPdaGroupCategory(TechGroup.InteriorModules, TechCategory.InteriorModule);
            prefab.SetRecipe(new RecipeData
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>
                {
                    new Ingredient(TechType.Glass, numberOfIngredientsRequired),
                    new Ingredient(TechType.ComputerChip, numberOfIngredientsRequired),
                    new Ingredient(TechType.AdvancedWiringKit, numberOfIngredientsRequired)
                }
            });
            prefab.Register();

            // Without this, the recipe has no fragment/scan requirement AND no explicit unlock,
            // so in Survival/Hardcore there's simply no path that ever marks it as known - Creative
            // mode ignores PDA unlock state entirely for crafting, which is why this went unnoticed.
            Nautilus.Handlers.KnownTechHandler.UnlockOnStart(info.TechType);
        }

        private static GameObject BuildGameObject(PrefabInfo info, Vector3? scale, bool isLarge)
        {
            var screen = Object.Instantiate(EntryPoint.RESOURCE_MONITOR_DISPLAY_MODEL);
            var screenModel = screen.transform.GetChild(0).gameObject;

            var shader = Shader.Find("MarmosetUBER");
            var renderers = screen.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.material.shader = shader;
            }

            PrefabUtils.AddBasicComponents(screen, info.ClassID, info.TechType, LargeWorldEntity.CellLevel.Medium);

            // Interior wall module: mountable on walls inside bases and submarines, not outside or on the ground.
            var constructableFlags = ConstructableFlags.Base | ConstructableFlags.Submarine | ConstructableFlags.Wall;
            PrefabUtils.AddConstructable(screen, info.TechType, constructableFlags, screenModel);

            screen.AddComponent<ConstructableBounds>().bounds = new OrientedBounds(new Vector3(-0.1f, -0.1f, 0f), new Quaternion(0, 0, 0, 0), new Vector3(0.9f, 0.5f, 0f));
            screen.AddComponent<VFXSurface>();
            screen.AddComponent<Components.ResourceMonitorLogic>().IsLargeMonitor = isLarge;

            if (scale.HasValue)
            {
                screen.transform.localScale = scale.Value;
            }

            return screen;
        }
    }
}

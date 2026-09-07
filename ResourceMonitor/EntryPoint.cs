using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace ResourceMonitor
{
    [BepInPlugin(GUID, "Resource Monitor Redux", VERSION)]
    [BepInDependency("com.snmodding.nautilus")]
    public class EntryPoint : BaseUnityPlugin
    {
        public const string GUID = "taylor.brett.ResourceMonitor.mod";
        public const string VERSION = "2.0.0";

        public static readonly string MOD_FOLDER_LOCATION = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string ASSETS_FOLDER_LOCATION = Path.Combine(MOD_FOLDER_LOCATION, "Assets");
        public static readonly string ASSET_BUNDLE_LOCATION = Path.Combine(ASSETS_FOLDER_LOCATION, "resources");
        public static readonly string SETTINGS_FILE_LOCATION = Path.Combine(MOD_FOLDER_LOCATION, "Settings.json");
        public static readonly string DONT_TRACK_LOCATION = Path.Combine(MOD_FOLDER_LOCATION, "DontTrackList.txt");

        public static GameObject RESOURCE_MONITOR_DISPLAY_UI_PREFAB { get; private set; }
        public static GameObject RESOURCE_MONITOR_DISPLAY_ITEM_UI_PREFAB { get; private set; }
        public static GameObject RESOURCE_MONITOR_DISPLAY_MODEL { get; private set; }
        public static SettingsData SETTINGS { get; private set; }

        private void Awake()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), GUID);

            LoadDontTrackList();
            LoadAssets();
            SETTINGS = File.Exists(SETTINGS_FILE_LOCATION) ? LoadSettings() : CreateSettingsIfItDoesntExist();

            Game_Items.ResourceMonitorScreenSmall.Register();
            Game_Items.ResourceMonitorScreenLarge.Register();

            Nautilus.Handlers.OptionsPanelHandler.RegisterModOptions(new ResourceMonitorOptions());

            Logger.LogInfo("Resource Monitor Redux loaded.");
        }

        private void LoadAssets()
        {
            var ab = AssetBundle.LoadFromFile(ASSET_BUNDLE_LOCATION);
            RESOURCE_MONITOR_DISPLAY_UI_PREFAB = ab.LoadAsset("ResourceMonitorDisplayUI") as GameObject;
            RESOURCE_MONITOR_DISPLAY_ITEM_UI_PREFAB = ab.LoadAsset("ResourceItem") as GameObject;
            RESOURCE_MONITOR_DISPLAY_MODEL = ab.LoadAsset("ResourceMonitorModel") as GameObject;
        }

        private void LoadDontTrackList()
        {
            if (File.Exists(DONT_TRACK_LOCATION))
            {
                Logger.LogInfo("Found the dont track list at location: " + DONT_TRACK_LOCATION);
                using (var reader = new StreamReader(DONT_TRACK_LOCATION))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(line) == false)
                        {
                            Components.ResourceMonitorLogic.DONT_TRACK_GAMEOBJECTS.Add(line.ToLower());
                        }
                    }
                }
                Components.ResourceMonitorLogic.DONT_TRACK_GAMEOBJECTS.Sort();
            }
            else
            {
                Logger.LogInfo("Did not find the dont track list at location: " + DONT_TRACK_LOCATION);
            }
        }

        private static SettingsData CreateSettingsIfItDoesntExist()
        {
            var data = new SettingsData();
            File.WriteAllText(SETTINGS_FILE_LOCATION, JsonConvert.SerializeObject(data, Formatting.Indented));
            return data;
        }

        private static SettingsData LoadSettings()
        {
            return JsonConvert.DeserializeObject<SettingsData>(File.ReadAllText(SETTINGS_FILE_LOCATION));
        }

        public static void SaveSettings()
        {
            File.WriteAllText(SETTINGS_FILE_LOCATION, JsonConvert.SerializeObject(SETTINGS, Formatting.Indented));
        }
    }
}

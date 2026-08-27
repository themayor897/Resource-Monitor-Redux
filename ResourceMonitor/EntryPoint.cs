using System;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ResourceMonitor
{
    [BepInPlugin(GUID, "Resource Monitor", VERSION)]
    [BepInDependency("com.snmodding.nautilus")]
    public class EntryPoint : BaseUnityPlugin
    {
        public const string GUID = "taylor.brett.ResourceMonitor.mod";
        public const string VERSION = "1.0.6";

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

            Game_Items.ResourceMonitorScreenLarge.Register();
            Game_Items.ResourceMonitorScreenSmall.Register();

            Nautilus.Handlers.OptionsPanelHandler.RegisterModOptions(new ResourceMonitorOptions());

            Logger.LogInfo("Resource Monitor loaded.");
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
            File.WriteAllText(SETTINGS_FILE_LOCATION, JsonConvert.SerializeObject(data, Formatting.Indented, new ColorJsonConverter()));
            return data;
        }

        private static SettingsData LoadSettings()
        {
            return JsonConvert.DeserializeObject<SettingsData>(File.ReadAllText(SETTINGS_FILE_LOCATION), new ColorJsonConverter());
        }

        public static void SaveSettings()
        {
            File.WriteAllText(SETTINGS_FILE_LOCATION, JsonConvert.SerializeObject(SETTINGS, Formatting.Indented, new ColorJsonConverter()));
        }

        /**
        * UnityEngine.Color exposes computed properties (linear, gamma, ...) that return another Color,
        * which Newtonsoft's default reflection-based serializer misreads as a self-referencing loop.
        * This converter limits serialization to just the four channel values.
        */
        private class ColorJsonConverter : JsonConverter<Color>
        {
            public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("r"); writer.WriteValue(value.r);
                writer.WritePropertyName("g"); writer.WriteValue(value.g);
                writer.WritePropertyName("b"); writer.WriteValue(value.b);
                writer.WritePropertyName("a"); writer.WriteValue(value.a);
                writer.WriteEndObject();
            }

            public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var obj = JObject.Load(reader);
                return new Color(
                    obj.Value<float?>("r") ?? 0f,
                    obj.Value<float?>("g") ?? 0f,
                    obj.Value<float?>("b") ?? 0f,
                    obj.Value<float?>("a") ?? 1f);
            }
        }
    }
}

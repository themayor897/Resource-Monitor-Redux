using Nautilus.Options;
using UnityEngine;

namespace ResourceMonitor
{
    /**
    * Adds Resource Monitor's settings to the game's own Mod Options screen. Backed by the same
    * SettingsData/Settings.json used everywhere else in the mod, so changes made here persist.
    */
    public class ResourceMonitorOptions : ModOptions
    {
        public const string SHOW_ZERO_AMOUNT_RAW_MATERIALS_ID = "ShowZeroAmountRawMaterials";
        public const string SHOW_ZERO_AMOUNT_BASIC_MATERIALS_ID = "ShowZeroAmountBasicMaterials";
        public const string SHOW_ZERO_AMOUNT_ADVANCED_MATERIALS_ID = "ShowZeroAmountAdvancedMaterials";
        public const string SHOW_ZERO_AMOUNT_ELECTRONICS_ID = "ShowZeroAmountElectronics";
        public const string ITEMS_PER_PAGE_SMALL_MONITOR_ID = "ItemsPerPageSmallMonitor";
        public const string ITEMS_PER_PAGE_LARGE_MONITOR_ID = "ItemsPerPageLargeMonitor";
        public const string ALLOW_SELECTING_ITEMS_ID = "AllowSelectingItemsFromMonitor";
        public const string ENABLE_IDLE_ID = "EnableIdle";
        public const string IDLE_TIME_ID = "IdleTime";
        public const string IDLE_TIME_RANDOMNESS_LOW_BOUND_ID = "IdleTimeRandomnessLowBound";
        public const string IDLE_TIME_RANDOMNESS_HIGH_BOUND_ID = "IdleTimeRandomnessHighBound";
        public const string MAX_INTERACTION_DISTANCE_ID = "MaxInteractionDistance";
        public const string MAX_INTERACTION_IDLE_PAGE_DISTANCE_ID = "MaxInteractionIdlePageDistance";

        private const int MIN_ITEMS_PER_PAGE_LARGE_MONITOR = 4;
        private const int MAX_ITEMS_PER_PAGE_LARGE_MONITOR = 28;

        private const int MIN_ITEMS_PER_PAGE_SMALL_MONITOR = 4;
        private const int MAX_ITEMS_PER_PAGE_SMALL_MONITOR = 21;

        private const float MIN_IDLE_TIME = 5f;
        private const float MAX_IDLE_TIME = 120f;
        private const float MIN_IDLE_TIME_RANDOMNESS = 0f;
        private const float MAX_IDLE_TIME_RANDOMNESS = 30f;
        private const float MIN_INTERACTION_DISTANCE = 1f;
        private const float MAX_INTERACTION_DISTANCE = 10f;
        private const float MIN_IDLE_PAGE_DISTANCE = 1f;
        private const float MAX_IDLE_PAGE_DISTANCE = 15f;

        public ResourceMonitorOptions() : base("Resource Monitor Redux")
        {
            AddItem(ModToggleOption.Create(
                SHOW_ZERO_AMOUNT_RAW_MATERIALS_ID,
                "Keep depleted raw materials listed (as x0)",
                EntryPoint.SETTINGS.ShowZeroAmountRawMaterials,
                "Once a raw material has been seen, keep it on the screen showing x0 instead of removing it once none remain."));

            AddItem(ModToggleOption.Create(
                SHOW_ZERO_AMOUNT_BASIC_MATERIALS_ID,
                "Keep depleted basic materials listed (as x0)",
                EntryPoint.SETTINGS.ShowZeroAmountBasicMaterials,
                "Once a basic material has been seen, keep it on the screen showing x0 instead of removing it once none remain."));

            AddItem(ModToggleOption.Create(
                SHOW_ZERO_AMOUNT_ADVANCED_MATERIALS_ID,
                "Keep depleted advanced materials listed (as x0)",
                EntryPoint.SETTINGS.ShowZeroAmountAdvancedMaterials,
                "Once an advanced material has been seen, keep it on the screen showing x0 instead of removing it once none remain."));

            AddItem(ModToggleOption.Create(
                SHOW_ZERO_AMOUNT_ELECTRONICS_ID,
                "Keep depleted electronics listed (as x0)",
                EntryPoint.SETTINGS.ShowZeroAmountElectronics,
                "Once an electronics item has been seen, keep it on the screen showing x0 instead of removing it once none remain."));

            AddItem(ModSliderOption.Create(
                ITEMS_PER_PAGE_SMALL_MONITOR_ID,
                "Items per page (small monitor)",
                MIN_ITEMS_PER_PAGE_SMALL_MONITOR,
                MAX_ITEMS_PER_PAGE_SMALL_MONITOR,
                EntryPoint.SETTINGS.ItemsPerPageSmallMonitor,
                tooltip: "How many items are shown per page on the small Resource Monitor Screen (default: 8)."));

            AddItem(ModSliderOption.Create(
                ITEMS_PER_PAGE_LARGE_MONITOR_ID,
                "Items per page (large monitor)",
                MIN_ITEMS_PER_PAGE_LARGE_MONITOR,
                MAX_ITEMS_PER_PAGE_LARGE_MONITOR,
                EntryPoint.SETTINGS.ItemsPerPageLargeMonitor,
                tooltip: "How many items are shown per page on the large Resource Monitor Screen (default: 18)."));

            AddItem(ModToggleOption.Create(
                ALLOW_SELECTING_ITEMS_ID,
                "Allow taking items from the screen",
                EntryPoint.SETTINGS.AllowSelectingItemsFromMonitor,
                "When enabled, clicking a tracked item on the screen takes one directly from storage (default: on)."));

            AddItem(ModToggleOption.Create(
                ENABLE_IDLE_ID,
                "Enable idle screensaver",
                EntryPoint.SETTINGS.EnableIdle,
                "When enabled, the screen switches to an idle animation after a period of inactivity (default: on)."));

            AddItem(ModSliderOption.Create(
                IDLE_TIME_ID,
                "Idle timeout (seconds)",
                MIN_IDLE_TIME,
                MAX_IDLE_TIME,
                EntryPoint.SETTINGS.IdleTime,
                valueFormat: "{0:F0}",
                tooltip: "How many seconds of inactivity before the screen goes idle (default: 20)."));

            AddItem(ModSliderOption.Create(
                IDLE_TIME_RANDOMNESS_LOW_BOUND_ID,
                "Idle timeout randomness (min)",
                MIN_IDLE_TIME_RANDOMNESS,
                MAX_IDLE_TIME_RANDOMNESS,
                EntryPoint.SETTINGS.IdleTimeRandomnessLowBound,
                valueFormat: "{0:F1}",
                tooltip: "Minimum extra random seconds added to the idle timeout, so it doesn't trigger at exactly the same time every time (default: 1)."));

            AddItem(ModSliderOption.Create(
                IDLE_TIME_RANDOMNESS_HIGH_BOUND_ID,
                "Idle timeout randomness (max)",
                MIN_IDLE_TIME_RANDOMNESS,
                MAX_IDLE_TIME_RANDOMNESS,
                EntryPoint.SETTINGS.IdleTimeRandomnessHighBound,
                valueFormat: "{0:F1}",
                tooltip: "Maximum extra random seconds added to the idle timeout (default: 10)."));

            AddItem(ModSliderOption.Create(
                MAX_INTERACTION_DISTANCE_ID,
                "Max interaction distance",
                MIN_INTERACTION_DISTANCE,
                MAX_INTERACTION_DISTANCE,
                EntryPoint.SETTINGS.MaxInteractionDistance,
                valueFormat: "{0:F1}",
                tooltip: "How close you need to be to click the screen's buttons (default: 2.5)."));

            AddItem(ModSliderOption.Create(
                MAX_INTERACTION_IDLE_PAGE_DISTANCE_ID,
                "Max idle wake-up distance",
                MIN_IDLE_PAGE_DISTANCE,
                MAX_IDLE_PAGE_DISTANCE,
                EntryPoint.SETTINGS.MaxInteractionIdlePageDistance,
                valueFormat: "{0:F1}",
                tooltip: "How close you need to approach for the idle screensaver to wake back up (default: 5)."));

            OnChanged += Options_OnChanged;
        }

        private void Options_OnChanged(object sender, OptionEventArgs e)
        {
            if (e is ToggleChangedEventArgs toggleArgs)
            {
                switch (e.Id)
                {
                    case SHOW_ZERO_AMOUNT_RAW_MATERIALS_ID:
                        EntryPoint.SETTINGS.ShowZeroAmountRawMaterials = toggleArgs.Value;
                        break;
                    case SHOW_ZERO_AMOUNT_BASIC_MATERIALS_ID:
                        EntryPoint.SETTINGS.ShowZeroAmountBasicMaterials = toggleArgs.Value;
                        break;
                    case SHOW_ZERO_AMOUNT_ADVANCED_MATERIALS_ID:
                        EntryPoint.SETTINGS.ShowZeroAmountAdvancedMaterials = toggleArgs.Value;
                        break;
                    case SHOW_ZERO_AMOUNT_ELECTRONICS_ID:
                        EntryPoint.SETTINGS.ShowZeroAmountElectronics = toggleArgs.Value;
                        break;
                    case ALLOW_SELECTING_ITEMS_ID:
                        EntryPoint.SETTINGS.AllowSelectingItemsFromMonitor = toggleArgs.Value;
                        break;
                    case ENABLE_IDLE_ID:
                        EntryPoint.SETTINGS.EnableIdle = toggleArgs.Value;
                        break;
                    default:
                        return;
                }

                EntryPoint.SaveSettings();
            }
            else if (e is SliderChangedEventArgs sliderArgs)
            {
                switch (e.Id)
                {
                    case ITEMS_PER_PAGE_SMALL_MONITOR_ID:
                        EntryPoint.SETTINGS.ItemsPerPageSmallMonitor = Mathf.RoundToInt(sliderArgs.Value);
                        EntryPoint.SaveSettings();
                        Components.ResourceMonitorDisplay.RefreshAllForItemsPerPageChange();
                        return;
                    case ITEMS_PER_PAGE_LARGE_MONITOR_ID:
                        EntryPoint.SETTINGS.ItemsPerPageLargeMonitor = Mathf.RoundToInt(sliderArgs.Value);
                        EntryPoint.SaveSettings();
                        Components.ResourceMonitorDisplay.RefreshAllForItemsPerPageChange();
                        return;
                    case IDLE_TIME_ID:
                        EntryPoint.SETTINGS.IdleTime = sliderArgs.Value;
                        break;
                    case IDLE_TIME_RANDOMNESS_LOW_BOUND_ID:
                        EntryPoint.SETTINGS.IdleTimeRandomnessLowBound = sliderArgs.Value;
                        break;
                    case IDLE_TIME_RANDOMNESS_HIGH_BOUND_ID:
                        EntryPoint.SETTINGS.IdleTimeRandomnessHighBound = sliderArgs.Value;
                        break;
                    case MAX_INTERACTION_DISTANCE_ID:
                        EntryPoint.SETTINGS.MaxInteractionDistance = sliderArgs.Value;
                        break;
                    case MAX_INTERACTION_IDLE_PAGE_DISTANCE_ID:
                        EntryPoint.SETTINGS.MaxInteractionIdlePageDistance = sliderArgs.Value;
                        break;
                    default:
                        return;
                }

                EntryPoint.SaveSettings();
            }
        }
    }
}

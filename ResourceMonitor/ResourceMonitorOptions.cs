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

        private const int MIN_ITEMS_PER_PAGE_LARGE_MONITOR = 4;
        private const int MAX_ITEMS_PER_PAGE_LARGE_MONITOR = 28;

        private const int MIN_ITEMS_PER_PAGE_SMALL_MONITOR = 4;
        private const int MAX_ITEMS_PER_PAGE_SMALL_MONITOR = 21;

        public ResourceMonitorOptions() : base("Resource Monitor")
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
                        break;
                    case ITEMS_PER_PAGE_LARGE_MONITOR_ID:
                        EntryPoint.SETTINGS.ItemsPerPageLargeMonitor = Mathf.RoundToInt(sliderArgs.Value);
                        break;
                    default:
                        return;
                }

                EntryPoint.SaveSettings();
                Components.ResourceMonitorDisplay.RefreshAllForItemsPerPageChange();
            }
        }
    }
}

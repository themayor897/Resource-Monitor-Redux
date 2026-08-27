using Nautilus.Options;

namespace ResourceMonitor
{
    /**
    * Adds Resource Monitor's settings to the game's own Mod Options screen. Backed by the same
    * SettingsData/Settings.json used everywhere else in the mod, so changes made here persist.
    */
    public class ResourceMonitorOptions : ModOptions
    {
        public const string SHOW_ZERO_AMOUNT_RESOURCES_ID = "ShowZeroAmountResources";

        public ResourceMonitorOptions() : base("Resource Monitor")
        {
            AddItem(ModToggleOption.Create(
                SHOW_ZERO_AMOUNT_RESOURCES_ID,
                "Keep depleted resources listed (as x0)",
                EntryPoint.SETTINGS.ShowZeroAmountResources,
                "Once a raw, basic, advanced, or electronics material has been seen, keep it on the screen showing x0 instead of removing it once none remain."));

            OnChanged += Options_OnChanged;
        }

        private void Options_OnChanged(object sender, OptionEventArgs e)
        {
            if (e.Id == SHOW_ZERO_AMOUNT_RESOURCES_ID && e is ToggleChangedEventArgs toggleArgs)
            {
                EntryPoint.SETTINGS.ShowZeroAmountResources = toggleArgs.Value;
                EntryPoint.SaveSettings();
            }
        }
    }
}

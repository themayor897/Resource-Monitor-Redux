namespace ResourceMonitor
{
    [System.Serializable]
    public class SettingsData
    {
        public bool AllowSelectingItemsFromMonitor = true;

        // Also editable in-game via Mod Options > Resource Monitor.
        public bool ShowZeroAmountRawMaterials = true;
        public bool ShowZeroAmountBasicMaterials = false;
        public bool ShowZeroAmountAdvancedMaterials = false;
        public bool ShowZeroAmountElectronics = false;

        public float MaxInteractionDistance = 2.5f;

        public float MaxInteractionIdlePageDistance = 5f;

        public bool EnableIdle = true;

        public float IdleTime = 20f;

        public float IdleTimeRandomnessLowBound = 1f;

        public float IdleTimeRandomnessHighBound = 10f;
    }
}

using System.Collections.Generic;

namespace ResourceMonitor
{
    [System.Serializable]
    public class SettingsData
    {
        public bool AllowSelectingItemsFromMonitor = true;

        // One of "Category" (default, hand-audited tier order), "Alphabetical", or "Quantity".
        public string SortOrder = "Category";

        // Hides item name labels so more icons fit visually without feeling cramped.
        public bool CompactDisplay = false;

        // TechType.AsString().ToLower() entries a player has chosen to stop tracking while Item
        // management mode is on. Separate from DontTrackList.txt, which excludes by container name.
        public List<string> HiddenItemTypes = new List<string>();

        // While on, clicking an item on the monitor stops tracking it instead of taking it -
        // replaces the old always-active right-click-to-hide, which was too easy to trigger by
        // accident.
        public bool ItemManagementModeEnabled = false;

        // While on, pressing the container-toggle key while a storage container is open flips
        // whether that specific container is tracked.
        public bool ContainerManagementModeEnabled = false;

        // Stable per-container identifiers (SceneObjectIdentifier.Id on Subnautica; falls back to
        // GameObject name on Below Zero, which isn't guaranteed unique - see GetStableContainerId)
        // a player has excluded via container management mode.
        public List<string> ExcludedContainerIds = new List<string>();

        // Also editable in-game via Mod Options > Resource Monitor.
        public bool ShowZeroAmountRawMaterials = true;
        public bool ShowZeroAmountBasicMaterials = false;
        public bool ShowZeroAmountAdvancedMaterials = false;
        public bool ShowZeroAmountElectronics = false;

        public int ItemsPerPageSmallMonitor = 8;
        public int ItemsPerPageLargeMonitor = 18;

        public float MaxInteractionDistance = 2.5f;

        public float MaxInteractionIdlePageDistance = 5f;

        public bool EnableIdle = true;

        public float IdleTime = 20f;

        public float IdleTimeRandomnessLowBound = 1f;

        public float IdleTimeRandomnessHighBound = 10f;
    }
}

using UnityEngine;

namespace ResourceMonitor.Game_Items
{
    /**
    * The resource monitor (large version) that will be placed in the sea base (or cyclops or any internal area).
    */
    public static class ResourceMonitorScreenLarge
    {
        public const string CLASS_ID = "ResourceMonitorBuildableLarge";
        public const string NICE_NAME = "Resource Monitor Screen Large";
        public const string DESCRIPTION = "Track how many resources you have stored away in your sea base on one handy large screen.";
        public const string ICON_FILE_NAME = "ResourceMonitorLarge.png";
        public const int INGREDIENTS_REQUIRED = 2;
        public static readonly Vector3 SCALE = new Vector3(2.3f, 2.3f, 1f);

        public static void Register()
        {
            ResourceMonitorScreenGeneric.Register(CLASS_ID, NICE_NAME, DESCRIPTION, ICON_FILE_NAME, INGREDIENTS_REQUIRED, SCALE);
        }
    }
}

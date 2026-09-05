namespace ResourceMonitor.Game_Items
{
    /**
    * The resource monitor (small version) that will be placed in the sea base (or cyclops or any internal area).
    */
    public static class ResourceMonitorScreenSmall
    {
        public const string CLASS_ID = "ResourceMonitorBuildableSmall";
        public const string NICE_NAME = "Resource Monitor Screen Small";
        public const string DESCRIPTION = "Track how many resources you have stored away in your sea base on one handy small screen.";
        public const string ICON_FILE_NAME = "ResourceMonitorSmall.png";
        public const int INGREDIENTS_REQUIRED = 1;

        public static void Register()
        {
            ResourceMonitorScreenGeneric.Register(CLASS_ID, NICE_NAME, DESCRIPTION, ICON_FILE_NAME, INGREDIENTS_REQUIRED, null, isLarge: false);
        }
    }
}

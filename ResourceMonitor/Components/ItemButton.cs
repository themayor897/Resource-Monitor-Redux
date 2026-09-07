using UnityEngine.EventSystems;

namespace ResourceMonitor.Components
{
    /**
     * Component that will be added onto the item button. Hover/click handling comes from
     * OnScreenButton; visuals are authored directly on the prefab in Unity.
     */
    public class ItemButton : OnScreenButton, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public int Amount { set; get; }
        private TechType type = TechType.None;

        public TechType Type
        {
            set
            {
                var name = ResourceMonitorLogic.GetSafeDisplayName(value);
                HoverText = EntryPoint.SETTINGS.ItemManagementModeEnabled
                    ? $"Stop tracking {name}"
                    : EntryPoint.SETTINGS.AllowSelectingItemsFromMonitor
                        ? $"Take {name}"
                        : name;

                type = value;
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            if (IsHovered == false || ResourceMonitorDisplay?.ResourceMonitorLogic == null || type == TechType.None)
            {
                return;
            }

            if (EntryPoint.SETTINGS.ItemManagementModeEnabled)
            {
                ResourceMonitorDisplay.ResourceMonitorLogic.HideItemType(type);
            }
            else if (EntryPoint.SETTINGS.AllowSelectingItemsFromMonitor)
            {
                ResourceMonitorDisplay.ResourceMonitorLogic.AttemptToTakeItem(type);
            }
        }
    }
}

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
                if (EntryPoint.SETTINGS.AllowSelectingItemsFromMonitor)
                    HoverText = "Take " + Language.main.Get(value);
                else
                    HoverText = Language.main.Get(value);

                type = value;
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            if (EntryPoint.SETTINGS.AllowSelectingItemsFromMonitor && IsHovered && ResourceMonitorDisplay != null && ResourceMonitorDisplay.ResourceMonitorLogic != null && type != TechType.None)
            {
                ResourceMonitorDisplay.ResourceMonitorLogic.AttemptToTakeItem(type);
            }
        }
    }
}

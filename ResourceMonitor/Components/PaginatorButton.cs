using UnityEngine.EventSystems;

namespace ResourceMonitor.Components
{
    /**
     * Component that will be added onto the paginator buttons. Hover/click handling comes from
     * OnScreenButton; visuals are authored directly on the prefab in Unity.
     */
    public class PaginatorButton : OnScreenButton, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        public int AmountToChangePageBy { get; set; } = 1;

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            if (IsHovered)
            {
                ResourceMonitorDisplay.ChangePageBy(AmountToChangePageBy);
            }
        }
    }
}

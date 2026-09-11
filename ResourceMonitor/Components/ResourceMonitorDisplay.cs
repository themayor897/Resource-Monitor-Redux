using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ResourceMonitor.Components
{
    /**
    * Component that contains the controller to the view. Majority of the view is set up already on the prefab inside of the unity editor.
    * Handles such things as the paginator, drawing all the items that are on the "current page"
    * Handles the idle screen saver.
    * Handles the welcome animations.
    */
    public class ResourceMonitorDisplay : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private static readonly float WELCOME_ANIMATION_TIME = 8.5f;
        private static readonly float MAIN_SCREEN_ANIMATION_TIME = 1.2f;
        private static readonly List<ResourceMonitorDisplay> ActiveDisplays = new List<ResourceMonitorDisplay>();

        // IconCircle/ItemHolder are authored at a fixed 150px size in the prefab, but the grid's
        // cell size is computed dynamically (bigger with fewer items, smaller on a full page) -
        // without this, the icon stays a constant pixel size and looks tiny in a full grid.
        private const float ICON_FILL_RATIO = 0.75f;
        private const float ICON_CIRCLE_BASE_SIZE = 150f;

        // Icons shouldn't keep growing just because very few items are tracked - cap their size
        // at whatever they'd be with a reasonably full two-row page, per monitor size.
        private const int MAX_ICON_SCALE_REFERENCE_ITEMS_LARGE = 10;
        private const int MAX_ICON_SCALE_REFERENCE_ITEMS_SMALL = 8;

        private float MaxIconCellSize => ComputeBestCellSize(
            ResourceMonitorLogic.IsLargeMonitor ? MAX_ICON_SCALE_REFERENCE_ITEMS_LARGE : MAX_ICON_SCALE_REFERENCE_ITEMS_SMALL,
            out _);

        /**
        * Called when a Mod Options change affects how the current page looks (items per page,
        * sort order, compact display, hidden items), so a screen already on screen picks it up
        * immediately instead of only on its next natural redraw (changing pages, re-approaching
        * it, etc).
        */
        public static void RefreshAllDisplays()
        {
            foreach (var display in ActiveDisplays)
            {
                display.DrawPage(display.currentPage);
            }
        }

        /**
        * Called after Mod Options > Resource Monitor > Clear hidden items list, so previously
        * hidden items reappear immediately instead of only once their container is next touched.
        */
        public static void RetrackPreviouslyHiddenItems(HashSet<string> keysToRetrack)
        {
            foreach (var display in ActiveDisplays)
            {
                display.ResourceMonitorLogic?.RetrackItems(keysToRetrack);
            }

            RefreshAllDisplays();
        }

        /**
        * Called by ResourceMonitorLogic.ToggleContainerTrackingGlobally after the container
        * exclusion list changes, so whichever active display actually owns that container (if
        * any) picks up the change immediately.
        */
        public static void SyncContainerExclusionForAll(StorageContainer sc, bool excluded)
        {
            foreach (var display in ActiveDisplays)
            {
                display.ResourceMonitorLogic?.SyncContainerExclusion(sc, excluded);
            }

            RefreshAllDisplays();
        }

        private int ItemsPerPage => ResourceMonitorLogic.IsLargeMonitor
            ? EntryPoint.SETTINGS.ItemsPerPageLargeMonitor
            : EntryPoint.SETTINGS.ItemsPerPageSmallMonitor;

        public ResourceMonitorLogic ResourceMonitorLogic { get; private set; }
        private Dictionary<TechType, GameObject> trackedResourcesDisplayElements;
        private int currentPage = 1;
        private int maxPage = 1;
        private float idlePeriodLength = EntryPoint.SETTINGS.IdleTime;
        private float timeSinceLastInteraction = 0f;
        private bool isIdle = false;
        private bool isHovered = false;
        private bool isHoveredOutOfRange = false;

        public GameObject CanvasGameObject { get; private set; }
        private Animator animator;
        private GameObject blackCover;
        private GameObject welcomeScreen;
        private GameObject mainScreen;
        private GameObject mainScreensCover;
        private GameObject mainScreenItemGrid;
        private GridLayoutGroup mainScreenItemGridLayout;
        private GameObject previousPageGameObject;
        private GameObject nextPageGameObject;
        private GameObject pageCounterGameObject;
        private TextMeshProUGUI pageCounterText;
        private GameObject idleScreen;

        public void Setup(ResourceMonitorLogic rml)
        {
            if (rml.IsBeingDeleted == true) return;

            ResourceMonitorLogic = rml;
            trackedResourcesDisplayElements = new Dictionary<TechType, GameObject>();

            if (FindAllComponents() == false)
            {
                TurnDisplayOff();
                return;
            }

            ActiveDisplays.Add(this);
            CalculateNewIdleTime();
            currentPage = 1;
            UpdatePaginator();

            StartCoroutine(FinalSetup());
        }

        private IEnumerator FinalSetup()
        {
            animator.enabled = true;
            welcomeScreen.SetActive(false);
            blackCover.SetActive(false);
            mainScreen.SetActive(false);
            animator.Play("Reset");

            yield return new WaitForEndOfFrame();
            if (ResourceMonitorLogic.IsBeingDeleted == true) yield break;

            animator.Play("Welcome");

            if (ResourceMonitorLogic.IsBeingDeleted == true) yield break;
            yield return new WaitForSeconds(WELCOME_ANIMATION_TIME);
            if (ResourceMonitorLogic.IsBeingDeleted == true) yield break;

            animator.Play("ShowMainScreen");
            DrawPage(1);

            if (ResourceMonitorLogic.IsBeingDeleted == true) yield break;
            yield return new WaitForSeconds(MAIN_SCREEN_ANIMATION_TIME);
            if (ResourceMonitorLogic.IsBeingDeleted == true) yield break;

            welcomeScreen.SetActive(false);
            blackCover.SetActive(false);
            mainScreen.SetActive(true);
            animator.enabled = false;

            // The DrawPage(1) call above ran while mainScreen (and so MainGrid) was still
            // inactive, so GridLayoutGroup never actually repositioned anything to match the
            // cell size/column count UpdateGridLayout computed - Unity's layout system doesn't
            // process inactive hierarchies. Redraw now that it's actually active.
            DrawPage(currentPage);
        }

        public void TurnDisplayOff()
        {
            ActiveDisplays.Remove(this);
            StopCoroutine(FinalSetup());
            blackCover?.SetActive(true);
            trackedResourcesDisplayElements?.Clear();
            trackedResourcesDisplayElements = null;
        }

        public void ItemModified(TechType type, int newAmount)
        {
            if (newAmount > 0 && trackedResourcesDisplayElements.ContainsKey(type))
            {
                trackedResourcesDisplayElements[type].GetComponentInChildren<TextMeshProUGUI>().text = "x" + newAmount;
                return;
            }

            DrawPage(currentPage);
        }

        private void CalculateNewMaxPages()
        {
            maxPage = Mathf.CeilToInt((ResourceMonitorLogic.TrackedResources.Count - 1) / ItemsPerPage) + 1;
            if (currentPage > maxPage)
            {
                currentPage = maxPage;
            }
        }

        public void ChangePageBy(int amount)
        {
            DrawPage(currentPage + amount);
        }

        private void DrawPage(int page)
        {
            currentPage = page;
            if (currentPage <= 0)
            {
                currentPage = 1;
            }
            else if (currentPage > maxPage)
            {
                currentPage = maxPage;
            }

            var itemsPerPage = ItemsPerPage;
            var sortedResources = ResourceMonitorLogic.GetSortedTrackedResources();

            var startingPosition = (currentPage - 1) * itemsPerPage;
            var endingPosition = startingPosition + itemsPerPage;
            if (endingPosition > sortedResources.Count)
            {
                endingPosition = sortedResources.Count;
            }

            // Size cells to how many items are actually on this page, not the page's max
            // capacity, so icons stay as large as possible and only shrink once the page fills up.
            UpdateGridLayout(endingPosition - startingPosition);
            ClearPage();
            for (var i = startingPosition; i < endingPosition; i++)
            {
                var resource = sortedResources[i];
                CreateAndAddItemDisplay(resource.TechType, resource.Amount);
            }

            UpdatePaginator();
        }

        private void UpdatePaginator()
        {
            CalculateNewMaxPages();
            pageCounterText.text = $"Page {currentPage} Of {maxPage}";
            previousPageGameObject.SetActive(currentPage != 1);
            nextPageGameObject.SetActive(currentPage != maxPage);
        }

        /**
        * Picks a column count and cell size that fit itemsPerPage items into MainGrid's actual
        * available area as close to square as possible, so the configurable items-per-page
        * setting (Mod Options > Resource Monitor) still lays out cleanly instead of overflowing
        * or under-filling the fixed 200x200 cell size the prefab was originally authored with.
        */
        private const int GRID_TOP_PADDING = 20;

        private void UpdateGridLayout(int itemsPerPage)
        {
            var padding = mainScreenItemGridLayout.padding;
            padding.top = GRID_TOP_PADDING;
            mainScreenItemGridLayout.padding = padding;

            var bestCellSize = ComputeBestCellSize(itemsPerPage, out var bestColumns);
            if (bestCellSize <= 0f)
            {
                return;
            }

            mainScreenItemGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            mainScreenItemGridLayout.constraintCount = bestColumns;
            mainScreenItemGridLayout.cellSize = new Vector2(bestCellSize, bestCellSize);
        }

        /**
        * Picks the column count and cell size that fit itemsPerPage items into MainGrid's actual
        * available area as close to square as possible. Shared by UpdateGridLayout (for the real
        * page) and the icon-scale cap (for a hypothetical reference item count).
        */
        private float ComputeBestCellSize(int itemsPerPage, out int bestColumns)
        {
            bestColumns = 1;
            var bestCellSize = 0f;

            var padding = mainScreenItemGridLayout.padding;
            var gridRect = ((RectTransform)mainScreenItemGrid.transform).rect;
            var availableWidth = gridRect.width - padding.left - padding.right;
            var availableHeight = gridRect.height - padding.top - padding.bottom;
            if (availableWidth <= 0f || availableHeight <= 0f || itemsPerPage <= 0)
            {
                return 0f;
            }

            var spacing = mainScreenItemGridLayout.spacing;

            // Try every column count and keep whichever yields the largest (still-square) cell
            // size; on a tie, prefer the count that divides itemsPerPage evenly (no half-empty
            // last row) over one that happens to match the same cell size less cleanly.
            for (var columns = 1; columns <= itemsPerPage; columns++)
            {
                var rows = Mathf.CeilToInt((float)itemsPerPage / columns);
                var cellWidth = (availableWidth - spacing.x * (columns - 1)) / columns;
                var cellHeight = (availableHeight - spacing.y * (rows - 1)) / rows;
                var cellSize = Mathf.Min(cellWidth, cellHeight);
                if (cellSize <= 0f)
                {
                    continue;
                }

                var better = cellSize > bestCellSize + 0.01f;
                var tie = !better && cellSize >= bestCellSize - 0.01f;
                var fewerLeftovers = tie && LeftoverSlots(itemsPerPage, columns) < LeftoverSlots(itemsPerPage, bestColumns);

                if (better || fewerLeftovers)
                {
                    bestCellSize = cellSize;
                    bestColumns = columns;
                }
            }

            return bestCellSize;
        }

        private static int LeftoverSlots(int itemsPerPage, int columns)
        {
            var remainder = itemsPerPage % columns;
            return remainder == 0 ? 0 : columns - remainder;
        }

        private static void ScaleAboutOrigin(RectTransform rect, float sizeScale, float positionScale)
        {
            rect.localScale = Vector3.one * sizeScale;
            rect.anchoredPosition *= positionScale;
        }


        private void ClearPage()
        {
            for (int i = 0; i < mainScreenItemGrid.transform.childCount; i++)
            {
                Destroy(mainScreenItemGrid.transform.GetChild(i).gameObject);
            }

            if (trackedResourcesDisplayElements != null)
            {
                trackedResourcesDisplayElements.Clear();
            }
        }

        private void CreateAndAddItemDisplay(TechType type, int amount)
        {
            var itemDisplay = Instantiate(EntryPoint.RESOURCE_MONITOR_DISPLAY_ITEM_UI_PREFAB);
            itemDisplay.transform.SetParent(mainScreenItemGrid.transform, false);

            // IconCircle, ItemHolder, and ItemName are all siblings positioned relative to a
            // shared center point, authored to line up correctly at scale 1. Scaling only one of
            // them (e.g. just the icon, or just the name) breaks that arrangement at any other
            // scale, so all three need their position - not just their size - scaled together as
            // a single rigid group around that shared origin.
            // In compact mode there's no name label below the icon, so it's re-centered (position
            // scale 0 zeroes out the authored +20 offset that normally makes room for that label)
            // instead of sitting off-center with empty space beneath it.
            var cappedCellSize = Mathf.Min(mainScreenItemGridLayout.cellSize.x, MaxIconCellSize);
            var iconScale = (cappedCellSize * ICON_FILL_RATIO) / ICON_CIRCLE_BASE_SIZE;
            var iconPositionScale = EntryPoint.SETTINGS.CompactDisplay ? 0f : iconScale;
            ScaleAboutOrigin((RectTransform)itemDisplay.transform.Find("IconCircle"), iconScale, iconPositionScale);
            ScaleAboutOrigin((RectTransform)itemDisplay.transform.Find("ItemHolder"), iconScale, iconPositionScale);

            var itemNameGameObject = itemDisplay.transform.Find("ItemName").gameObject;
            if (EntryPoint.SETTINGS.CompactDisplay)
            {
                itemNameGameObject.SetActive(false);
            }
            else
            {
                // The name's own text size still scales fully with the icon (stays legible/proportional),
                // but its distance from the icon never shrinks below what's authored in Unity - only
                // ever grows further out when the icon scales up past its base size. A pure
                // proportional gap looks fine in Unity's static preview (which only ever shows scale 1)
                // but reads as cramped once icons are actually scaled down below that at runtime.
                var namePositionScale = Mathf.Max(iconScale, 1f);
                ScaleAboutOrigin((RectTransform)itemNameGameObject.transform, iconScale, namePositionScale);
                itemNameGameObject.GetComponent<TextMeshProUGUI>().text = ResourceMonitorLogic.GetSafeDisplayName(type);
            }

            itemDisplay.transform.Find("IconCircle/Text").GetComponent<TextMeshProUGUI>().text = "x" + amount;

            var itemButton = itemDisplay.AddComponent<ItemButton>();
            itemButton.Type = type;
            itemButton.Amount = amount;
            itemButton.ResourceMonitorDisplay = this;

            var icon = itemDisplay.transform.Find("ItemHolder").gameObject.AddComponent<uGUI_Icon>();
            icon.sprite = SpriteManager.Get(type);

            trackedResourcesDisplayElements.Add(type, itemDisplay);
        }

        public void Update()
        {
            if (isIdle == false && timeSinceLastInteraction < idlePeriodLength)
            {
                timeSinceLastInteraction += Time.deltaTime;
            }

            if (EntryPoint.SETTINGS.EnableIdle && isIdle == false && timeSinceLastInteraction >= idlePeriodLength)
            {
                EnterIdleScreen();
            }

            if (isHovered == false && isHoveredOutOfRange == true && InIdleInteractionRange() == true)
            {
                isHovered = true;
                ExitIdleScreen();
            }

            if (isHovered == true)
            {
                ResetIdleTimer();
            }
        }

        private bool InIdleInteractionRange()
        {
            return Mathf.Abs(Vector3.Distance(gameObject.transform.position, Player.main.transform.position)) <= EntryPoint.SETTINGS.MaxInteractionIdlePageDistance;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (isIdle && InIdleInteractionRange())
            {
                ExitIdleScreen();
            }
            
            if (isIdle == false)
            {
                ResetIdleTimer();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHoveredOutOfRange = true;
            if (InIdleInteractionRange())
            {
                isHovered = true;
            }

            if (isIdle && InIdleInteractionRange())
            {
                ExitIdleScreen();
            }

            if (isIdle == false)
            {
                ResetIdleTimer();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHoveredOutOfRange = false;
            isHovered = false;
            if (isIdle && InIdleInteractionRange())
            {
                ExitIdleScreen();
            }

            if (isIdle == false)
            {
                ResetIdleTimer();
            }
        }

        private void EnterIdleScreen()
        {
            isIdle = true;
            mainScreen.SetActive(false);
            idleScreen.SetActive(true);
        }

        private void ExitIdleScreen()
        {
            isIdle = false;
            ResetIdleTimer();
            CalculateNewIdleTime();
            mainScreen.SetActive(true);
            idleScreen.SetActive(false);

            // Item changes that happened while mainScreen was inactive (idle) ran DrawPage
            // against an inactive hierarchy, so GridLayoutGroup never actually laid out the
            // new icons. Redraw now that it's active again to fix any resulting overlap.
            DrawPage(currentPage);
        }
        
        private void CalculateNewIdleTime()
        {
            idlePeriodLength = EntryPoint.SETTINGS.IdleTime + Random.Range(EntryPoint.SETTINGS.IdleTimeRandomnessLowBound, EntryPoint.SETTINGS.IdleTimeRandomnessHighBound);
        }

        public void ResetIdleTimer()
        {
            timeSinceLastInteraction = 0f;
        }

        public void OnApplicationQuit()
        {
            StopAllCoroutines();
        }

        private bool FindAllComponents()
        {
            CanvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;
            if (CanvasGameObject == null)
            {
                System.Console.WriteLine("[ResourceMonitor] Canvas not found.");
                return false;
            }

            animator = CanvasGameObject.GetComponent<Animator>();
            if (animator == null)
            {
                System.Console.WriteLine("[ResourceMonitor] Animator not found.");
                return false;
            }

            blackCover = CanvasGameObject.FindChild("BlackCover")?.gameObject;
            if (blackCover == null)
            {
                System.Console.WriteLine("[ResourceMonitor] BlackCover not found.");
                return false;
            }

            var screenHolder = CanvasGameObject.transform.Find("Screens")?.gameObject;
            if (screenHolder == null)
            {
                System.Console.WriteLine("[ResourceMonitor] Screen Holder Gameobject not found.");
                return false;
            }

            welcomeScreen = screenHolder.FindChild("WelcomeScreen")?.gameObject;
            if (welcomeScreen == null)
            {
                System.Console.WriteLine("[ResourceMonitor] Screen: WelcomeScreen not found.");
                return false;
            }

            mainScreen = screenHolder.FindChild("MainScreen")?.gameObject;
            if (mainScreen == null)
            {
                System.Console.WriteLine("[ResourceMonitor] Screen: MainScreen not found.");
                return false;
            }

            mainScreensCover = mainScreen.FindChild("BlackCover")?.gameObject;
            if (mainScreensCover == null)
            {
                System.Console.WriteLine("[ResourceMonitor] Screen: MainScreen Cover not found.");
                return false;
            }

            var actualMainScreen = mainScreen.FindChild("ActualScreen")?.gameObject;
            if (actualMainScreen == null)
            {
                System.Console.WriteLine("[ResourceMonitor] Screen: Actual Main Screen not found.");
                return false;
            }

            mainScreenItemGrid = actualMainScreen.FindChild("MainGrid")?.gameObject;
            if (mainScreenItemGrid == null)
            {
                System.Console.WriteLine("[ResourceMonitor] Screen: Main Screen Item Grid not found.");
                return false;
            }

            mainScreenItemGridLayout = mainScreenItemGrid.GetComponent<GridLayoutGroup>();
            if (mainScreenItemGridLayout == null)
            {
                System.Console.WriteLine("[ResourceMonitor] Screen: Main Screen Item Grid has no GridLayoutGroup.");
                return false;
            }

            var paginator = actualMainScreen.FindChild("Paginator")?.gameObject;
            if (paginator == null)
            {
                System.Console.WriteLine("[ResourceMonitor] Screen: Paginator not found.");
                return false;
            }

            previousPageGameObject = actualMainScreen.FindChild("PreviousPage")?.gameObject;
            if (previousPageGameObject == null)
            {
                System.Console.WriteLine("[ResourceMonitor] Screen: Previous Page GameObject not found.");
                return false;
            }
            
            var pb = previousPageGameObject.AddComponent<PaginatorButton>();
            pb.ResourceMonitorDisplay = this;
            pb.AmountToChangePageBy = -1;
            pb.HoverText = "Previous Page";

            nextPageGameObject = actualMainScreen.FindChild("NextPage")?.gameObject;
            if (nextPageGameObject == null)
            {
                System.Console.WriteLine("[ResourceMonitor] Screen: Next Page GameObject not found.");
                return false;
            }
            var pb2 = nextPageGameObject.AddComponent<PaginatorButton>();
            pb2.ResourceMonitorDisplay = this;
            pb2.AmountToChangePageBy = 1;
            pb2.HoverText = "Next Page";

            pageCounterGameObject = paginator.FindChild("PageCounter")?.gameObject;
            if (pageCounterGameObject == null)
            {
                System.Console.WriteLine("[ResourceMonitor] Screen: Page Counter GameObject not found.");
                return false;
            }

            pageCounterText = pageCounterGameObject.GetComponent<TextMeshProUGUI>();
            if (pageCounterText == null)
            {
                System.Console.WriteLine("[ResourceMonitor] Screen: Page Counter Text not found.");
                return false;
            }

            idleScreen = screenHolder.FindChild("IdleScreen")?.gameObject;
            if (idleScreen == null)
            {
                System.Console.WriteLine("[ResourceMonitor] Screen: IdleScreen not found.");
                return false;
            }

            AddSpinToLogo(welcomeScreen);
            AddSpinToLogo(idleScreen);

            return true;
        }

        private static void AddSpinToLogo(GameObject screen)
        {
            var logo = screen.FindChild("AlterraTitleBackground")?.gameObject;
            if (logo == null)
            {
                System.Console.WriteLine($"[ResourceMonitor] Screen: AlterraTitleBackground not found under {screen.name}, logo won't spin.");
                return;
            }

            if (logo.GetComponent<SpinningLogo>() == null)
            {
                logo.AddComponent<SpinningLogo>();
            }
        }
    }
}
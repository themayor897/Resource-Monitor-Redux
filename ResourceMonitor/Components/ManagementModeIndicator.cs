using Nautilus.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ResourceMonitor.Components
{
    /**
    * Persistent, code-built (no Unity asset needed) HUD reminder shown whenever Item management
    * mode or Container management mode is on - both change what a normal click/key-press does in
    * a way that's easy to forget about, so this stays visible the whole time either is active
    * rather than a one-off message that fades. Also owns the container-toggle key listener, since
    * that needs to work anywhere in the world, not just while looking at a Resource Monitor.
    */
    public class ManagementModeIndicator : MonoBehaviour
    {
        private const KeyCode CONTAINER_TOGGLE_KEY = KeyCode.H;

        private GameObject indicatorCanvas;
        private TextMeshProUGUI label;

        private void Awake()
        {
            BuildUi();
        }

        private void BuildUi()
        {
            indicatorCanvas = new GameObject("ResourceMonitorManagementModeCanvas");
            indicatorCanvas.transform.SetParent(transform, false);

            var canvas = indicatorCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // stay on top of the game's own HUD

            indicatorCanvas.AddComponent<CanvasScaler>();

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(indicatorCanvas.transform, false);

            // Legacy UI.Text needs a UnityEngine.Font, and Resources.GetBuiltinResource<Font>
            // ("LegacyRuntime.ttf"/"Arial.ttf") fails to load in Subnautica's stripped build
            // ("The resource ... could not be loaded from the resource file!" in the log), which
            // silently leaves the label with no font and nothing renders. Nautilus ships its own
            // TextMeshPro font asset specifically so mods don't hit this - use TMP instead.
            // FontUtils.Aller_Rg is itself still null this early (EntryPoint.Awake runs before
            // Nautilus finishes loading its font asset bundle), so the real assignment happens
            // lazily in Update() below once it's actually available.
            label = labelObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = 20f;
            label.alignment = TextAlignmentOptions.TopLeft;
            label.color = new Color(1f, 0.85f, 0.2f);
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Overflow;

            var outline = labelObject.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var rect = label.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(20f, -20f);
            rect.sizeDelta = new Vector2(600f, 100f);

            indicatorCanvas.SetActive(false);
        }

        private void Update()
        {
            // Idempotent (checks the invocation list first) - has to be called every frame rather
            // than once in Awake because InGameMenuQuitPatcher clears its handler list after each
            // firing, and this object survives across multiple save loads/quits in the same game
            // process, unlike ResourceMonitorLogic's own use of this same hook.
            Patchers.InGameMenuQuitPatcher.AddEventHandlerIfMissing(DisableManagementModesOnQuit);

            if (label.font == null)
            {
                var font = FontUtils.Aller_Rg;
                if (font != null)
                {
                    label.font = font;
                }
            }

            var itemMode = EntryPoint.SETTINGS.ItemManagementModeEnabled;
            var containerMode = EntryPoint.SETTINGS.ContainerManagementModeEnabled;

            UpdateLabel(itemMode, containerMode);

            if (containerMode && Input.GetKeyDown(CONTAINER_TOGGLE_KEY))
            {
                TryToggleOpenContainer();
            }
        }

        private void UpdateLabel(bool itemMode, bool containerMode)
        {
            if (itemMode == false && containerMode == false)
            {
                indicatorCanvas.SetActive(false);
                return;
            }

            indicatorCanvas.SetActive(true);

            if (itemMode && containerMode)
            {
                label.text = "Resource Monitor - Item + Container management mode ON\n" +
                    "Click an item on a screen to stop tracking it.\n" +
                    $"With a storage container open, press {CONTAINER_TOGGLE_KEY} to toggle tracking it.";
            }
            else if (itemMode)
            {
                label.text = "Resource Monitor - Item management mode ON\n" +
                    "Click an item on a screen to stop tracking it.";
            }
            else
            {
                label.text = "Resource Monitor - Container management mode ON\n" +
                    $"With a storage container open, press {CONTAINER_TOGGLE_KEY} to toggle tracking it.";
            }
        }

        /**
        * Uses whatever storage UI the player currently has open (the same locker/container they
        * just clicked on) rather than raycasting from the camera - a raycast from screen center
        * starts inside the player's own capsule collider and hits that before anything else, which
        * is why the previous look-and-press-H approach never actually found a container.
        */
        private static void TryToggleOpenContainer()
        {
            var usedStorageCount = Inventory.main.GetUsedStorageCount();
            if (usedStorageCount <= 0)
            {
                ErrorMessage.AddMessage("Resource Monitor: open a storage container first.");
                return;
            }

            for (var i = 0; i < usedStorageCount; i++)
            {
                var container = FindOwningStorageContainer(Inventory.main.GetUsedStorage(i));
                if (container != null)
                {
                    ResourceMonitorLogic.ToggleContainerTrackingGlobally(container);
                    return;
                }
            }

            ErrorMessage.AddMessage("Resource Monitor: this isn't a trackable storage container.");
        }

        /**
        * Walking up from ItemsContainer.tr (as the raycast-replacement code originally did) only
        * finds the StorageContainer when it happens to be an ancestor of tr - true for ordinary
        * base lockers, but not for the Cyclops: each locker's tr is its own "LockerNNStorageRoot"
        * under a shared "StorageRoot" sibling branch, with the actual StorageContainer component
        * living elsewhere in the hierarchy entirely. Matching by reference against every
        * StorageContainer's own .container field works regardless of how any given prefab is
        * structured, since that field IS the exact ItemsContainer instance Inventory reports open.
        */
        private static StorageContainer FindOwningStorageContainer(IItemsContainer usedStorage)
        {
            foreach (var sc in FindObjectsOfType<StorageContainer>())
            {
                if (ReferenceEquals(sc.container, usedStorage))
                {
                    return sc;
                }
            }
            return null;
        }

        private static void DisableManagementModesOnQuit()
        {
            if (EntryPoint.SETTINGS.ItemManagementModeEnabled == false && EntryPoint.SETTINGS.ContainerManagementModeEnabled == false)
            {
                return;
            }

            EntryPoint.SETTINGS.ItemManagementModeEnabled = false;
            EntryPoint.SETTINGS.ContainerManagementModeEnabled = false;
            EntryPoint.SaveSettings();
        }
    }
}

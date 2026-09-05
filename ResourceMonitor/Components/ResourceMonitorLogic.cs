using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ResourceMonitor.Components
{
    /**
     * Stores information about a resource. The type, amount and what storage containers contain this.
     */
    public class TrackedResource
    {
        public TechType TechType { get; set; }
        public int Amount { get; set; }
        public HashSet<StorageContainer> Containers { get; set; }
    }

    /**
    * Component that contains all the logic related to the resource monitor component.
    * When set up: We find the base we was placed in. Get all StorageContainer components in that base gameobject children.
    * We then subscribe to the events of items getting added and removed from that StorageContainer.
    * We care about when a new locker is added though as we need to subscribe to its events we use Harmony's Postfix on StorageContainer.Awake() for that.
    * We care about when a locker is removed though so we can unsubscribe from it. Either if its getting deleted or unloaded due to game exit. TO:DO implemenet this.
    */
    public class ResourceMonitorLogic : MonoBehaviour, IConstructable
    {
        public static List<string> DONT_TRACK_GAMEOBJECTS { get; private set; } = new List<string>();
        private static readonly float COOLDOWN_TIME_BETWEEN_PICKING_UP_LAST_ITEM_TYPE = 0.7f;

        public Dictionary<TechType, TrackedResource> TrackedResources { private set; get; } = new Dictionary<TechType, TrackedResource>();
        public bool IsBeingDeleted { get; private set; } = false;
        // Plain field (not an auto-property) because Nautilus clones the cached prefab via
        // Object.Instantiate() when the player actually places one; Unity's serializer only
        // copies public fields across that clone, not auto-property backing fields, so this
        // would silently reset to false on every placed instance if it stayed a property.
        public bool IsLargeMonitor;
        private ResourceMonitorDisplay rmd;
        private GameObject seaBase;
        private float timerTillNextPickup = .0f;
        private bool isEnabled = false;
        private bool runStartUpOnEnable = false;

        private IEnumerator Startup()
        {
            if (IsBeingDeleted == true) yield break;
            yield return new WaitForEndOfFrame();
            if (IsBeingDeleted == true) yield break;

            seaBase = gameObject?.transform?.parent?.gameObject;
            if (seaBase == null)
            {
                ErrorMessage.AddMessage("[ResourceMonitor] ERROR: Can not work out what base it was placed inside.");
                System.Console.WriteLine("[ResourceMonitor] ERROR: Can not work out what base it was placed inside.");
                yield break;
            }

            TrackExistingStorageContainers();
            Patchers.StorageContainerAwakePatcher.RegisterForNewStorageContainerUpdates(this);
            Patchers.InGameMenuQuitPatcher.AddEventHandlerIfMissing(CleanUp);
            TurnDisplayOn();
        }

        public void OnEnable()
        {
            if (runStartUpOnEnable)
            {
                StartCoroutine(Startup());
                runStartUpOnEnable = false;
            }
        }

        public void OnConstructedChanged(bool constructed)
        {
            if (constructed)
            {
                if (isEnabled == false)
                {
                    isEnabled = true;

                    // Big Little Update Subnautica has caused this to be called when isActiveAndEnabled is false when loading a saved game.
                    if (isActiveAndEnabled)
                    {
                        StartCoroutine(Startup());
                    }
                    else
                    {
                        runStartUpOnEnable = true;
                    }
                }
                else
                {
                    TurnDisplayOn();
                }
            }
            else
            {
                if (isEnabled)
                {
                    TurnDisplayOff();
                }
            }
        }

        public bool CanDeconstruct(out string reason)
        {
            reason = null;
            return true;
        }

        public bool IsDeconstructionObstacle()
        {
            return false;
        }

        private void TurnDisplayOn()
        {
            if (IsBeingDeleted == true) return;

            if (rmd != null)
            {
                TurnDisplayOff();
            }

            rmd = gameObject.AddComponent<ResourceMonitorDisplay>();
            rmd.Setup(this);
        }

        private void TurnDisplayOff()
        {
            if (IsBeingDeleted == true) return;

            if (rmd != null)
            {
                rmd.TurnDisplayOff();
                Destroy(rmd);
                rmd = null;
            }
        }

        private void TrackExistingStorageContainers()
        {
            StorageContainer[] containers = seaBase.GetComponentsInChildren<StorageContainer>();
            foreach (StorageContainer sc in containers)
            {
                TrackStorageContainer(sc);
            }
        }

        public void AlertNewStorageContainerPlaced(StorageContainer sc)
        {
            StartCoroutine("TrackNewStorageContainerCoroutine", sc);
        }

        public IEnumerator TrackNewStorageContainerCoroutine(StorageContainer sc)
        {
            // We yield to the end of the frame as we need the parent/children tree to update.
            yield return new WaitForEndOfFrame();
            GameObject newSeaBase = sc?.gameObject?.transform?.parent?.gameObject;
            if (newSeaBase != null && newSeaBase == seaBase)
            {
                TrackStorageContainer(sc);
            }

            StopCoroutine("TrackNewStorageContainerCoroutine");
        }

        private void TrackStorageContainer(StorageContainer sc)
        {
            if (sc == null || sc.container == null)
            {
                return;
            }

#if SUBNAUTICA
            // Plant pots/wall planters use a StorageContainer purely to hold the seeds/samples
            // growing in them - that's not a resource stash and shouldn't be tracked.
            if (sc.GetComponentInParent<Planter>() != null)
            {
                return;
            }
#endif

            foreach (string notTrackedObject in DONT_TRACK_GAMEOBJECTS)
            {
                if (sc.gameObject.name.ToLower().Contains(notTrackedObject))
                {
                    return;
                }
            }

            foreach (var item in sc.container.GetItemTypes())
            {
                AddItemsToTracker(sc, item, sc.container.GetCount(item));
            }

            sc.container.onAddItem += (item) => AddItemsToTracker(sc, item.item.GetTechType());
            sc.container.onRemoveItem += (item) => RemoveItemsFromTracker(sc, item.item.GetTechType());
        }

        private void AddItemsToTracker(StorageContainer sc, TechType item, int amountToAdd = 1)
        {
            if (IsBeingDeleted == true) return;

            if (DONT_TRACK_GAMEOBJECTS.Contains(item.AsString().ToLower()))
            {
                return;
            }

            if (TrackedResources.ContainsKey(item))
            {
                TrackedResources[item].Amount = TrackedResources[item].Amount + amountToAdd;
                TrackedResources[item].Containers.Add(sc);
            }
            else
            {
                TrackedResources.Add(item, new TrackedResource()
                {
                    TechType = item,
                    Amount = amountToAdd,
                    Containers = new HashSet<StorageContainer>()
                    {
                        sc
                    }
                });
            }

            rmd?.ItemModified(item, TrackedResources[item].Amount);
        }

        /**
        * Returns the tracked resources ordered the way the screen displays them, per the tier
        * order in ResourceCategoryRanker. Within raw/basic/electronics/advanced materials, items
        * follow the hand-audited PRIORITY order (a modded item with no explicit priority sorts by
        * recipe usage count, then alphabetically); every other tier is purely alphabetical.
        */
        public List<TrackedResource> GetSortedTrackedResources()
        {
            var sorted = new List<TrackedResource>(TrackedResources.Values);
            sorted.Sort((a, b) =>
            {
                var rankA = ResourceCategoryRanker.GetRank(a.TechType);
                var rankB = ResourceCategoryRanker.GetRank(b.TechType);
                if (rankA != rankB) return rankA.CompareTo(rankB);

                if (rankA <= ResourceCategoryRanker.ADVANCED_MATERIALS)
                {
                    var priorityA = ResourceCategoryRanker.GetPriority(a.TechType);
                    var priorityB = ResourceCategoryRanker.GetPriority(b.TechType);
                    if (priorityA != priorityB) return priorityA.CompareTo(priorityB);

                    var usageA = ResourceCategoryRanker.GetRecipeUsageCount(a.TechType);
                    var usageB = ResourceCategoryRanker.GetRecipeUsageCount(b.TechType);
                    if (usageA != usageB) return usageB.CompareTo(usageA);
                }

                return string.Compare(Language.main.Get(a.TechType), Language.main.Get(b.TechType));
            });
            return sorted;
        }

        private void RemoveItemsFromTracker(StorageContainer sc, TechType item, int amountToRemove = 1)
        {
            if (IsBeingDeleted == true) return;

            if (TrackedResources.ContainsKey(item))
            {
                TrackedResource trackedResource = TrackedResources[item];
                int newAmount = trackedResource.Amount - amountToRemove;
                trackedResource.Amount = newAmount;

                if (newAmount <= 0)
                {
                    // Each material tier has its own Mod Options toggle for whether a depleted
                    // item stays listed at x0 instead of disappearing once the last one is
                    // used/removed.
                    if (ShouldRetainAtZero(item))
                    {
                        newAmount = 0;
                        trackedResource.Amount = 0;
                        trackedResource.Containers.Clear();
                    }
                    else
                    {
                        TrackedResources.Remove(item);
                    }
                }
                else
                {
                    int amountLeftInContainer = sc.container.GetCount(item);
                    if (amountLeftInContainer <= 0)
                    {
                        trackedResource.Containers.Remove(sc);
                    }
                }

                rmd?.ItemModified(item, newAmount);
            }
        }

        private static bool ShouldRetainAtZero(TechType item)
        {
            switch (ResourceCategoryRanker.GetRank(item))
            {
                case ResourceCategoryRanker.RAW_MATERIALS:
                    return EntryPoint.SETTINGS.ShowZeroAmountRawMaterials;
                case ResourceCategoryRanker.BASIC_MATERIALS:
                    return EntryPoint.SETTINGS.ShowZeroAmountBasicMaterials;
                case ResourceCategoryRanker.ADVANCED_MATERIALS:
                    return EntryPoint.SETTINGS.ShowZeroAmountAdvancedMaterials;
                case ResourceCategoryRanker.ELECTRONICS:
                    return EntryPoint.SETTINGS.ShowZeroAmountElectronics;
                default:
                    return false;
            }
        }

        public void Update()
        {
            if (timerTillNextPickup > 0f)
            {
                timerTillNextPickup -= Time.deltaTime;
            }
        }

        public void AttemptToTakeItem(TechType item)
        {
            if (IsBeingDeleted == true) return;

            if (timerTillNextPickup > 0f)
            {
                return;
            }

            if (TrackedResources.ContainsKey(item))
            {
                TrackedResource trackedResource = TrackedResources[item];
                int beforeRemoveAmount = trackedResource.Amount;
                if (trackedResource.Containers.Count >= 1)
                {
                    StorageContainer sc = trackedResource.Containers.ElementAt(0);
                    if (sc.container.Contains(item))
                    {                    
                        Pickupable pickup = sc.container.RemoveItem(item);
                        if (pickup != null)
                        {
                            if (Inventory.main.Pickup(pickup))
                            {
                                CrafterLogic.NotifyCraftEnd(Player.main.gameObject, item);
                                if (beforeRemoveAmount == 1)
                                {
                                    timerTillNextPickup = COOLDOWN_TIME_BETWEEN_PICKING_UP_LAST_ITEM_TYPE;
                                }
                            }
                            else
                            {
                                // If it fails to get added to the inventory lets add it back into the storage container.
                                sc.container.AddItem(pickup);
                            }
                        }
                    }
                }   
            }
        }

        public void OnApplicationQuit()
        {
            CleanUp();
        }

        public void CleanUp()
        {
            StopAllCoroutines();
            IsBeingDeleted = true;
            if (rmd != null)
            {
                rmd.StopAllCoroutines();
                Destroy(rmd.CanvasGameObject);
                Destroy(rmd);
            }
        }
    }
}
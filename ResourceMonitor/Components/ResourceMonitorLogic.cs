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
        private readonly HashSet<StorageContainer> subscribedContainers = new HashSet<StorageContainer>();
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
            if (IsExcludedContainer(sc))
            {
                return;
            }

            foreach (var item in sc.container.GetItemTypes())
            {
                AddItemsToTracker(sc, item, sc.container.GetCount(item));
            }

            // Guards against subscribing twice if this container gets excluded then re-included
            // via container management mode - onAddItem/onRemoveItem can't be unsubscribed with
            // an anonymous lambda, so this just makes sure we never attach a second copy.
            if (subscribedContainers.Add(sc))
            {
                sc.container.onAddItem += (item) => AddItemsToTracker(sc, item.item.GetTechType());
                sc.container.onRemoveItem += (item) => RemoveItemsFromTracker(sc, item.item.GetTechType());
            }
        }

        /**
        * Checked both when a container is first found (deciding whether to subscribe at all) and
        * on every subsequent onAddItem/onRemoveItem event (so toggling container management mode
        * mid-game takes effect immediately without needing to unsubscribe/resubscribe events).
        */
        private static bool IsExcludedContainer(StorageContainer sc)
        {
            if (sc == null || sc.container == null)
            {
                return true;
            }

#if SUBNAUTICA
            // Plant pots/wall planters use a StorageContainer purely to hold the seeds/samples
            // growing in them - that's not a resource stash and shouldn't be tracked.
            if (sc.GetComponentInParent<Planter>() != null)
            {
                return true;
            }
#endif

            if (EntryPoint.SETTINGS.ExcludedContainerIds.Contains(GetStableContainerId(sc)))
            {
                return true;
            }

            foreach (string notTrackedObject in DONT_TRACK_GAMEOBJECTS)
            {
                if (sc.gameObject.name.ToLower().Contains(notTrackedObject))
                {
                    return true;
                }
            }

            return false;
        }

        /**
        * SceneObjectIdentifier.Id survives save/load and is unique to this exact instance, so
        * excluding a container by it only ever affects the one the player pointed at. Below Zero
        * doesn't expose an equivalent we've been able to verify, so it falls back to the
        * GameObject's name there - like DontTrackList.txt, that could affect every container
        * sharing that name, not just the selected one.
        */
        private static string GetStableContainerId(StorageContainer sc)
        {
#if SUBNAUTICA
            var identifier = sc.GetComponent<SceneObjectIdentifier>() ?? sc.GetComponentInParent<SceneObjectIdentifier>();
            if (identifier != null)
            {
                return identifier.Id;
            }
#endif
            return sc.gameObject.name;
        }

        /**
        * Called after Mod Options > Resource Monitor > Clear hidden items list to bring back
        * previously-hidden items immediately, without re-subscribing onAddItem/onRemoveItem
        * (TrackStorageContainer already did that) or double-counting items that are still
        * tracked - only items matching a just-cleared key, not yet back in TrackedResources, get
        * (re-)added.
        */
        public void RetrackItems(HashSet<string> keysToRetrack)
        {
            if (seaBase == null) return;

            foreach (var sc in seaBase.GetComponentsInChildren<StorageContainer>())
            {
                if (IsExcludedContainer(sc))
                {
                    continue;
                }

                foreach (var item in sc.container.GetItemTypes())
                {
                    if (TrackedResources.ContainsKey(item) == false && keysToRetrack.Contains(item.AsString().ToLower()))
                    {
                        AddItemsToTracker(sc, item, sc.container.GetCount(item));
                    }
                }
            }
        }

        private void AddItemsToTracker(StorageContainer sc, TechType item, int amountToAdd = 1)
        {
            if (IsBeingDeleted == true) return;

            if (IsExcludedContainer(sc))
            {
                return;
            }

            if (DONT_TRACK_GAMEOBJECTS.Contains(item.AsString().ToLower()))
            {
                return;
            }

            if (EntryPoint.SETTINGS.HiddenItemTypes.Contains(item.AsString().ToLower()))
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

        public const string SORT_ORDER_CATEGORY = "Category";
        public const string SORT_ORDER_ALPHABETICAL = "Alphabetical";
        public const string SORT_ORDER_QUANTITY = "Quantity";

        /**
        * Returns the tracked resources ordered per Mod Options > Resource Monitor > Sort order.
        * "Category" (default) follows the tier order in ResourceCategoryRanker - within
        * raw/basic/electronics/advanced materials, items follow the hand-audited PRIORITY order
        * (a modded item with no explicit priority sorts by recipe usage count, then
        * alphabetically); every other tier is purely alphabetical. "Alphabetical" and "Quantity"
        * ignore tiers entirely.
        */
        public List<TrackedResource> GetSortedTrackedResources()
        {
            var sorted = new List<TrackedResource>(TrackedResources.Values);
            switch (EntryPoint.SETTINGS.SortOrder)
            {
                case SORT_ORDER_ALPHABETICAL:
                    sorted.Sort((a, b) => string.Compare(GetSafeDisplayName(a.TechType), GetSafeDisplayName(b.TechType)));
                    break;
                case SORT_ORDER_QUANTITY:
                    sorted.Sort((a, b) =>
                    {
                        if (a.Amount != b.Amount) return b.Amount.CompareTo(a.Amount);
                        return string.Compare(GetSafeDisplayName(a.TechType), GetSafeDisplayName(b.TechType));
                    });
                    break;
                default:
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

                        return string.Compare(GetSafeDisplayName(a.TechType), GetSafeDisplayName(b.TechType));
                    });
                    break;
            }
            return sorted;
        }

        public static string GetSafeDisplayName(TechType type)
        {
            try
            {
                return Language.main.Get(type);
            }
            catch
            {
                return type.AsString();
            }
        }

        private void RemoveItemsFromTracker(StorageContainer sc, TechType item, int amountToRemove = 1)
        {
            if (IsBeingDeleted == true) return;

            // Guards against a container that gets excluded mid-game (its onRemoveItem
            // subscription is never actually torn down - see IsExcludedContainer). The one-time
            // purge when a container is newly excluded goes through PurgeContainerContribution
            // instead, which doesn't call this method at all.
            if (IsExcludedContainer(sc))
            {
                return;
            }

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

        /**
        * Called by ManagementModeIndicator (the container-toggle key) to flip whether a specific
        * storage container contributes to tracking. Static because the exclusion list is global -
        * this updates it once, then asks every active ResourceMonitorLogic to sync (each one
        * checks whether the container is even part of its own seaBase before doing anything).
        */
        public static void ToggleContainerTrackingGlobally(StorageContainer sc)
        {
            if (sc == null || sc.container == null) return;

            var id = GetStableContainerId(sc);
            var nowExcluded = EntryPoint.SETTINGS.ExcludedContainerIds.Contains(id) == false;

            if (nowExcluded)
            {
                EntryPoint.SETTINGS.ExcludedContainerIds.Add(id);
            }
            else
            {
                EntryPoint.SETTINGS.ExcludedContainerIds.Remove(id);
            }
            EntryPoint.SaveSettings();

            ResourceMonitorDisplay.SyncContainerExclusionForAll(sc, nowExcluded);

            ErrorMessage.AddMessage(nowExcluded
                ? "Resource Monitor: container tracking disabled."
                : "Resource Monitor: container tracking re-enabled.");
        }

        /**
        * Reacts to a container being excluded/re-included, but only if it's actually part of this
        * instance's own base - ToggleContainerTrackingGlobally broadcasts to every active
        * ResourceMonitorLogic, most of which have nothing to do with the container in question.
        */
        public void SyncContainerExclusion(StorageContainer sc, bool excluded)
        {
            if (seaBase == null || sc.transform.IsChildOf(seaBase.transform) == false)
            {
                return;
            }

            if (excluded)
            {
                PurgeContainerContribution(sc);
            }
            else if (subscribedContainers.Contains(sc))
            {
                foreach (var item in sc.container.GetItemTypes().ToList())
                {
                    AddItemsToTracker(sc, item, sc.container.GetCount(item));
                }
            }
            else
            {
                TrackStorageContainer(sc);
            }
        }

        /**
        * Removes exactly this container's contribution to each item it holds, unlike
        * RemoveItemsFromTracker (which infers "still in the container?" from the container's
        * current contents - correct for a genuine removal, wrong here since the item is still
        * physically there, it just shouldn't count anymore).
        */
        private void PurgeContainerContribution(StorageContainer sc)
        {
            foreach (var item in sc.container.GetItemTypes().ToList())
            {
                if (TrackedResources.TryGetValue(item, out var trackedResource) == false) continue;

                var newAmount = Mathf.Max(trackedResource.Amount - sc.container.GetCount(item), 0);
                trackedResource.Containers.Remove(sc);

                if (newAmount <= 0 && ShouldRetainAtZero(item) == false)
                {
                    TrackedResources.Remove(item);
                }
                else
                {
                    trackedResource.Amount = newAmount;
                }

                rmd?.ItemModified(item, newAmount);
            }
        }

        /**
        * Clicking an item while Item management mode is on calls this to permanently hide it from
        * every Resource Monitor (a global preference, same as DontTrackList.txt). Unlike removing
        * a container, this only stops new tracking going forward - it doesn't retroactively
        * re-scan other bases, so a cleared hide list only brings items back as their containers
        * are next touched (or Clear hidden items list re-scans this base immediately).
        */
        public void HideItemType(TechType item)
        {
            var key = item.AsString().ToLower();
            if (EntryPoint.SETTINGS.HiddenItemTypes.Contains(key) == false)
            {
                EntryPoint.SETTINGS.HiddenItemTypes.Add(key);
                EntryPoint.SaveSettings();
            }

            if (TrackedResources.Remove(item))
            {
                rmd?.ItemModified(item, 0);
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
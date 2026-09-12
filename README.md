SN Nexus: https://www.nexusmods.com/subnautica/mods/4165
BZ Coming Soon.

# Resource Monitor Redux
A mod for Subnautica and Subnautica: Below Zero. Adds two buildable interior screens that automatically track everything held in your base's storage containers, so you can see what you have at a glance instead of opening locker after locker.

![](https://staticdelivery.nexusmods.com/mods/1155/images/4165/4165-1788808631-1033801111.jpg)

#### New Items
* **Resource Monitor (Large)** - a large wall screen that takes up the majority of a wall panel. Requires more resources to build.
* **Resource Monitor (Small)** - a small wall screen that takes up a lot less space. Requires fewer resources.

Both work the same way: place one inside a base or vehicle (including the Cyclops), and it scans every storage container built inside that same structure, then keeps itself updated live as items are added or removed, including containers built afterward.

#### Features
* **Take items directly** - click a tracked item on the screen to pull one straight from storage into your inventory (can be turned off in Mod Options).
* **Sort order** - Category (default, hand-tuned by material tier), Alphabetical, or Quantity.
* **Configurable items per page** - Adjust the number of items that appear on the large screen and the small screen.
* **Compact display** - hide item name labels to fit more on screen.
* **Idle screensaver** - the screen switches to an idle animation after a period of inactivity, with configurable timeout and interaction range.
* **Item management mode** - a Mod Options toggle that changes what clicking an item does: instead of taking it, it stops tracking that item type everywhere. A persistent on-screen reminder shows while it's active, and a "Clear hidden items list" button in Mod Options brings everything back.
* **Container management mode** - a Mod Options toggle plus a keybind (H): open a storage container and press H to toggle whether that specific container contributes to tracking, without affecting any other container of the same type.
* **`DontTrackList.txt`** - an optional, self-documenting text file for permanently excluding specific containers (by name) or item types (by TechType) across every base, independent of the in-game toggles above.
* Nearly everything is configurable from the game's own Mod Options menu - items per page, idle timing, interaction distances, per-tier "keep depleted items listed at x0" behavior, and all of the above.

#### Libraries
* BepInEx 5
* Nautilus
* Harmony (bundled with BepInEx)

#### Building
1. Install the [.NET SDK](https://dotnet.microsoft.com/download) and [BepInEx 5.4.21](https://www.nexusmods.com/subnautica/mods/1108) into your Subnautica (and/or Below Zero) install.
2. Install [Nautilus](https://www.nexusmods.com/subnautica/mods/1262) into `BepInEx/plugins`.
3. Build the `SN` configuration for Subnautica or the `BZ` configuration for Below Zero, e.g. `dotnet build -c SN`.
4. Copy the built `ResourceMonitor.dll`, along with an `Assets` folder containing the `resources` asset bundle and the icon PNGs from [`Resources`](Resources), into `BepInEx/plugins/ResourceMonitor/`.

Thank you to https://github.com/RandyKnapp. The code used to create a canvas in 3D world space on top of the model is his.

Nexus: https://www.nexusmods.com/subnautica/mods/166

# Subnautica Resource Monitor Game Modification
A mod for the game Subnautica. Adds two new interior modules to the game that when placed will keep track of all items held in storage containers within the base.

![](https://i.imgur.com/AHkDRYk.jpg)

#### New Items:
Two new items:
* Resource Monitor Screen Large - A large screen that takes up the majority of the wall. Requires more resources to build.
* Resource Monitor Screen Small - A small screen that takes up alot less space on the wall.

#### Libraries
* BepInEx 5
* Nautilus (formerly SMLHelper)
* Harmony (bundled with BepInEx)

#### Building
1. Install the [.NET SDK](https://dotnet.microsoft.com/download) and [BepInEx 5.4.21](https://www.nexusmods.com/subnautica/mods/1108) into your Subnautica (and/or Below Zero) install.
2. Install [Nautilus](https://www.nexusmods.com/subnautica/mods/1262) into `BepInEx/plugins`.
3. Build the `SN` configuration for Subnautica or the `BZ` configuration for Below Zero, e.g. `dotnet build -c SN`.
4. Copy the built `ResourceMonitor.dll`, along with an `Assets` folder containing the `resources` asset bundle and the icon PNGs from [`Resources`](Resources), into `BepInEx/plugins/ResourceMonitor/`.

Thank you to https://github.com/RandyKnapp. The code used to create a canvas in 3D world space on top of the model is his.

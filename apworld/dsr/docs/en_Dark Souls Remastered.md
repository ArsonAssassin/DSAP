# Dark Souls Remastered

## What do I need to do to randomize DSR?

See full instructions on [the setup page](./setup_en.md).

## What does randomization do to this game?

1. Every loose item on the ground, and potentially fog walls, are "locations" or "checks".
   Some guaranteed drops are also locations, in addition to the White Sign Soapstone location from the first Solaire encounter.
   All items in those locations will be shuffled into the randomized multiworld Item Pool.
   This means they can be found elsewhere, and that the items at those locations will themselves be replaced by other items in the Item Pool.

2. All keys and progression items (e.g. Lordvessel) will be forced into the Item Pool, unless they drop or are shoppable from non-randomized locations.

3. Undead Asylum is only partially randomized. This is intended behavior so as to not put the player into BK mode immediately.
   The keys and Estus flask are not randomized, but class equipment can be, depending on your settings.
   The only randomized ground item in the AP item pool is the one by the stairs to the exit.

4. By default, the starting equipment for each class is randomized. This can be
   disabled by setting "Randomize Starting Loadout" to false.

5. By setting the "Upgraded Weapons Percentage", you can randomize whether the
   weapons you find will be upgraded and/or infused.

6. You can exclude some locations from being randomized at all using specific options.

There are also options that can make playing the game more convenient or
bring a new experience, like removing start requiements for spells or weapons.
Check the options for more.

## How is the item pool constructed?
* Item Pool is currently constructed as follows:
    1. First all items in locations which were randomized are added to the pool.
    2. Then, key items and embers will replace any filler items. This includes any "Fog Wall Keys", if either `fogwall_sanity` option is on. This includes the three living Firekeepers' Souls.
    3. Then, Guaranteed items are added to the pool.
    4. If there are any Filler or Junk items left over, they are replaced with Souls of a Proud Knight (2000 souls each).
* If locations are excluded and excluded_locations_behavior is set to "do_not_randomize", then the items in those locations will not be added to the pool, and those locations will have their vanilla items. Even with `fogwall_sanity` on, fog walls excluded in this manner this will provide no item, as fog walls do not provide items in the base game.

## What's the goal?

Your goal is to find the Lordvessel, two "Lord Soul" items, and two Bequeathed Lord Soul Shards items randomized into the
multiworld, and defeat the boss in the Kiln of the First Flame.

## Isn't the game extremely open from the start? What is fogwall sanity?

In order to prevent ~60% of the game being open and available immediately, this mod introduces "Fogwall Sanity", which is on by default.    
With fogwall sanity, fogwalls in DSR will be locked, and will require an item from the Item Pool to unlock them.  
Passing through them will also count as a location or "check". 
* This includes the Fog Wall near the beginning of the Upper Undead Burg. If it's blocked - you'll have to go somewhere else! (e.g. check the skeleton courtyard, and part of Upper New Londo Ruins)
* This does not include the fogwall in the Northern Undead Asylum.
* This `fogwall_sanity` option can be disabled, but doing so is not recommended. Without this option, about 60% of the locations in your game would be consdiered in logic immediately - this is also considered a big "Sphere 1". With a big Sphere 1, you might need to get to the last check in the immediately available 60% of your game, in order to find an item that unlocks your friend who is stuck at 10% of their game. Because of how large Dark Souls is, it could take a long time to do that many checks - and make your friends have to wait on you, for quite a while.
* There is an additional `boss_fogwall_sanity` option that can be turned on, which makes most boss' arena fogs be similarly locked.
* The `Catacombs` and `Lower New Londo Ruins` fogwalls can be bypassed by basic platforming & elevator usage repsecitvely. As a result, they do not logically block access to their other sides.

## Do I have to check every item in every area?

By default, you will have to check every non-missable item.
This does not include failable quests or other missable items.

## What if I don't want to do the whole game?

If you want a shorter DSR randomizer experience, you can exclude certain locations
from containing progression items. By default, the items from those regions will
still be included in the randomization pool, but none of them will be mandatory.
However, you can set the `Excluded Location Behavior` option to `do_not_randomize`
in order to have their items not in the item pool and have them not impacted by AP randomization at all.

Besides excluding individual locations, you can exclude whole regions via the [Location Groups](#Location-Groups) listed below.

For example, the following configuration will not require you to do the DLC, The Great Hollow, or Ash Lake:

```yaml
Dark Souls Remastered:
  exclude_locations:
    # Exclude DLC and out-of-the-way regions
    - All DLC regions
    - The Great Hollow
    - Ash Lake
```

## Artificial Logic Without Fogwall Locking
* If you disable the options for fogwall or boss fogwall locking, some artificial "logic" is introduced to limit the number of items that are "in logic" extremely early. This does not affect actual access, but affects what the randomizer considers "logically possible" to access at any point. Such rules are listed below:
* Access to the Great Hollow is behind Blighttown access + Lordvessel item.
* Access to New Londo Ruins Door to the Seal + Lower New Londo Ruins from Upper New Londo Ruins requires access to being able to defeat Ornstein and Smough (in addition to having the Key to the Seal - the default rule).

## How do I read the location names?

Locations are generally in this format:  
region code: item name - (optional) location

Region codes:  
UA - Undead Asylum  
UA2 - Undead Asylum 2nd visit  
FS - Firelink Shrine  
UB - Undead Burg  
UP - Undead Parish  
DE - Depths  
BT - Blighttown  
VotD - Valley of Drakes  
DB - Darkroot Basin  
DG - Darkroot Garden  
GH - The Great Hollow  
ASH - Ash Lake  
SF - Sen's Fortress  
AL - Anor Londo  
PW - The Painted World of Ariamis  
NL - New Londo  
TA - The Abyss  
DA - Duke's Archives  
CC - Crystal Cave  
DR - Demon Ruins  
LI - Lost Izalith  
TC - The Catacombs  
TotG - Tomb of the Giants  
FA - Firelink Altar  
KoFF - Kiln of First Flame  
SG - Sanctuary Garden  
OS - Oolacile Sanctuary  
RW - Royal Wood  
OT - Oolacile Township  
CotA - Chasm of the Abyss  


## Location Groups
All Boss Fog Walls  
All DLC regions  
All Doors  
All Fog Walls  
All Item Lots  
Anor Londo  
Ash Lake  
Chasm of the Abyss  
Crystal Cave  
Darkroot Basin  
Darkroot Garden  
Demon Ruins  
Depths  
Firelink Shrine  
Kiln of the First Flame  
Lost Izalith  
Lower Blighttown  
Lower New Londo Ruins  
Lower Undead Burg  
Northern Undead Asylum  
Northern Undead Asylum Second Visit  
Oolacile Sanctuary  
Oolacile Township  
Painted World of Ariamis  
Royal Wood  
Sanctuary Garden  
Sen's Fortress  
The Abyss  
The Catacombs  
The Duke's Archives  
The Great Hollow  
Tomb of the Giants  
Undead Asylum Cell  
Undead Parish  
Upper Blighttown Depths Side  
Upper Blighttown VotD Side  
Upper New Londo Ruins  
Upper Undead Burg  
Valley of the Drakes  
Watchtower Basement  

## Item Groups
Ammunition  
Armor  
Boss Fog Wall Keys  
Boss Souls  
Carvings  
Catalysts  
Consumables  
Covenant Items  
Embers  
Fire Keeper Souls  
Fog Wall Keys  
Junk  
Key items  
Lord Souls  
Melee Weapons  
Multiplayer Items  
Progression Items  
Ranged Weapons  
Rings  
Shields  
Souls  
Spell Tools  
Spells  
Talismans  
Traps  
Upgrade Materials  
Weapons  


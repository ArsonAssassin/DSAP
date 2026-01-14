import typing
from BaseClasses import Location, Region
from .Locations import location_tables, location_skip_categories, location_dictionary
from .Items import item_dictionary, DSRItemCategory, DSRWeaponType, _all_items_base

multiplayer_items = [
    "Eye of Death",
    "Cracked Red Eye Orb",
    "Indictment",
    "White Sign Soapstone",
    "Red Sign Soapstone",
    "Red Eye Orb",
    "Black Separation Crystal",
    "Orange Guidance Soapstone",
    "Book of the Guilty",
    "Servant Roster",
    "Blue Eye Orb",
    "Dragon Eye",
    "Black Eye Orb",
    "Purple Coward's Crystal",
    "Dried Finger",
    "Cat Covenant Ring",
    "Darkmoon Blade Covenant Ring",
]

covenant_items = [
    "Sunlight Medal",
    "Cat Covenant Ring",
    "Souvenir of Reprisal",
]

progression_items = [
    "Annex Key",
    "Archive Prison Extra Key",
    "Archive Tower Cell Key",
    "Archive Tower Giant Cell Key",
    "Archive Tower Giant Door Key",
    "Bequeathed Lord Soul Shard (Four Kings)",
    "Bequeathed Lord Soul Shard (Seath)",
    "Big Pilgrim's Key",
    "Blighttown Key",
    "Broken Pendant",
    "Cage Key",
    "Covenant of Artorias",
    "Crest Key",
    "Crest of Artorias",
    "Dungeon Cell Key",
    "Key to Depths",
    "Key to New Londo Ruins",
    "Lord Soul (Bed of Chaos)",
    "Lord Soul (Nito)",
    "Lordvessel",
    "Peculiar Doll",
    "Residence Key",
    "Sewer Chamber Key",
    "Skull Lantern",
    "Undead Asylum F2 East Key",
    "Undead Asylum F2 West Key",
    "Watchtower Basement Key",
]

item_name_groups = {
    "Key items"         : [item.name for item in item_dictionary.values() if item.category in [DSRItemCategory.KEY_ITEM]] + ["Covenant of Artorias","Orange Charred Ring", "Skull Lantern"],
    "Fog Wall Keys"     : [item.name for item in item_dictionary.values() if item.category in [DSRItemCategory.FOGWALL]],
    "Boss Fog Wall Keys": [item.name for item in item_dictionary.values() if item.category in [DSRItemCategory.BOSSFOGWALL]],
    "Consumables"       : [item.name for item in item_dictionary.values() if item.category in [DSRItemCategory.CONSUMABLE] and "soul" not in item.name.lower() and "fire keeper" not in item.name.lower()],
    "Souls"             : [item.name for item in item_dictionary.values() if item.category in [DSRItemCategory.CONSUMABLE] and "soul" in item.name.lower() and "fire keeper" not in item.name.lower()],
    "Rings"             : [item.name for item in item_dictionary.values() if item.category in [DSRItemCategory.RING]],
    "Upgrade Materials" : [item.name for item in item_dictionary.values() if item.category in [DSRItemCategory.UPGRADE_MATERIAL]],
    "Spells"            : [item.name for item in item_dictionary.values() if item.category in [DSRItemCategory.SPELL]],
    "Armor"             : [item.name for item in item_dictionary.values() if item.category in [DSRItemCategory.ARMOR]],
    "Weapons"           : [item.name for item in item_dictionary.values() if item.category in [DSRItemCategory.WEAPON]],
    "Shields"           : [item.name for item in item_dictionary.values() if item.category in [DSRItemCategory.SHIELD]],
    "Traps"             : [item.name for item in item_dictionary.values() if item.category in [DSRItemCategory.TRAP]],
    "Boss Souls"        : [item.name for item in item_dictionary.values() if item.category in [DSRItemCategory.BOSS_SOUL]],
    "Embers"            : [item.name for item in item_dictionary.values() if item.category in [DSRItemCategory.EMBER]],
    # Weapon types
    "Ammunition"        : [item[0] for item in _all_items_base if item[2] == DSRItemCategory.WEAPON and item[3] == DSRWeaponType.RangedAmmunition],
    "Spell Tools"       : [item[0] for item in _all_items_base if item[2] == DSRItemCategory.WEAPON and item[3] == DSRWeaponType.SpellTool],
    "Melee Weapons"     : [item[0] for item in _all_items_base if item[2] == DSRItemCategory.WEAPON and item[3] == DSRWeaponType.Melee],
    "Ranged Weapons"    : [item[0] for item in _all_items_base if item[2] == DSRItemCategory.WEAPON and item[3] == DSRWeaponType.Ranged],
    # Spell Tool types
    "Catalysts"         : [item for item in item_dictionary.keys() if "catalyst" in item.lower()],
    "Talismans"         : [item for item in item_dictionary.keys() if "talisman" in item.lower() and "lloyd" not in item.lower()],
    # Useful items
    "Progression Items" : [item for item in progression_items],
    "Lord Souls"        : [item for item in item_dictionary.keys() if "lord soul" in item.lower()],
    "Fire Keeper Souls" : [item for item in item_dictionary.keys() if "fire keeper" in item.lower()],
    # Mostly useless items
    "Carvings"          : [item for item in item_dictionary.keys() if "carving" in item.lower()],
    "Multiplayer Items" : [item for item in multiplayer_items],
    "Covenant Items"    : [item for item in covenant_items],
    "Junk"              : [item for item in item_dictionary.keys() if "carving" in item.lower()] + [item for item in multiplayer_items] + [item for item in covenant_items] + ["Pendant"]
}

location_name_groups = {
    "All Doors": set(),
    "All Item Lots": set(),
    "All DLC regions": set(),
    "All Fog Walls": set(),
    "All Boss Fog Walls": set()
}

category_to_loc_name_map = {
    "DOOR": "All Doors",
    "ITEM_LOT": "All Item Lots",
    "FOG_WALL": "All Fog Walls",
    "BOSS_FOG_WALL": "All Boss Fog Walls"
}

# regions to add to "All DLC regions" group
dlc_regions = [
    "Sanctuary Garden",
    "Oolacile Sanctuary", 
    "Royal Wood", 
    "Royal Wood - After Hawkeye Gough",
    "Oolacile Township", 
    "Oolacile Township - Behind Light-Dispelled Walls",
    "Oolacile Township - After Crest Key",
    "Chasm of the Abyss", 
]

# Map door+shortcut regions to their "parent" region
region_parents = {
    "Undead Asylum Cell Door"   : "Undead Asylum Cell",
    "Undead Burg Basement Door" : "Upper Undead Burg",
    "Depths to Blighttown Door" : "Depths",
    "Door between Upper New Londo and Valley of the Drakes" : "Upper New Londo Ruins",
    "New Londo Ruins Door to the Seal" : "Upper New Londo Ruins",
    "Demon Ruins Shortcut" : "Demon Ruins",
}

## Add all locations to their region, DLC, and category groups
for region in location_tables.keys(): # For each region
    location_name_groups[region] = set() # Create a location name group
    for location in location_tables[region]: # For each location in each region
        # Add each location to its region location group
        location_name_groups[region].add(location.name)
        # Add all DLC locations to the DLC location group
        if (region in dlc_regions): 
            location_name_groups['All DLC regions'].add(location.name)

        # Add each location to its category type location group (e.g. DOOR -> All Doors, ITEM_LOT -> All ITEM_LOTs, etc)
        if location.category.name in category_to_loc_name_map.keys():
            location_name_groups[category_to_loc_name_map[location.category.name]].add(location.name)

# Combine region groups into their un-conditioned selves (e.g. "Northern Undead Asylum - After F2 East Door" -> "Northern Undead Asylum")
for group in list(location_name_groups.keys()):
    gsplit = group.split(' - ')
    if len(gsplit) > 1 and gsplit[0] in location_name_groups.keys():
        location_name_groups[gsplit[0]] = location_name_groups[gsplit[0]].union(location_name_groups[group])
        del location_name_groups[group]

# Further, combine any remaining regions that have differently-named parents into those parents
for group in list(location_name_groups.keys()):
    if group in region_parents.keys():
        pgroup = region_parents[group]
        location_name_groups[pgroup] = location_name_groups[pgroup].union(location_name_groups[group])
        del location_name_groups[group]

# Cleanup loc groups to not bother with skipped locations: those are locked / will give warnings during generate if we exclude them
for group in list(location_name_groups.keys()):
    for location in list(location_name_groups[group]):
        if location_dictionary[location].category in location_skip_categories:
            location_name_groups[group].remove(location)

# # Print loc groups and how many elements they have
# for group in location_name_groups.keys():
#     print (f'Location Group {group} has {len(location_name_groups[group])} elements')
# # Print item groups and how many elements they have
# for group in item_name_groups.keys():
#     print (f'Item Group {group} has {len(item_name_groups[group])} elements')

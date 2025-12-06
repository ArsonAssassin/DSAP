import typing
from BaseClasses import Location, Region
from .Locations import location_tables


item_name_groups = {
}

location_name_groups = {
    "All Doors": set(),
    "All Item Lots": set(),
    "All DLC regions": set()
}

category_to_name_map = {
    "DOOR": "All Doors",
    "ITEM_LOT": "All Item Lots"
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
        if location.category.name in category_to_name_map.keys():
            location_name_groups[category_to_name_map[location.category.name]].add(location.name)

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

# Print groups and how many elements they have
# for group in location_name_groups.keys():
#     print (f'Group {group} has {len(location_name_groups[group])} elements')



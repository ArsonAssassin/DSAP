# world/dsr/__init__.py
from typing import Dict, Set, List, ClassVar, TextIO

from BaseClasses import MultiWorld, Region, Item, Entrance, Tutorial, ItemClassification
from Options import Toggle, OptionError

from worlds.AutoWorld import World, WebWorld
from worlds.generic.Rules import set_rule, add_rule, add_item_rule

from .Items import DSRItem, DSRItemCategory, item_dictionary, key_item_names, item_descriptions, BuildRequiredItemPool, BuildGuaranteedItemPool, UpgradeEquipment
from .Locations import DSRLocation, DSRLocationCategory, location_tables, location_dictionary, location_skip_categories
from .Groups import location_name_groups, item_name_groups
from .Options import DSROption, option_groups

from settings import Group, FilePath

class DSRWeb(WebWorld):
    bug_report_page = ""
    theme = "stone"
    setup_en = Tutorial(
        "Multiworld Setup Guide",
        "A guide to setting up the Archipelago Dark Souls Remastered randomizer on your computer.",
        "English",
        "setup_en.md",
        "setup/en",
        ["ArsonAssassin, dank_santa"]
    )
    option_groups = option_groups


    tutorials = [setup_en]


class DSRSettings(Group):
    class UTPoptrackerPath(FilePath):
        """Path to the user's DSR Poptracker Pack."""
        description= "DSR Poptracker Pack zip file"
        required = False
    ut_poptracker_path: UTPoptrackerPath | str = UTPoptrackerPath()

class DSRWorld(World):
    """
    Dark Souls is a game where you die.
    """

    game: str = "Dark Souls Remastered"
    options_dataclass = DSROption
    options: DSROption
    topology_present: bool = True
    web = DSRWeb()
    data_version = 0
    base_id = 11110000
    enabled_location_categories: Set[DSRLocationCategory]
    required_client_version = (0, 5, 1)
    item_name_to_id = DSRItem.get_name_to_id()
    location_name_to_id = DSRLocation.get_name_to_id()
    item_name_groups = item_name_groups
    item_descriptions = item_descriptions
    location_name_groups = location_name_groups
    settings: ClassVar[DSRSettings]
    tracker_world: ClassVar = {
        "map_page_maps" : "maps/maps.json",
        "map_page_locations" : "locations/locations.json",
        "external_pack_key" : "ut_poptracker_path"
    }

    gc = 0
    bc = 0



    def __init__(self, multiworld: MultiWorld, player: int):
        super().__init__(multiworld, player)
        self.locked_items = []
        self.locked_locations = []
        self.main_path_locations = []
        self.enabled_location_categories = set()
        self.all_excluded_locations = set()


    def generate_early(self):
        # if upgrade level max < min, reverse them
        if self.options.upgraded_weapons_percentage.value > 0 and self.options.upgraded_weapons_max_level.value < self.options.upgraded_weapons_min_level.value:
            (self.options.upgraded_weapons_min_level, self.options.upgraded_weapons_max_level) = (self.options.upgraded_weapons_max_level, self.options.upgraded_weapons_min_level)

        # If % > 0 but no allowed infusion types, default to normal
        if self.options.upgraded_weapons_percentage.value > 0 and len(self.options.upgraded_weapons_allowed_infusions.value) == 0:
            self.options.upgraded_weapons_allowed_infusions.value = ['Normal']

        self.enabled_location_categories.add(DSRLocationCategory.EVENT)
        self.enabled_location_categories.add(DSRLocationCategory.BOSS)
        self.enabled_location_categories.add(DSRLocationCategory.ITEM_LOT)
        self.enabled_location_categories.add(DSRLocationCategory.BONFIRE)
        # self.enabled_location_categories.add(DSRLocationCategory.DOOR)
        if (self.options.fogwall_sanity.value == True):
            self.enabled_location_categories.add(DSRLocationCategory.FOG_WALL)
        if (self.options.boss_fogwall_sanity.value == True):
            self.enabled_location_categories.add(DSRLocationCategory.BOSS_FOG_WALL)

        self.all_excluded_locations.update(self.options.exclude_locations.value)


    def create_regions(self):
        # Create Regions
        regions: Dict[str, Region] = {}
        regions["Menu"] = self.create_region("Menu", [])

        our_regions = [
            "Undead Asylum Cell",
            "Undead Asylum Cell Door",
            "Northern Undead Asylum - F2 East Door",
            "Northern Undead Asylum", 
            "Northern Undead Asylum - After Fog",
            "Northern Undead Asylum - After F2 East Door",
            "Northern Undead Asylum - Big Pilgrim Door",
            "Firelink Shrine", 
            "Upper Undead Burg - Before Fog", 
            "Upper Undead Burg - Fog", 
            "Upper Undead Burg", 
            "Upper Undead Burg - Pine Resin Chest",
            "Upper Undead Burg - Taurus Demon",
            "Upper Undead Burg - After Taurus Demon",
            "Undead Parish - Before Fog", 
            "Undead Parish - Fog", 
            "Undead Parish", 
            "Undead Parish - Bell Gargoyles",
            "Firelink Shrine - After Undead Parish Elevator",
            "Northern Undead Asylum Second Visit",
            "Northern Undead Asylum Second Visit - F2 West Door",
            "Northern Undead Asylum Second Visit - Behind F2 West Door",
            "Northern Undead Asylum Second Visit - Snuggly Trades",
            "Undead Burg Basement Door",
            "Lower Undead Burg", 
            "Lower Undead Burg - After Residence Key",
            "Lower Undead Burg - Capra Demon",
            "Lower Undead Burg - After Capra Demon",
            "Watchtower Basement",
            "Depths", 
            "Depths - After Sewer Chamber Key",
            "Depths - Gaping Dragon",
            "Depths - After Gaping Dragon",
            "Depths to Blighttown Door",
            "Upper Blighttown Depths Side", 
            "Upper Blighttown VotD Side", 
            "Lower Blighttown - Fog", 
            "Lower Blighttown", 
            "Lower Blighttown - Quelaag", 
            "Lower Blighttown - After Quelaag", 
            "Valley of the Drakes", 
            "Valley of the Drakes - After Defeating Four Kings", 
            "Door between Upper New Londo and Valley of the Drakes",
            "Darkroot Basin", 
            "Darkroot Garden - Before Fog",
            "Darkroot Garden", 
            "Darkroot Garden - Behind Artorias Door", 
            "Darkroot Garden - Moonlight Butterfly",
            "Darkroot Garden - After Moonlight Butterfly",
            "The Great Hollow", 
            "Ash Lake",
            "Sen's Fortress",
            "Sen's Fortress - After First Fog",
            "Sen's Fortress - After Second Fog",
            "Sen's Fortress - After Cage Key",
            "Sen's Fortress - Iron Golem",
            "Sen's Fortress - After Iron Golem",
            "Anor Londo",
            "Anor Londo - After First Fog",
            "Anor Londo - After Second Fog",
            "Anor Londo - Ornstein and Smough",
            "Anor Londo - After Ornstein and Smough",
            "Anor Londo - Gwyndolin",
            "Anor Londo - After Gwyndolin",
            "Painted World of Ariamis",
            "Painted World of Ariamis - After Fog",
            "Painted World of Ariamis - After Annex Key",
            "Painted World of Ariamis - Crossbreed Priscilla",
            "Upper New Londo Ruins",
            "Upper New Londo Ruins - After Fog",
            "New Londo Ruins Door to the Seal",
            "Lower New Londo Ruins", 
            "The Abyss", 
            "The Abyss - After Four Kings", 
            "The Duke's Archives", 
            "The Duke's Archives - After First Seath Encounter",
            "The Duke's Archives - Cell Door",
            "The Duke's Archives - Getting out of Cell",
            "The Duke's Archives - After Archive Prison Extra Key",
            "The Duke's Archives - After Archive Tower Giant Door Key", 
            "The Duke's Archives - Courtyard",
            "The Duke's Archives - Giant Cell", 
            "Crystal Cave", 
            "Crystal Cave - After Seath", 
            "The Duke's Archives - First Arena after Seath's Death", 
            "Demon Ruins - Early",
            "Demon Ruins - Ceaseless Discharge",
            "Demon Ruins", 
            "Demon Ruins - Demon Firesage",
            "Demon Ruins - After Demon Firesage",
            "Demon Ruins - Centipede Demon",
            "Demon Ruins Shortcut",
            "Lost Izalith", 
            "Lost Izalith - Bed of Chaos", 
            "The Catacombs", 
            "The Catacombs - Door 1",
            "The Catacombs - After Door 1",
            "The Catacombs - Pinwheel",
            "The Catacombs - After Pinwheel",
            "Tomb of the Giants", 
            "Tomb of the Giants - After White Fog", 
            "Tomb of the Giants - Behind Golden Fog Wall",
            "Tomb of the Giants - Nito",
            "Tomb of the Giants - After Nito",
            "Kiln of the First Flame",
            "Kiln of the First Flame - Gwyn",
            "Sanctuary Garden", 
            "Sanctuary Garden - Santuary Guardian",
            "Oolacile Sanctuary", 
            "Royal Wood", 
            "Royal Wood - Artorias",
            "Royal Wood - After Hawkeye Gough",
            "Oolacile Township", 
            "Oolacile Township - Behind Light-Dispelled Walls",
            "Oolacile Township - After Crest Key",
            "Chasm of the Abyss",
            "Chasm of the Abyss - Manus", 
            ]
        regions.update({region_name: self.create_region(region_name, location_tables[region_name]) for region_name in our_regions})
       
        print("created " + str(self.gc) + " real and "+ str(self.bc) + " fake locations")

        # Connect Regions
        def create_connection(from_region: str, to_region: str):
            connection = Entrance(self.player, f"{from_region} -> {to_region}", regions[from_region])
            regions[from_region].exits.append(connection)
            connection.connect(regions[to_region])
            #print(f"Connecting {from_region} to {to_region} Using entrance: " + connection.name) 

        def create_connection_2way(from_region: str, to_region: str):
            create_connection(from_region, to_region)
            create_connection(to_region, from_region)
            
        create_connection("Menu", "Undead Asylum Cell")    
        
        create_connection("Undead Asylum Cell", "Undead Asylum Cell Door") 
        create_connection("Undead Asylum Cell Door", "Northern Undead Asylum")
        create_connection("Northern Undead Asylum", "Northern Undead Asylum - After Fog")
        create_connection("Northern Undead Asylum - After Fog", "Northern Undead Asylum - F2 East Door")
        create_connection("Northern Undead Asylum - F2 East Door", "Northern Undead Asylum - After F2 East Door")
        create_connection("Northern Undead Asylum - After F2 East Door", "Northern Undead Asylum - Big Pilgrim Door")
        create_connection("Northern Undead Asylum - Big Pilgrim Door", "Firelink Shrine")

        create_connection("Firelink Shrine", "Upper Undead Burg - Before Fog")
        create_connection("Firelink Shrine", "The Catacombs")
        create_connection("Firelink Shrine", "Upper New Londo Ruins")
        create_connection("Firelink Shrine - After Undead Parish Elevator", "Northern Undead Asylum Second Visit")
        create_connection("Firelink Shrine", "Kiln of the First Flame")
        create_connection("Kiln of the First Flame", "Kiln of the First Flame - Gwyn")
        
        create_connection("Northern Undead Asylum Second Visit", "Northern Undead Asylum Second Visit - F2 West Door")
        create_connection("Northern Undead Asylum Second Visit - F2 West Door", "Northern Undead Asylum Second Visit - Behind F2 West Door")
        
        create_connection_2way("Upper Undead Burg - Before Fog", "Upper Undead Burg - Fog")
        create_connection_2way("Upper Undead Burg - Fog", "Upper Undead Burg")
        create_connection("Upper Undead Burg", "Undead Burg Basement Door")
        create_connection("Upper Undead Burg", "Upper Undead Burg - Taurus Demon")
        create_connection("Upper Undead Burg - Taurus Demon", "Upper Undead Burg - After Taurus Demon")
        create_connection_2way("Upper Undead Burg - After Taurus Demon", "Undead Parish - Before Fog")

        create_connection("Upper Undead Burg", "Upper Undead Burg - Pine Resin Chest")
        
        create_connection_2way("Upper Undead Burg", "Watchtower Basement")
        create_connection_2way("Darkroot Basin", "Watchtower Basement")

        create_connection_2way("Undead Parish - Before Fog", "Undead Parish - Fog")
        create_connection_2way("Undead Parish - Fog", "Undead Parish")
        create_connection("Undead Parish", "Undead Parish - Before Fog") # Lever->Gate, or drop near fog
        create_connection("Undead Parish", "Undead Parish - Bell Gargoyles")
        create_connection("Undead Parish", "Firelink Shrine - After Undead Parish Elevator")
        create_connection_2way("Undead Parish", "Darkroot Garden - Before Fog")
        create_connection("Undead Parish", "Sen's Fortress")

        create_connection_2way("Darkroot Garden - Before Fog", "Darkroot Basin")
        create_connection("Darkroot Garden - Before Fog", "Darkroot Garden - Behind Artorias Door")

        create_connection("Darkroot Garden - Before Fog", "Darkroot Garden")
        create_connection("Darkroot Garden", "Darkroot Garden - Moonlight Butterfly")
        create_connection("Darkroot Garden - Moonlight Butterfly", "Darkroot Garden - After Moonlight Butterfly")

        create_connection("Undead Burg Basement Door", "Lower Undead Burg")
        create_connection("Lower Undead Burg", "Depths")
        create_connection("Lower Undead Burg", "Lower Undead Burg - After Residence Key")
        create_connection("Lower Undead Burg", "Lower Undead Burg - Capra Demon")
        create_connection("Lower Undead Burg - Capra Demon", "Lower Undead Burg - After Capra Demon")

        create_connection("Upper New Londo Ruins", "Upper New Londo Ruins - After Fog")
        create_connection("Upper New Londo Ruins - After Fog", "New Londo Ruins Door to the Seal")
        create_connection("New Londo Ruins Door to the Seal", "Lower New Londo Ruins")
        
        create_connection_2way("Upper New Londo Ruins", "Door between Upper New Londo and Valley of the Drakes")
        create_connection_2way("Door between Upper New Londo and Valley of the Drakes", "Valley of the Drakes")

        create_connection("Lower New Londo Ruins", "Valley of the Drakes")

        create_connection("Depths", "Depths - After Sewer Chamber Key")
        create_connection("Depths", "Depths to Blighttown Door")
        create_connection("Depths", "Depths - Gaping Dragon")
        create_connection("Depths - Gaping Dragon", "Depths - After Gaping Dragon")

        create_connection_2way("Valley of the Drakes", "Upper Blighttown VotD Side")
        create_connection_2way("Valley of the Drakes", "Darkroot Basin")
        create_connection("Valley of the Drakes", "Valley of the Drakes - After Defeating Four Kings")

        create_connection("Depths to Blighttown Door", "Upper Blighttown Depths Side")
        
        create_connection("Upper Blighttown Depths Side", "Depths to Blighttown Door")
        
        create_connection("Upper Blighttown Depths Side", "Lower Blighttown") # Able to fall to bypass fog, by Whip
        create_connection_2way("Upper Blighttown Depths Side", "Lower Blighttown - Fog")
        create_connection_2way("Lower Blighttown - Fog", "Lower Blighttown")
        create_connection_2way("Lower Blighttown", "Upper Blighttown VotD Side")
        create_connection("Lower Blighttown", "Demon Ruins - Early")
        create_connection("Lower Blighttown", "The Great Hollow")

        create_connection("Lower Blighttown", "Lower Blighttown - Quelaag")
        create_connection("Lower Blighttown - Quelaag", "Lower Blighttown - After Quelaag")

        create_connection("The Great Hollow", "Ash Lake")

        create_connection("Sen's Fortress", "Sen's Fortress - After First Fog")
        create_connection("Sen's Fortress - After First Fog", "Sen's Fortress - After Second Fog")
        create_connection("Sen's Fortress - After Second Fog", "Sen's Fortress - Iron Golem")
        create_connection("Sen's Fortress - Iron Golem", "Sen's Fortress - After Iron Golem")
        create_connection("Sen's Fortress - After First Fog", "Sen's Fortress - After Cage Key")
        create_connection("Sen's Fortress - After Iron Golem", "Anor Londo")

        create_connection("Anor Londo", "Anor Londo - After First Fog")
        create_connection("Anor Londo - After First Fog", "Anor Londo - After Second Fog")
        create_connection("Anor Londo - After Second Fog", "Anor Londo - Ornstein and Smough")
        create_connection("Anor Londo - Ornstein and Smough", "Anor Londo - After Ornstein and Smough")
        create_connection("Anor Londo - After Ornstein and Smough", "Anor Londo - Gwyndolin")
        create_connection("Anor Londo - Gwyndolin", "Anor Londo - After Gwyndolin")

        create_connection("Anor Londo", "The Duke's Archives")
        create_connection("Anor Londo - After First Fog", "Painted World of Ariamis")

        create_connection("Painted World of Ariamis", "Painted World of Ariamis - After Fog")
        create_connection("Painted World of Ariamis - After Fog", "Painted World of Ariamis - After Annex Key")
        create_connection("Painted World of Ariamis - After Fog", "Painted World of Ariamis - Crossbreed Priscilla")

        create_connection("The Duke's Archives", "The Duke's Archives - After First Seath Encounter")
        create_connection("The Duke's Archives - After First Seath Encounter", "The Duke's Archives - Cell Door")
        create_connection("The Duke's Archives - Cell Door", "The Duke's Archives - Getting out of Cell")
        create_connection("The Duke's Archives - Getting out of Cell", "The Duke's Archives - After Archive Prison Extra Key")
        create_connection("The Duke's Archives - After Archive Prison Extra Key", "The Duke's Archives - After Archive Tower Giant Door Key")
        create_connection("The Duke's Archives - Getting out of Cell", "The Duke's Archives - Giant Cell")
        create_connection("The Duke's Archives - After Archive Tower Giant Door Key", "The Duke's Archives - Courtyard")
        create_connection("The Duke's Archives - Courtyard", "Crystal Cave")
        create_connection("Crystal Cave", "Crystal Cave - After Seath")
        create_connection("Crystal Cave", "The Duke's Archives - First Arena after Seath's Death")

        create_connection("The Catacombs", "The Catacombs - Door 1")
        create_connection("The Catacombs - Door 1", "The Catacombs - After Door 1")
        create_connection("The Catacombs - After Door 1", "The Catacombs - Pinwheel")
        create_connection("The Catacombs - Pinwheel", "The Catacombs - After Pinwheel")
        create_connection("The Catacombs - After Pinwheel", "Tomb of the Giants")

        create_connection("Tomb of the Giants", "Tomb of the Giants - After White Fog")
        create_connection("Tomb of the Giants - After White Fog", "Tomb of the Giants - Behind Golden Fog Wall")
        create_connection("Tomb of the Giants - Behind Golden Fog Wall", "Tomb of the Giants - Nito")
        create_connection("Tomb of the Giants - Nito", "Tomb of the Giants - After Nito")

        create_connection("Lower New Londo Ruins", "The Abyss")
        create_connection("The Abyss", "The Abyss - After Four Kings")
        create_connection("The Abyss - After Four Kings", "Kiln of the First Flame")

        create_connection("Demon Ruins - Early", "Demon Ruins")
        create_connection("Demon Ruins - Early", "Demon Ruins - Ceaseless Discharge")
        create_connection("Demon Ruins", "Demon Ruins - Demon Firesage")
        create_connection("Demon Ruins - Demon Firesage", "Demon Ruins - After Demon Firesage")
        create_connection("Demon Ruins - After Demon Firesage", "Demon Ruins - Centipede Demon")
        create_connection("Demon Ruins - Centipede Demon", "Lost Izalith")
        create_connection("Demon Ruins - After Demon Firesage", "Demon Ruins Shortcut")
        create_connection("Lost Izalith", "Demon Ruins Shortcut")
        create_connection("Lost Izalith", "Lost Izalith - Bed of Chaos")


        # DLC Entrances
        create_connection("Darkroot Basin", "Sanctuary Garden")
        create_connection("Sanctuary Garden", "Sanctuary Garden - Santuary Guardian")
        create_connection("Sanctuary Garden - Santuary Guardian", "Oolacile Sanctuary")
        create_connection("Oolacile Sanctuary", "Royal Wood")
        create_connection("Royal Wood", "Royal Wood - Artorias")
        create_connection("Royal Wood", "Oolacile Township")
        create_connection("Oolacile Township", "Oolacile Township - After Crest Key")
        create_connection("Oolacile Township", "Oolacile Township - Behind Light-Dispelled Walls")
        create_connection("Oolacile Township - After Crest Key", "Royal Wood - After Hawkeye Gough")
        create_connection("Oolacile Township", "Chasm of the Abyss")
        create_connection("Chasm of the Abyss", "Chasm of the Abyss - Manus")
        # end of entrances
        
    # For each region, add the associated locations retrieved from the corresponding location_table
    def create_region(self, region_name, location_table) -> Region:
        new_region = Region(region_name, self.player, self.multiworld)
        #print("location table size: " + str(len(location_table)))
        
        for location in location_table:
            #print("Creating location: " + location.name)

            if (location.category in self.enabled_location_categories and 
                location.category not in location_skip_categories # [DSRLocationCategory.EVENT, DSRLocationCategory.DOOR]:
                and not (self.options.excluded_location_behavior == "do_not_randomize" and location.name in self.all_excluded_locations)): 
                self.gc = self.gc + 1
                default_item = location.default_item
                if (location.category in [DSRLocationCategory.FOG_WALL, DSRLocationCategory.BOSS_FOG_WALL]):
                    default_item = "Fogwall Filler"
                # print("Adding location: " + location.name + " with default item " + location.default_item)
                new_location = DSRLocation(
                    self.player,
                    location.name,
                    location.category,
                    default_item,
                    self.location_name_to_id[location.name],
                    new_region
                )
            else:
                self.bc = self.bc + 1
                default_item = location.default_item
                if (location.category in [DSRLocationCategory.FOG_WALL, DSRLocationCategory.BOSS_FOG_WALL, 
                                          DSRLocationCategory.DOOR]):
                    default_item = "Nothing"
                    print("Placing event: " + default_item + " in location: " + location.name)

                # Replace non-randomized progression items with events
                event_item = self.create_item(default_item)
                # if event_item.classification != ItemClassification.progression:
                #    continue
                # print("Adding Location: " + location.name + " as an event with default item " + default_item)
                new_location = DSRLocation(
                    self.player,
                    location.name,
                    location.category,
                    default_item,
                    None,
                    new_region
                )
                event_item.code = None
                new_location.place_locked_item(event_item)
                

            new_region.locations.append(new_location)
        
        # print("created " + str(len(new_region.locations)) + " locations")
        self.multiworld.regions.append(new_region)
        #print("adding region: " + region_name)
        return new_region


    def create_items(self):
        skip_itemlocs: List[DSRItem, Location] = []
        skipitempool: List[DSRItem] = []
        itempool: List[DSRItem] = []
        itempoolSize = 0
        
        # print("Creating items")
        for location in self.multiworld.get_locations(self.player):            
            item_data = item_dictionary[location.default_item_name]
            if item_data.category in [DSRItemCategory.SKIP] or location.category in location_skip_categories: # [DSRLocationCategory.EVENT]:
                # print("Adding skip item: " + location.default_item_name + " for location: " + location.name)
                skip_itemlocs.append((self.create_item(location.default_item_name), location))
                skipitempool.append(self.create_item(location.default_item_name))
            elif location.category in self.enabled_location_categories:
                if self.options.excluded_location_behavior == "do_not_randomize" and location.name in self.all_excluded_locations:
                    # print("Adding skip item: " + location.default_item_name + " for location: " + location.name)
                    skip_itemlocs.append((self.create_item(location.default_item_name), location))
                    skipitempool.append(self.create_item(location.default_item_name))
                else:
                    #print("Adding item: " + location.default_item_name)
                    itempoolSize += 1
                    itempool.append(self.create_item(location.default_item_name))
        
        # print("Requesting itempool size: " + str(itempoolSize))
        # foo = BuildItemPool(itempoolSize, self.options, self)
        # print("Created item pool size: " + str(len(foo)))

        # Add any Key + useful items
        rip = BuildRequiredItemPool(self, itempoolSize)
        crip = [self.create_item(item.name) for item in rip]
        disabled_items = [self.create_item(loc.default_item) for loc in location_dictionary.values() if loc.category not in self.enabled_location_categories]
        StillRequiredPool = [item for item in crip if item not in itempool and item not in skipitempool and item not in disabled_items]
        guaranteedpool = BuildGuaranteedItemPool(self)

        filler_items = [item for item in itempool if item_dictionary[item.name].category in [DSRItemCategory.FILLER]]
        junk_items = [item for item in itempool if item.name in item_name_groups["Junk"]]
        removable_items = filler_items + junk_items

        # print("marked " + str(len(removable_items)) + " items as removable")
        # print("marked " + str(len(filler_items)) + " items as filler")
        # print("marked " + str(len(junk_items)) + " items as non filler")
        # for item in junk_items:
        #     print("junk:" + item.name)
        # print("itempool size " + str(len(itempool)) + "itempoolsize=" + str(itempoolSize))
        # print("skip_itemlocs size " + str(len(skip_itemlocs)))
        # print("rip size " + str(len(rip)))
        # print("StillRequiredPool size " + str(len(StillRequiredPool)))
        # print("disabled items " + str(len(disabled_items)))
        # for item in disabled_items:
        #     print("disabled:" + item.name)
        # for item in StillRequiredPool:
        #     print("StillRequiredPool item: " + str(item))
        # for item in skipitempool:
        #     print("skip item: " + str(item))
        limited_pool = [item for item in StillRequiredPool if item_dictionary[item.name].category not in [DSRItemCategory.FOGWALL, DSRItemCategory.BOSSFOGWALL]]
        for item in limited_pool:
            print("non-fogwall required item: " + str(item))

        # Replace "Soul of a Lost Undead" if needed
        if len(StillRequiredPool) + len(guaranteedpool) > len(removable_items):
            print("Adding " + str(len([item for item in itempool if item.name == 'Soul of a Lost Undead'])) +" Souls of a Lost Undead to removable items")
            removable_items += [item for item in itempool if item.name == 'Soul of a Lost Undead']
            print("now " + str(len(removable_items)) + " are removable")

        # Replace "Large Soul of a Lost Undead" if needed
        if len(StillRequiredPool) > len(removable_items):
            print("Adding " + str(len([item for item in itempool if item.name == 'Large Soul of a Lost Undead'])) +" Large Souls of a Lost Undead to removable items")
            removable_items += [item for item in itempool if item.name == 'Large Soul of a Lost Undead']
            print("now " + str(len(removable_items)) + " are removable")

        for item in removable_items:
            if len(StillRequiredPool) > 0:
                # print("removable item: " + item.name)
                itempool.remove(item)
                itempool.append(self.create_item(StillRequiredPool.pop().name))
            elif len(guaranteedpool) > 0:
                itempool.remove(item)
                itempool.append(self.create_item(guaranteedpool.pop().name))
            else:
                break

        filler_items = [item for item in itempool if item_dictionary[item.name].category in [DSRItemCategory.FILLER]]
        junk_items = [item for item in itempool if item.name in item_name_groups["Junk"]]
        removable_items = filler_items + junk_items

        filler_items = [item for item in itempool if item_dictionary[item.name].category in [DSRItemCategory.FILLER]]
        junk_items = [item for item in itempool if item.name in item_name_groups["Junk"]]
        removable_items = filler_items + junk_items
        print("leftover removable items: " + str(len(removable_items)))
        print("leftover filler items: " + str(len(filler_items)))

        for item in removable_items:
            # print("removable item: " + item.name)
            itempool.remove(item)
            itempool.append(self.create_item("Soul of a Proud Knight"))

        # Add regular items to itempool
        self.multiworld.itempool += itempool

        # Handle SKIP items separately
        for skip_item_loc in skip_itemlocs:
            location = skip_item_loc[1]
            location.place_locked_item(skip_item_loc[0])    
            #self.multiworld.itempool.append(skip_item)
            #print("Placing skip item: " + skip_item.name + " in location: " + location.name)
        
        #print("Final Item pool: ")
        #for item in self.multiworld.itempool:
            #print(item.name)


    def create_item(self, name: str) -> Item:
        useful_categories = {
            DSRItemCategory.EMBER,
        }
        data = self.item_name_to_id[name]

        if name in key_item_names or item_dictionary[name].category in [DSRItemCategory.EVENT, DSRItemCategory.KEY_ITEM, DSRItemCategory.FOGWALL, DSRItemCategory.BOSSFOGWALL]:
            item_classification = ItemClassification.progression
        elif item_dictionary[name].category in useful_categories:
            item_classification = ItemClassification.useful
        else:
            item_classification = ItemClassification.filler

        return DSRItem(name, item_classification, data, self.player)


    def get_filler_item_name(self) -> str:
        return "1000 Souls"
    
    def set_rules(self) -> None:           
        #print("Setting rules")   
        for region in self.multiworld.get_regions(self.player):
            for location in region.locations:
                    set_rule(location, lambda state: True)        
        self.multiworld.completion_condition[self.player] = lambda state: state.has("Gwyn, Lord of Cinder Defeated", self.player)

        set_rule(self.multiworld.get_entrance("Undead Asylum Cell -> Undead Asylum Cell Door", self.player), lambda state: state.has("Dungeon Cell Key", self.player))   
        #set_rule(self.multiworld.get_entrance("Undead Asylum Cell Door -> Northern Undead Asylum", self.player), lambda state: state.has("Dungeon Cell Key", self.player))      
        set_rule(self.multiworld.get_entrance("Northern Undead Asylum - After Fog -> Northern Undead Asylum - F2 East Door", self.player), lambda state: state.has("Undead Asylum F2 East Key", self.player))
        set_rule(self.multiworld.get_entrance("Northern Undead Asylum - After F2 East Door -> Northern Undead Asylum - Big Pilgrim Door", self.player), lambda state: state.has("Big Pilgrim's Key", self.player))
        set_rule(self.multiworld.get_entrance("Upper Undead Burg -> Undead Burg Basement Door", self.player), lambda state:state.has("Taurus Demon Defeated", self.player) and state.has ("Basement Key", self.player))
        set_rule(self.multiworld.get_entrance("Upper Undead Burg - Taurus Demon -> Upper Undead Burg - After Taurus Demon", self.player), lambda state:state.has("Taurus Demon Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Upper Undead Burg -> Upper Undead Burg - Pine Resin Chest", self.player), lambda state: state.has("Master Key", self.player) or state.has("Residence Key", self.player))
        set_rule(self.multiworld.get_entrance("Upper Undead Burg -> Watchtower Basement", self.player), lambda state: state.has("Master Key", self.player) or state.has("Watchtower Basement Key", self.player))
        
        # set_rule(self.multiworld.get_location("Snuggly: Pendant -> Souvenir of Reprisal", self.player), lambda state: state.has("Pendant", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Rubbish -> Titanite Chunk", self.player), lambda state: state.has("Rubbish", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Sunlight Medal -> White Titanite Chunk", self.player), lambda state: state.has("Sunlight Medal", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Bloodred Moss Clump -> Twinkling Titanite", self.player), lambda state: state.has("Bloodred Moss Clump", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Purple Moss Clump -> Twinkling Titanite", self.player), lambda state: state.has("Purple Moss Clump", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Blooming Purple Moss Clump -> Twinkling Titanite x2", self.player), lambda state: state.has("Blooming Purple Moss Clump", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Cracked Red Eye Orb -> Purging Stone x2", self.player), lambda state: state.has("Cracked Red Eye Orb", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Humanity -> Ring of Sacrifice", self.player), lambda state: state.has("Humanity", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Twin Humanities -> Rare Ring of Sacrifice", self.player), lambda state: state.has("Twin Humanities", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Dung Pie -> Demon Titanite", self.player), lambda state: state.has("Dung Pie", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Pyromancy Flame -> Red Titanite Chunk", self.player), lambda state: state.has("Pyromancy Flame", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Pyromancy Flame (Ascended) -> Red Titanite Slab", self.player), lambda state: state.has("Pyromancy Flame (Ascended)", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Egg Vermifuge -> Dragon Scale", self.player), lambda state: state.has("Egg Vermifuge", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Sunlight Maggot -> Old Witch's Ring", self.player), lambda state: state.has("Sunlight Maggot", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Sack -> Demon's Great Hammer", self.player), lambda state: state.has("Sack", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Skull Lantern -> Ring of Fog", self.player), lambda state: state.has("Skull Lantern", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Ring of the Sun Princess -> Divine Blessing x2", self.player), lambda state: state.has("Ring of the Sun Princess", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Xanthous Crown -> Ring of Favor and Protection", self.player), lambda state: state.has("Xanthous Crown", self.player))
        # set_rule(self.multiworld.get_location("Snuggly: Soul of Manus -> Sorcery: Pursuers", self.player), lambda state: state.has("Soul of Manus", self.player))
        
        set_rule(self.multiworld.get_entrance("Darkroot Basin -> Watchtower Basement", self.player), lambda state: state.has("Master Key", self.player) or state.has("Watchtower Basement Key", self.player))
        set_rule(self.multiworld.get_entrance("Northern Undead Asylum Second Visit -> Northern Undead Asylum Second Visit - F2 West Door", self.player), lambda state: state.has("Undead Asylum F2 West Key", self.player))
        set_rule(self.multiworld.get_entrance("Darkroot Garden - Before Fog -> Darkroot Garden - Behind Artorias Door", self.player), lambda state: state.has("Crest of Artorias", self.player))
        # Else no rule - player can access without problem

        set_rule(self.multiworld.get_entrance("Darkroot Garden - Moonlight Butterfly -> Darkroot Garden - After Moonlight Butterfly", self.player), lambda state: state.has("Moonlight Butterfly Defeated", self.player))


        set_rule(self.multiworld.get_entrance("Lower Undead Burg -> Depths", self.player), lambda state: state.has("Key to Depths", self.player))
        
        set_rule(self.multiworld.get_entrance("Lower Undead Burg -> Lower Undead Burg - After Residence Key", self.player), lambda state: state.has("Residence Key", self.player))
            
        set_rule(self.multiworld.get_entrance("Lower Undead Burg - Capra Demon -> Lower Undead Burg - After Capra Demon", self.player), lambda state: state.has("Capra Demon Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Upper New Londo Ruins -> Door between Upper New Londo and Valley of the Drakes", self.player), lambda state: state.has("Key to New Londo Ruins", self.player) or state.has("Master Key", self.player))
        set_rule(self.multiworld.get_entrance("Valley of the Drakes -> Door between Upper New Londo and Valley of the Drakes", self.player), lambda state: state.has("Key to New Londo Ruins", self.player) or state.has("Master Key", self.player))

        set_rule(self.multiworld.get_entrance("Depths -> Depths - After Sewer Chamber Key", self.player), lambda state: state.has("Sewer Chamber Key", self.player))
        set_rule(self.multiworld.get_entrance("Depths - Gaping Dragon -> Depths - After Gaping Dragon", self.player), lambda state: state.has("Gaping Dragon Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Depths -> Depths to Blighttown Door", self.player), lambda state: state.has("Blighttown Key", self.player))
        set_rule(self.multiworld.get_entrance("Upper Blighttown Depths Side -> Depths to Blighttown Door", self.player), lambda state: state.has("Depths -> Blighttown opened", self.player))
        set_rule(self.multiworld.get_entrance("Lower Blighttown - Quelaag -> Lower Blighttown - After Quelaag", self.player), lambda state: state.has("Chaos Witch Quelaag Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Lower Blighttown -> Demon Ruins - Early", self.player), lambda state: state.has("Chaos Witch Quelaag Defeated", self.player))
        
        set_rule(self.multiworld.get_location("UP: Bell of Awakening #1 rung", self.player), lambda state: state.has("Bell Gargoyles Defeated", self.player))
        # set_rule(self.multiworld.get_location("BT: Bell of Awakening #2 rung", self.player), lambda state: state.has("Chaos Witch Quelaag Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Undead Parish -> Sen's Fortress", self.player), lambda state: state.has("Bell of Awakening #1", self.player) and state.has("Bell of Awakening #2", self.player))
        set_rule(self.multiworld.get_entrance("Sen's Fortress - After First Fog -> Sen's Fortress - After Cage Key", self.player), lambda state: state.has("Master Key", self.player) or state.has("Cage Key", self.player))
        set_rule(self.multiworld.get_entrance("Sen's Fortress - Iron Golem -> Sen's Fortress - After Iron Golem", self.player), lambda state: state.has("Iron Golem Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Anor Londo -> The Duke's Archives", self.player), lambda state: state.has("Lordvessel", self.player))
        set_rule(self.multiworld.get_entrance("Anor Londo - Ornstein and Smough -> Anor Londo - After Ornstein and Smough", self.player), lambda state: state.has("Ornstein and Smough Defeated", self.player))

        set_rule(self.multiworld.get_location("NL: Key to the Seal", self.player), lambda state: state.has("Lordvessel", self.player))
        set_rule(self.multiworld.get_entrance("Upper New Londo Ruins - After Fog -> New Londo Ruins Door to the Seal", self.player), lambda state: state.has("Key to the Seal", self.player))
        set_rule(self.multiworld.get_entrance("Valley of the Drakes -> Valley of the Drakes - After Defeating Four Kings", self.player), lambda state: state.has("Four Kings Defeated", self.player))
                
        set_rule(self.multiworld.get_entrance("The Duke's Archives - After First Seath Encounter -> The Duke's Archives - Cell Door", self.player), lambda state: state.has("Archive Tower Cell Key", self.player))
        set_rule(self.multiworld.get_entrance("The Duke's Archives - Getting out of Cell -> The Duke's Archives - After Archive Prison Extra Key", self.player), lambda state: state.has("Archive Prison Extra Key", self.player))
        set_rule(self.multiworld.get_entrance("The Duke's Archives - After Archive Prison Extra Key -> The Duke's Archives - After Archive Tower Giant Door Key", self.player), lambda state: state.has("Archive Tower Giant Door Key", self.player))
        set_rule(self.multiworld.get_entrance("The Duke's Archives - Getting out of Cell -> The Duke's Archives - Giant Cell", self.player), lambda state: state.has("Archive Tower Giant Cell Key", self.player))
        set_rule(self.multiworld.get_location("DA: Broken Pendant", self.player), lambda state: state.has("Dusk Rescued", self.player))
        set_rule(self.multiworld.get_entrance("Crystal Cave -> Crystal Cave - After Seath", self.player), lambda state: state.has("Seath the Scaleless Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Crystal Cave -> The Duke's Archives - First Arena after Seath's Death", self.player), lambda state: state.has("Seath the Scaleless Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Anor Londo - After First Fog -> Painted World of Ariamis", self.player), lambda state: state.has("Peculiar Doll", self.player))
        set_rule(self.multiworld.get_entrance("Painted World of Ariamis - After Fog -> Painted World of Ariamis - After Annex Key", self.player), lambda state: state.has("Annex Key", self.player))
        
        set_rule(self.multiworld.get_entrance("Lower New Londo Ruins -> The Abyss", self.player), lambda state: state.has("Covenant of Artorias", self.player) and ((self.options.boss_fogwall_sanity.value == False) or state.has ("Boss Fog Wall Key - Four Kings", self.player)))
        
        set_rule(self.multiworld.get_entrance("Demon Ruins -> Demon Ruins - Demon Firesage", self.player), lambda state: state.has("Lordvessel", self.player) and ((self.options.boss_fogwall_sanity.value == False) or state.has ("Boss Fog Wall Key - Demon Firesage", self.player)))
        set_rule(self.multiworld.get_entrance("Demon Ruins - Early -> Demon Ruins", self.player), lambda state: state.has("Ceaseless Discharge Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Lost Izalith -> Demon Ruins Shortcut", self.player), lambda state: state.has("Bed of Chaos Defeated", self.player))

        # some demon ruins checks require Orange Charred Ring
        set_rule(self.multiworld.get_location("DR: Large Soul of a Proud Knight - First Jump over the Lava", self.player), lambda state: state.has("Orange Charred Ring", self.player))
        set_rule(self.multiworld.get_location("DR: Chaos Flame Ember", self.player), lambda state: state.has("Orange Charred Ring", self.player))

        set_rule(self.multiworld.get_entrance("Demon Ruins - After Demon Firesage -> Demon Ruins Shortcut", self.player), lambda state: state.has("Demon Ruins Shortcut opened", self.player))
        set_rule(self.multiworld.get_entrance("Demon Ruins - Centipede Demon -> Lost Izalith", self.player), lambda state: state.has("Orange Charred Ring", self.player) and state.has("Centipede Demon Defeated", self.player))
        set_rule(self.multiworld.get_entrance("The Catacombs - Pinwheel -> The Catacombs - After Pinwheel", self.player), lambda state: state.has("Pinwheel Defeated", self.player))
        set_rule(self.multiworld.get_entrance("The Catacombs - After Pinwheel -> Tomb of the Giants", self.player), lambda state: state.has("Skull Lantern", self.player))
        set_rule(self.multiworld.get_entrance("Tomb of the Giants - After White Fog -> Tomb of the Giants - Behind Golden Fog Wall", self.player), lambda state: state.has("Lordvessel", self.player))
        set_rule(self.multiworld.get_entrance("Tomb of the Giants - Nito -> Tomb of the Giants - After Nito", self.player), lambda state: state.has("Gravelord Nito Defeated", self.player))

        # Frampt entrance to Kiln
        set_rule(self.multiworld.get_entrance("Firelink Shrine -> Kiln of the First Flame", self.player), 
            lambda state: state.has("Lord Soul (Bed of Chaos)", self.player) and state.has("Lord Soul (Nito)", self.player) and state.has("Bequeathed Lord Soul Shard (Four Kings)", self.player) and state.has("Bequeathed Lord Soul Shard (Seath)", self.player) 
            and state.has("Lordvessel", self.player)
            and state.has("Bell of Awakening #1", self.player) and state.has("Bell of Awakening #2", self.player))

        # Kaathe entrance to Kiln
        set_rule(self.multiworld.get_entrance("The Abyss - After Four Kings -> Kiln of the First Flame", self.player), 
            lambda state: state.has("Lord Soul (Bed of Chaos)", self.player) and state.has("Lord Soul (Nito)", self.player) and state.has("Bequeathed Lord Soul Shard (Four Kings)", self.player) and state.has("Bequeathed Lord Soul Shard (Seath)", self.player) 
            and state.has("Lordvessel", self.player))
        
              
        # DLC areas
        set_rule(self.multiworld.get_entrance("Darkroot Basin -> Sanctuary Garden", self.player), lambda state: state.has("Broken Pendant", self.player))

        set_rule(self.multiworld.get_entrance("Sanctuary Garden - Santuary Guardian -> Oolacile Sanctuary", self.player), lambda state: state.has("Sanctuary Guardian Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Royal Wood -> Oolacile Township", self.player), lambda state: state.has("Artorias the Abysswalker Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Oolacile Township -> Oolacile Township - After Crest Key", self.player), lambda state: state.has("Crest Key", self.player))
        set_rule(self.multiworld.get_entrance("Oolacile Township -> Oolacile Township - Behind Light-Dispelled Walls", self.player), lambda state: state.has("Skull Lantern", self.player))
    
        # artificial logic - don't require jumping around BT fog wall without a "real" way to return
        set_rule(self.multiworld.get_entrance("Upper Blighttown Depths Side -> Lower Blighttown", self.player), lambda state: state.has("Lordvessel", self.player))

        # artificial logic
        if (self.options.fogwall_sanity == False and self.options.boss_fogwall_sanity == False):
            set_rule(self.multiworld.get_entrance("Firelink Shrine -> The Catacombs", self.player), lambda state: state.has("Ornstein and Smough Defeated", self.player))
            set_rule(self.multiworld.get_entrance("Upper New Londo Ruins - After Fog -> New Londo Ruins Door to the Seal", self.player), lambda state: state.has("Ornstein and Smough Defeated", self.player) and state.has("Key to the Seal", self.player))
            set_rule(self.multiworld.get_entrance("Lower Blighttown -> The Great Hollow", self.player), lambda state: state.has("Lordvessel", self.player))
            set_rule(self.multiworld.get_entrance("The Catacombs - After Pinwheel -> Tomb of the Giants", self.player), lambda state: state.has("Ornstein and Smough Defeated", self.player) and state.has("Skull Lantern", self.player))



        # fogwall rules
        def add_fog_rule(fogwall_item: str, from_region: str, to_region: str):
            set_rule(self.multiworld.get_entrance(f"{from_region} -> {to_region}", self.player), 
                lambda state: (self.options.fogwall_sanity.value == False) or state.has (fogwall_item, self.player))

        #early - removed
        # set_rule(self.multiworld.get_entrance("Northern Undead Asylum -> Northern Undead Asylum - After Fog", self.player), lambda state: (self.options.fogwall_sanity.value == False) or (self.options.fogwall_sanity_include_ua.value == False) or state.has ("Fog Wall Key - Northern Undead Asylum", self.player))
        
        #normal
        add_fog_rule("Fog Wall Key - Undead Burg", "Upper Undead Burg - Before Fog", "Upper Undead Burg - Fog")
        add_fog_rule("Fog Wall Key - Undead Burg", "Upper Undead Burg", "Upper Undead Burg - Fog")

        add_fog_rule("Fog Wall Key - Undead Parish", "Undead Parish - Before Fog", "Undead Parish - Fog")
        add_fog_rule("Fog Wall Key - Undead Parish", "Undead Parish", "Undead Parish - Fog")

        add_fog_rule("Fog Wall Key - Darkroot Garden", "Darkroot Garden - Before Fog", "Darkroot Garden")
        
        # Depths fog doesn't affect entrance logic, but is itself only accessible with the fog item
        set_rule(self.multiworld.get_location("DE: Fog Wall - Depths Rat Room", self.player), lambda state: (self.options.fogwall_sanity.value == False) or state.has ("Fog Wall Key - Depths Rat Room", self.player))

        add_fog_rule("Fog Wall Key - Lower Blighttown Entrance", "Upper Blighttown Depths Side", "Lower Blighttown - Fog")
        add_fog_rule("Fog Wall Key - Lower Blighttown Entrance", "Lower Blighttown", "Lower Blighttown - Fog")

        add_fog_rule("Fog Wall Key - Ash Lake Entrance", "The Great Hollow", "Ash Lake")

        add_fog_rule("Fog Wall Key - Sen's Fortress #1 (Outside Stairs)", "Sen's Fortress", "Sen's Fortress - After First Fog")
        add_fog_rule("Fog Wall Key - Sen's Fortress #2 (Upper Entrance)", "Sen's Fortress - After First Fog", "Sen's Fortress - After Second Fog")


        add_fog_rule("Fog Wall Key - Anor Londo #1 (Rafters)", "Anor Londo", "Anor Londo - After First Fog")
        add_fog_rule("Fog Wall Key - Anor Londo #2 (Archers)", "Anor Londo - After First Fog", "Anor Londo - After Second Fog")

        add_fog_rule("Fog Wall Key - Duke's Archives Courtyard Entrance", "The Duke's Archives - After Archive Tower Giant Door Key", "The Duke's Archives - Courtyard")
        
        # Catacombs fog does not affect entrance logic, but is itself only accessible with the fog item
        set_rule(self.multiworld.get_location("TC: Fog Wall - Catacombs", self.player), lambda state: (self.options.fogwall_sanity.value == False) or state.has ("Fog Wall Key - Catacombs", self.player))

        add_fog_rule("Fog Wall Key - Tomb of the Giants", "Tomb of the Giants", "Tomb of the Giants - After White Fog")
        add_fog_rule("Fog Wall Key - New Londo (Upper)", "Upper New Londo Ruins", "Upper New Londo Ruins - After Fog")

        # Lower new londo fog does not affect entrance logic, but is itself only accessible with the fog item
        set_rule(self.multiworld.get_location("NL: Fog Wall - New Londo (Lower)", self.player), lambda state: (self.options.fogwall_sanity.value == False) or state.has ("Fog Wall Key - New Londo (Lower)", self.player))

        add_fog_rule("Fog Wall Key - Painted World", "Painted World of Ariamis", "Painted World of Ariamis - After Fog")

        #bosses
        def add_boss_fog_rule(fogwall_item: str, from_region: str, to_region: str):
            set_rule(self.multiworld.get_entrance(f"{from_region} -> {to_region}", self.player), 
                lambda state: (self.options.boss_fogwall_sanity.value == False) or state.has (fogwall_item, self.player))

        add_boss_fog_rule("Boss Fog Wall Key - Taurus Demon", "Upper Undead Burg", "Upper Undead Burg - Taurus Demon")
        add_boss_fog_rule("Boss Fog Wall Key - Capra Demon", "Lower Undead Burg", "Lower Undead Burg - Capra Demon")
        add_boss_fog_rule("Boss Fog Wall Key - Bell Gargoyles", "Undead Parish", "Undead Parish - Bell Gargoyles")
        add_boss_fog_rule("Boss Fog Wall Key - Moonlight Butterfly", "Darkroot Garden", "Darkroot Garden - Moonlight Butterfly")

        add_boss_fog_rule("Boss Fog Wall Key - Gaping Dragon", "Depths", "Depths - Gaping Dragon")
        add_boss_fog_rule("Boss Fog Wall Key - Quelaag", "Lower Blighttown", "Lower Blighttown - Quelaag")
        add_boss_fog_rule("Boss Fog Wall Key - Iron Golem", "Sen's Fortress - After Second Fog", "Sen's Fortress - Iron Golem")
        add_boss_fog_rule("Boss Fog Wall Key - Ornstein and Smough", "Anor Londo - After Second Fog", "Anor Londo - Ornstein and Smough")
        add_boss_fog_rule("Boss Fog Wall Key - Gwyndolin", "Anor Londo - After Ornstein and Smough", "Anor Londo - Gwyndolin")
        add_boss_fog_rule("Boss Fog Wall Key - Seath First Encounter", "The Duke's Archives", "The Duke's Archives - After First Seath Encounter")

        add_boss_fog_rule("Boss Fog Wall Key - Pinwheel", "The Catacombs - After Door 1", "The Catacombs - Pinwheel")
        add_boss_fog_rule("Boss Fog Wall Key - Nito", "Tomb of the Giants - Behind Golden Fog Wall", "Tomb of the Giants - Nito")

        # 4 kings defined above (because it also needs covenant of the abyss)

        add_boss_fog_rule("Boss Fog Wall Key - Ceaseless Discharge", "Demon Ruins - Early", "Demon Ruins - Ceaseless Discharge")
        # Demon Firesage boss fog is earlier, because of golden fog (lordvessel) requirement
        add_boss_fog_rule("Boss Fog Wall Key - Centipede Demon", "Demon Ruins - After Demon Firesage", "Demon Ruins - Centipede Demon")
        add_boss_fog_rule("Boss Fog Wall Key - Bed of Chaos", "Lost Izalith", "Lost Izalith - Bed of Chaos")

        add_boss_fog_rule("Boss Fog Wall Key - Crossbreed Priscilla", "Painted World of Ariamis - After Fog", "Painted World of Ariamis - Crossbreed Priscilla")

        add_boss_fog_rule("Boss Fog Wall Key - Gwyn", "Kiln of the First Flame", "Kiln of the First Flame - Gwyn")

        # dlc bosses
        add_boss_fog_rule("Boss Fog Wall Key - Sanctuary Guardian", "Sanctuary Garden", "Sanctuary Garden - Santuary Guardian")
        add_boss_fog_rule("Boss Fog Wall Key - Artorias", "Royal Wood", "Royal Wood - Artorias")
        add_boss_fog_rule("Boss Fog Wall Key - Manus", "Chasm of the Abyss", "Chasm of the Abyss - Manus")

        # end of areas

        # for debugging purposes, you may want to visualize the layout of your world. Uncomment the following code to
        # write a PlantUML diagram to the file "my_world.puml" that can help you see whether your regions and locations
        # are connected and placed as desired
        # from Utils import visualize_regions
        # visualize_regions(self.multiworld.get_region("Menu", self.player), "my_world.puml")
 
        
    def fill_slot_data(self) -> Dict[str, object]:
        slot_data: Dict[str, object] = {}
        name_to_dsr_code = {item.name: item.dsr_code for item in item_dictionary.values()}
        # Create the mandatory lists to generate the player's output file
        items_id = []
        items_names = []
        items_upgrades = []
        items_address = []
        locations_id = []
        locations_address = []
        locations_target = []
        for location in self.multiworld.get_filled_locations():
            if location.item.player == self.player:
                #we are the receiver of the item
                items_id.append(location.item.code)
                items_names.append(location.item.name)
                upgrade = UpgradeEquipment(location.item.code, self.options, self)
                items_upgrades.append(upgrade)
                items_address.append(f'{location.player}:{location.address}')

            if location.player == self.player:
                #we are the sender of the location check
                locations_address.append(item_dictionary[location_dictionary[location.name].default_item].dsr_code)
                locations_id.append(location.address)
                if location.item.player == self.player:
                    locations_target.append(name_to_dsr_code[location.item.name])
                else:
                    locations_target.append(0)

        slot_data = {
            "options": {
                "guaranteed_items": self.options.guaranteed_items.value,
                "fogwall_sanity": self.options.fogwall_sanity.value,
                "boss_fogwall_sanity": self.options.boss_fogwall_sanity.value,
                "upgraded_weapons_percentage": self.options.upgraded_weapons_percentage.value,
                "upgraded_weapons_allowed_infusions": self.options.upgraded_weapons_allowed_infusions.value,
                "upgraded_weapons_adjusted_levels": self.options.upgraded_weapons_adjusted_levels.value,
                "upgraded_weapons_min_level": self.options.upgraded_weapons_min_level.value,
                "upgraded_weapons_max_level": self.options.upgraded_weapons_max_level.value,
                "enable_deathlink": self.options.enable_deathlink.value,
            },
            "seed": self.multiworld.seed_name,  # to verify the server's multiworld
            "slot": self.multiworld.player_name[self.player],  # to connect to server
            "base_id": self.base_id,  # to merge location and items lists
            "locationsId": locations_id,
            "locationsAddress": locations_address,
            "locationsTarget": locations_target,
            "itemsId": items_id,
            "itemsUpgrades": items_upgrades,
            "itemsAddress": items_address,
            "apworld_api_version" : "0.0.21.1" # Manually set our apworld api level, for detecting compatibility with client
        }

        self.items_id = items_id
        self.items_names = items_names
        self.items_upgrades = items_upgrades
        self.items_address = items_address

        return slot_data

    def write_spoiler(self, spoiler_handle: TextIO) -> None:
        wrote_items = False
        if (len(self.items_upgrades) > 0):
            spoiler_handle.write(f"\nDSR weapon upgrades for {self.multiworld.player_name[self.player]}:\n")
            for i in range(len(self.items_upgrades)):
                if self.items_upgrades[i] == None or self.items_upgrades[i] == "":
                    continue
                spoiler_handle.write(f"\nitem {self.items_names[i]} at loc {self.items_address[i]} upgraded to {self.items_upgrades[i]}.")
                wrote_items = True
            if not wrote_items:
                spoiler_handle.write("\nNo items upgraded")
            spoiler_handle.write("\n") # Spacing

# world/dsr/__init__.py
from typing import Dict, Set, List

from BaseClasses import MultiWorld, Region, Item, Entrance, Tutorial, ItemClassification
from Options import Toggle

from worlds.AutoWorld import World, WebWorld
from worlds.generic.Rules import set_rule, add_rule, add_item_rule

from .Items import DSRItem, DSRItemCategory, item_dictionary, key_item_names, item_descriptions, BuildItemPool
from .Locations import DSRLocation, DSRLocationCategory, location_tables, location_dictionary, location_skip_categories
from .Options import DSROption

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


    tutorials = [setup_en]


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
    item_name_groups = {
    }
    item_descriptions = item_descriptions


    def __init__(self, multiworld: MultiWorld, player: int):
        super().__init__(multiworld, player)
        self.locked_items = []
        self.locked_locations = []
        self.main_path_locations = []
        self.enabled_location_categories = set()


    def generate_early(self):
        self.enabled_location_categories.add(DSRLocationCategory.EVENT),
        self.enabled_location_categories.add(DSRLocationCategory.BOSS),
        self.enabled_location_categories.add(DSRLocationCategory.ITEM_LOT),
        self.enabled_location_categories.add(DSRLocationCategory.BONFIRE),
        self.enabled_location_categories.add(DSRLocationCategory.DOOR),

    def create_regions(self):
        # Create Regions
        regions: Dict[str, Region] = {}
        regions["Menu"] = self.create_region("Menu", [])
        regions.update({region_name: self.create_region(region_name, location_tables[region_name]) for region_name in [
            "Undead Asylum Cell",
            "Undead Asylum Cell Door",
            "Northern Undead Asylum F2 East Door",
            "Northern Undead Asylum", 
            "Northern Undead Asylum - After F2 East Door", 
            "Undead Asylum Big Pilgrim Door",
            "Firelink Shrine", 
            "Upper Undead Burg", 
            "Upper Undead Burg - Pine Resin Chest",
            "Undead Parish", 
            "Firelink Shrine - After Undead Parish Elevator",
            "Northern Undead Asylum - Second Visit F2 West Door",
            "Northern Undead Asylum - Second Visit Snuggly Trades",
            "Northern Undead Asylum - Second Visit Behind F2 West Door",
            "Undead Burg Basement Door",
            "Lower Undead Burg", 
            "Lower Undead Burg - After Residence Key",
            "Watchtower Basement",
            "Depths", 
            "Depths - After Sewer Chamber Key",
            "Depths to Blighttown Door",
            "Blighttown", 
            "Valley of the Drakes", 
            "Valley of the Drakes - After Defeating Four Kings", 
            "Door between Upper New Londo and Valley of the Drakes",
            "Darkroot Basin", 
            "Darkroot Garden", 
            "Darkroot Garden - Behind Artorias Door", 
            "The Great Hollow", 
            "Ash Lake", 
            "Sen's Fortress",
            "Sen's Fortress - After Cage Key",
            "Anor Londo", 
            "Painted World of Ariamis",
            "Painted World of Ariamis - After Annex Key",
            "Upper New Londo Ruins",
            "New Londo Ruins Door to the Seal",
            "Lower New Londo Ruins", 
            "The Abyss", 
            "The Duke's Archives", 
            "The Duke's Archives Cell Door",
            "The Duke's Archives - Getting out of Cell",
            "The Duke's Archives - After Archive Prison Extra Key",
            "The Duke's Archives - After Archive Tower Giant Door Key", 
            "The Duke's Archives - Giant Cell",
            "Crystal Cave", 
            "The Duke's Archives - First Arena after Seath's Death", 
            "Demon Ruins", 
            "Demon Ruins - Behind Golden Fog Wall",
            "Demon Ruins Shortcut",
            "Lost Izalith", 
            "The Catacombs", 
            "The Catacombs - Door 1",
            "The Catacombs - After Door 1",
            "Tomb of the Giants", 
            "Tomb of the Giants - Behind Golden Fog Wall",
            "Kiln of the First Flame", 
            #"Sanctuary Garden", 
            #"Oolacile Sanctuary", 
            #"Royal Wood", 
            #"Royal Wood - After Hawkeye Gough",
            #"Oolacile Township", 
            #"Oolacile Township - After Crest Key",
            #"Chasm of the Abyss", 
                ]})
       
        # Connect Regions
        def create_connection(from_region: str, to_region: str, rule = None):
            connection = Entrance(self.player, f"{from_region} -> {to_region}", regions[from_region])
            regions[from_region].exits.append(connection)
            connection.connect(regions[to_region], rule)
            #print(f"Connecting {from_region} to {to_region} Using entrance: " + connection.name) 
        create_connection("Menu", "Undead Asylum Cell")    
        
        create_connection("Undead Asylum Cell", "Undead Asylum Cell Door") 
        create_connection("Undead Asylum Cell Door", "Northern Undead Asylum")
        create_connection("Northern Undead Asylum", "Northern Undead Asylum F2 East Door")
        create_connection("Northern Undead Asylum F2 East Door", "Northern Undead Asylum - After F2 East Door")
        create_connection("Northern Undead Asylum - After F2 East Door", "Undead Asylum Big Pilgrim Door")
        create_connection("Undead Asylum Big Pilgrim Door", "Firelink Shrine")

        create_connection("Firelink Shrine", "Upper Undead Burg")
        create_connection("Firelink Shrine", "The Catacombs")
        create_connection("Firelink Shrine", "Upper New Londo Ruins")
        create_connection("Firelink Shrine - After Undead Parish Elevator", "Northern Undead Asylum - Second Visit Snuggly Trades")
        create_connection("Firelink Shrine", "Kiln of the First Flame")
        
        create_connection("Northern Undead Asylum - Second Visit Snuggly Trades", "Northern Undead Asylum - Second Visit F2 West Door")
        create_connection("Northern Undead Asylum - Second Visit F2 West Door", "Northern Undead Asylum - Second Visit Behind F2 West Door")
        
        create_connection("Upper Undead Burg", "Undead Burg Basement Door")
        create_connection("Upper Undead Burg", "Undead Parish")
        create_connection("Upper Undead Burg", "Darkroot Basin")
        create_connection("Upper Undead Burg", "Upper Undead Burg - Pine Resin Chest")
        
        create_connection("Upper Undead Burg", "Watchtower Basement")
        create_connection("Darkroot Basin", "Watchtower Basement")

        create_connection("Undead Parish", "Firelink Shrine - After Undead Parish Elevator")
        create_connection("Undead Parish", "Darkroot Garden")
        create_connection("Undead Parish", "Sen's Fortress")

        create_connection("Darkroot Garden", "Darkroot Basin")
        create_connection("Darkroot Garden", "Darkroot Garden - Behind Artorias Door")

        create_connection("Undead Burg Basement Door", "Lower Undead Burg")
        create_connection("Lower Undead Burg", "Depths")
        create_connection("Lower Undead Burg", "Lower Undead Burg - After Residence Key")

        create_connection("Upper New Londo Ruins", "New Londo Ruins Door to the Seal")
        create_connection("New Londo Ruins Door to the Seal", "Lower New Londo Ruins")
        
        create_connection("Upper New Londo Ruins", "Door between Upper New Londo and Valley of the Drakes")
        create_connection("Door between Upper New Londo and Valley of the Drakes", "Valley of the Drakes")

        create_connection("Lower New Londo Ruins", "Valley of the Drakes")

        create_connection("Depths", "Depths - After Sewer Chamber Key")
        create_connection("Depths", "Depths to Blighttown Door")

        create_connection("Valley of the Drakes", "Blighttown")
        create_connection("Valley of the Drakes", "Darkroot Basin")
        create_connection("Valley of the Drakes", "Valley of the Drakes - After Defeating Four Kings")

        create_connection("Depths to Blighttown Door", "Blighttown")
        create_connection("Blighttown", "Depths to Blighttown Door")
        create_connection("Blighttown", "Demon Ruins")
        create_connection("Blighttown", "The Great Hollow")

        create_connection("The Great Hollow", "Ash Lake")

        create_connection("Sen's Fortress", "Sen's Fortress - After Cage Key")
        create_connection("Sen's Fortress", "Anor Londo")

        create_connection("Anor Londo", "The Duke's Archives")
        create_connection("Anor Londo", "Painted World of Ariamis")
        create_connection("Painted World of Ariamis", "Painted World of Ariamis - After Annex Key")

        create_connection("The Duke's Archives", "The Duke's Archives Cell Door")
        create_connection("The Duke's Archives Cell Door", "The Duke's Archives - Getting out of Cell")
        create_connection("The Duke's Archives - Getting out of Cell", "The Duke's Archives - After Archive Prison Extra Key")
        create_connection("The Duke's Archives - After Archive Prison Extra Key", "The Duke's Archives - After Archive Tower Giant Door Key")
        create_connection("The Duke's Archives - Getting out of Cell", "The Duke's Archives - Giant Cell")
        create_connection("The Duke's Archives - After Archive Tower Giant Door Key", "Crystal Cave")
        create_connection("The Duke's Archives", "The Duke's Archives - First Arena after Seath's Death")
        create_connection("Crystal Cave", "The Duke's Archives - First Arena after Seath's Death")

        create_connection("The Catacombs", "The Catacombs - Door 1")
        create_connection("The Catacombs - Door 1", "The Catacombs - After Door 1")
        create_connection("The Catacombs - After Door 1", "Tomb of the Giants")
        create_connection("Tomb of the Giants", "Tomb of the Giants - Behind Golden Fog Wall")

        create_connection("Lower New Londo Ruins", "The Abyss")

        create_connection("Demon Ruins", "Demon Ruins - Behind Golden Fog Wall")
        create_connection("Demon Ruins - Behind Golden Fog Wall", "Lost Izalith")
        create_connection("Demon Ruins - Behind Golden Fog Wall", "Demon Ruins Shortcut")
        create_connection("Lost Izalith", "Demon Ruins Shortcut")


        # DLC Entrances
        #create_connection("Darkroot Basin", "Sanctuary Garden")
        #create_connection("Sanctuary Garden", "Oolacile Sanctuary")
        #create_connection("Oolacile Sanctuary", "Royal Wood")
        #create_connection("Royal Wood", "Oolacile Township")
        #create_connection("Oolacile Township", "Oolacile Township - After Crest Key")
        #create_connection("Oolacile Township - After Crest Key", "Royal Wood - After Hawkeye Gough")
        #create_connection("Oolacile Township", "Chasm of the Abyss")
      
        
    # For each region, add the associated locations retrieved from the corresponding location_table
    def create_region(self, region_name, location_table) -> Region:
        new_region = Region(region_name, self.player, self.multiworld)
        #print("location table size: " + str(len(location_table)))
        for location in location_table:
            #print("Creating location: " + location.name)
            if location.category in self.enabled_location_categories and location.category not in location_skip_categories:# [DSRLocationCategory.EVENT, DSRLocationCategory.DOOR]:
                #print("Adding location: " + location.name + " with default item " + location.default_item)
                new_location = DSRLocation(
                    self.player,
                    location.name,
                    location.category,
                    location.default_item,
                    self.location_name_to_id[location.name],
                    new_region
                )
            else:
                # Replace non-randomized progression items with events
                event_item = self.create_item(location.default_item)
                #if event_item.classification != ItemClassification.progression:
                #    continue
                #print("Adding Location: " + location.name + " as an event with default item " + location.default_item)
                new_location = DSRLocation(
                    self.player,
                    location.name,
                    location.category,
                    location.default_item,
                    None,
                    new_region
                )
                event_item.code = None
                new_location.place_locked_item(event_item)
                #print("Placing event: " + event_item.name + " in location: " + location.name)

            new_region.locations.append(new_location)
        #print("created " + str(len(new_region.locations)) + " locations")
        self.multiworld.regions.append(new_region)
        #print("adding region: " + region_name)
        return new_region


    def create_items(self):
        skip_items: List[DSRItem] = []
        itempool: List[DSRItem] = []
        itempoolSize = 0
        
        #print("Creating items")
        for location in self.multiworld.get_locations(self.player):            
            item_data = item_dictionary[location.default_item_name]
            if item_data.category in [DSRItemCategory.SKIP] or location.category in location_skip_categories:# [DSRLocationCategory.EVENT]:                
                #print("Adding skip item: " + location.default_item_name)
                skip_items.append(self.create_item(location.default_item_name))
            elif location.category in self.enabled_location_categories:
                #print("Adding item: " + location.default_item_name)
                itempoolSize += 1
                itempool.append(self.create_item(location.default_item_name))
        
        #print("Requesting itempool size: " + str(itempoolSize))
        foo = BuildItemPool(itempoolSize, self.options)
        #print("Created item pool size: " + str(len(foo)))

        removable_items = [item for item in itempool if item.classification != ItemClassification.progression]
        #print("marked " + str(len(removable_items)) + " items as removable")
        
        for item in removable_items:
            #print("removable item: " + item.name)
            itempool.remove(item)
            itempool.append(self.create_item(foo.pop().name))

        # Add regular items to itempool
        self.multiworld.itempool += itempool

        # Handle SKIP items separately
        for skip_item in skip_items:
            location = next(loc for loc in self.multiworld.get_locations(self.player) if loc.default_item_name == skip_item.name)
            location.place_locked_item(skip_item)
            #self.multiworld.itempool.append(skip_item)
            #print("Placing skip item: " + skip_item.name + " in location: " + location.name)
        
        #print("Final Item pool: ")
        #for item in self.multiworld.itempool:
            #print(item.name)


    def create_item(self, name: str) -> Item:
        useful_categories = {
        }
        data = self.item_name_to_id[name]

        if name in key_item_names or item_dictionary[name].category in [DSRItemCategory.EVENT, DSRItemCategory.KEY_ITEM]:
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
        set_rule(self.multiworld.get_entrance("Northern Undead Asylum -> Northern Undead Asylum F2 East Door", self.player), lambda state: state.has("Undead Asylum F2 East Key", self.player))
        set_rule(self.multiworld.get_entrance("Northern Undead Asylum - After F2 East Door -> Undead Asylum Big Pilgrim Door", self.player), lambda state: state.has("Big Pilgrim's Key", self.player))

        set_rule(self.multiworld.get_entrance("Upper Undead Burg -> Undead Burg Basement Door", self.player), lambda state:state.has("Taurus Demon Defeated", self.player) and state.has ("Basement Key", self.player))
        set_rule(self.multiworld.get_entrance("Upper Undead Burg -> Upper Undead Burg - Pine Resin Chest", self.player), lambda state: state.has("Master Key", self.player) or state.has("Residence Key", self.player))        
        set_rule(self.multiworld.get_entrance("Upper Undead Burg -> Undead Parish", self.player), lambda state: state.has("Taurus Demon Defeated", self.player))
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
        set_rule(self.multiworld.get_entrance("Northern Undead Asylum - Second Visit Snuggly Trades -> Northern Undead Asylum - Second Visit F2 West Door", self.player), lambda state: state.has("Undead Asylum F2 West Key", self.player))
        set_rule(self.multiworld.get_entrance("Darkroot Garden -> Darkroot Garden - Behind Artorias Door", self.player), lambda state: state.has("Crest of Artorias", self.player))
        set_rule(self.multiworld.get_entrance("Lower Undead Burg -> Depths", self.player), lambda state: state.has("Key to Depths", self.player))
        set_rule(self.multiworld.get_entrance("Lower Undead Burg -> Lower Undead Burg - After Residence Key", self.player), lambda state: state.has("Residence Key", self.player))
        set_rule(self.multiworld.get_entrance("Upper New Londo Ruins -> Door between Upper New Londo and Valley of the Drakes", self.player), lambda state: state.has("Key to New Londo Ruins", self.player) or state.has("Master Key", self.player))
        set_rule(self.multiworld.get_entrance("Door between Upper New Londo and Valley of the Drakes -> Valley of the Drakes", self.player), lambda state: state.has("Key to New Londo Ruins", self.player) or state.has("Master Key", self.player))

        set_rule(self.multiworld.get_entrance("Depths -> Depths - After Sewer Chamber Key", self.player), lambda state: state.has("Sewer Chamber Key", self.player))
        set_rule(self.multiworld.get_entrance("Depths -> Depths to Blighttown Door", self.player), lambda state: state.has("Blighttown Key", self.player))
        set_rule(self.multiworld.get_entrance("Blighttown -> Depths to Blighttown Door", self.player), lambda state: state.has("Depths -> Blighttown opened", self.player))
        set_rule(self.multiworld.get_entrance("Blighttown -> Demon Ruins", self.player), lambda state: state.has("Chaos Witch Quelaag Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Blighttown -> The Great Hollow", self.player), lambda state: state.has("Lordvessel", self.player))
        
        set_rule(self.multiworld.get_location("UP: Bell of Awakening #1 rung", self.player), lambda state: state.has("Bell Gargoyles Defeated", self.player))
        set_rule(self.multiworld.get_location("BT: Bell of Awakening #2 rung", self.player), lambda state: state.has("Chaos Witch Quelaag Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Undead Parish -> Sen's Fortress", self.player), lambda state: state.has("Bell of Awakening #1", self.player) and state.has("Bell of Awakening #2", self.player))
        set_rule(self.multiworld.get_entrance("Sen's Fortress -> Sen's Fortress - After Cage Key", self.player), lambda state: state.has("Cage Key", self.player))
        set_rule(self.multiworld.get_entrance("Sen's Fortress -> Anor Londo", self.player), lambda state: state.has("Iron Golem Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Anor Londo -> The Duke's Archives", self.player), lambda state: state.has("Lordvessel", self.player))
        
        
        set_rule(self.multiworld.get_entrance("Upper New Londo Ruins -> New Londo Ruins Door to the Seal", self.player), lambda state: state.has("Ornstein and Smough Defeated", self.player) and state.has("Key to the Seal", self.player))
        set_rule(self.multiworld.get_entrance("Valley of the Drakes -> Valley of the Drakes - After Defeating Four Kings", self.player), lambda state: state.has("Four Kings Defeated", self.player))
                
        set_rule(self.multiworld.get_entrance("The Duke's Archives -> The Duke's Archives Cell Door", self.player), lambda state: state.has("Archive Tower Cell Key", self.player))
        set_rule(self.multiworld.get_entrance("The Duke's Archives - Getting out of Cell -> The Duke's Archives - After Archive Prison Extra Key", self.player), lambda state: state.has("Archive Prison Extra Key", self.player))
        set_rule(self.multiworld.get_entrance("The Duke's Archives - After Archive Prison Extra Key -> The Duke's Archives - After Archive Tower Giant Door Key", self.player), lambda state: state.has("Archive Tower Giant Door Key", self.player))
        set_rule(self.multiworld.get_entrance("The Duke's Archives - Getting out of Cell -> The Duke's Archives - Giant Cell", self.player), lambda state: state.has("Archive Tower Giant Cell Key", self.player))
        set_rule(self.multiworld.get_entrance("The Duke's Archives -> The Duke's Archives - First Arena after Seath's Death", self.player), lambda state: state.has("Seath the Scaleless Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Anor Londo -> Painted World of Ariamis", self.player), lambda state: state.has("Peculiar Doll", self.player))
        set_rule(self.multiworld.get_entrance("Painted World of Ariamis -> Painted World of Ariamis - After Annex Key", self.player), lambda state: state.has("Annex Key", self.player))
        set_rule(self.multiworld.get_entrance("Firelink Shrine -> The Catacombs", self.player), lambda state: state.has("Ornstein and Smough Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Lower New Londo Ruins -> The Abyss", self.player), lambda state: state.has("Covenant of Artorias", self.player))
        set_rule(self.multiworld.get_entrance("Demon Ruins -> Demon Ruins - Behind Golden Fog Wall", self.player), lambda state: state.has("Lordvessel", self.player))
        set_rule(self.multiworld.get_entrance("Lost Izalith -> Demon Ruins Shortcut", self.player), lambda state: state.has("Bed of Chaos Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Demon Ruins - Behind Golden Fog Wall -> Demon Ruins Shortcut", self.player), lambda state: state.has("Demon Ruins Shortcut opened", self.player))
        set_rule(self.multiworld.get_entrance("Demon Ruins - Behind Golden Fog Wall -> Lost Izalith", self.player), lambda state: state.has("Orange Charred Ring", self.player) and state.has("Centipede Demon Defeated", self.player))
        set_rule(self.multiworld.get_entrance("The Catacombs - After Door 1 -> Tomb of the Giants", self.player), lambda state: state.has("Ornstein and Smough Defeated", self.player))
        set_rule(self.multiworld.get_entrance("Tomb of the Giants -> Tomb of the Giants - Behind Golden Fog Wall", self.player), lambda state: state.has("Lordvessel", self.player))
        set_rule(self.multiworld.get_entrance("Firelink Shrine -> Kiln of the First Flame", self.player), lambda state: state.has("Lord Soul (Bed of Chaos)", self.player) and state.has("Lord Soul (Nito)", self.player) and state.has("Bequeathed Lord Soul Shard (Four Kings)", self.player) and state.has("Bequeathed Lord Soul Shard (Seath)", self.player) and state.has("Lordvessel", self.player))
        
      
        
        #set_rule(self.multiworld.get_entrance("Darkroot Basin -> Sanctuary Garden", self.player), lambda state: state.has("Broken Pendant", self.player))
        #set_rule(self.multiworld.get_entrance("Sanctuary Garden -> Oolacile Sanctuary", self.player), lambda state: state.has("Sanctuary Guardian Defeated", self.player))
        #set_rule(self.multiworld.get_entrance("Royal Wood -> Oolacile Township", self.player), lambda state: state.has("Artorias the Abysswalker Defeated", self.player))
        #set_rule(self.multiworld.get_entrance("Oolacile Township -> Oolacile Township - After Crest Key", self.player), lambda state: state.has("Crest Key", self.player))
        
 
        
    def fill_slot_data(self) -> Dict[str, object]:
        slot_data: Dict[str, object] = {}
        name_to_dsr_code = {item.name: item.dsr_code for item in item_dictionary.values()}
        # Create the mandatory lists to generate the player's output file
        items_id = []
        items_address = []
        locations_id = []
        locations_address = []
        locations_target = []
        for location in self.multiworld.get_filled_locations():
            if location.item.player == self.player:
                #we are the receiver of the item
                items_id.append(location.item.code)
                items_address.append(name_to_dsr_code[location.item.name])


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
                # "enable_deathlink": self.options.enable_deathlink.value,
                "enable_masterkey": self.options.enable_masterkey.value,
                "unique_souls": self.options.unique_souls.value
            },
            "seed": self.multiworld.seed_name,  # to verify the server's multiworld
            "slot": self.multiworld.player_name[self.player],  # to connect to server
            "base_id": self.base_id,  # to merge location and items lists
            "locationsId": locations_id,
            "locationsAddress": locations_address,
            "locationsTarget": locations_target,
            "itemsId": items_id,
            "itemsAddress": items_address
        }

        return slot_data

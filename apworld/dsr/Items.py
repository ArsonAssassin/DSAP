from enum import IntEnum
from typing import NamedTuple
import random
from BaseClasses import Item


class DSRItemCategory(IntEnum):
    SKIP = 0,
    EVENT = 1,
    CONSUMABLE = 2
    KEY_ITEM = 3
    RING = 4
    UPGRADE_MATERIAL = 5


class DSRItemData(NamedTuple):
    name: str
    dsr_code: int
    category: DSRItemCategory


class DSRItem(Item):
    game: str = "Dark Souls Remastered"

    @staticmethod
    def get_name_to_id() -> dict:
        base_id = 11110000
        return {item_data.name: (base_id + item_data.dsr_code if item_data.dsr_code is not None else None) for item_data in _all_items}


key_item_names = {
}


_all_items = [DSRItemData(row[0], row[1], row[2]) for row in [    
   
    ("Asylum Demon Defeated", 1000, DSRItemCategory.EVENT),
    ("Tauros Demon Defeated", 1001, DSRItemCategory.EVENT),
    ("Bell Gargoyles Defeated", 1002, DSRItemCategory.EVENT),
    ("Capra Demon Defeated", 1003, DSRItemCategory.EVENT),
    ("Ceaseless Discharge Defeated", 1004, DSRItemCategory.EVENT),
    ("Centipede Demon Defeated", 1005, DSRItemCategory.EVENT),
    ("Chaos Witch Quelaag Defeated", 1006, DSRItemCategory.EVENT),
    ("Crossbreed Priscilla Defeated", 1007, DSRItemCategory.EVENT),
    ("Demon Firesage Defeated", 1008, DSRItemCategory.EVENT),
    ("Ornstein and Smough Defeated", 1009, DSRItemCategory.EVENT),
    ("Four Kings Defeated", 1010, DSRItemCategory.EVENT),
    ("Gaping Dragon Defeated", 1011, DSRItemCategory.EVENT),
    ("Gravelord Nito Defeated", 1012, DSRItemCategory.EVENT),
    ("Great Grey Wolf Sif Defeated", 1013, DSRItemCategory.EVENT),
    ("Gwyn, Lord of Cinder Defeated", 1014, DSRItemCategory.EVENT),
    ("Iron Golem Defeated", 1015, DSRItemCategory.EVENT),
    ("Moonlight Butterfly Defeated", 1016, DSRItemCategory.EVENT),
    ("Pinwheel Defeated", 1017, DSRItemCategory.EVENT),
    ("Seath the Scaleless Defeated", 1018, DSRItemCategory.EVENT),
    ("Black Dragon Kalameet Defeated", 1019, DSRItemCategory.EVENT),
    ("Bed of Chaos Defeated", 1020, DSRItemCategory.EVENT),
    ("Manus, Father of the Abyss Defeated", 1021, DSRItemCategory.EVENT),
    ("Knight Artorias Defeated", 1022, DSRItemCategory.EVENT),    

    ("Eye of Death", 109, DSRItemCategory.CONSUMABLE),
    ("Cracked Red Eye Orb", 111, DSRItemCategory.CONSUMABLE),
    ("Elizabeth's Mushroom", 230, DSRItemCategory.CONSUMABLE),
    ("Divine Blessing", 240, DSRItemCategory.CONSUMABLE),
    ("Green Blossom", 260, DSRItemCategory.CONSUMABLE),
    ("Bloodred Moss Clump", 270, DSRItemCategory.CONSUMABLE),
    ("Purple Moss Clump", 271, DSRItemCategory.CONSUMABLE),
    ("Blooming Purple Moss Clump", 272, DSRItemCategory.CONSUMABLE),
    ("Purging Stone", 274, DSRItemCategory.CONSUMABLE),
    ("Egg Vermifuge", 275, DSRItemCategory.CONSUMABLE),
    ("Repair Powder", 280, DSRItemCategory.CONSUMABLE),
    ("Throwing Knife", 290, DSRItemCategory.CONSUMABLE),
    ("Poison Throwing Knife", 291, DSRItemCategory.CONSUMABLE),
    ("Firebomb", 292, DSRItemCategory.CONSUMABLE),
    ("Dung Pie", 293, DSRItemCategory.CONSUMABLE),
    ("Alluring Skull", 294, DSRItemCategory.CONSUMABLE),
    ("Lloyd's Talisman", 296, DSRItemCategory.CONSUMABLE),
    ("Black Firebomb", 297, DSRItemCategory.CONSUMABLE),
    ("Charcoal Pine Resin", 310, DSRItemCategory.CONSUMABLE),
    ("Gold Pine Resin", 311, DSRItemCategory.CONSUMABLE),
    ("Transient Curse", 312, DSRItemCategory.CONSUMABLE),
    ("Rotten Pine Resin", 313, DSRItemCategory.CONSUMABLE),
    ("Homeward Bone", 330, DSRItemCategory.CONSUMABLE),
    ("Prism Stone", 370, DSRItemCategory.CONSUMABLE),
    ("Indictment", 373, DSRItemCategory.CONSUMABLE),
    ("Souvenir of Reprisal", 374, DSRItemCategory.CONSUMABLE),
    ("Sunlight Medal", 375, DSRItemCategory.CONSUMABLE),
    ("Pendant", 376, DSRItemCategory.CONSUMABLE),
    ("Rubbish", 380, DSRItemCategory.CONSUMABLE),
    ("Copper Coin", 381, DSRItemCategory.CONSUMABLE),
    ("Silver Coin", 382, DSRItemCategory.CONSUMABLE),
    ("Gold Coin", 383, DSRItemCategory.CONSUMABLE),
    ("Fire Keeper Soul (Anastacia of Astora)", 390, DSRItemCategory.CONSUMABLE),
    ("Fire Keeper Soul (Darkmoon Knightess)", 391, DSRItemCategory.CONSUMABLE),
    ("Fire Keeper Soul (Daughter of Chaos)", 392, DSRItemCategory.CONSUMABLE),
    ("Fire Keeper Soul (New Londo)", 393, DSRItemCategory.CONSUMABLE),
    ("Fire Keeper Soul (Blighttown)", 394, DSRItemCategory.CONSUMABLE),
    ("Fire Keeper Soul (Duke's Archives)", 395, DSRItemCategory.CONSUMABLE),
    ("Fire Keeper Soul (Undead Parish)", 396, DSRItemCategory.CONSUMABLE),
    ("Soul of a Lost Undead", 400, DSRItemCategory.CONSUMABLE),
    ("Large Soul of a Lost Undead", 401, DSRItemCategory.CONSUMABLE),
    ("Soul of a Nameless Soldier", 402, DSRItemCategory.CONSUMABLE),
    ("Large Soul of a Nameless Soldier", 403, DSRItemCategory.CONSUMABLE),
    ("Soul of a Proud Knight", 404, DSRItemCategory.CONSUMABLE),
    ("Large Soul of a Proud Knight", 405, DSRItemCategory.CONSUMABLE),
    ("Soul of a Brave Warrior", 406, DSRItemCategory.CONSUMABLE),
    ("Large Soul of a Brave Warrior", 407, DSRItemCategory.CONSUMABLE),
    ("Soul of a Hero", 408, DSRItemCategory.CONSUMABLE),
    ("Soul of a Great Hero", 409, DSRItemCategory.CONSUMABLE),
    ("Humanity", 500, DSRItemCategory.CONSUMABLE),
    ("Twin Humanities", 501, DSRItemCategory.CONSUMABLE),
    ("Soul of Quelaag", 700, DSRItemCategory.CONSUMABLE),
    ("Soul of Sif", 701, DSRItemCategory.CONSUMABLE),
    ("Soul of Gwyn, Lord of Cinder", 702, DSRItemCategory.CONSUMABLE),
    ("Core of an Iron Golem", 703, DSRItemCategory.CONSUMABLE),
    ("Soul of Ornstein", 704, DSRItemCategory.CONSUMABLE),
    ("Soul of Moonlight Butterfly", 705, DSRItemCategory.CONSUMABLE),
    ("Soul of Smough", 706, DSRItemCategory.CONSUMABLE),
    ("Soul of Priscilla", 707, DSRItemCategory.CONSUMABLE),
    ("Soul of Gwyndolin", 708, DSRItemCategory.CONSUMABLE),
    ("Guardian Soul", 709, DSRItemCategory.CONSUMABLE),
    ("Soul of Artorias", 710, DSRItemCategory.CONSUMABLE),
    ("Soul of Manus", 711, DSRItemCategory.CONSUMABLE),

    ("Peculiar Doll", 384, DSRItemCategory.KEY_ITEM),
    ("Basement Key", 2001, DSRItemCategory.KEY_ITEM),
    ("Crest of Artorias", 2002, DSRItemCategory.KEY_ITEM),
    ("Cage Key", 2003, DSRItemCategory.KEY_ITEM),
    ("Archive Tower Cell Key", 2004, DSRItemCategory.KEY_ITEM),
    ("Archive Tower Giant Door Key", 2005, DSRItemCategory.KEY_ITEM),
    ("Archive Tower Giant Cell Key", 2006, DSRItemCategory.KEY_ITEM),
    ("Blighttown Key", 2007, DSRItemCategory.KEY_ITEM),
    ("Key to New Londo Ruins", 2008, DSRItemCategory.KEY_ITEM),
    ("Annex Key", 2009, DSRItemCategory.KEY_ITEM),
    ("Dungeon Cell Key", 2010, DSRItemCategory.KEY_ITEM),
    ("Big Pilgrim's Key", 2011, DSRItemCategory.KEY_ITEM),
    ("Undead Asylum F2 East Key", 2012, DSRItemCategory.KEY_ITEM),
    ("Key to the Seal", 2013, DSRItemCategory.KEY_ITEM),
    ("Key to Depths", 2014, DSRItemCategory.KEY_ITEM),
    ("Undead Asylum F2 West Key", 2016, DSRItemCategory.KEY_ITEM),
    ("Mystery Key", 2017, DSRItemCategory.KEY_ITEM),
    ("Sewer Chamber Key", 2018, DSRItemCategory.KEY_ITEM),
    ("Watchtower Basement Key", 2019, DSRItemCategory.KEY_ITEM),
    ("Archive Prison Extra Key", 2020, DSRItemCategory.KEY_ITEM),
    ("Residence Key", 2021, DSRItemCategory.KEY_ITEM),
    ("Crest Key", 2022, DSRItemCategory.KEY_ITEM),
    ("Master Key", 2100, DSRItemCategory.KEY_ITEM),
    ("Lord Soul (Nito)", 2500, DSRItemCategory.KEY_ITEM),
    ("Lord Soul (Bed of Chaos)", 2501, DSRItemCategory.KEY_ITEM),
    ("Bequeathed Lord Soul Shard (Four Kings)", 2502, DSRItemCategory.KEY_ITEM),
    ("Bequeathed Lord Soul Shard (Seath)", 2503, DSRItemCategory.KEY_ITEM),
    ("Lordvessel", 2510, DSRItemCategory.KEY_ITEM),
    ("Broken Pendant", 2520, DSRItemCategory.KEY_ITEM),
    ("Weapon Smithbox", 2600, DSRItemCategory.KEY_ITEM),
    ("Armor Smithbox", 2601, DSRItemCategory.KEY_ITEM),
    ("Repairbox", 2602, DSRItemCategory.KEY_ITEM),
    ("Rite of Kindling", 2607, DSRItemCategory.KEY_ITEM),
    ("Bottomless Box", 2608, DSRItemCategory.KEY_ITEM),

    ("Havel's Ring", 100, DSRItemCategory.RING),
    ("Red Tearstone Ring", 101, DSRItemCategory.RING),
    ("Darkmoon Blade Covenant Ring", 102, DSRItemCategory.RING),
    ("Cat Covenant Ring", 103, DSRItemCategory.RING),
    ("Cloranthy Ring", 104, DSRItemCategory.RING),
    ("Flame Stoneplate Ring", 105, DSRItemCategory.RING),
    ("Thunder Stoneplate Ring", 106, DSRItemCategory.RING),
    ("Spell Stoneplate Ring", 107, DSRItemCategory.RING),
    ("Speckled Stoneplate Ring", 108, DSRItemCategory.RING),
    ("Bloodbite Ring", 109, DSRItemCategory.RING),
    ("Poisonbite Ring", 110, DSRItemCategory.RING),
    ("Tiny Being's Ring", 111, DSRItemCategory.RING),
    ("Cursebite Ring", 113, DSRItemCategory.RING),
    ("White Seance Ring", 114, DSRItemCategory.RING),
    ("Bellowing Dragoncrest Ring", 115, DSRItemCategory.RING),
    ("Dusk Crown Ring", 116, DSRItemCategory.RING),
    ("Hornet Ring", 117, DSRItemCategory.RING),
    ("Hawk Ring", 119, DSRItemCategory.RING),
    ("Ring of Steel Protection", 120, DSRItemCategory.RING),
    ("Covetous Gold Serpent Ring", 121, DSRItemCategory.RING),
    ("Covetous Silver Serpent Ring", 122, DSRItemCategory.RING),
    ("Slumbering Dragoncrest Ring", 123, DSRItemCategory.RING),
    ("Ring of Fog", 124, DSRItemCategory.RING),
    ("Rusted Iron Ring", 125, DSRItemCategory.RING),
    ("Ring of Sacrifice", 126, DSRItemCategory.RING),
    ("Rare Ring of Sacrifice", 127, DSRItemCategory.RING),
    ("Dark Wood Grain Ring", 128, DSRItemCategory.RING),
    ("Ring of the Sun Princess", 130, DSRItemCategory.RING),
    ("Old Witch's Ring", 137, DSRItemCategory.RING),
    ("Covenant of Artorias", 138, DSRItemCategory.RING),
    ("Orange Charred Ring", 139, DSRItemCategory.RING),
    ("Lingering Dragoncrest Ring", 141, DSRItemCategory.RING),
    ("Ring of the Evil Eye", 142, DSRItemCategory.RING),
    ("Ring of Favor and Protection", 143, DSRItemCategory.RING),
    ("Leo Ring", 144, DSRItemCategory.RING),
    ("East Wood Grain Ring", 145, DSRItemCategory.RING),
    ("Wolf Ring", 146, DSRItemCategory.RING),
    ("Blue Tearstone Ring", 147, DSRItemCategory.RING),
    ("Ring of the Sun's Firstborn", 148, DSRItemCategory.RING),
    ("Darkmoon Seance Ring", 149, DSRItemCategory.RING),
    ("Calamity Ring", 150, DSRItemCategory.RING),

    ("Large Ember", 800, DSRItemCategory.UPGRADE_MATERIAL),
    ("Very Large Ember", 801, DSRItemCategory.UPGRADE_MATERIAL),
    ("Crystal Ember", 802, DSRItemCategory.UPGRADE_MATERIAL),
    ("Large Magic Ember", 806, DSRItemCategory.UPGRADE_MATERIAL),
    ("Enchanted Ember", 807, DSRItemCategory.UPGRADE_MATERIAL),
    ("Divine Ember", 808, DSRItemCategory.UPGRADE_MATERIAL),
    ("Large Divine Ember", 809, DSRItemCategory.UPGRADE_MATERIAL),
    ("Dark Ember", 810, DSRItemCategory.UPGRADE_MATERIAL),
    ("Large Flame Ember", 812, DSRItemCategory.UPGRADE_MATERIAL),
    ("Chaos Flame Ember", 813, DSRItemCategory.UPGRADE_MATERIAL),
    ("Titanite Shard", 1000, DSRItemCategory.UPGRADE_MATERIAL),
    ("Large Titanite Shard", 1010, DSRItemCategory.UPGRADE_MATERIAL),
    ("Green Titanite Shard", 1020, DSRItemCategory.UPGRADE_MATERIAL),
    ("Titanite Chunk", 1030, DSRItemCategory.UPGRADE_MATERIAL),
    ("Blue Titanite Chunk", 1040, DSRItemCategory.UPGRADE_MATERIAL),
    ("White Titanite Chunk", 1050, DSRItemCategory.UPGRADE_MATERIAL),
    ("Red Titanite Chunk", 1060, DSRItemCategory.UPGRADE_MATERIAL),
    ("Titanite Slab", 1070, DSRItemCategory.UPGRADE_MATERIAL),
    ("Blue Titanite Slab", 1080, DSRItemCategory.UPGRADE_MATERIAL),
    ("White Titanite Slab", 1090, DSRItemCategory.UPGRADE_MATERIAL),
    ("Red Titanite Slab", 1100, DSRItemCategory.UPGRADE_MATERIAL),
    ("Dragon Scale", 1110, DSRItemCategory.UPGRADE_MATERIAL),
    ("Demon Titanite", 1120, DSRItemCategory.UPGRADE_MATERIAL),
    ("Twinkling Titanite", 1130, DSRItemCategory.UPGRADE_MATERIAL),
]]

item_descriptions = {
}

item_dictionary = {item_data.name: item_data for item_data in _all_items}

def BuildItemPool(count, options):
    item_pool = []
    included_itemcount = 0

    if options.guaranteed_items.value:
        for item_name in options.guaranteed_items.value:
            item = item_dictionary[item_name]
            item_pool.append(item)
            included_itemcount = included_itemcount + 1
    remaining_count = count - included_itemcount
    
    filler_items = [item for item in _all_items if item.category != DSRItemCategory.EVENT]

    for i in range(remaining_count):
        itemList = [item for item in filler_items]
        item = random.choice(itemList)
        item_pool.append(item)
    
    random.shuffle(item_pool)
    return item_pool

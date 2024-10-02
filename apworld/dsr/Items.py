from enum import IntEnum
from typing import NamedTuple
import random
from BaseClasses import Item


class DSRItemCategory(IntEnum):
    SKIP = 0,
    EVENT = 1,
    CONSUMABLE = 2


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

    ("Firebomb", 2000, DSRItemCategory.CONSUMABLE),
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

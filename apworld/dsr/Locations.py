from enum import IntEnum
from typing import Optional, NamedTuple, Dict

from BaseClasses import Location, Region
from .Items import DSRItem

class DSRLocationCategory(IntEnum):
    SKIP = 0,
    EVENT = 1,
    BOSS = 2,


class DSRLocationData(NamedTuple):
    name: str
    default_item: str
    category: DSRLocationCategory


class DSRLocation(Location):
    game: str = "Dark Souls Remastered"
    category: DSRLocationCategory
    default_item_name: str

    def __init__(
            self,
            player: int,
            name: str,
            category: DSRLocationCategory,
            default_item_name: str,
            address: Optional[int] = None,
            parent: Optional[Region] = None):
        super().__init__(player, name, address, parent)
        self.default_item_name = default_item_name
        self.category = category

    @staticmethod
    def get_name_to_id() -> dict:
        base_id = 11110000
        table_offset = 1000

        table_order = [
            "Bosses", "Bonfires"
         ]

        output = {}
        for i, region_name in enumerate(table_order):
            if len(location_tables[region_name]) > table_offset:
                raise Exception("A location table has {} entries, that is more than {} entries (table #{})".format(len(location_tables[region_name]), table_offset, i))

            output.update({location_data.name: id for id, location_data in enumerate(location_tables[region_name], base_id + (table_offset * i))})

        return output

    def place_locked_item(self, item: DSRItem):
        self.item = item
        self.locked = True
        item.location = self

location_tables = {
"Bosses": [
    DSRLocationData(f"Asylum Demon Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Tauros Demon Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Bell Gargoyles Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Capra Demon Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Ceaseless Discharge Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Centipede Demon Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Chaos Witch Quelaag Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Crossbreed Priscilla Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Demon Firesage Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Ornstein and Smough Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Four Kings Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Gaping Dragon Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Gravelord Nito Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Great Grey Wolf Sif Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Gwyn, Lord of Cinder Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Iron Golem Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Moonlight Butterfly Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Pinwheel Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Seath the Scaleless Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Black Dragon Kalameet Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Bed of Chaos Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Manus, Father of the Abyss Defeated", f"Firebomb", DSRLocationCategory.BOSS),
    DSRLocationData(f"Knight Artorias Defeated", f"Firebomb", DSRLocationCategory.BOSS),
],
"Bonfires": [

]


}

location_dictionary: Dict[str, DSRLocationData] = {}
for location_table in location_tables.values():
    location_dictionary.update({location_data.name: location_data for location_data in location_table})

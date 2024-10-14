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
    SPELL = 6
    ARMOR = 7
    WEAPON = 8
    SHIELD = 9


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
    ("Sanctuary Guardian Defeated", 1023, DSRItemCategory.EVENT),    
    ("Gwyndolin Defeated", 1024, DSRItemCategory.EVENT),    

    ("Eye of Death", 2000, DSRItemCategory.CONSUMABLE),
    ("Cracked Red Eye Orb", 2001, DSRItemCategory.CONSUMABLE),
    ("Elizabeth's Mushroom", 2002, DSRItemCategory.CONSUMABLE),
    ("Divine Blessing", 2003, DSRItemCategory.CONSUMABLE),
    ("Green Blossom", 2004, DSRItemCategory.CONSUMABLE),
    ("Bloodred Moss Clump", 2005, DSRItemCategory.CONSUMABLE),
    ("Purple Moss Clump", 2006, DSRItemCategory.CONSUMABLE),
    ("Blooming Purple Moss Clump", 2007, DSRItemCategory.CONSUMABLE),
    ("Purging Stone", 2008, DSRItemCategory.CONSUMABLE),
    ("Egg Vermifuge", 2009, DSRItemCategory.CONSUMABLE),
    ("Repair Powder", 2010, DSRItemCategory.CONSUMABLE),
    ("Throwing Knife", 2011, DSRItemCategory.CONSUMABLE),
    ("Poison Throwing Knife", 2012, DSRItemCategory.CONSUMABLE),
    ("Firebomb", 2013, DSRItemCategory.CONSUMABLE),
    ("Dung Pie", 2014, DSRItemCategory.CONSUMABLE),
    ("Alluring Skull", 2015, DSRItemCategory.CONSUMABLE),
    ("Lloyd's Talisman", 2016, DSRItemCategory.CONSUMABLE),
    ("Black Firebomb", 2017, DSRItemCategory.CONSUMABLE),
    ("Charcoal Pine Resin", 2018, DSRItemCategory.CONSUMABLE),
    ("Gold Pine Resin", 2019, DSRItemCategory.CONSUMABLE),
    ("Transient Curse", 2020, DSRItemCategory.CONSUMABLE),
    ("Rotten Pine Resin", 2021, DSRItemCategory.CONSUMABLE),
    ("Homeward Bone", 2022, DSRItemCategory.CONSUMABLE),
    ("Prism Stone", 2023, DSRItemCategory.CONSUMABLE),
    ("Indictment", 2024, DSRItemCategory.CONSUMABLE),
    ("Souvenir of Reprisal", 2025, DSRItemCategory.CONSUMABLE),
    ("Sunlight Medal", 2026, DSRItemCategory.CONSUMABLE),
    ("Pendant", 2027, DSRItemCategory.CONSUMABLE),
    ("Rubbish", 2028, DSRItemCategory.CONSUMABLE),
    ("Copper Coin", 2029, DSRItemCategory.CONSUMABLE),
    ("Silver Coin", 2030, DSRItemCategory.CONSUMABLE),
    ("Gold Coin", 2031, DSRItemCategory.CONSUMABLE),
    ("Fire Keeper Soul (Anastacia of Astora)", 2032, DSRItemCategory.CONSUMABLE),
    ("Fire Keeper Soul (Darkmoon Knightess)", 2033, DSRItemCategory.CONSUMABLE),
    ("Fire Keeper Soul (Daughter of Chaos)", 2034, DSRItemCategory.CONSUMABLE),
    ("Fire Keeper Soul (New Londo)", 2035, DSRItemCategory.CONSUMABLE),
    ("Fire Keeper Soul (Blighttown)", 2036, DSRItemCategory.CONSUMABLE),
    ("Fire Keeper Soul (Duke's Archives)", 2037, DSRItemCategory.CONSUMABLE),
    ("Fire Keeper Soul (Undead Parish)", 2038, DSRItemCategory.CONSUMABLE),
    ("Soul of a Lost Undead", 2039, DSRItemCategory.CONSUMABLE),
    ("Large Soul of a Lost Undead", 2040, DSRItemCategory.CONSUMABLE),
    ("Soul of a Nameless Soldier", 2041, DSRItemCategory.CONSUMABLE),
    ("Large Soul of a Nameless Soldier", 2042, DSRItemCategory.CONSUMABLE),
    ("Soul of a Proud Knight", 2043, DSRItemCategory.CONSUMABLE),
    ("Large Soul of a Proud Knight", 2044, DSRItemCategory.CONSUMABLE),
    ("Soul of a Brave Warrior", 2045, DSRItemCategory.CONSUMABLE),
    ("Large Soul of a Brave Warrior", 2046, DSRItemCategory.CONSUMABLE),
    ("Soul of a Hero", 2047, DSRItemCategory.CONSUMABLE),
    ("Soul of a Great Hero", 2048, DSRItemCategory.CONSUMABLE),
    ("Humanity", 2049, DSRItemCategory.CONSUMABLE),
    ("Twin Humanities", 2050, DSRItemCategory.CONSUMABLE),
    ("Soul of Quelaag", 2051, DSRItemCategory.CONSUMABLE),
    ("Soul of Sif", 2052, DSRItemCategory.CONSUMABLE),
    ("Soul of Gwyn, Lord of Cinder", 2053, DSRItemCategory.CONSUMABLE),
    ("Core of an Iron Golem", 2054, DSRItemCategory.CONSUMABLE),
    ("Soul of Ornstein", 2055, DSRItemCategory.CONSUMABLE),
    ("Soul of Moonlight Butterfly", 2056, DSRItemCategory.CONSUMABLE),
    ("Soul of Smough", 2057, DSRItemCategory.CONSUMABLE),
    ("Soul of Priscilla", 2058, DSRItemCategory.CONSUMABLE),
    ("Soul of Gwyndolin", 2059, DSRItemCategory.CONSUMABLE),
    ("Guardian Soul", 2060, DSRItemCategory.CONSUMABLE),
    ("Soul of Artorias", 2061, DSRItemCategory.CONSUMABLE),
    ("Soul of Manus", 2062, DSRItemCategory.CONSUMABLE),
    ("White Soapstone", 2063, DSRItemCategory.CONSUMABLE),
    ("Red Sign Soapstone", 2064, DSRItemCategory.CONSUMABLE),
    ("Red Eye Orb", 2065, DSRItemCategory.CONSUMABLE),
    ("Black Separation Crystal", 2066, DSRItemCategory.CONSUMABLE),
    ("Orange Guidance Soapstone", 2067, DSRItemCategory.CONSUMABLE),
    ("Book of the Guilty", 2068, DSRItemCategory.CONSUMABLE),
    ("Servant Roster", 2069, DSRItemCategory.CONSUMABLE),
    ("Blue Eye Orb", 2070, DSRItemCategory.CONSUMABLE),
    ("Dragon Eye", 2071, DSRItemCategory.CONSUMABLE),
    ("Black Eye Orb", 2072, DSRItemCategory.CONSUMABLE),
    ("Darksign", 2073, DSRItemCategory.CONSUMABLE),
    ("Purple Coward's Crystal", 2074, DSRItemCategory.CONSUMABLE),
    ("Silver Pendant", 2075, DSRItemCategory.CONSUMABLE),
    ("Dried Finger", 2076, DSRItemCategory.CONSUMABLE),
    ("Carving: HELLO!", 2077, DSRItemCategory.CONSUMABLE),
    ("Carving: THANK YOU!", 2078, DSRItemCategory.CONSUMABLE),
    ("Carving: VERY GOOD!", 2079, DSRItemCategory.CONSUMABLE),
    ("Carving: I'M SORRY!", 2080, DSRItemCategory.CONSUMABLE),
    ("Carving: HELP ME!", 2081, DSRItemCategory.CONSUMABLE),

    ("Peculiar Doll", 3000, DSRItemCategory.KEY_ITEM),
    ("Basement Key", 3001, DSRItemCategory.KEY_ITEM),
    ("Crest of Artorias", 3002, DSRItemCategory.KEY_ITEM),
    ("Cage Key", 3003, DSRItemCategory.KEY_ITEM),
    ("Archive Tower Cell Key", 3004, DSRItemCategory.KEY_ITEM),
    ("Archive Tower Giant Door Key", 3005, DSRItemCategory.KEY_ITEM),
    ("Archive Tower Giant Cell Key", 3006, DSRItemCategory.KEY_ITEM),
    ("Blighttown Key", 3007, DSRItemCategory.KEY_ITEM),
    ("Key to New Londo Ruins", 3008, DSRItemCategory.KEY_ITEM),
    ("Annex Key", 3009, DSRItemCategory.KEY_ITEM),
    ("Dungeon Cell Key", 3010, DSRItemCategory.KEY_ITEM),
    ("Big Pilgrim's Key", 3011, DSRItemCategory.KEY_ITEM),
    ("Undead Asylum F2 East Key", 3012, DSRItemCategory.KEY_ITEM),
    ("Key to the Seal", 3013, DSRItemCategory.KEY_ITEM),
    ("Key to Depths", 3014, DSRItemCategory.KEY_ITEM),
    ("Undead Asylum F2 West Key", 3015, DSRItemCategory.KEY_ITEM),
    ("Mystery Key", 3016, DSRItemCategory.KEY_ITEM),
    ("Sewer Chamber Key", 3017, DSRItemCategory.KEY_ITEM),
    ("Watchtower Basement Key", 3018, DSRItemCategory.KEY_ITEM),
    ("Archive Prison Extra Key", 3019, DSRItemCategory.KEY_ITEM),
    ("Residence Key", 3020, DSRItemCategory.KEY_ITEM),
    ("Crest Key", 3021, DSRItemCategory.KEY_ITEM),
    ("Master Key", 3022, DSRItemCategory.KEY_ITEM),
    ("Lord Soul (Nito)", 3023, DSRItemCategory.KEY_ITEM),
    ("Lord Soul (Bed of Chaos)", 3024, DSRItemCategory.KEY_ITEM),
    ("Bequeathed Lord Soul Shard (Four Kings)", 3025, DSRItemCategory.KEY_ITEM),
    ("Bequeathed Lord Soul Shard (Seath)", 3026, DSRItemCategory.KEY_ITEM),
    ("Lordvessel", 3027, DSRItemCategory.KEY_ITEM),
    ("Broken Pendant", 3028, DSRItemCategory.KEY_ITEM),
    ("Weapon Smithbox", 3029, DSRItemCategory.KEY_ITEM),
    ("Armor Smithbox", 3030, DSRItemCategory.KEY_ITEM),
    ("Repairbox", 3031, DSRItemCategory.KEY_ITEM),
    ("Rite of Kindling", 3032, DSRItemCategory.KEY_ITEM),
    ("Bottomless Box", 3033, DSRItemCategory.KEY_ITEM),

    ("Havel's Ring", 4000, DSRItemCategory.RING),
    ("Red Tearstone Ring", 4001, DSRItemCategory.RING),
    ("Darkmoon Blade Covenant Ring", 4002, DSRItemCategory.RING),
    ("Cat Covenant Ring", 4003, DSRItemCategory.RING),
    ("Cloranthy Ring", 4004, DSRItemCategory.RING),
    ("Flame Stoneplate Ring", 4005, DSRItemCategory.RING),
    ("Thunder Stoneplate Ring", 4006, DSRItemCategory.RING),
    ("Spell Stoneplate Ring", 4007, DSRItemCategory.RING),
    ("Speckled Stoneplate Ring", 4008, DSRItemCategory.RING),
    ("Bloodbite Ring", 4009, DSRItemCategory.RING),
    ("Poisonbite Ring", 4010, DSRItemCategory.RING),
    ("Tiny Being's Ring", 4011, DSRItemCategory.RING),
    ("Cursebite Ring", 4012, DSRItemCategory.RING),
    ("White Seance Ring", 4013, DSRItemCategory.RING),
    ("Bellowing Dragoncrest Ring", 4014, DSRItemCategory.RING),
    ("Dusk Crown Ring", 4015, DSRItemCategory.RING),
    ("Hornet Ring", 4016, DSRItemCategory.RING),
    ("Hawk Ring", 4017, DSRItemCategory.RING),
    ("Ring of Steel Protection", 4018, DSRItemCategory.RING),
    ("Covetous Gold Serpent Ring", 4019, DSRItemCategory.RING),
    ("Covetous Silver Serpent Ring", 4020, DSRItemCategory.RING),
    ("Slumbering Dragoncrest Ring", 4021, DSRItemCategory.RING),
    ("Ring of Fog", 4022, DSRItemCategory.RING),
    ("Rusted Iron Ring", 4023, DSRItemCategory.RING),
    ("Ring of Sacrifice", 4024, DSRItemCategory.RING),
    ("Rare Ring of Sacrifice", 4025, DSRItemCategory.RING),
    ("Dark Wood Grain Ring", 4026, DSRItemCategory.RING),
    ("Ring of the Sun Princess", 4027, DSRItemCategory.RING),
    ("Old Witch's Ring", 4028, DSRItemCategory.RING),
    ("Covenant of Artorias", 4029, DSRItemCategory.RING),
    ("Orange Charred Ring", 4030, DSRItemCategory.RING),
    ("Lingering Dragoncrest Ring", 4031, DSRItemCategory.RING),
    ("Ring of the Evil Eye", 4032, DSRItemCategory.RING),
    ("Ring of Favor and Protection", 4033, DSRItemCategory.RING),
    ("Leo Ring", 4034, DSRItemCategory.RING),
    ("East Wood Grain Ring", 4035, DSRItemCategory.RING),
    ("Wolf Ring", 4036, DSRItemCategory.RING),
    ("Blue Tearstone Ring", 4037, DSRItemCategory.RING),
    ("Ring of the Sun's Firstborn", 4038, DSRItemCategory.RING),
    ("Darkmoon Seance Ring", 4039, DSRItemCategory.RING),
    ("Calamity Ring", 4040, DSRItemCategory.RING),

    ("Large Ember", 5000, DSRItemCategory.UPGRADE_MATERIAL),
    ("Very Large Ember", 5001, DSRItemCategory.UPGRADE_MATERIAL),
    ("Crystal Ember", 5002, DSRItemCategory.UPGRADE_MATERIAL),
    ("Large Magic Ember", 5003, DSRItemCategory.UPGRADE_MATERIAL),
    ("Enchanted Ember", 5004, DSRItemCategory.UPGRADE_MATERIAL),
    ("Divine Ember", 5005, DSRItemCategory.UPGRADE_MATERIAL),
    ("Large Divine Ember", 5006, DSRItemCategory.UPGRADE_MATERIAL),
    ("Dark Ember", 5007, DSRItemCategory.UPGRADE_MATERIAL),
    ("Large Flame Ember", 5008, DSRItemCategory.UPGRADE_MATERIAL),
    ("Chaos Flame Ember", 5009, DSRItemCategory.UPGRADE_MATERIAL),
    ("Titanite Shard", 5010, DSRItemCategory.UPGRADE_MATERIAL),
    ("Large Titanite Shard", 5011, DSRItemCategory.UPGRADE_MATERIAL),
    ("Green Titanite Shard", 5012, DSRItemCategory.UPGRADE_MATERIAL),
    ("Titanite Chunk", 5013, DSRItemCategory.UPGRADE_MATERIAL),
    ("Blue Titanite Chunk", 5014, DSRItemCategory.UPGRADE_MATERIAL),
    ("White Titanite Chunk", 5015, DSRItemCategory.UPGRADE_MATERIAL),
    ("Red Titanite Chunk", 5016, DSRItemCategory.UPGRADE_MATERIAL),
    ("Titanite Slab", 5017, DSRItemCategory.UPGRADE_MATERIAL),
    ("Blue Titanite Slab", 5018, DSRItemCategory.UPGRADE_MATERIAL),
    ("White Titanite Slab", 5019, DSRItemCategory.UPGRADE_MATERIAL),
    ("Red Titanite Slab", 5020, DSRItemCategory.UPGRADE_MATERIAL),
    ("Dragon Scale", 5021, DSRItemCategory.UPGRADE_MATERIAL),
    ("Demon Titanite", 5022, DSRItemCategory.UPGRADE_MATERIAL),
    ("Twinkling Titanite", 5023, DSRItemCategory.UPGRADE_MATERIAL),

    ("Sorcery: Soul Arrow", 6000, DSRItemCategory.SPELL),
    ("Sorcery: Great Soul Arrow", 6001, DSRItemCategory.SPELL),
    ("Sorcery: Heavy Soul Arrow", 6002, DSRItemCategory.SPELL),
    ("Sorcery: Great Heavy Soul Arrow", 6003, DSRItemCategory.SPELL),
    ("Sorcery: Homing Soulmass", 6004, DSRItemCategory.SPELL),
    ("Sorcery: Homing Crystal Soulmass", 6005, DSRItemCategory.SPELL),
    ("Sorcery: Soul Spear", 6006, DSRItemCategory.SPELL),
    ("Sorcery: Crystal Soul Spear", 6007, DSRItemCategory.SPELL),
    ("Sorcery: Magic Weapon", 6008, DSRItemCategory.SPELL),
    ("Sorcery: Great Magic Weapon", 6009, DSRItemCategory.SPELL),
    ("Sorcery: Crystal Magic Weapon", 6010, DSRItemCategory.SPELL),
    ("Sorcery: Magic Shield", 6011, DSRItemCategory.SPELL),
    ("Sorcery: Strong Magic Shield", 6012, DSRItemCategory.SPELL),
    ("Sorcery: Hidden Weapon", 6013, DSRItemCategory.SPELL),
    ("Sorcery: Hidden Body", 6014, DSRItemCategory.SPELL),
    ("Sorcery: Cast Light", 6015, DSRItemCategory.SPELL),
    ("Sorcery: Hush", 6016, DSRItemCategory.SPELL),
    ("Sorcery: Aural Decoy", 6017, DSRItemCategory.SPELL),
    ("Sorcery: Repair", 6018, DSRItemCategory.SPELL),
    ("Sorcery: Fall Control", 6019, DSRItemCategory.SPELL),
    ("Sorcery: Chameleon", 6020, DSRItemCategory.SPELL),
    ("Sorcery: Resist Curse", 6021, DSRItemCategory.SPELL),
    ("Sorcery: Remedy", 6022, DSRItemCategory.SPELL),
    ("Sorcery: White Dragon Breath", 6023, DSRItemCategory.SPELL),
    ("Sorcery: Dark Orb", 6024, DSRItemCategory.SPELL),
    ("Sorcery: Dark Bead", 6025, DSRItemCategory.SPELL),
    ("Sorcery: Dark Fog", 6026, DSRItemCategory.SPELL),
    ("Sorcery: Pursuers", 6027, DSRItemCategory.SPELL),
    ("Pyromancy: Fireball", 6028, DSRItemCategory.SPELL),
    ("Pyromancy: Fire Orb", 6029, DSRItemCategory.SPELL),
    ("Pyromancy: Great Fireball", 6030, DSRItemCategory.SPELL),
    ("Pyromancy: Firestorm", 6031, DSRItemCategory.SPELL),
    ("Pyromancy: Fire Tempest", 6032, DSRItemCategory.SPELL),
    ("Pyromancy: Fire Surge", 6033, DSRItemCategory.SPELL),
    ("Pyromancy: Fire Whip", 6034, DSRItemCategory.SPELL),
    ("Pyromancy: Combustion", 6035, DSRItemCategory.SPELL),
    ("Pyromancy: Great Combustion", 6036, DSRItemCategory.SPELL),
    ("Pyromancy: Poison Mist", 6037, DSRItemCategory.SPELL),
    ("Pyromancy: Toxic Mist", 6038, DSRItemCategory.SPELL),
    ("Pyromancy: Acid Surge", 6039, DSRItemCategory.SPELL),
    ("Pyromancy: Iron Flesh", 6040, DSRItemCategory.SPELL),
    ("Pyromancy: Flash Sweat", 6041, DSRItemCategory.SPELL),
    ("Pyromancy: Undead Rapport", 6042, DSRItemCategory.SPELL),
    ("Pyromancy: Power Within", 6043, DSRItemCategory.SPELL),
    ("Pyromancy: Great Chaos Fireball", 6044, DSRItemCategory.SPELL),
    ("Pyromancy: Chaos Storm", 6045, DSRItemCategory.SPELL),
    ("Pyromancy: Chaos Fire Whip", 6046, DSRItemCategory.SPELL),
    ("Pyromancy: Black Flame", 6047, DSRItemCategory.SPELL),
    ("Miracle: Heal", 6048, DSRItemCategory.SPELL),
    ("Miracle: Great Heal", 6049, DSRItemCategory.SPELL),
    ("Miracle: Great Heal Excerpt", 6050, DSRItemCategory.SPELL),
    ("Miracle: Soothing Sunlight", 6051, DSRItemCategory.SPELL),
    ("Miracle: Replenishment", 6052, DSRItemCategory.SPELL),
    ("Miracle: Bountiful Sunlight", 6053, DSRItemCategory.SPELL),
    ("Miracle: Gravelord Sword Dance", 6054, DSRItemCategory.SPELL),
    ("Miracle: Gravelord Greatsword Dance", 6055, DSRItemCategory.SPELL),
    ("Miracle: Escape Death", 6056, DSRItemCategory.SPELL),
    ("Miracle: Homeward", 6057, DSRItemCategory.SPELL),
    ("Miracle: Force", 6058, DSRItemCategory.SPELL),
    ("Miracle: Wrath of the Gods", 6059, DSRItemCategory.SPELL),
    ("Miracle: Emit Force", 6060, DSRItemCategory.SPELL),
    ("Miracle: Seek Guidance", 6061, DSRItemCategory.SPELL),
    ("Miracle: Lightning Spear", 6062, DSRItemCategory.SPELL),
    ("Miracle: Great Lightning Spear", 6063, DSRItemCategory.SPELL),
    ("Miracle: Sunlight Spear", 6064, DSRItemCategory.SPELL),
    ("Miracle: Magic Barrier", 6065, DSRItemCategory.SPELL),
    ("Miracle: Great Magic Barrier", 6066, DSRItemCategory.SPELL),
    ("Miracle: Karmic Justice", 6067, DSRItemCategory.SPELL),
    ("Miracle: Tranquil Walk of Peace", 6068, DSRItemCategory.SPELL),
    ("Miracle: Vow of Silence", 6069, DSRItemCategory.SPELL),
    ("Miracle: Sunlight Blade", 6070, DSRItemCategory.SPELL),
    ("Miracle: Darkmoon Blade", 6071, DSRItemCategory.SPELL),

    ("Catarina Helm", 7000, DSRItemCategory.ARMOR),
    ("Catarina Armor", 7001, DSRItemCategory.ARMOR),
    ("Catarina Gauntlets", 7002, DSRItemCategory.ARMOR),
    ("Catarina Leggings", 7003, DSRItemCategory.ARMOR),
    ("Paladin Helm", 7004, DSRItemCategory.ARMOR),
    ("Paladin Armor", 7005, DSRItemCategory.ARMOR),
    ("Paladin Gauntlets", 7006, DSRItemCategory.ARMOR),
    ("Paladin Leggings", 7007, DSRItemCategory.ARMOR),
    ("Dark Mask", 7008, DSRItemCategory.ARMOR),
    ("Dark Armor", 7009, DSRItemCategory.ARMOR),
    ("Dark Gauntlets", 7010, DSRItemCategory.ARMOR),
    ("Dark Leggings", 7011, DSRItemCategory.ARMOR),
    ("Brigand Hood", 7012, DSRItemCategory.ARMOR),
    ("Brigand Armor", 7013, DSRItemCategory.ARMOR),
    ("Brigand Gauntlets", 7014, DSRItemCategory.ARMOR),
    ("Brigand Trousers", 7015, DSRItemCategory.ARMOR),
    ("Shadow Mask", 7016, DSRItemCategory.ARMOR),
    ("Shadow Garb", 7017, DSRItemCategory.ARMOR),
    ("Shadow Gauntlets", 7018, DSRItemCategory.ARMOR),
    ("Shadow Leggings", 7019, DSRItemCategory.ARMOR),
    ("Black Iron Helm", 7020, DSRItemCategory.ARMOR),
    ("Black Iron Armor", 7021, DSRItemCategory.ARMOR),
    ("Black Iron Gauntlets", 7022, DSRItemCategory.ARMOR),
    ("Black Iron Leggings", 7023, DSRItemCategory.ARMOR),
    ("Smough's Helm", 7024, DSRItemCategory.ARMOR),
    ("Smough's Armor", 7025, DSRItemCategory.ARMOR),
    ("Smough's Gauntlets", 7026, DSRItemCategory.ARMOR),
    ("Smough's Leggings", 7027, DSRItemCategory.ARMOR),
    ("Six-Eyed Helm of the Channelers", 7028, DSRItemCategory.ARMOR),
    ("Robe of the Channelers", 7029, DSRItemCategory.ARMOR),
    ("Gauntlets of the Channelers", 7030, DSRItemCategory.ARMOR),
    ("Waistcloth of the Channelers", 7031, DSRItemCategory.ARMOR),
    ("Helm of Favor", 7032, DSRItemCategory.ARMOR),
    ("Embraced Armor of Favor", 7033, DSRItemCategory.ARMOR),
    ("Gauntlets of Favor", 7034, DSRItemCategory.ARMOR),
    ("Leggings of Favor", 7035, DSRItemCategory.ARMOR),
    ("Helm of the Wise", 7036, DSRItemCategory.ARMOR),
    ("Armor of the Glorious", 7037, DSRItemCategory.ARMOR),
    ("Gauntlets of the Vanquisher", 7038, DSRItemCategory.ARMOR),
    ("Boots of the Explorer", 7039, DSRItemCategory.ARMOR),
    ("Stone Helm", 7040, DSRItemCategory.ARMOR),
    ("Stone Armor", 7041, DSRItemCategory.ARMOR),
    ("Stone Gauntlets", 7042, DSRItemCategory.ARMOR),
    ("Stone Leggings", 7043, DSRItemCategory.ARMOR),
    ("Crystalline Helm", 7044, DSRItemCategory.ARMOR),
    ("Crystalline Armor", 7045, DSRItemCategory.ARMOR),
    ("Crystalline Gauntlets", 7046, DSRItemCategory.ARMOR),
    ("Crystalline Leggings", 7047, DSRItemCategory.ARMOR),
    ("Mask of the Sealer", 7048, DSRItemCategory.ARMOR),
    ("Crimson Robe", 7049, DSRItemCategory.ARMOR),
    ("Crimson Gloves", 7050, DSRItemCategory.ARMOR),
    ("Crimson Waistcloth", 7051, DSRItemCategory.ARMOR),
    ("Mask of Velka", 7052, DSRItemCategory.ARMOR),
    ("Black Cleric Robe", 7053, DSRItemCategory.ARMOR),
    ("Black Manchette", 7054, DSRItemCategory.ARMOR),
    ("Black Tights", 7055, DSRItemCategory.ARMOR),
    ("Iron Helm", 7056, DSRItemCategory.ARMOR),
    ("Armor of the Sun", 7057, DSRItemCategory.ARMOR),
    ("Iron Bracelet", 7058, DSRItemCategory.ARMOR),
    ("Iron Leggings", 7059, DSRItemCategory.ARMOR),
    ("Chain Helm", 7060, DSRItemCategory.ARMOR),
    ("Chain Armor", 7061, DSRItemCategory.ARMOR),
    ("Leather Gauntlets", 7062, DSRItemCategory.ARMOR),
    ("Chain Leggings", 7063, DSRItemCategory.ARMOR),
    ("Cleric Helm", 7064, DSRItemCategory.ARMOR),
    ("Cleric Armor", 7065, DSRItemCategory.ARMOR),
    ("Cleric Gauntlets", 7066, DSRItemCategory.ARMOR),
    ("Cleric Leggings", 7067, DSRItemCategory.ARMOR),
    ("Sunlight Maggot", 7068, DSRItemCategory.ARMOR),
    ("Helm of Thorns", 7069, DSRItemCategory.ARMOR),
    ("Armor of Thorns", 7070, DSRItemCategory.ARMOR),
    ("Gauntlets of Thorns", 7071, DSRItemCategory.ARMOR),
    ("Leggings of Thorns", 7072, DSRItemCategory.ARMOR),
    ("Standard Helm", 7073, DSRItemCategory.ARMOR),
    ("Hard Leather Armor", 7074, DSRItemCategory.ARMOR),
    ("Hard Leather Gauntlets", 7075, DSRItemCategory.ARMOR),
    ("Hard Leather Boots", 7076, DSRItemCategory.ARMOR),
    ("Sorcerer Hat", 7077, DSRItemCategory.ARMOR),
    ("Sorcerer Cloak", 7078, DSRItemCategory.ARMOR),
    ("Sorcerer Gauntlets", 7079, DSRItemCategory.ARMOR),
    ("Sorcerer Boots", 7080, DSRItemCategory.ARMOR),
    ("Tattered Cloth Hood", 7081, DSRItemCategory.ARMOR),
    ("tattered Cloth Robe", 7082, DSRItemCategory.ARMOR),
    ("Tattered Cloth Machette", 7083, DSRItemCategory.ARMOR),
    ("Heavy Boots", 7084, DSRItemCategory.ARMOR),
    ("Pharis's Hat", 7085, DSRItemCategory.ARMOR),
    ("Leather Armor", 7086, DSRItemCategory.ARMOR),
    ("Leather Gloves", 7087, DSRItemCategory.ARMOR),
    ("Leather Boots", 7088, DSRItemCategory.ARMOR),
    ("Painting Guardian Hood", 7089, DSRItemCategory.ARMOR),
    ("Painting Guardian Robe", 7090, DSRItemCategory.ARMOR),
    ("Painting Guardian Gloves", 7091, DSRItemCategory.ARMOR),
    ("Painting Guardian Waistcloth", 7092, DSRItemCategory.ARMOR),
    ("Ornstein's Helm", 7093, DSRItemCategory.ARMOR),
    ("Ornstein's Armor", 7094, DSRItemCategory.ARMOR),
    ("Ornstein's Gauntlets", 7095, DSRItemCategory.ARMOR),
    ("Ornstein's Leggings", 7096, DSRItemCategory.ARMOR),
    ("Eastern Helm", 7097, DSRItemCategory.ARMOR),
    ("Eastern Armor", 7098, DSRItemCategory.ARMOR),
    ("Eastern Gauntlets", 7099, DSRItemCategory.ARMOR),
    ("Eastern Leggings", 7100, DSRItemCategory.ARMOR),
    ("Xanthous Crown", 7101, DSRItemCategory.ARMOR),
    ("Xanthous Overcoat", 7102, DSRItemCategory.ARMOR),
    ("Xanthous Gloves", 7103, DSRItemCategory.ARMOR),
    ("Xanthous Waistcloth", 7104, DSRItemCategory.ARMOR),
    ("Thief Mask", 7105, DSRItemCategory.ARMOR),
    ("Black Leather Armor", 7106, DSRItemCategory.ARMOR),
    ("Black Leather Gloves", 7107, DSRItemCategory.ARMOR),
    ("Black Leather Boots", 7108, DSRItemCategory.ARMOR),
    ("Priest's Hat", 7109, DSRItemCategory.ARMOR),
    ("Holy Robe", 7110, DSRItemCategory.ARMOR),
    ("Traveling Gloves", 7111, DSRItemCategory.ARMOR),
    ("Holy Trousers", 7112, DSRItemCategory.ARMOR),
    ("Black Knight Helm", 7113, DSRItemCategory.ARMOR),
    ("Black Knight Armor", 7114, DSRItemCategory.ARMOR),
    ("Black Knight Gauntlets", 7115, DSRItemCategory.ARMOR),
    ("Black Knight Leggings", 7116, DSRItemCategory.ARMOR),
    ("Crown of Dusk", 7117, DSRItemCategory.ARMOR),
    ("Antiquated Dress", 7118, DSRItemCategory.ARMOR),
    ("Antiquated Gloves", 7119, DSRItemCategory.ARMOR),
    ("Antiquated Skirt", 7120, DSRItemCategory.ARMOR),
    ("Witch Hat", 7121, DSRItemCategory.ARMOR),
    ("Witch Cloak", 7122, DSRItemCategory.ARMOR),
    ("Witch Gloves", 7123, DSRItemCategory.ARMOR),
    ("Witch Skirt", 7124, DSRItemCategory.ARMOR),
    ("Elite Knight Helm", 7125, DSRItemCategory.ARMOR),
    ("Elite Knight Armor", 7126, DSRItemCategory.ARMOR),
    ("Elite Knight Gauntlets", 7127, DSRItemCategory.ARMOR),
    ("Elite Knight Leggings", 7128, DSRItemCategory.ARMOR),
    ("Wnaderer Hood", 7129, DSRItemCategory.ARMOR),
    ("Wanderer Coat", 7130, DSRItemCategory.ARMOR),
    ("Wanderer Manchette", 7131, DSRItemCategory.ARMOR),
    ("Wanderer Boots", 7132, DSRItemCategory.ARMOR),
    ("Mage Smith Hat", 7133, DSRItemCategory.ARMOR),
    ("Mage Smith Coat", 7134, DSRItemCategory.ARMOR),
    ("Mage Smith Gauntlet", 7135, DSRItemCategory.ARMOR),
    ("Mage Smith Gauntlets", 7136, DSRItemCategory.ARMOR),
    ("Mage Smith Boots", 7137, DSRItemCategory.ARMOR),
    ("Big Hat", 7138, DSRItemCategory.ARMOR),
    ("Sage Robe", 7139, DSRItemCategory.ARMOR),
    ("Traveling Gloves", 7140, DSRItemCategory.ARMOR),
    ("Traveling Boots", 7141, DSRItemCategory.ARMOR),
    ("Knight Helm", 7142, DSRItemCategory.ARMOR),
    ("Knight Armor", 7143, DSRItemCategory.ARMOR),
    ("Knight Gauntlets", 7144, DSRItemCategory.ARMOR),
    ("Knight Leggings", 7145, DSRItemCategory.ARMOR),
    ("Dingy Hood", 7146, DSRItemCategory.ARMOR),
    ("Dingy Robe", 7147, DSRItemCategory.ARMOR),
    ("Dingy Gloves", 7148, DSRItemCategory.ARMOR),
    ("Blood-Stained Skirt", 7149, DSRItemCategory.ARMOR),
    ("Maiden Hood", 7150, DSRItemCategory.ARMOR),
    ("Maiden Robe", 7151, DSRItemCategory.ARMOR),
    ("Maiden Gloves", 7152, DSRItemCategory.ARMOR),
    ("Maiden Skirt", 7153, DSRItemCategory.ARMOR),
    ("Silver Knight Helm", 7154, DSRItemCategory.ARMOR),
    ("Silver Knight Armor", 7155, DSRItemCategory.ARMOR),
    ("Silver Knight Gauntlets", 7156, DSRItemCategory.ARMOR),
    ("Silver Knight Leggings", 7157, DSRItemCategory.ARMOR),
    ("Havel's Helm", 7158, DSRItemCategory.ARMOR),
    ("Havel's Armor", 7159, DSRItemCategory.ARMOR),
    ("Havel's Gauntlets", 7160, DSRItemCategory.ARMOR),
    ("Havel's Leggings", 7161, DSRItemCategory.ARMOR),
    ("Brass Helm", 7162, DSRItemCategory.ARMOR),
    ("Brass Armor", 7163, DSRItemCategory.ARMOR),
    ("Brass Gauntlets", 7164, DSRItemCategory.ARMOR),
    ("Brass Leggings", 7165, DSRItemCategory.ARMOR),
    ("Gold-Hemmed Black Hood", 7166, DSRItemCategory.ARMOR),
    ("Gold-Hemmed Black Cloak", 7167, DSRItemCategory.ARMOR),
    ("Gold-Hemmed Black Gloves", 7168, DSRItemCategory.ARMOR),
    ("Gold-Hemmed Black Skirt", 7169, DSRItemCategory.ARMOR),
    ("Golem Helm", 7170, DSRItemCategory.ARMOR),
    ("Golem Armor", 7171, DSRItemCategory.ARMOR),
    ("Golem Gauntlets", 7172, DSRItemCategory.ARMOR),
    ("Golem Leggings", 7173, DSRItemCategory.ARMOR),
    ("Hollow Soldier Helm", 7174, DSRItemCategory.ARMOR),
    ("Hollow Soldier Armor", 7175, DSRItemCategory.ARMOR),
    ("Hollow Soldier Waistcloth", 7176, DSRItemCategory.ARMOR),
    ("Steel Helm", 7177, DSRItemCategory.ARMOR),
    ("Steel Armor", 7178, DSRItemCategory.ARMOR),
    ("Steel Gauntlets", 7179, DSRItemCategory.ARMOR),
    ("Steel Leggings", 7180, DSRItemCategory.ARMOR),
    ("Hollow Thief's Hood", 7181, DSRItemCategory.ARMOR),
    ("Hollow Thief's Leather Armor", 7182, DSRItemCategory.ARMOR),
    ("Hollow Thief's Tights", 7183, DSRItemCategory.ARMOR),
    ("Balder Helm", 7184, DSRItemCategory.ARMOR),
    ("Balder Armor", 7185, DSRItemCategory.ARMOR),
    ("Balder Gauntlets", 7186, DSRItemCategory.ARMOR),
    ("Balder Leggings", 7187, DSRItemCategory.ARMOR),
    ("Hollow Warrior Helm", 7188, DSRItemCategory.ARMOR),
    ("Hollow Warrior Armor", 7189, DSRItemCategory.ARMOR),
    ("Hollow Warrior Waistcloth", 7190, DSRItemCategory.ARMOR),
    ("Giant Helm", 7191, DSRItemCategory.ARMOR),
    ("Giant Armor", 7192, DSRItemCategory.ARMOR),
    ("Giant Gauntlets", 7193, DSRItemCategory.ARMOR),
    ("Giant Leggings", 7194, DSRItemCategory.ARMOR),
    ("Crown of the Dark Sun", 7195, DSRItemCategory.ARMOR),
    ("Moonlight Robe", 7196, DSRItemCategory.ARMOR),
    ("Moonlight Gloves", 7197, DSRItemCategory.ARMOR),
    ("Moonlight Waistcloth", 7198, DSRItemCategory.ARMOR),
    ("Crown of the Great Lord", 7199, DSRItemCategory.ARMOR),
    ("Robe of the Great Lord", 7200, DSRItemCategory.ARMOR),
    ("Bracelet of the Great Lord", 7201, DSRItemCategory.ARMOR),
    ("Anklet of the Great Lord", 7202, DSRItemCategory.ARMOR),
    ("Sack", 7203, DSRItemCategory.ARMOR),
    ("Symbol of Avarice", 7204, DSRItemCategory.ARMOR),
    ("Royal Helm", 7205, DSRItemCategory.ARMOR),
    ("Mask of the Father", 7206, DSRItemCategory.ARMOR),
    ("Mask of the Mother", 7207, DSRItemCategory.ARMOR),
    ("Mask of the Child", 7208, DSRItemCategory.ARMOR),
    ("Fang Boar Helm", 7209, DSRItemCategory.ARMOR),
    ("Gargoyle Helm", 7210, DSRItemCategory.ARMOR),
    ("Black Sorcerer Hat", 7211, DSRItemCategory.ARMOR),
    ("Black Sorcerer Cloak", 7212, DSRItemCategory.ARMOR),
    ("Black Sorcerer Gauntlets", 7213, DSRItemCategory.ARMOR),
    ("Black Sorcerer Boots", 7214, DSRItemCategory.ARMOR),
    ("Elite Cleric Helm", 7215, DSRItemCategory.ARMOR),
    ("Elite Cleric Armor", 7216, DSRItemCategory.ARMOR),
    ("Elite Cleric Gauntlets", 7217, DSRItemCategory.ARMOR),
    ("Elite Cleric Leggings", 7218, DSRItemCategory.ARMOR),

    ("Dagger", 8000, DSRItemCategory.WEAPON) # melee weapons
    ("Parrying Dagger", 8001, DSRItemCategory.WEAPON)
    ("Ghost Blade", 8002, DSRItemCategory.WEAPON)
    ("Bandit's Knife", 8003, DSRItemCategory.WEAPON)
    ("Priscilla's Dagger", 8004, DSRItemCategory.WEAPON)
    ("Shortsword", 8005, DSRItemCategory.WEAPON)
    ("Longsword", 8006, DSRItemCategory.WEAPON)
    ("BroadSword", 8007, DSRItemCategory.WEAPON)
    ("Broken Straight Sword", 8008, DSRItemCategory.WEAPON)
    ("Balder Side Sword", 8009, DSRItemCategory.WEAPON)
    ("Straight Sword", 8010, DSRItemCategory.WEAPON)
    ("Barbed Straight Sword", 8011, DSRItemCategory.WEAPON)
    ("Silver Knight Straight Sword", 8012, DSRItemCategory.WEAPON)
    ("Astora's Straight Sword", 8013, DSRItemCategory.WEAPON)
    ("Darksword", 8014, DSRItemCategory.WEAPON)
    ("Drake Sword", 8015, DSRItemCategory.WEAPON)
    ("Straight Sword Hilt", 8016, DSRItemCategory.WEAPON)
    ("Bastard Sword", 8017, DSRItemCategory.WEAPON)
    ("Claymore", 8018, DSRItemCategory.WEAPON)
    ("Man-Serpent Greatsword", 8019, DSRItemCategory.WEAPON)
    ("Flamberge", 8020, DSRItemCategory.WEAPON)
    ("Crystal Greatsword", 8021, DSRItemCategory.WEAPON)
    ("Stone Greatsword", 8022, DSRItemCategory.WEAPON)
    ("Greatsword of Artorias", 8023, DSRItemCategory.WEAPON)
    ("Moonlight Greatsword", 8024, DSRItemCategory.WEAPON)
    ("Black Knight Sword", 8025, DSRItemCategory.WEAPON)
    ("Great Lord Greatsword", 8026, DSRItemCategory.WEAPON)
    ("Zweihander", 8027, DSRItemCategory.WEAPON)
    ("Greatsword", 8028, DSRItemCategory.WEAPON)
    ("Demon Great Machete", 8029, DSRItemCategory.WEAPON)
    ("Dragon Greatsword", 8030, DSRItemCategory.WEAPON)
    ("Black Knight Greatsword", 8031, DSRItemCategory.WEAPON)
    ("Scimitar", 8032, DSRItemCategory.WEAPON)
    ("Falchion", 8033, DSRItemCategory.WEAPON)
    ("Shotel", 8034, DSRItemCategory.WEAPON)
    ("Jagged Ghost Blade", 8035, DSRItemCategory.WEAPON)
    ("Painting Guardian Sword", 8036, DSRItemCategory.WEAPON)
    ("Quelaag's Furysword", 8037, DSRItemCategory.WEAPON)
    ("Server", 8038, DSRItemCategory.WEAPON)
    ("Murakumo", 8039, DSRItemCategory.WEAPON)
    ("Gravelord Sword", 8040, DSRItemCategory.WEAPON)
    ("Uchigatana", 8041, DSRItemCategory.WEAPON)
    ("Washing Pole", 8042, DSRItemCategory.WEAPON)
    ("Iaito", 8043, DSRItemCategory.WEAPON)
    ("Chaos Blade", 8044, DSRItemCategory.WEAPON)
    ("Mail Breaker", 8045, DSRItemCategory.WEAPON)
    ("Rapier", 8046, DSRItemCategory.WEAPON)
    ("Estoc", 8047, DSRItemCategory.WEAPON)
    ("Velka's Rapier", 8048, DSRItemCategory.WEAPON)
    ("Ricard's Rapier", 8049, DSRItemCategory.WEAPON)
    ("Hand Axe", 8050, DSRItemCategory.WEAPON)
    ("Battle Axe", 8051, DSRItemCategory.WEAPON)
    ("Crescent Axe", 8052, DSRItemCategory.WEAPON)
    ("Butcher Knife", 8053, DSRItemCategory.WEAPON)
    ("Golem Axe", 8054, DSRItemCategory.WEAPON)
    ("Gargoyle Tail Axe", 8055, DSRItemCategory.WEAPON)
    ("Greataxe", 8056, DSRItemCategory.WEAPON)
    ("Demon's Greataxe", 8057, DSRItemCategory.WEAPON)
    ("Club", 8058, DSRItemCategory.WEAPON)
    ("Mace", 8059, DSRItemCategory.WEAPON)
    ("Morning Star", 8060, DSRItemCategory.WEAPON)
    ("Warpick", 8061, DSRItemCategory.WEAPON)
    ("Pickaxe", 8062, DSRItemCategory.WEAPON)
    ("Reinforced Club", 8063, DSRItemCategory.WEAPON)
    ("Blacksmith Hammer", 8064, DSRItemCategory.WEAPON)
    ("Hammer of Vamos", 8065, DSRItemCategory.WEAPON)
    ("Great Club", 8066, DSRItemCategory.WEAPON)
    ("Grant", 8067, DSRItemCategory.WEAPON)
    ("Demon's Great Hammer", 8068, DSRItemCategory.WEAPON)
    ("Dragon Tooth", 8069, DSRItemCategory.WEAPON)
    ("Large Club", 8070, DSRItemCategory.WEAPON)
    ("Smough's Hammer", 8071, DSRItemCategory.WEAPON)
    ("Caestus", 8072, DSRItemCategory.WEAPON)
    ("Claw", 8073, DSRItemCategory.WEAPON)
    ("Dragon Bone Fist", 8074, DSRItemCategory.WEAPON)
    ("Dark Hand", 8075, DSRItemCategory.WEAPON)
    ("Spear", 8076, DSRItemCategory.WEAPON)
    ("Winged Spear", 8077, DSRItemCategory.WEAPON)
    ("Partizan", 8078, DSRItemCategory.WEAPON)
    ("Demon's Spear", 8079, DSRItemCategory.WEAPON)
    ("Channeler's Trident", 8080, DSRItemCategory.WEAPON)
    ("Silver Knight Spear", 8081, DSRItemCategory.WEAPON)
    ("Pike", 8082, DSRItemCategory.WEAPON)
    ("Dragonslayer Spear", 8083, DSRItemCategory.WEAPON)
    ("Moonlight Butterfly Horn", 8084, DSRItemCategory.WEAPON)
    ("Halberd", 8085, DSRItemCategory.WEAPON)
    ("Giant's Halberd", 8086, DSRItemCategory.WEAPON)
    ("Titanite Catch Pole", 8087, DSRItemCategory.WEAPON)
    ("Gargoyles's Halberd", 8088, DSRItemCategory.WEAPON)
    ("Black Knight Halberd", 8089, DSRItemCategory.WEAPON)
    ("Lucerne", 8090, DSRItemCategory.WEAPON)
    ("Scythe", 8091, DSRItemCategory.WEAPON)
    ("Great Scythe", 8092, DSRItemCategory.WEAPON)
    ("Lifehunt Scythe", 8093, DSRItemCategory.WEAPON)
    ("Whip", 8094, DSRItemCategory.WEAPON)
    ("Notched Whip", 8095, DSRItemCategory.WEAPON)

    ("Short Bow", 8096, DSRItemCategory.WEAPON) # ranged weapons
    ("Longbow", 8097, DSRItemCategory.WEAPON)
    ("Black Bow of Pharis", 8098, DSRItemCategory.WEAPON)
    ("Dragonslayer Greatbow", 8099, DSRItemCategory.WEAPON)
    ("Composite Bow", 8100, DSRItemCategory.WEAPON)
    ("Darkmoon Bow", 8101, DSRItemCategory.WEAPON)
    ("Light Crossbow", 8102, DSRItemCategory.WEAPON)
    ("Heavy Crossbow", 8103, DSRItemCategory.WEAPON)
    ("Avelyn", 8104, DSRItemCategory.WEAPON)
    ("Sniper Crossbow", 8105, DSRItemCategory.WEAPON)

    ("Sorcerer's Catalyst", 8106, DSRItemCategory.WEAPON) # magic weapons
    ("Beatrice's Catalyst", 8107, DSRItemCategory.WEAPON)
    ("Tin Banishment Catalyst", 8108, DSRItemCategory.WEAPON)
    ("Logan's Catalyst", 8109, DSRItemCategory.WEAPON)
    ("Tin Darkmoon Catalyst", 8110, DSRItemCategory.WEAPON)
    ("Oolacile Ivory Catalyst", 8111, DSRItemCategory.WEAPON)
    ("Tin Crystallization Catalyst", 8112, DSRItemCategory.WEAPON)
    ("Demon's Catalyst", 8113, DSRItemCategory.WEAPON)
    ("Izalith Catalyst", 8114, DSRItemCategory.WEAPON)
    ("Pyromancy Flame", 8115, DSRItemCategory.WEAPON)
    ("Talisman", 8116, DSRItemCategory.WEAPON)
    ("Canvas Talisman", 8117, DSRItemCategory.WEAPON)
    ("Thorolund Talisman", 8118, DSRItemCategory.WEAPON)
    ("Ivory Talisman", 8119, DSRItemCategory.WEAPON)
    ("Sunlight Talisman", 8120, DSRItemCategory.WEAPON)
    ("Darkmoon Talisman", 8121, DSRItemCategory.WEAPON)
    ("Velka's Talisman", 8122, DSRItemCategory.WEAPON)

    ("Skull Lantern", 9000, DSRItemCategory.SHIELD)
    ("East-West Shield", 9001, DSRItemCategory.SHIELD)
    ("Wooden Shield", 9002, DSRItemCategory.SHIELD)
    ("Large Leather Shield", 9003, DSRItemCategory.SHIELD)
    ("Small Leather Shield", 9004, DSRItemCategory.SHIELD)
    ("Target Shield", 9005, DSRItemCategory.SHIELD)
    ("Buckler", 9006, DSRItemCategory.SHIELD)
    ("Cracked Round Shield", 9007, DSRItemCategory.SHIELD)
    ("Leather Shield", 9008, DSRItemCategory.SHIELD)
    ("Plank Shield", 9009, DSRItemCategory.SHIELD)
    ("Caduceus Round Shield", 9010, DSRItemCategory.SHIELD)
    ("Crystal Ring Shield", 9011, DSRItemCategory.SHIELD)
    ("Heater Shield", 9012, DSRItemCategory.SHIELD)
    ("Knight Shield", 9013, DSRItemCategory.SHIELD)
    ("Tower Kite Shield", 9014, DSRItemCategory.SHIELD)
    ("Grass Crest Shield", 9015, DSRItemCategory.SHIELD)
    ("Hollow Soldier Shield", 9016, DSRItemCategory.SHIELD)
    ("Balder Shield", 9017, DSRItemCategory.SHIELD)
    ("Crest Shield", 9018, DSRItemCategory.SHIELD)
    ("Dragon Crest Shield", 9019, DSRItemCategory.SHIELD)
    ("Warrior's Round Shield", 9020, DSRItemCategory.SHIELD)
    ("Iron Round Shield", 9021, DSRItemCategory.SHIELD)
    ("Spider Shield", 9022, DSRItemCategory.SHIELD)
    ("Spiked Shield", 9023, DSRItemCategory.SHIELD)
    ("Crystal Shield", 9024, DSRItemCategory.SHIELD)
    ("Sunlight Shield", 9025, DSRItemCategory.SHIELD)
    ("Silver Knight Shield", 9026, DSRItemCategory.SHIELD)
    ("Black Knight Shield", 9027, DSRItemCategory.SHIELD)
    ("Pierce Shield", 9028, DSRItemCategory.SHIELD)
    ("Red and White Round Shield", 9029, DSRItemCategory.SHIELD)
    ("Caduceus Kite Shield", 9030, DSRItemCategory.SHIELD)
    ("Gargoyle's Shield", 9031, DSRItemCategory.SHIELD)
    ("Eagle Shield", 9032, DSRItemCategory.SHIELD)
    ("Tower Shield", 9033, DSRItemCategory.SHIELD)
    ("Giant Shield", 9034, DSRItemCategory.SHIELD)
    ("Stone Greatshield", 9035, DSRItemCategory.SHIELD)
    ("Havel's Greatshield", 9036, DSRItemCategory.SHIELD)
    ("Bonewheel Shield", 9037, DSRItemCategory.SHIELD)
    ("Greatshield of Artorias", 9038, DSRItemCategory.SHIELD)
    ("Effigy Shield", 9039, DSRItemCategory.SHIELD)
    ("Sanctus", 9040, DSRItemCategory.SHIELD)
    ("Bloodshield", 9041, DSRItemCategory.SHIELD)
    ("Black Iron Greatshield", 9042, DSRItemCategory.SHIELD)
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

import typing
from dataclasses import dataclass
from Options import Toggle, DefaultOnToggle, Option, Range, Choice, ItemDict, DeathLink, PerGameCommonOptions



class GuaranteedItemsOption(ItemDict):
    """Guarantees that the specified items will be in the item pool"""
    display_name = "Guaranteed Items"

class EnableMasterKeyOption(Toggle):
    """Includes the Master Key in the item pool"""
    display_name = "Enable Master Key"

@dataclass
class DSROption(PerGameCommonOptions):
    #goal: GoalOption
    guaranteed_items: GuaranteedItemsOption
    enable_masterkey: EnableMasterKeyOption
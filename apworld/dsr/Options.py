import typing
from dataclasses import dataclass
from Options import Toggle, DefaultOnToggle, Option, Range, Choice, ItemDict, OptionList, DeathLink, PerGameCommonOptions
from Options import OptionGroup



class GuaranteedItemsOption(ItemDict):
    """Guarantees that the specified items will be in the item pool"""
    display_name = "Guaranteed Items"

class EnableMasterKeyOption(Toggle):
    """Includes the Master Key in the item pool"""
    display_name = "Enable Master Key"

class UniqueSoulOption(Toggle):
    """Adds only one of each boss soul to the item pool"""
    display_name = "Singular Boss Souls"

class FogwallLock(Toggle):
    """Makes area fog walls uninteractable until you receive their items from the item pool"""
    display_name = "Fogwall Lock" 

class FogwallLockIncludeUA(Toggle):
    """Includes Undead Asylum early fogwall in the pool. This can lead to extremely early BK mode."""
    display_name = "Fogwall Lock Include Undead Asylum" 

class BossFogwallLock(Toggle):
    """Makes boss fog walls uninteractable until you receive their items from the item pool"""
    display_name = "Boss Fogwall Lock" 

class UpgradedWeaponsPercentage(Range):
    """Percentage of weapons (including shields) in the pool that will be replaced with upgraded versions, if possible."""
    display_name = "Upgraded Weapons Percentage"
    range_start = 0
    range_end = 100
    default = 0

class UpgradedWeaponsAllowedInfusions(OptionList):
    """Which infusions are allowed if UpgradedWeapons.
    Available infusion types are Normal, Raw, Magic, Fire, Divine, Chaos, Enchanted, Occult, Crystal, and Lightning.
    If "Normal" is removed, all upgraded weapons will have a different, available infusion."""
    display_name = "Upgrade Weapons Allowed Infusions"
    default = {"Normal", "Raw", "Magic", "Fire", "Divine", "Chaos", "Enchanted", "Occult", "Crystal", "Lightning"}
    valid_keys = ["Normal", "Raw", "Magic", "Fire", "Divine", "Chaos", "Enchanted", "Occult", "Crystal", "Lightning"]

class UpgradedWeaponsAdjustedLevels(Toggle):
    """For upgraded weapons added to the pool, Whether to 'adjust' weapon levels for applying the ranges.
    When true, the min/max levels will apply to an calculated "level" of an infused weapon adjusted by:
     +5 for Raw/Magic/Fire/Divine
     +10 for Chaos/Enchanted/Occult/Crystal/Lightning 
    When false, the min/max levels will not be adjusted."""
    display_name = "Upgraded Weapons - Adjust Ranges"
    default = True

class UpgradedWeaponsMinLevel(Range):
    """Minimum upgrade value on upgraded weapons in the pool.
    This can exclude certain infusions if their calculated level cannot be at the minimum level. """
    display_name = "Upgraded Weapons Minimum Level"
    range_start = 0
    range_end = 15
    default = 0


class UpgradedWeaponsMaxLevel(Range):
    """Maximum upgrade plus value on upgraded weapons in the pool."""
    display_name = "Upgraded Weapons Maximum Level"
    range_start = 0
    range_end = 15
    default = 15

class EnableDeathlinkOption(Toggle):
    """Includes Deathlink"""
    display_name = "Enable Deathlink"


# Group relevant options
option_groups = [
    OptionGroup("Fogwall Lock", [
        FogwallLock,
        FogwallLockIncludeUA,
        BossFogwallLock,
        ]),
    OptionGroup("Upgraded Weapons", [
        UpgradedWeaponsPercentage,
        UpgradedWeaponsAllowedInfusions,
        UpgradedWeaponsAdjustedLevels,
        UpgradedWeaponsMinLevel,
        UpgradedWeaponsMaxLevel,
        ])
    ]


@dataclass
class DSROption(PerGameCommonOptions):
    #goal: GoalOption
    guaranteed_items: GuaranteedItemsOption
    enable_masterkey: EnableMasterKeyOption
    unique_souls: UniqueSoulOption
    fogwall_lock: FogwallLock
    fogwall_lock_include_ua: FogwallLockIncludeUA
    boss_fogwall_lock: BossFogwallLock
    upgraded_weapons_percentage: UpgradedWeaponsPercentage
    upgraded_weapons_allowed_infusions: UpgradedWeaponsAllowedInfusions
    upgraded_weapons_adjusted_levels : UpgradedWeaponsAdjustedLevels
    upgraded_weapons_min_level: UpgradedWeaponsMinLevel
    upgraded_weapons_max_level: UpgradedWeaponsMaxLevel
    enable_deathlink: EnableDeathlinkOption

import typing
from dataclasses import dataclass
from Options import Toggle, DefaultOnToggle, Option, Range, Choice, ItemDict, OptionList, DeathLink, PerGameCommonOptions
from Options import OptionGroup



class GuaranteedItemsOption(ItemDict):
    """Guarantees that the specified items will be in the item pool"""
    display_name = "Guaranteed Items"

class ExcludedLocationBehaviorOption(Choice):
    """How to choose items for excluded locations in DSR.

    - **Forbid Useful:** Neither progression items nor useful items can be placed in excluded
      locations.
    - **Do Not Randomize:** Excluded locations always contain the same item as in vanilla
      Dark Souls: Remastered.
    """
    display_name = "Excluded Locations Behavior"
    option_forbid_useful = 2
    option_do_not_randomize = 3
    default = 2

class FogwallSanity(DefaultOnToggle):
    """Makes area fog walls uninteractable until you receive their items from the item pool. 
    Fog walls will also give checks when you pass through them.
    The Undead Asylum fog wall will not be locked, to avoid super-early BK.
    Recommended to turn this on."""
    display_name = "Fogwall Sanity" 

class BossFogwallSanity(Toggle):
    """Makes boss fog walls uninteractable until you receive their items from the item pool.
    Boss fog walls will also give checks when you pass through them.
    The following will be neither locations nor checks as they all do not have fog walls
    upon your first entry into their arena:
      Asylum Demon (first encounter),
      Sif,
      Seath (2nd encounter), and
      Kalameet."""
    display_name = "Boss Fogwall Sanity" 

class LogicToAccessCatacombs(Choice):
    """Artificial logic for The Catacombs access. 
     Before the chosen condition, The Catacombs will be considered "out of logic"

    - **no_logic:** (not recommended) Catacombs is in-logic as soon as you get to Firelink Shrine.
    - **undead_merchant:** Access to Undead Merchant in the Upper Undead Burg puts Catacombs in-logic.
    - **andre:** Access to Andre puts Catacombs in-logic.
    - **andre_or_undead_merchant:** Access to either Andre or Undead Merchant puts Catacombs in-logic.
    - **ornstein_and_smough:** Access to Ornstein and Smough puts Catcombs in-logic."""
    display_name = "Logic Requirement to Access Catacombs"
    option_no_logic = 0
    option_undead_merchant = 1
    option_andre = 2
    option_andre_or_undead_merchant = 3
    option_ornstein_and_smough = 4
    default = 3

# class LogicToAccessDukesArchivePrison(Choice):
#     """Artificial logic for Duke's Archives Prison cell (e.g. post 1st Seath Encounter). 
#      Before the chosen condition, catacombs will be considered "out of logic"

#     - **no_logic:** Duke's Archives Prison Cell is in-logic as soon as you can reach Seath's 1st encounter
#                     (may require usage of unstuck)
#     - **cell_key:** DA Prison is in-logic once you have the Archive Tower Cell Key
#                     (may require usage of unstuck)
#     - **cell_and_giant_door_key:** (recommended) DA Prison is in-logic once you have  
#                                     both the Archive Tower Cell Key and Giant Door Key.
#     - **all_keys:** DA Prison is in-logic only once you have all the DA Prison keys."""
#     display_name = "Logic Requirement to Access Duke's Archives Prison"
#     option_no_logic = 0
#     option_cell_key = 1
#     option_cell_and_giant_door_key = 2
#     option_all_keys = 3
#     default = 2

# class LogicToAccessPaintedWorld(Choice):
#     """Artificial logic for The Painted World of Ariamis (PW) access.
#      Before the chosen condition, PW will be considered "out of logic".
#     If you do not have fog_wall_sanity or boss_fog_wall_sanity options on, this will have no effect.

#     - **no_logic:** Painted World is in-logic as soon as it is accessible (via Anor Londo painting with Peculiar doll)
#                     (may require usage of unstuck)
#     - **all_fogs:** PW is in-logic once player has all fog walls that they have access to."""
    
#     display_name = "Logic Requirement to Access Painted World"
#     option_no_logic = 0
#     option_all_fogs = 1
#     default = 1

class UpgradedWeaponsPercentage(Range):
    """Percentage of weapons (including shields) in the pool that will be replaced with upgraded versions, if possible.
    Choose a higher value for an easier time."""
    display_name = "Upgraded Weapons Percentage"
    range_start = 0
    range_end = 100
    default = 0

class UpgradedWeaponsAllowedInfusions(OptionList):
    """Which infusions are allowed if UpgradedWeapons.
    Available infusion types are Normal, Raw, Magic, Fire, Divine, Chaos, Enchanted, Occult, Crystal, and Lightning.
    If "Normal" is removed, all upgraded weapons will have a different, available infusion."""
    display_name = "Upgraded Weapons - Allowed Infusions"
    default = {"Normal", "Raw", "Magic", "Fire", "Divine", "Chaos", "Enchanted", "Occult", "Crystal", "Lightning"}
    valid_keys = ["Normal", "Raw", "Magic", "Fire", "Divine", "Chaos", "Enchanted", "Occult", "Crystal", "Lightning"]

class UpgradedWeaponsAdjustedLevels(DefaultOnToggle):
    """For upgraded weapons added to the pool, Whether to 'adjust' weapon levels for applying the ranges.
    When true, the min/max levels will apply to an calculated "level" of an infused weapon adjusted by:
     +5 for Raw/Magic/Fire/Divine
     +10 for Chaos/Enchanted/Occult/Crystal/Lightning 
    When false, the min/max levels will not be adjusted."""
    display_name = "Upgraded Weapons - Adjust Ranges"

class UpgradedWeaponsMinLevel(Range):
    """Minimum upgrade value on upgraded weapons in the pool.
    This can exclude certain infusions if their calculated level cannot be at the minimum level. """
    display_name = "Upgraded Weapons - Minimum Level"
    range_start = 0
    range_end = 15
    default = 0


class UpgradedWeaponsMaxLevel(Range):
    """Maximum upgrade plus value on upgraded weapons in the pool."""
    display_name = "Upgraded Weapons - Maximum Level"
    range_start = 0
    range_end = 15
    default = 15

class EnableDeathlinkOption(Toggle):
    """Includes Deathlink"""
    display_name = "Enable Deathlink"


# Group relevant options
option_groups = [
    OptionGroup("Sanity", [
        FogwallSanity,
        BossFogwallSanity,
        ]),
    OptionGroup("Logic", [
        LogicToAccessCatacombs,
        LogicToAccessDukesArchivePrison,
        LogicToAccessPaintedWorld,
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
    excluded_location_behavior: ExcludedLocationBehaviorOption
    fogwall_sanity: FogwallSanity
    boss_fogwall_sanity: BossFogwallSanity
    logic_to_access_catacombs: LogicToAccessCatacombs
    logic_to_access_dukes_archives_prison: LogicToAccessDukesArchivePrison
    logic_to_access_painted_world: LogicToAccessPaintedWorld
    upgraded_weapons_percentage: UpgradedWeaponsPercentage
    upgraded_weapons_allowed_infusions: UpgradedWeaponsAllowedInfusions
    upgraded_weapons_adjusted_levels : UpgradedWeaponsAdjustedLevels
    upgraded_weapons_min_level: UpgradedWeaponsMinLevel
    upgraded_weapons_max_level: UpgradedWeaponsMaxLevel
    enable_deathlink: EnableDeathlinkOption

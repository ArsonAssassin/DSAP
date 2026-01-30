Archipelago implementation for Dark Souls Remastered by ArsonAssassin

# **This implementation is still considered unstable/in alpha. Expect bugs and issues.**

#### Table of Contents
[How it Works](#How-it-works)  
[Initial Setup](#Initial-Setup)  
[Before you go back online](#Before-you-go-back-online)  
[Usage](#Usage)  
[Troubleshooting](#Troubleshooting)  
[Compatibility](#Compatibility)  
[Frequently Asked Questions (FAQ)](#Frequently-Asked-Questions-FAQ)  
[Known issues](#Known-issues)  
[Changelog](#Changelog)  
[Roadmap](#Roadmap)  
[Location Groups](#Location-Groups)  
[Item Groups](#Item-Groups)  
[Artificial Logic Without Fogwall Locking](#Artificial-Logic-Without-Fogwall-Locking)  
[Co-op Toleration](#Co-op-Toleration)  

# How it works
* Every loose item on the ground, and potentially fog walls, are "locations" or "checks", and can each contain any item from the multiworld randomized item pool.
* All items in those locations will be randomized into the item pool.
* Keys and progression items (e.g. Lordvessel) will be forced into the item pool, unless they drop or are shoppable from non-randomized locations.
* Undead Asylum is not randomized. This is intended behavior so as to not put the player into BK mode immediately.
* Enemy loot and shop items are not yet randomized. Some bosses are exceptions to this.
* Item pool is currently constructed as follows:
    1. First all items from randomized are added to the pool.
    2. Then, key items and embers will replace any filler items. This includes any "Fog Wall Keys", if either fogwall_sanity is on.
    3. Then, Guaranteed items are added to the pool.
    4. If there are any Filler or Junk items left over, they are replaced with Souls of a Proud Knight (2000 souls each).
* If locations are excluded and excluded_locations_behavior is set to "do_not_randomize", then the items in those locations will not be added to the pool, and those locations will have their vanilla items. Even with fogwall_sanity on, fog walls excluded in this manner this will provide nothing, as fog walls do not provide items in the base game.

# Initial Setup
1. Download the latest APWorld and Client from the releases page.
2. Double click on the APWorld to install it to your Archipelago installation
3. Run the Archipelago Launcher and click "Generate Template Options".
4. A file explorer window will open with some .yaml files. Find the relevant yaml for your game and copy it to your Archipelago/Players directory.
    * Optional: edit the yaml setting to your preference
5. Either run Generate.py or press the Generate button on the launcher to generate a seed.
6. When this has completed, you will have a zip file in your Archipelago/Output folder.
7. Host the output zip either using the Host option on the launcher or by uploading it to Archipelago.gg
8. Optional: Back up your existing DSR saves into another folder and label that folder appropriately.

# Before you go back online
* You must close this program, and then restart Dark Souls Remastered. 
* You should move your AP DSR saves elsewhere, and restore your original backed up saves.
* If you do not do the above, you risk loading into a save with this program's modifications still in effect, and **risk facing negative consequences by FromSoft as mentioned below.**

# Usage
1. When you start up the game, ensure you are **disconnected from the network**. It is recommended to configure Dark Souls Remastered's settings in System->Network Settings->Launch Setting="Start Offline", to avoid accidentally starting online.
**WARNING: You should never connect to the FromSoft network while using this mod. If you are connected to the online Servers while using this mod, or with a save in which this mod was used, you will likely face account restrictions (bans) by FromSoft!!**
2. Run Dark Souls Remastered.
3. Load into your save file created specifically for this seed/multiworld, or start a New Game, create your character, and proceed to the point where you are able to control and move your character.
    * **Be careful not load into a wrong save** - if it has locations checked that have not been checked in your save for this seed, you will end up sending checks you have not yet made, which would be a bummer and not fun.
4. Verify you loaded into the correct save.
5. With both the Archipelago server and game running, Open the client folder you unzipped earlier and run DSAP.Desktop.exe.
6. Click on the three-horizontal-line icon at the top left of the DSAP client window.
7. Fill in your host, slot and password (if required) and press Connect.
    * You can click in the DSAP client window outside of the left-hand-side menu to show the Log.
8. This will cause your game to reload as if you had used a homeward bone. This is necessary to update the items that are in the game.
9. You should now be ready to play.


# Troubleshooting
* If you encounter issues, first make sure your Dark Souls Remastered game is up to date (Main menu should show the text "App ver. 1.03.1" & "Regulation ver. 1.04"). If it is not, use the "verify game files" in Steam. It is recommended to also make sure any other residual mods are removed before doing so.
* Then, check the known issues listed below. If your issue is in the list for your version, the issue you have may be resolved by updating to a later version.
* If item lots are not replaced, or the client cannot connect, **try running DSAP.client.exe as administrator**. This program requires authorization to modify the memory of another process, so it may require elevated permissions depending on your system configuration.
* If none of the above resolve your issues, you may be able to search for answers in the AP Discord channel for dark-souls-1.
  * First check the pins. Then try searching the channel specifically to see if others have encountered your issue.
  * If you don't find anything, please comment in the channel and include the version number of DSAP that you are using, the Archipelago version you are using, and a description of the issue, including context.
  * If either DSAP or DSR crashed, note the time of the error, then open "Event Viewer" from the start menu, go to Windows Logs->Application, and look for an "Error" level log entry. Right click the relevant entry to copy the details as text, and provide the file with your report. If there are multiple Error entries at the time of error, provide both.

# Compatibility
* This version has been tested with Dark Souls Remastered, Steam version (App ver. 1.03.1 & Regulation ver. 1.04) on Windows 11, with Archipelago version 0.6.3. 
* Linux, even through Proton/Wine, is not yet supported

# Frequently Asked Questions (FAQ)
* Q: Can I use this with seamless co-op?
  * A: Toleration has been added for multiple players. See "Co-op Toleration" section below.
* Q: Can I use this to randomize enemies?
  * A: This mod will not randomize enemies, but some players have had success with external enemy and boss randomizers. That said, we cannot guarantee they will continue to work, and that future updates won't break compatibility.
* Q: Does this work with Prepare to Die edition?
  * A: No, The current release only works with Dark Souls Remastered. There may be potential to make it compatible with PTDE but not until we are feature-complete on remastered, as there isn't a way to legally obtain a new copy of PTDE anymore.
* Q: Does this work on Linux?
  * A: We are working on it.
* Q: Can I randomized starting gear? 
  * A: Not yet - this is planned for the future. Currently, it is recommended to create your character before connecting with the DSAP client.
* Q: Is there a tracker?
  * There is a poptracker pack available at https://github.com/routhken/Dark_Souls_Remastered_tracker/releases (poptracker download itself at https://github.com/black-sliver/PopTracker/releases)
  * Universal Tracker (UT) also works, and in it you can import the maps from the poptracker pack. UT download can be found at https://github.com/FarisTheAncient/Archipelago/releases
* Q: Why do I keep dropping items (yellow bags) on the floor?
  * A: You probably have a full stack of Prism Stones in your inventory. This is the item we use to replace vanilla items when the Archipelago item is in another players' world. As long as you've "picked up" the item from the original location, leaving the dropped bag won't cause any issues. We technically check whether the item is still in the original location, not whether it is in your inventory. To prevent this, you can discard most/all of your prism stones when they get up to or close to 99.

# Known issues
* Master Key chosen from character creation (whether as a gift or thief starting item) is not considered to be in-logic, regardless of your yaml settings. Randomized starting gear, and potentially gifts, is planned for the future.
* Boss fog walls in the DLC do not correctly "Lock" with boss fog wall locks on.
* v0.0.21.0 and lower: Items will only ever be sent to one save. This is fixed in v0.0.22.0+.
* v0.0.20.0 and lower: Goal may not send upon completion. It is recommended to upgrade the DSAP client to v0.0.21.0, connect, and run the /goalcheck command. v0.0.21.0 of the client is fully compatible with v0.0.20.0-generated apworlds.
* v0.0.19.1 and lower: If you receive an item while on the main menu, it will be lost, requiring admin intervention. **For safety, you should only run the client once loaded into game.**
  * Furthermore, **you should close the client before quitting to menu or quitting the game.**
* v0.0.19.1 and lower: Looting the "key item chest" in Firelink Shrine behind Frampt **will break logic for DSR**. In a vanilla playthrough, this chest is usually empty/already open, and only has items if you somehow don't have a key item you "should have", depending on where you are in the game. In a randomizer environment, those normal circumstances don't apply! As an example: if you loot this chest after looting the vanilla "Basement Key" location, it will have a Basement Key - but in AP randomizer that key can even be in another game! In v0.0.20+ it gives rubbish instead.
* v0.0.19.1 and lower: On reconnect, player can receive duplicate items. The items are specifically those from "door"-type location checks in their own world.
* v0.0.19.1 and lower: Some enemy drops (invaders, Havel, etc) are erroneously replaced with prism stones, but do not grant an AP item. The player should get the standard enemy drop in these locations instead.
* v0.0.19.1 and lower: Not receiving deathlinks - a potential workaround is to close DSAP client, completely exit game to desktop, relaunch DSR + DSAP, load in with your character, and then reconnect with DSAP client. This occurs when the game happens to load your player character information near enough to a 65k boundary (limit of a 2^16 "short" int), which could in some cases happen each time you load in. Anecdotally, restarting the game from desktop makes it most likely to allocate your memory in a better spot. This is fixed in v0.0.20+.
* v0.0.19.1 prerelease: While unhollowed/human, the player is detected as "not in game". This can result in no items or deathlinks being sent to other players.
* v0.0.18.3: DSR game and DSAP.client.exe both crash upon connect - you must load into the game and be able to move your character around before connecting with the client.
* v0.0.18.2 and lower: Items do not get replaced. Upgrade your client version.

# Changelog
## Version 0.0.22.0 (upcoming)
* Version update -> 0.0.22.0. Both Apworld and Client have updated. This Client version will NOT be compatible with earlier versions of the apworld.
* Feature: Item pool is now generated based on the vanilla item pool, with slight modifications. You can actually get rings, and the Zweihander now!
* Feature: Multi-save support! Now you (or somebody else!) can start a new character on your slot without risk of losing items! Note: Most local items will still have to be re-acquired in the new save. Same-slot seamless co-op play may also be possible (not thoroughly tested) - see the [Co-op Toleration](#Co-op-Toleration) section below.
* Feature: "Excluded Location Behavior" yaml option added, to allow for not randomizing excluded areas at all, instead of just making them have non-priority, non-useful randomized items.
* Update: Yaml options simplified - Fogwall Lock and Fogwall Sanity options to just Fogwall Sanity (same for the Boss equivalents).
* Update: Yaml option for "Locked Undead Asylum Fog Wall" was removed, and not folded into any other option.
* Update: Yaml option for "Singular Boss Souls" was removed, due to being irrelevant.
* Update: Yaml option for "Enable Master Key" was removed, due to being confusing.
* Fix: Account for shop items and un-randomized drops for being in logic, instead of also adding them to the pool
* Fix: Massive reduction of bandwidth and game data storage usage

## Version 0.0.21.0
* Version update -> 0.0.21.0. Client is compatible with 0.0.20.0-generated apworlds.
* Feature: Add yaml option "fogwall_lock" - introduces AP items which are "keys" that you must acquire before you can go through fog walls. Default on.
* Feature: Add yaml option "boss_fogwall_lock" - like above, but for boss fog walls. Does not include Asylum Demon (first encounter), Sif, or Seath (2nd encounter), as all of them do not have fog walls upon your first entry to their arena.
* Feature: Add "Fogwall Sanity" and "Boss Fogwall Sanity" - get items when you pass through fog walls and the first time through boss fog walls, respectively.
* Feature: Add Universal Tracker (UT) support for importing poptracker maps.
* Feature: Added commands /fog and /bossfog for tracking which of the above keys you've acquired. Also added /lock to view all lockable events (currently only bossfogs and fogs).
* Feature: Added command /diag to collect diagnostic information. If you report a bug, you can include the screenshot from this output for more information.
* Feature: Add Overlay toggle (on by default).
* Feature: Autoscroll on by default.
* Feature: Upgrade information is now in spoiler log. 
* Fix: Goal should now reliably send upon defeating Gwyn. Made SendGoal put its work on the scheduler to avoid a deadlock, and added a message for when it does try to send.
* Fix: Various fixes to async processing.
* Fix: Added command /goalcheck to re-try sending the goal if goal conditions have been met (either Gwyn is defeated or player is in NG+). If it works or doesn't work, please send us a screenshot with the results.
* Fix: Logic - Require skull lantern for Tomb of the Giants.
* Fix: Logic - Added Kaathe entrance to the Lordvessel Altar/Kiln (was irrelevant before Fog Wall Lock)
* Fix: Logic - 2 demon ruins items require Orange Charred Ring
* Fix: Logic - Removed Upper Undead Burg -> Darkroot Basin connection that does not go through the Watchtower Basement.
* Fix: Logic - Add 2-way connections to relevant Entrances in the logic.
* Fix: Logic - Corrected/adjusted many region connections
* Fix: Logic - NL: Key to the Seal now requires Lordvessel (no longer potentially require killing the NPC)
* Fix: Logic - Make artificial logic only apply when both fogwall lock and boss fogwall lock options are off.


## Version 0.0.20.0
* Feature: Enable DLC (#50). Exclude-able as location group "All DLC regions" as noted below
* Feature: Game overlay to display AP messages from the DSAP client while in game (including received and found items). Note: Overlay may not show up in streams of the game on discord, etc, as it uses another window (which may appear mostly blank in the alt-tab display).
* Feature: Weapon Upgrades via yaml option (#48)
* Feature: Add /deathlink command to toggle it post-seed creation, and /help command to list DSAP-specific commands (#68)
* Feature: Add location groups (#62). Using this feature you can exclude whole groups of locations such as "All DLC regions" or "Painted World", "The Great Hollow", "Ash Lake", "Upper Blighttown Depths Side", etc (See [Location Groups](#Location-Groups) below). Excluding locations does not prevent them being randomized at this point, but only makes it so they should not have items classified as useful or priority.
* Feature: Add item groups (#62). Not excludable yet, but you are now able to mark them as local or non-local.
* Feature: Unstuck button (#64) - will teleport you to Firelink Shrine if you've at least reached there. Useful if stuck in the Duke's Archive without a key.
* Feature: Compatibility support for 0.0.19.1 and warning/error messages for unsupported apworld versions
* Fix: Goal detection now requires being in-game (#52)
* Fix: Items should now only be sent while player is actually in game - and queued for sending when received while they aren't in game (#71)
* Fix: Fixed Deathlink "desync" issue and potentially other random things failing due to incorrectly calculating addresses for their in-memory structures (#66)
* Fix: No longer replacing every enemy lot (#47)
* Fix: Embers are always added to the item pool, and only added once (#47)
* Fix: Firelink Shrine Chest behind Frampt ("key item lost and found") replaced with rubbish to prevent accidentally breaking AP logic (#64)
* Fix: Item pool - Soul spells added to pool instead of consumable soul items (#70)
* Fix: Location - nonexistent DA: Reah's Cell location removed (#47)
* Fix: Location - PW: Twin Humanities not being detected as "checked" fixed (#49)
* Fix: Location - PW: Velka's Rapier now requires Annex Key (#49)
* Fix: Location - PW: Gold Coin doesn't require Annex Key (#54)
* Fix: Location - PW: "Next to Stairs" check hint text improved
* Fix: Items - Elite Cleric Armor set added (#47)
* Fix: Post-Seath Arena entry logic (#61)
* General: Better error mesaging (#50, #65, #68, etc)

# Roadmap
## v0.1.0 (planned)
* Feature: Starting Item randomization

## v0.2.0 (planned)
* Feature: Shop Items randomized

## v1.0.0 (planned)
* Feature: Enemy drops randomized

## At some point
* Linux support
* Traps

# Location Groups
All Boss Fog Walls  
All DLC regions  
All Doors  
All Fog Walls  
All Item Lots  
Anor Londo  
Ash Lake  
Chasm of the Abyss  
Crystal Cave  
Darkroot Basin  
Darkroot Garden  
Demon Ruins  
Depths  
Firelink Shrine  
Kiln of the First Flame  
Lost Izalith  
Lower Blighttown  
Lower New Londo Ruins  
Lower Undead Burg  
Northern Undead Asylum  
Northern Undead Asylum Second Visit  
Oolacile Sanctuary  
Oolacile Township  
Painted World of Ariamis  
Royal Wood  
Sanctuary Garden  
Sen's Fortress  
The Abyss  
The Catacombs  
The Duke's Archives  
The Great Hollow  
Tomb of the Giants  
Undead Asylum Cell  
Undead Parish  
Upper Blighttown Depths Side  
Upper Blighttown VotD Side  
Upper New Londo Ruins  
Upper Undead Burg  
Valley of the Drakes  
Watchtower Basement  

# Item Groups
Ammunition  
Armor  
Boss Fog Wall Keys  
Boss Souls  
Carvings  
Catalysts  
Consumables  
Covenant Items  
Embers  
Fire Keeper Souls  
Fog Wall Keys  
Junk  
Key items  
Lord Souls  
Melee Weapons  
Multiplayer Items  
Progression Items  
Ranged Weapons  
Rings  
Shields  
Souls  
Spell Tools  
Spells  
Talismans  
Traps  
Upgrade Materials  
Weapons  

# Artificial Logic Without Fogwall Locking
* If you disable the options for fogwall or boss fogwall locking, some artificial "logic" is introduced to limit the number of items that are "in logic" extremely early. This does not affect actual access, but affects what the randomizer considers "logically possible" to access at any point. Such rules are listed below.
* Access to The Catacombs is behind defeating Ornstein and Smough.
* Access to the Great Hollow is behind Blighttown access + Lordvessel item.
* Access to New Londo Ruins Door to the Seal + Lower New Londo Ruins from Upper New Londo Ruins requires access to being able to defeat Ornstein and Smough (in addition to having the Key to the Seal - the default rule).
* Access to Tomb of the Giants from after Pinwheel requires access to being able to defeat Ornstein and Smouth (in addition to having the Skull Lantern - the default rule).

# Co-op Toleration
As of v0.0.22.0, using the Seamless Co-op mod may work with DSAP. It has not been very thoroughly tested, and if there are any crashes or instability caused by the Seamless Co-op mod itself, we cannot do much about it. Please read the information below.
* Q: How to set it up?
  * A: **Both players should always connect with the DSAP client to the same slot** once they load into the game after creating their characters.
        This must be done before doing any checks, so it is recommended to do so **before hosting or joining** the host's session.
        On first connect, you will need to head to the Undead Asylum bonfire and get your co-op items before joining the other's session.
* Q: What items are shared?
  * A: Any items sent by other slots will be sent to both players. Any fog wall keys found in this slot's own world will be sent to both players. Any checks the co-op players get for other slots will be sent immediately when the first player picks it up or makes the check.
* Q: What items aren't shared?
  * A: Everything else - any items that the player would normally get the item popup for in-game will need to be received by each player. For boss kills, when the boss is killed with both players in the session, they will both get the item. For items on the ground, each player will have to pick them up individually.
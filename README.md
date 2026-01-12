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
[Artificial Logic Without Fogwall Locking](#Artificial-Logic-Without-Fogwall-Locking)  

# How it works
* Every loose item on the ground, and some doors, are "locations" or "checks", and can each contain any item from the multiworld randomized item pool.
* Keys and progression items (e.g. Lordvessel) will be randomized into the item pool.
* Other items (consumables, souls, unique items, equipment) will also be randomized into the item pool.
* Undead Asylum is not randomized. This is intended behavior so as to not put the player into BK mode immediately.
* Enemy loot and shop items are not yet randomized.
* Item pool is currently constructed as follows:
    1. First all guaranteed items and key items are added to the pool.
    2. Then the master key is added if it is enabled in the yaml options.
    3. Then all embers are added to the pool.
    4. With what's left, 20% are consumables, 30% are specifically souls, 20% are materials, and the rest are weapons, armour, shields, spells or rings.
  This means that not all items are guaranteed to be in the item pool.

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
  * A: Not yet, but it is being researched.
* Q: Can I use this to randomize enemies?
  * A: This mod will not randomize enemies, but some players have had success with external enemy and boss randomizers. That said, we cannot guarantee they will continue to work, and that future updates won't break compatibility.
* Q: Does this work with Prepare to Die edition?
  * A: No, The current release only works with Dark Souls Remastered. There may be potential to make it compatible with PTDE but not until we are feature-complete on remastered, as there isn't a way to legally obtain a new copy of PTDE anymore.
* Q: Does this work on Linux?
  * A: Not yet. The underlying client library doesn't support linux yet, as the way memory is read and written to is a bit different. Work is ongoing to add support but we're not there quite yet.
* Q: Can I randomized starting gear? 
  * A: Not yet - this is planned for the future. Currently, it is recommended to create your character before connecting with the DSAP client.
* Q: Is there a tracker?
  * There is a poptracker pack available at https://github.com/routhken/Dark_Souls_Remastered_tracker/releases
* Q: Why do i keep dropping items (yellow bags) on the floor?
  * A: You probably have a full stack of Prism Stones in your inventory. This is the item we use to replace vanilla items when the Archipelago item is in another players' world. As long as you've "picked up" the item from the original location, leaving the dropped bag won't cause any issues. We technically check whether the item is still in the original location, not whether it is in your inventory. To prevent this, you can discard most/all of your prism stones when they get up to or close to 99.

# Known issues
* Master Key chosen from character creation (whether as a gift or thief starting item) is not considered to be in-logic, regardless of your yaml settings.
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
## Version 0.0.21.0 (upcoming)
* Version update -> 0.0.21.0. Client is compatible with 0.0.20.0-generated apworlds.
* Feature: Add yaml option "fogwall_lock" - introduces AP items which are "keys" that you must acquire before you can go through fog walls. Default on.
* Feature: Add yaml option "boss_fogwall_lock" - like above, but for boss fog walls. Does not include Asylum Demon (first encounter), Sif, or Seath (2nd encounter), as all of them do not have fog walls upon your first entry to their arena.
* Feature: Add "Fogwall Sanity" and "Boss Fogwall Sanity" - get items when you pass through fog walls and the first time through boss fog walls, respectively.
* Feature: Add Universal Tracker (UT) support for importing poptracker maps.
* Feature: Added commands /fog and /bossfog for tracking which of the above keys you've acquired. Also added /lock to view all lockable events (currently only bossfogs and fogs).
* Feature: Added command /diag to collect diagnostic information. If you report a bug, you can include the screenshot from this output for more information.
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
## 0.0.22 (planned)
* Feature: Item pool Balancing and options
* Feature: Starting Item randomization

## At some point
* Seamless co-op and "new save file" support

# Location Groups
All DLC regions  
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

# Artificial Logic Without Fogwall Locking
* If you disable the options for fogwall or boss fogwall locking, some artificial "logic" is introduced to limit the number of items that are "in logic" extremely early. This does not affect actual access, but affects what the randomizer considers "logically possible" to access at any point. Such rules are listed below.
* Access to The Catacombs is behind defeating Ornstein and Smough.
* Access to the Great Hollow is behind Blighttown access + Lordvessel item.
* Access to New Londo Ruins Door to the Seal + Lower New Londo Ruins from Upper New Londo Ruins requires access to being able to defeat Orenstein and Smough (in addition to having the Key to the Seal - the default rule).
* Access to Tomb of the Giants from after Pinwheel requires access to being able to defeat Ornstein and Smouth (in addition to having the Skull Lantern - the default rule).

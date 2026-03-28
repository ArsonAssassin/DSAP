# Archipelago implementation for Dark Souls Remastered

## **This implementation is still considered unstable/in alpha. Expect bugs and issues.**

#### Table of Contents
[How Randomization Works](#How-Randomization-Works)  
[How Fogwall Sanity Works (important)](How-Fogwall-Sanity-Works)  
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
[Contributors](#Contributors)  

# How Randomization Works
* Every loose item on the ground, and potentially fog walls, are "locations" or "checks". Some guaranteed drops are also locations, in addition to the White Sign Soapstone location from the first Solaire encounter.
* All items in those locations will be shuffled into the randomized multiworld Item Pool. This means they can be found elsewhere, and that the items at those locations will themselves be replaced by other items in the Item Pool.
* All keys and progression items (e.g. Lordvessel) will be forced into the Item Pool, unless they drop or are shoppable from non-randomized locations.
* Undead Asylum is mostly not randomized. This is intended behavior so as to not put the player into BK mode immediately. The only randomized item is the one by the stairs to the exit.
* Enemy loot and shop items are not yet randomized. Some bosses are exceptions to this.
* Item Pool is currently constructed as follows:
    1. First all items in locations which were randomized are added to the pool.
    2. Then, key items and embers will replace any filler items. This includes any "Fog Wall Keys", if either `fogwall_sanity` option is on. This includes the three living Firekeepers' Souls.
    3. Then, Guaranteed items are added to the pool.
    4. If there are any Filler or Junk items left over, they are replaced with Souls of a Proud Knight (2000 souls each).
* If locations are excluded and excluded_locations_behavior is set to "do_not_randomize", then the items in those locations will not be added to the pool, and those locations will have their vanilla items. Even with `fogwall_sanity` on, fog walls excluded in this manner this will provide no item, as fog walls do not provide items in the base game.

# How Fogwall Sanity Works
* Fogwalls in DSR will be locked, and will require an item from the Item Pool to unlock them. Passing through them will also count as a location or "check". This is called "fogwall sanity", and it is on by default.
* This includes the Fog Wall near the beginning of the Upper Undead Burg. If it's blocked - you'll have to go somewhere else! (e.g. check the skeleton courtyard, and part of Upper New Londo Ruins)
* This does not include the fogwall in the Northern Undead Asylum.
* This `fogwall_sanity` option can be disabled, but doing so is not recommended. Without this option, about 60% of the locations in your game would be consdiered in logic immediately - this is also considered a big "Sphere 1". With a big Sphere 1, you might need to get to the last check in the immediately available 60% of your game, in order to find an item that unlocks your friend who is stuck at 10% of their game. Because of how large Dark Souls is, it could take a long time to do that many checks - and make your friends have to wait on you, for quite a while.
* There is an additional `boss_fogwall_sanity` option that can be turned on, which makes most boss' arena fogs be similarly locked.
* The `Catacombs` and `Lower New Londo Ruins` fogwalls can be bypassed by basic platforming & elevator usage repsecitvely. As a result, they do not logically block access to their other sides.

# Initial Setup
1. Download the latest APWorld and Client from the releases page.
2. Double click on the APWorld to install it to your Archipelago installation
3. Extract the Client zip. It is recommended to not extra it to Program Files / your game install directory.
4. Run the Archipelago Launcher and open "Options Creator" (Archipelago v0.6.5+ only). In Options Creator:
    * Select "Dark Souls Remastered", type in a player name for your slot, and edit the options to your liking.
    * Export to your Archipelago "Players/" folder (not the Templates subfolder!). This will put a yaml with your player name in that folder.
5. Either run Generate.py or press the Generate button on the launcher to generate a seed/multiworld.
6. When this has completed, you will have a zip file in your Archipelago/Output folder.
7. Host the output zip either using the Host option on the launcher or by uploading it to Archipelago.gg
8. Optional: Back up your existing DSR saves into another folder and label that folder appropriately.

# Before you go back online
* You must close this program, and then restart Dark Souls Remastered. 
* You should move your AP DSR saves elsewhere, and restore your original backed up saves.
* If you do not do the above, you risk loading into a save with this program's modifications still in effect, and **risk facing negative consequences by FromSoft as mentioned below.**

# Usage
1. When you start up the game, ensure you are **disconnected from the network**. It is recommended to configure Dark Souls Remastered's settings in System->Network Settings->Launch Setting="Start Offline", to avoid accidentally starting online.
**WARNING: You should never connect to the FromSoft network while using this mod or its saves. If you are connected to the online Servers while using this mod, or with a save in which this mod was used, you will likely face account restrictions (bans) by FromSoft!!**
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
* This version has been tested with Dark Souls: Remastered, Steam version (App ver. 1.03.1 & Regulation ver. 1.04) on Windows 11, with Archipelago Launcher version 0.6.6. Using incorrect versions of Dark Souls: Remastered may result in a crash upon connecting.
* Linux now has preliminary support via Proton with v0.1.0. You should be able to add `PROTON_REMOTE_DEBUG_CMD="/full/path/to/DSAP.client.exe" %command%` to your steam Launch Options (tested with Proton Hotfix branch on 2026-03-27) to run both DSAP and DS:R in the same environment. It has not been thoroughly tested, however, so 1) consider it unstable, 2) let us know how it plays/runs (whether well or badly), and 3) Please report any issues.

# Frequently Asked Questions (FAQ)
* Q: Can I use this with seamless co-op?
  * A: Toleration has been added for multiple players, but not thoroughly tested. See [Co-op Toleration](Co-op-Toleration) section below.
* Q: Can I use this to randomize enemies?
  * A: This mod will not randomize enemies, but some players have had success with external enemy and boss randomizers. That said, we cannot guarantee they will continue to work, and that future updates won't break compatibility.
* Q: Does this work with Prepare to Die edition?
  * A: No, The current release only works with Dark Souls Remastered. There may be potential to make it compatible with PTDE but not until we are feature-complete on remastered, as there isn't a way to legally obtain a new copy of PTDE anymore.
* Q: Does this work on Linux?
  * A: With v0.1.0 - DSAP seems to work under Proton! Huge thanks to discord user @theabysmalkraken for finding the missing piece. See [Compatibility](Compatibility) section above for more details.
* Q: Can I randomized starting gear? 
  * A: Not yet - this is planned for the future. Currently, it is recommended to create your character before connecting with the DSAP client.
* Q: Is there a tracker?
  * There is a poptracker pack available at https://github.com/routhken/Dark_Souls_Remastered_tracker/releases (poptracker download itself at https://github.com/black-sliver/PopTracker/releases)
  * Universal Tracker (UT) also works, and in it you can import the maps from the poptracker pack. UT download can be found at https://github.com/FarisTheAncient/Archipelago/releases

# Known issues
* Master Key chosen from character creation (whether as a gift or thief starting item) is not considered to be in-logic. Randomized Character creation/gifts replaces the master key from the thief's starting item and the starting gifts respectively.
* Placing Lord Souls at Firelink Altar does not open the door - This seems to be due to not having received some number of the Lord Souls or Lordvessel. We could use information for this - If you see this, please run the /lordvessel command, which will both provide diagnostic information & the missing items. Please provide a screenshot of the output with any additional context you can provide about the missing items to the dark-souls-1 channel in the AP discord (such as, if you know it, did the items come in while you were offline, was it with other items, etc).
* v0.0.22.0 and v0.0.21.0: Hard lock / infinite loop of receiving Rubbish if player has been /send'd a valid AP item that the client doesn't know about (Estus flask, Event items, etc). Resolved in v0.1.0 with an error message instead.
* v0.0.21.0: Dispelling of Golden fogwalls inconcorrectly considered in logic once player had Lordvessel, even if it cannot be placed at Firelink Altar.
* v0.0.21.0: Boss fog walls in the DLC do not correctly "Lock" with boss fog wall locks on.
* v0.0.21.0 and lower: Once a save receives an item from the server, it cannot be re-received to a new save or different player. Fixed with v0.0.22.0 (`Multi-save support!`).
* v0.0.21.0 and lower: Prism stone received at locations in DSR player's game which are replaced with other multiworld players' items. Updated to no longer occur with v0.0.22.0 (`AP items as DSR items`).
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
## Version 0.1.0
* Version update -> 0.1.0. Both Apworld and Client have updated. **This Client version will NOT be compatible with earlier versions of the apworld.**
* Feature: Linux support - huge thanks to discord user @theabysmalkraken. From basic tests appears to work, but not tested thoroughly - should be considered somewhat more unstable.
* Feature: Starting loadout, gifts, and spells randomization - including yaml options for controlling them. See the options for more details.
* Feature: Server-delivered items - Items will now always be delivered by the server. This may cause a slightly delayed item popup.
* Feature: Synced "looted" items between saves - Now upon starting a new save on an in-progress slot, you'll get all the items that slot ever looted. For now, "empty" items at those locations will still exist in the world. Possible due to Server-delivered items.
* Feature: Custom Controls window for client settings. Settings do not yet persist between sessions, but are a lot easier to change. Deathlink can be more easily toggled from here as well.
* Feature - Item popup options - In the "Custom Controls" window, player can now change the categories of items for which they will get popups. Works for both items from your own game & items sent from others' games.
* Feature: Yaml option - Remove weapon stat requirements
* Feature: Yaml option - Remove spell stat requirements
* Feature: Yaml option - Remove miracle covenant requirements - some miracles have 
* Feature: Add /lordvessel command - For players to use if placing all 4 souls at the Firelink Altar doesn't open the Kiln door. Intended to catch the case where the client didn't receive the items correctly - both getting diagnostics & making player whole (gives them the missing "received" items). Please provide the output to us in the dark-souls-1 discord channel if you have to use this command to help us debug this issue!
* QoL: Sanitization on host and slot, remove "/connect " prefix if it's in host string, and trim spaces from both strings.
* Fix: Logic - Basement Door access no longer requires Taurus Demon defeat
* Fix: Unreceivable items causing infinite rubbish loop. Now they will just display an error message instead.
* Code quality: massive refactoring of code which updates DSR item lots, messages, and params
* Fix: Improved messaging for case where player connects with save from a previous instance of a multiworld (when a 2nd room is created from 1 seed / AP_####.zip).
* Fix: DLC Boss Fog Walls added to `All DLC regions` location group.

## Version 0.0.22.1 (Client Hotfix)
* Client Version update -> 0.0.22.1. Fully compatible with 0.0.22.0 worlds, but not compatible with apworlds at or below v0.0.21.
* Apworld is unchanged.
* Fix: Crashes, incorrect items when too many non-local items are located within DSR, due to incorrectly built item params.

## Version 0.0.22.0
* Version update -> 0.0.22.0. Both Apworld and Client have updated. **This Client version will NOT be compatible with earlier versions of the apworld.**
* Feature: The Item Pool is now generated based on the vanilla item pool, with slight modifications. You can actually get rings, and the Zweihander now! You can expect to get less Soul items / less souls from the average soul item compared to v0.0.21 and lower.
* Feature: See received items! Item popups will now appear for items received by other players. You can disable showing the popup for non-progression or all items via the `/ripshow` text command in DSAP (see `/help` for details)
* Feature: See sent items! AP Items as DSR Items: Items for other players, and your own Fog Wall Keys now show what they are in the standard in-game item popup, instead of showing up as prism stones. And no more of those stones weighing down your bag! They do still *look* like prism stones, though.
* Feature/Fix: Multi-save support! Now you (or somebody else!) can start a new character on your slot without risk of losing items! Note: Most local items will still have to be re-acquired in the new save. Same-slot seamless co-op play may also be possible (not thoroughly tested) - see the [Co-op Toleration](#Co-op-Toleration) section below.
* Feature: "Excluded Location Behavior" yaml option added, to allow for not randomizing excluded areas at all, instead of just making them have non-priority, non-useful randomized items.
* Feature: No more need to immediately battle through the difficult Catacombs immediately! Added yaml option `logic_to_access_catacombs`, which allows you to add additional conditions to catacombs logical access - via access to Andre, Undead Merchant (in Undead Burg), or Ornstein and Smough. Default is access to either Andre or the Undead Merchant.
* Feature: Yaml-less Universal Tracker (UT) support.
* Update: Doors checks have been removed (for now). They may be re-introduced in the future with a "door sanity" type of option.
* Update: Yaml options simplified - Fogwall Lock and Fogwall Sanity options combined into just Fogwall Sanity (same for the Boss equivalents).
* Update: Yaml option for "Locked Undead Asylum Fog Wall" was removed, and not folded into any other option. It too quickly caused BK mode.
* Update: Yaml option for "Singular Boss Souls" was removed, due to being irrelevant.
* Update: Yaml option for "Enable Master Key" was removed, due to being confusing. It is possible to add it into the pool via the Guaranteed Items option.
* Update: Added multiple text commands to the client: `/connect`, `/unstuck`, a limited `/warp` command for re-accessing blocked areas, `/ripshow` for limiting popups for items received from other games.
* Update: Saves will now keep track of the slot (and generated seed) they last connected to, and warn player if the save they load into was both 1) made by a v0.0.22+ client and 2) previously connected to a different slot (and seed). This should help guard against accidentally sending the checks completed by a save which was used for a different generated multiworld. Does not protect against this when loading v0.0.21 and lower saves.
* Fix: Account for shop items and un-randomized drops being in logic, instead of also adding them to the pool.
* Fix: Logic - Golden fogwalls no longer considered clearable until you can actually *place* the Lordvessel.
* Fix: Location name: Renamed `DR: Soul of a Brave Warrior - Ruins/Domain Shortcut` to `DR: Soul of a Brave Warrior - Ruins/Domain Elevator` for clarity
* Fix: Location name: Renamed `DR: Soul of a Brave Warrior - Chaos Door` to `DR: Soul of a Brave Warrior - Chaos Covenant Door`
* Fix: DLC Boss Fogs now made to work correctly.
* Fix: Broken Pendant, Duke's Archives Cell Key, Residence Key, Crest of Artorias, and Covenant of Artorias should now only be in their vanilla locations, instead of also randomized into the Item Pool.
* Fix: Some improvements to client behavior after reconnection to AP Server.
* Fix: Massive reduction of bandwidth and game data storage usage
* Fix: Pressing `Enter` while in the client text box will now submit text commands.

## Version 0.0.21.0
* Version update -> 0.0.21.0. Client is compatible with 0.0.20.0-generated apworlds.
* Feature: Add yaml option `fogwall_lock` - introduces AP items which are "keys" that you must acquire before you can go through fog walls. Default on.
* Feature: Add yaml option `boss_fogwall_lock` - like above, but for boss fog walls. Does not include Asylum Demon (first encounter), Sif, or Seath (2nd encounter), as all of them do not have fog walls upon your first entry to their arena.
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
## v0.2.0 (planned)
* Feature: Shop Items randomized

## v1.0.0 (planned)
* Feature: Enemy drops randomized

## At some point
* Traps
* Option to consider certain in-game skips logical.

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
* If you disable the options for fogwall or boss fogwall locking, some artificial "logic" is introduced to limit the number of items that are "in logic" extremely early. This does not affect actual access, but affects what the randomizer considers "logically possible" to access at any point. Such rules are listed below:
* Access to the Great Hollow is behind Blighttown access + Lordvessel item.
* Access to New Londo Ruins Door to the Seal + Lower New Londo Ruins from Upper New Londo Ruins requires access to being able to defeat Ornstein and Smough (in addition to having the Key to the Seal - the default rule).

# Co-op Toleration
As of v0.0.22.0, using the Seamless Co-op mod may work with DSAP. It has not been very thoroughly tested, and if there are any crashes or instability caused by the Seamless Co-op mod itself, we cannot do much about it. Please read the information below.
* Q: How to set it up?
  * A: **Both players should always connect with the DSAP client to the same slot** once they load into the game after creating their characters.
        This must be done before doing any checks, so it is recommended to do so **before hosting or joining** the host's session.
        On first connect, you will need to head to the Undead Asylum bonfire and get your co-op items before joining the other's session.
* Q: What items are shared?
  * A: Any items sent by other slots will be sent to both players. Any items found in your own world should also be immediately sent to both players. Any items found in your world for other worlds will be sent to the server as a check only once (even if it appears to send multiple times).
* Q: What items aren't shared?
  * A: Unrandomized items (mostly enemy drops), and the "fake" randomized item locations in DSR. The latter means that when 1 player picks up such an item, the 2nd player will still see an "item pickup" in the world, but it will be empty if they go to pick it up. This is because the server will have already sent the actual item to both players, and this is what triggers the item receive notification. Because the server will not re-send the item, there will be no notification on picking up such items on the 2nd+ time.

# Contributors:
* ArsonAssassin - Creator and Maintainer
* tathxo (aka noka) - Contributor
* Nave - Contributor

Archipelago implementation for Dark Souls Remastered by ArsonAssassin

# **This implementation is still considered unstable/in alpha. Expect bugs and issues.**

#### Table of Contents
[How it Works](#How-it-works)  
[Artificial Logic](#Artificial-Logic)  
[Initial Setup](#Initial-Setup)  
[Before you go back online](#Before-you-go-back-online)  
[Usage](#Usage)  
[Troubleshooting](#Troubleshooting)  
[Compatibility](#Compatibility)  
[Frequently Asked Questions (FAQ)](#Frequently-Asked-Questions-FAQ)  
[Known issues](#Known-issues)  

# How it works
* Every loose item on the ground, and some doors, are "locations" or "checks", and can each contain any item from the multiworld randomized item pool.
* Keys and progression items (e.g. Lordvessel) will be randomized into the item pool.
* Other items (consumables, souls, unique items, equipment) will also be randomized into the item pool.
* Undead Asylum is not randomized. This is intended behavior so as to not put the player into BK mode immediately.
* Enemy loot and shop items are not yet randomized.
* Item pool is currently constructed as follows:
    1. First all guaranteed items and key items are added to the pool.
    2. Then the master key is added if it is enabled in the yaml options.
    3. With what's left, 20% are consumables, 30% are specifically souls, 20% are materials (including embers), and the rest are weapons, armour, shields, spells or rings.
  This means that not all items are guaranteed to be in the item pool.

# Artificial Logic
* While in an alpha state, some artificial "logic" is introduced to limit the number of items that are "in logic" extremely early. This does not affect actual access, but affects what the randomizer considers "logically possible" to access at any point. Most such rules are listed below.
* Access to The Catacombs is behind defeating Ornstein and Smough.
* Access to the Great Hollow is behind Blighttown access + Lordvessel item.
* Access to Lost Izalith is behind Orange Charred Ring item.
* Access to New Londo Ruins Door to the Seal + Lower New Londo Ruins is behind Orenstein and Smough defeated + Key to the Seal item.
* Access to Kiln of the First Flame is behind Lord Soul (Bed of Chaos).

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
**WARNING: You should never connect to the FromSoft network while using this mod. If you are connected to the online Servers while using this mod, or with a save in which this mod was used, you will likely face account restrictions by FromSoft!!**
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
* If you encounter issues, first check the known issues listed below. Then, check what version you are on - the issue you have may be resolved by updating to a later version.
* If item lots are not replaced, or the client cannot connect, try running DSAP.client.exe as administrator. This program requires authorization to modify the memory of another process, so it may require elevated permissions depending on your system configuration.
* After that, you may be able to find answers in the AP Discord channel for dark-souls-1.
  * If there are no answers, you can comment in the channel and include the version number of DSAP that you are using, the Archipelago version you are using, and a description of the issue.
  * If the program crashed, note the time of the error, then open "Event Viewer" from the start menu, go to Windows Logs->Application, and look for an "Error" level log entry. Right click the relevant entry to copy the details as text, and provide the file with your report. If there are multiple Error entries at the time of error, provide both.

# Compatibility
* This version has been tested with Dark Souls Remastered, Steam version (App ver. 1.03.1 & Regulation ver. 1.04) on Windows 11, with Archipelago version 0.6.3.
* Linux, even through Proton/Wine, is not yet supported

# Frequently Asked Questions (FAQ)
* Q: Randomized starting gear? 
  * A: Not supported yet. You must create your character before connecting with the DSAP client.
* Q: Randomized enemies/enemizer?
  * A: Not included. Some players have had success with using a separate enemy randomizer. Randomizing bosses could introduce logic issues, but other enemy drops are not item "locations", so randomizing them should be fine.

# Known issues
* If you receive an item while on the main menu, it may get lost, requiring admin intervention. **For safety, you should only run the client once loaded into game.**
  * Furthermore, **you should close the client before quitting to menu or quitting the game.**
* Looting the "key item chest" in Firelink Shrine behind Frampt **will break logic for DSR**. In a vanilla playthrough, this chest is usually empty/already open, and only has items if you somehow don't have a key item you "should have", depending on where you are in the game. In a randomizer environment, those normal circumstances don't apply! As an example: if you loot this chest after looting the vanilla "Basement Key" location, it will have a Basement Key - but in AP randomizer that key can even be in another game!
* Master Key chosen from character creation (whether as a gift or thief starting item) is not considered to be in-logic, regardless of your yaml settings.
* v0.0.19.1 and lower: On reconnect, player can receive duplicate items. The items are specifically those from "door"-type location checks in their own world.
* v0.0.19.1 and lower: Some enemy drops (invaders, Havel, etc) are erroneously replaced with prism stones, but do not grant an AP item. The player should get the standard enemy drop in these locations instead.
* v0.0.19.1 prerelease: Player does not receive deathlinks from other players.
* v0.0.19.1 prerelease: While unhollowed/human, the player is detected as "not in game". This can result in no items or deathlinks being sent to other players.
* v0.0.18.3: DSR game and DSAP.client.exe both crash upon connect - you must load into the game and be able to move your character around before connecting with the client.
* v0.0.18.2 and lower: Items do not get replaced. Upgrade your client version.

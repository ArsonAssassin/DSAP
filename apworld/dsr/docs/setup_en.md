# Dark Souls: Remastered Archipelago Randomizer Setup Guide

## Required Software

- [Dark Souls: Remastered](https://store.steampowered.com/app/570940/DARK_SOULS_REMASTERED/)
- [dsr.apworld - Dark Souls: Remastered Apworld](https://github.com/ArsonAssassin/DSAP/releases/latest)
- [DSAP - Dark Souls: Remastered AP client](https://github.com/ArsonAssassin/DSAP/releases/latest)
- [Archipelago Launcher](https://github.com/ArchipelagoMW/Archipelago/releases/latest)

## Optional Software

- [Poptracker pack with maps](https://github.com/routhken/Dark_Souls_Remastered_tracker/releases/tag/v1.0.22)

## Setting Up

1. Download the latest APWorld (`dsr.apworld`) and DSAP Desktop Client zip (`dsr-Windows-x64.zip`) from the pages linked above.
2. Double click on the APWorld to install it to your Archipelago installation.
3. Extract the DSAP Desktop Client zip. It is recommended to not extra it to Program Files / your game install directory.

### Creating your Options File (yaml)
1. Install the latest Archipelago Launcher version from the page linked above.
2. Run the Archipelago Launcher and open "Options Creator" (Archipelago v0.6.5+ only). In Options Creator:
    * Select "Dark Souls Remastered", type in a player name for your slot, and edit the options to your liking.
    * Export to your Archipelago "Players/" folder (not the Templates subfolder!). This will put a yaml with your player name in that folder.

### Generating a World
1. Install the latest Archipelago Launcher version from the page linked above.
2. Place all the .yaml files you wish to generate with in the Players/ folder of your Archipelago Launcher installation.
3. Either run Generate.py or press the Generate button on the launcher to generate a seed/multiworld.
4. When this has completed, you will have a zip file in your Archipelago/Output folder.
5. Host the output zip either using the Host option on the launcher or by uploading it to Archipelago.gg

### Running and Connecting the Game

Optional: Backup or move your existing DSR saves into another folder and label that folder appropriately.
On Windows, DSR saves can typically be found at `C:\Users\<user>\Documents\nbgi\DARK SOULS REMASTERED\88627662\DRAKS0005.sl2`

To connect _Dark Souls: Remastered_ to Archipelago:

1. Start Steam.

2. To prevent you from getting penalized, **make sure to set _Dark Souls: Remastered_ to offline mode in the in-game options.** 
  It is recommended to configure settings in System->Network Settings->Launch Setting="Start Offline", to avoid
  accidentally starting online in future sessions.  
  **WARNING: You should never connect to the FromSoft network while using this mod or its saves.
  If you are connected to the online Servers while using this mod, or with a save in which this mod was used,
  you will likely face account restrictions (bans) by FromSoft!!**

3. Connect the Client:
   Run `DSAP.Client.exe` that you extracted earlier from the DSAP Desktop Client zip.
   You can do this while in the main menu, while in the New Game/character creation menu, or while loaded into a (AP-specific) save.  
   
   3a. Click the top-left three-horizontal-line ("hamburger menu") icon, and fill in your connection details: host, slot, and password (if required).

   3b. With _Dark Souls: Remastered_ running, press the "Connect" button in the DSAP client. You should see the client log start to populate, and if you have the overlay option on, messages appear over your game window.  
      You can click in the DSAP client window outside of the left-hand-side menu to show the Log.  
      If you were loaded into a save, this will cause a reload of the game, as if you had used a homeward bone. This is necessary to update the items that are in the game.
      
   3c. Right-click with your mouse into your _Dark Souls: Remastered window_. 
      Avoid left-clicking, because the game may interpret that as an "accept" on whatever option is currently selected.
      This could load into a previous save, if you have the "Connect" option, or an existing save on the Load menu, selected.

4. Start playing as normal. You must keep the DSAP client running while you play the game for items and location to be sent and received correctly. Note that the Keys and Estus Flask in the starting Undead Asylum area are not randomized, in order to prevent early BK mode.

### Before you go back online for standard non-AP play
* You must close this program, and then restart Dark Souls Remastered. 
* You should move your AP DSR saves elsewhere, and restore your original backed up saves.  
On Windows, DSR saves can typically be found at `C:\Users\<user>\Documents\nbgi\DARK SOULS REMASTERED\88627662\DRAKS0005.sl2`
* If you do not do the above, you risk loading into a save with this program's modifications still in effect, and **risk facing negative consequences by FromSoft as mentioned above.**

## Frequently Asked Questions

### What gets randomized?

See [the Game Page](./en_Dark%20Souls%20Remastered.md).

### Does this work on Linux?

Linux has preliminary support via Proton with v0.1.0. You should be able to add `PROTON_REMOTE_DEBUG_CMD="/full/path/to/DSAP.client.exe" %command%` to your steam Launch Options (tested with compatibility settings = Proton Hotfix branch on 2026-03-27) to run both DSAP and DS:R in the same environment. It has not been thoroughly tested, however, so 1) consider it unstable, 2) let us know how it plays/runs (whether well or badly), and 3) Please report any issues.

### Does this work with _Prepare to Die Edition_?
No, The current release only works with Dark Souls Remastered. There may be potential to make it compatible with PTDE but not until we are feature-complete on _Remastered_, as there isn't a way to legally obtain a new copy of PTDE anymore.

### Can I use this to randomize enemies?
This mod will not randomize enemies, but some players have had success with external enemy and boss randomizers. That said, we cannot guarantee they will continue to work, and that future updates won't break compatibility.

### Can I use this with seamless co-op?
Toleration has been added for multiple players, but not thoroughly tested. 
As of v0.0.22.0, using the Seamless Co-op mod may work with DSAP. It has not been very thoroughly tested, and if there are any crashes or instability caused by the Seamless Co-op mod itself, we cannot do much about it. Please read the information below.
* Q: How to set it up?
  * A: **Both players should always connect with the DSAP client to the same slot** once they load into the game after creating their characters.
        This must be done before doing any checks, so it is recommended to do so **before hosting or joining** the host's session.
        On first connect, you will need to head to the Undead Asylum bonfire and get your co-op items before joining the other's session.
* Q: What items are shared?
  * A: Any items sent by other slots will be sent to both players. Any items found in your own world should also be immediately sent to both players. Any items found in your world for other worlds will be sent to the server as a check only once (even if it appears to send multiple times).
* Q: What items aren't shared?
  * A: Unrandomized items (mostly enemy drops), and the "fake" randomized item locations in DSR. The latter means that when 1 player picks up such an item, the 2nd player will still see an "item pickup" in the world, but it will be empty if they go to pick it up. This is because the server will have already sent the actual item to both players, and this is what triggers the item receive notification. Because the server will not re-send the item, there will be no notification on picking up such items on the 2nd+ time.

## Troubleshooting

### Game crashes on connect
Check your DS:R version on the title menu. It should show the text "App ver. 1.03.1" & "Regulation ver. 1.04".
If it does not, update your game; you can force Steam to do so by Verifying Game files:
Right click game in library -> Properties -> Installed Files -> Verify integrity of game files

### Not receiving items, or receiving double items.
Try running DSAP.Desktop.exe as Administrator.

### Save Corrupted (usually upon quitting to menu), and progress lost
Because DSAP modifies DS:R memory and code directly, some antivirus software may see it or DS:R as malware when running in this mode. As a result, they restrict the ability of DS:R to make its save.
We have not had the opportunity to communicate directly with players who encountered this issue, but expect that adding DSAP, DS:R, or the .sl2 save file extension type to the antivirus' exceptions list may help to resolve the issue. If this works for you, please let us know in the dark-souls-1 channel in the AP discord.

### Placing Lord Souls at Firelink Altar does not open the door
This seems to be due to not having received some number of the Lord Souls or Lordvessel. If you see this, please run the /lordvessel command, which will both provide diagnostic information & the missing items. To help us debug this issue, please provide a screenshot of the output with any additional context you can provide about the missing items to the dark-souls-1 channel in the AP discord. Additional context that would be useful includes: did the items come in while you were offline, was it with other items, etc.

### Issue not listed here
Please let us know in the dark-souls-1 channel in the AP discord. Include any screenshots, including the output of the `/diag` command in the client.
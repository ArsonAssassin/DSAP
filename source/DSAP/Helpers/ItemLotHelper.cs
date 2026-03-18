using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.Models;
using DSAP.Models;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAP.Helpers
{
    internal class ItemLotHelper
    {
        private static ItemLotItem prismStoneLotItem = new ItemLotItem
        {
            CumulateLotPoint = 0,
            CumulateReset = false,
            EnableLuck = false,
            GetItemFlagId = -1,
            LotItemBasePoint = 100,
            LotItemCategory = (int)DSAP.Enums.DSItemCategory.Consumables,
            LotItemNum = 1,
            LotItemId = 370
        };
        private static ItemLotItem rubbishLotItem = new ItemLotItem
        {
            CumulateLotPoint = 0,
            CumulateReset = false,
            EnableLuck = false,
            GetItemFlagId = -1,
            LotItemBasePoint = 100,
            LotItemCategory = (int)DSAP.Enums.DSItemCategory.Consumables,
            LotItemNum = 1,
            LotItemId = 380
        };

        /// <summary>
        /// Build a mapping of the location flag values in eventflags to the ItemLot that should come from there, for items in our own game.
        /// </summary>
        /// <details>
        /// This is used for replacing item lots in our own game.
        /// </details>
        /// <param name="eventflags">A list of location flags of which items will be found.</param>
        /// <param name="resultMap">A dictionary mapping itemlot flags to "item lots".</param>
        /// <returns></returns>
        public static void BuildFlagToLotMap(out Dictionary<int, ItemLot> resultMap,
            List<EventFlag> eventflags,
            Dictionary<string, Tuple<int, string>> slotLocToItemUpgMap,
            Dictionary<long, ScoutedItemInfo> scoutedLocationInfo)
        {
            Dictionary<int, ItemLot> result = new Dictionary<int, ItemLot>();
            Dictionary<int, ItemLot> specialResult = new Dictionary<int, ItemLot>();

            var addonitems = 0;

            int i = 0;
            foreach (var (k, v) in scoutedLocationInfo)
            {
                i++;
                int locId = ((int)k);
                string target = v.Player.Name;
                EventFlag? lot = eventflags.Find(x => x.Id == locId);
                if (lot != null) /* found a location in our "item lots" */
                {
                    ItemLotItem newLotItem = new ItemLotItem { };
                    if (v.Player.Slot == App.Client.CurrentSession.ConnectionInfo.Slot) // it is us
                    {
                        /* Found an item of our own, located in our own game. 
                                 * Validate that it's in the eventflags we've been given, and find the matching item. */
                        DarkSoulsItem? item = App.AllItems.Find(x => x.ApId == v.ItemId);
                        if (item != null)
                        {
                            DarkSoulsItem repitem = item;
                            if (item.Category == Enums.DSItemCategory.AnyWeapon)
                            {
                                Log.Logger.Verbose($"Attempting to upgrade item: {App.Client.CurrentSession.ConnectionInfo.Slot}:{lot.Id} ({item.Name})");
                                if (App.DSOptions.UpgradedWeaponsPercentage > 0
                                    && slotLocToItemUpgMap.TryGetValue($"{App.Client.CurrentSession.ConnectionInfo.Slot}:{lot.Id}", out var itemupg))
                                {
                                    if (itemupg.Item1 == item.ApId) // if item apid matches
                                        repitem = MiscHelper.UpgradeItem(repitem, itemupg.Item2);
                                    else
                                    {
                                        Log.Logger.Error($"Item upgrade error: '{itemupg.Item1}' != '{item.ApId}', for item {item.Name} at {lot.Name}.");
                                        App.Client.AddOverlayMessage($"Item upgrade error: '{itemupg.Item1}' != '{item.ApId}', for item {item.Name} at {lot.Name}.");
                                    }
                                }
                            }

                            Log.Logger.Verbose($"Item {i} at location id{locId}/flag={lot.Flag} ({lot.Name}) is {target}/{repitem.Id}({repitem.Name})");
                            newLotItem = new ItemLotItem
                            {
                                CumulateLotPoint = 0,
                                CumulateReset = false,
                                EnableLuck = false,
                                GetItemFlagId = -1,
                                LotItemBasePoint = 100,
                                LotItemCategory = (int)repitem.Category,
                                LotItemNum = (byte)repitem.Quantity,
                                LotItemId = repitem.Id
                            };

                            if (item.Category == Enums.DSItemCategory.DsrEvent || item.Category == Enums.DSItemCategory.Trap)
                            {
                                Log.Logger.Verbose($"Item at loc {locId} detected as {item.Name} in category {item.Category} - replaced with AP item.");
                                var newspecialitemlot = new ItemLot
                                {
                                    Rarity = 1,
                                    GetItemFlagId = -1,
                                    CumulateNumFlagId = -1,
                                    CumulateNumMax = 0,
                                    Items = []
                                };

                                if (!specialResult.TryAdd(lot.Id, newspecialitemlot))
                                    addonitems++;
                                specialResult[lot.Id].Items.Add(newLotItem);

                                // replace with the AP item with the right fogwall key/trap
                                newLotItem.LotItemCategory = (int)Enums.DSItemCategory.KeyItems;
                            }
                        }
                        else
                        {
                            Log.Logger.Warning($"Item {i} not found for loc {locId} lotnull {lot == null}, {target} itemnull {item == null}");
                            App.Client.AddOverlayMessage($"Item {i} not found for loc {locId} lotnull {lot == null}, {target} itemnull {item == null}");
                            Log.Logger.Warning($"Item at loc {locId} replaced with prism stone instead.");
                            App.Client.AddOverlayMessage($"Item at loc {locId} replaced with prism stone instead.");
                            newLotItem = prismStoneLotItem;
                        }
                    }
                    else /* item not in own game, put the relevant id item instead */
                    {
                        Log.Logger.Verbose($"Item {i}/{locId} for target = {target}");
                        newLotItem = new ItemLotItem
                        {
                            CumulateLotPoint = 0,
                            CumulateReset = false,
                            EnableLuck = false,
                            GetItemFlagId = -1,
                            LotItemBasePoint = 100,
                            LotItemCategory = (int)Enums.DSItemCategory.KeyItems,
                            LotItemNum = 1,
                            LotItemId = locId
                        };
                    }
                    /* add the found location->item to the replacement dictionary */
                    var newitemlot = new ItemLot
                    {
                        Rarity = 1,
                        GetItemFlagId = -1,
                        CumulateNumFlagId = -1,
                        CumulateNumMax = 0,
                        Items = []
                    };
                    if (!result.TryAdd(lot.Flag, newitemlot))
                        addonitems++;
                    result[lot.Flag].Items.Add(newLotItem);
                }
            }
            Log.Logger.Debug($"replacement dict size = {result.Count}");
            Log.Logger.Debug($" {addonitems} addonitems");


            /* Populate frampt chest with rubbish */
            const int frampt_base = 50004000;
            /* Iterate over each pair of entries in the pair of lists */
            for (i = 0; i <= 69; i++)
            {
                /* Skip estus flask + upgrades */
                if (i >= 38 && i <= 45)
                    continue;

                int lotflag = frampt_base + i;
                /* lot with only rubbish */
                var newitemlot = new ItemLot
                {
                    Rarity = 1,
                    GetItemFlagId = -1,
                    CumulateNumFlagId = -1,
                    CumulateNumMax = 0,
                    Items = [rubbishLotItem]
                };
                result.Add(lotflag, newitemlot);
            }

            resultMap = result;
            return;
        }
        public static List<ItemLot> GetItemLots()
        {
            List<ItemLot> itemLots = new List<ItemLot>();

            var startAddress = AddressHelper.GetItemLotParamOffset();

            var dataOffset = Memory.ReadUInt(startAddress + 0x4);
            var rowCount = Memory.ReadUShort(startAddress + 0xA);
            var rowSize = 148;

            var paramTableBytes = Memory.ReadByteArray(startAddress + (ulong)(12 * rowCount), 0x30);

            for (int i = 0; i < rowCount; i++)
            {
                var tableOffset = i * 12;

                var currentAddress = startAddress + dataOffset + (ulong)(i * rowSize);
                var itemLot = ReadItemLot(currentAddress);
                itemLots.Add(itemLot);
            }
            return itemLots;
        }
        public static ItemLot ReadItemLot(ulong startAddress)
        {
            ItemLot lot = new ItemLot();
            lot.Items = new List<ItemLotItem>();
            var currentAddress = startAddress;
            var extraField = Memory.ReadByteArray(startAddress + 0x92, 2);
            var bitArray = new BitArray(extraField);
            for (int i = 0; i < 8; i++)
            {
                ItemLotItem item = new ItemLotItem
                {
                    LotItemId = Memory.ReadInt(startAddress + (ulong)(i * 4)),
                    LotItemCategory = Memory.ReadInt(startAddress + 0x20 + (ulong)(i * 4)),
                    LotItemBasePoint = Memory.ReadUShort(startAddress + 0x40 + (ulong)(i * 2)),
                    CumulateLotPoint = Memory.ReadUShort(startAddress + 0x50 + (ulong)(i * 2)),
                    GetItemFlagId = Memory.ReadInt(startAddress + 0x60 + (ulong)(i * 4)),
                    LotItemNum = Memory.ReadByte(startAddress + 0x8A + (ulong)i),
                    EnableLuck = bitArray.Get(i),
                    CumulateReset = bitArray.Get(i + 8)
                };
                lot.Items.Add(item);
            }
            lot.GetItemFlagId = Memory.ReadInt(startAddress + 0x80);
            lot.CumulateNumFlagId = Memory.ReadInt(startAddress + 0x84);
            lot.CumulateNumMax = Memory.ReadByte(startAddress + 0x88);
            lot.Rarity = Memory.ReadByte(startAddress + 0x89);
            return lot;
        }
        /// <summary>
        ///  Overwrite all of the item lots at once. 
        /// </summary>
        /// <param name="itemLotIds">A map of location ids to item Lots that replace them</param>
        public static void OverwriteItemLots(Dictionary<int, ItemLot> itemLotIds)
        {
            var startAddress = AddressHelper.GetItemLotParamOffset();
            var dataOffset = Memory.ReadUInt(startAddress + 0x4);
            var rowCount = Memory.ReadUShort(startAddress + 0xA);
            var foundItems = 0;
            const int rowSize = 148; // Size of each ItemLotParam
            Log.Logger.Debug($"ItemParam list rowcount='{rowCount}'");
            var tasks = new List<Task>();

            /* Reset the "number of itemlots placed" per id */
            foreach (var pair in itemLotIds)
            {
                pair.Value.numPlaced = 0;
            }

            for (int i = 0; i < rowCount; i++)
            {
                var currentAddress = startAddress + dataOffset + (ulong)(i * rowSize);
                var currentItemLotId = Memory.ReadInt(currentAddress + 0x80);  // GetItemFlagId is at offset 0x80

                /* Only if we are using Verbose logging, read in every ItemLotParam to print it out. */
                if (Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Verbose))
                {
                    var itemlotparams = Memory.ReadObject<ItemLotParam>(currentAddress);
                    Log.Logger.Verbose($"ilp '{i}'=" + itemlotparams.ToString());
                }

                ItemLot newItemLot;
                if (itemLotIds.TryGetValue(currentItemLotId, out newItemLot))
                {

                    foundItems++;
                    // We found the correct item lot or are using the default, now let's overwrite it

                    /* Check if we still have items to replace this location with */
                    short replaceidx = newItemLot.numPlaced;
                    newItemLot.numPlaced++;
                    Log.Logger.Verbose($"Incremented lot id numplaced id={currentItemLotId}, curr = {itemLotIds[currentItemLotId].numPlaced}");

                    if (newItemLot.numPlaced > newItemLot.Items.Count)
                    {
                        if (currentItemLotId == (int)Enums.SpecialItemLotIds.KeyToTheSeal
                         || currentItemLotId == (int)Enums.SpecialItemLotIds.WhiteSignSoapstone)
                        {
                            Log.Logger.Debug($"Special lot detected, sending to additional locations for lot id={currentItemLotId}");
                            replaceidx = 0;
                        }
                        else
                        {
                            Log.Logger.Warning($"More items detected than are placable, for lot id={currentItemLotId}");
                            App.Client.AddOverlayMessage($"More items detected than are placable, for lot id={currentItemLotId}");
                            continue; /* don't place anything there */
                        }
                    }


                    /* Parallelize the writing of memory to many tasks for speed-up. */
                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {

                            for (int j = 0; j < 8; j++)
                            {
                                OverwriteSingleItem(currentAddress, newItemLot.Items[replaceidx], j);
                                //RemoveSingleItem(currentAddress, j);
                            }

                            //   Memory.Write(currentAddress + 0x80, newItemLot.GetItemFlagId);
                            Memory.Write(currentAddress + 0x84, newItemLot.CumulateNumFlagId);
                            Memory.WriteByte(currentAddress + 0x88, newItemLot.CumulateNumMax);
                            Memory.WriteByte(currentAddress + 0x89, newItemLot.Rarity);

                            // Write EnableLuck and CumulateReset as a single ushort
                            ushort bitfield = 0;
                            for (int j = 0; j < 8; j++)
                            {
                                if (j < newItemLot.Items.Count)
                                {
                                    if (newItemLot.Items[j].EnableLuck)
                                        bitfield |= (ushort)(1 << j);
                                    if (newItemLot.Items[j].CumulateReset)
                                        bitfield |= (ushort)(1 << (j + 8));
                                }
                                // If item doesn't exist, its bits remain 0
                            }
                            Memory.Write(currentAddress + 0x92, bitfield);

                        }
                        catch (Exception e)
                        {
                            Log.Logger.Warning($"Overwrite Exception:{e.Message}, {replaceidx} lc {newItemLot.Items.Count}");
                            App.Client.AddOverlayMessage($"Overwrite Exception:{e.Message}, {replaceidx} lc {newItemLot.Items.Count}");
                        }
                    }));

                    Log.Logger.Verbose($"i='{i}' ItemLot with GetItemFlagId {currentItemLotId} has been overwritten at {currentAddress} to give {newItemLot.Items[replaceidx].LotItemId} in {newItemLot.Items[replaceidx].LotItemCategory}.");
                }
                else
                {
                    Log.Logger.Verbose($"i='{i}' ItemLot with GetItemFlagId {currentItemLotId} not overwritten.");
                }

            }
            int discrepancy_warnings = 0;
            foreach (var pair in itemLotIds)
            {
                var lot = pair.Value;
                if (lot.Items.Count != lot.numPlaced)
                {
                    if (pair.Key != (int)Enums.SpecialItemLotIds.KeyToTheSeal
                     && pair.Key != (int)Enums.SpecialItemLotIds.WhiteSignSoapstone)
                    {
                        Log.Logger.Warning($"Discrepancy: {lot.Items.Count} items in item lot {pair.Key}, but {lot.numPlaced} items placed.");
                        App.Client.AddOverlayMessage($"Discrepancy: {lot.Items.Count} items in item lot {pair.Key}, but {lot.numPlaced} items placed.");
                        discrepancy_warnings++;
                    }
                }
                if (discrepancy_warnings > 20)
                {
                    Log.Logger.Error($"More than 20 discrepancies detected.");
                    break;
                }
            }

            Task.WaitAll(tasks.ToArray());

            Log.Logger.Information($"{foundItems} items overwritten");
            App.Client.AddOverlayMessage($"{foundItems} items overwritten");

            if (foundItems == 0)
            {
                Log.Logger.Error($"Failed to overwrite items. Retry: restart game & client and reconnect");
                App.Client.AddOverlayMessage($"Failed to overwrite items. Retry: restart game & client and reconnect");
            }
        }
        public static void OverwriteSingleItem(ulong address, ItemLotItem newItemLot, int position)
        {

            //uint id =       Memory.ReadUInt(address + (ulong)(position * 4));
            //uint category = Memory.ReadUInt(address + 0x20 + (ulong)(position * 4));
            //uint itemnum = Memory.ReadUInt(address + 0x8A + (ulong)position);
            //uint itemflagid = Memory.ReadUInt(address + 0x60 + (ulong)(position * 4));
            //Log.Logger.Information($"id {id} cat {category} itemnum {itemnum} itemflag {itemflagid} overwritten");

            Memory.Write(address + (ulong)(position * 4), newItemLot.LotItemId);
            Memory.Write(address + 0x20 + (ulong)(position * 4), newItemLot.LotItemCategory);
            Memory.Write(address + 0x40 + (ulong)(position * 2), (ushort)newItemLot.LotItemBasePoint);
            Memory.Write(address + 0x50 + (ulong)(position * 2), (ushort)newItemLot.CumulateLotPoint);
            //Memory.Write(address + 0x60 + (ulong)(position * 4), newItemLot.Items[j].GetItemFlagId);
            Memory.WriteByte(address + 0x8A + (ulong)position, newItemLot.LotItemNum);
        }
        public static void RemoveSingleItem(ulong address, int position)
        {
            Memory.Write(address + (ulong)(position * 4), 0);
            Memory.Write(address + 0x20 + (ulong)(position * 4), 0);
            Memory.Write(address + 0x40 + (ulong)(position * 2), (ushort)0);
            Memory.Write(address + 0x50 + (ulong)(position * 2), (ushort)0);
            //Memory.Write(address + 0x60 + (ulong)(position * 4), newItemLot.Items[j].GetItemFlagId);
            Memory.WriteByte(address + 0x8A + (ulong)position, (byte)0);
        }
        public static DarkSoulsItem CreateItemFromLot(ItemLotItem lot)
        {
            var allItems = MiscHelper.GetAllItems();
            var item = allItems.FirstOrDefault(x => x.Id == lot.LotItemId);
            return item;
        }
        public static void ListItemLots()
        {
            var startAddress = AddressHelper.GetItemLotParamOffset();
            var dataOffset = Memory.ReadUInt(startAddress + 0x4);
            var rowCount = Memory.ReadUShort(startAddress + 0xA);
            var foundItems = 0;
            const int rowSize = 148; // Size of each ItemLotParam
            Log.Logger.Information($"ItemParam list rowcount='{rowCount}'");

            for (int i = 0; i < rowCount; i++)
            {
                var currentAddress = startAddress + dataOffset + (ulong)(i * rowSize);
                var currentItemLotId = Memory.ReadInt(currentAddress + 0x80);  // GetItemFlagId is at offset 0x80

                var itemlotparams = Memory.ReadObject<ItemLotParam>(currentAddress);
                Log.Logger.Information($"ilp '{i}'=" + itemlotparams.ToString(App.AllItems));

            }
        }
        public static void SetItemLot()
        {
            var startAddress = AddressHelper.GetItemLotParamOffset();

            var dataOffset = Memory.ReadUInt(startAddress + 0x4);
            var rowCount = Memory.ReadUShort(startAddress + 0xA);
            var rowSize = 148;

            var paramTableBytes = Memory.ReadByteArray(startAddress + (ulong)(12 * rowCount), 0x30);
            var itemlotflag = LocationHelper.GetItemLotFlags().Find(x => x.Name.ToLower().Contains("well"));

            ItemLotItem experimentalLotItem = new ItemLotItem
            {
                CumulateLotPoint = 0,
                CumulateReset = false,
                EnableLuck = false,
                GetItemFlagId = -1,
                LotItemBasePoint = 100,
                LotItemCategory = (int)DSAP.Enums.DSItemCategory.Consumables,
                LotItemNum = 1,
                LotItemId = 9015
            };

            for (int i = 0; i < rowCount; i++)
            {
                var tableOffset = i * 12;

                var currentAddress = startAddress + dataOffset + (ulong)(i * rowSize);
                var itemLot = ItemLotHelper.ReadItemLot(currentAddress);
                if (itemLot.GetItemFlagId == itemlotflag.Flag)
                {
                    ItemLotHelper.OverwriteSingleItem(currentAddress, experimentalLotItem, 0);
                }
            }
            Location loc = (Location)LocationHelper.GetItemLotLocations().Find(x => x.Name.ToLower().Contains("well"));
            LocationHelper.SetEventFlag(itemlotflag.Flag, 0);
            return;
        }
        public static List<(int id, int baseid, int itemid, int quantity)> loadout_itemlots = [];
        /// <summary>
        /// Update start character loadouts, and populate the "loadout specific item lots" (e.g. UA starter drops)
        /// </summary>
        /// <returns></returns>
        internal static bool UpdateCharaInits()
        {
            // Read in Chara Init params
            bool reloadRequired = ParamHelper.ReadFromBytes(out ParamStruct<CharaInitParam> paramStruct,
                                                     CharaInitParam.spOffset,
                                                     (ps) => ps.ParamEntries.Last().id >= 99999990);
            if (!reloadRequired)
            {
                Log.Logger.Debug("Skipping reload of Chara Inits");
                return false;
            }
            // Read in system text FMGs
            bool reload2Required = MsgManHelper.ReadFromBytes(out MsgManStruct msgManStruct,
                                                     0x3e0,
                                                     (ps) => ps.MsgEntries.Last().id >= 99999990);
            if (!reload2Required)
            {
                Log.Logger.Warning("Warning: Could not reload gift item names.");
                //return false;
            }

            //print all fmgs in 0x3e0 for research
            foreach (var msgentry in msgManStruct.MsgEntries)
            {
                if (msgentry.stringOffset >= 0)
                {
                    int maxbytes = msgManStruct.StringBytes.Length - msgentry.stringOffset;
                    int readbytes = Math.Min(500, maxbytes);
                    string s = Encoding.Unicode.GetString(msgManStruct.StringBytes, msgentry.stringOffset, readbytes);
                    if (s.Split("\0").Length > 1)
                        s = s.Split("\0")[0];
                    Log.Logger.Warning($"Message id={msgentry.id}={s}");
                }
            }

            // if we are here, we are updating the params.

            loadout_itemlots = new List<(int id, int baseid, int itemid, int quantity)>(); // initialize blank list
            var loadouts = MiscHelper.GetLoadouts();
            var melee_weapons = MiscHelper.GetMeleeWeapons().Where(x => !x.Name.Contains("Straight Sword Hilt") && !x.Name.Contains("Broken Straight Sword")).ToList();
            var ranged_weapons = MiscHelper.GetRangedWeapons().Where(x => !x.Name.Contains("Arrow") && !x.Name.Contains("Bolt")).ToList();
            var spell_tools = MiscHelper.GetSpellTools();
            var shields = MiscHelper.GetShields();

            var armors = MiscHelper.GetArmor();
            var head_armor = armors.Where(x => ((x.Id / 1000) % 10) == 0).ToList(); // 0 in 1000s place = head armor
            var body_armor = armors.Where(x => ((x.Id / 1000) % 10) == 1).ToList(); // 0 in 1000s place = body armor
            var arms_armor = armors.Where(x => ((x.Id / 1000) % 10) == 2).ToList(); // 0 in 1000s place = arms armor
            var legs_armor = armors.Where(x => ((x.Id / 1000) % 10) == 3).ToList(); // 0 in 1000s place = legs armor


            var gifts = MiscHelper.GetGiftParams();

            Random random = new Random(MiscHelper.HashSeed(App.Client.CurrentSession.RoomState.Seed) + App.Client.CurrentSession.ConnectionInfo.Slot);

            // modify our relevant entries
            foreach (var loadout in loadouts)
            {
                var charaInit_display = paramStruct.ParamEntries.Find(x => x.id == loadout.Id);
                var charaInit_ingame = paramStruct.ParamEntries.Find(x => x.id == loadout.Id - 1000);

                // get current param bytes
                byte[] display_parambytes = new byte[CharaInitParam.Size];
                Array.Copy(paramStruct.ParamBytes, charaInit_display.paramOffset, display_parambytes, 0, CharaInitParam.Size);
                byte[] ingame_parambytes = new byte[CharaInitParam.Size];
                Array.Copy(paramStruct.ParamBytes, charaInit_ingame.paramOffset, ingame_parambytes, 0, CharaInitParam.Size);

                var allowed_melee_weapons = melee_weapons;
                var allowed_shields = shields;
                var allowed_spell_tools = spell_tools;
                if (!App.DSOptions.NoWeaponRequirements)
                {
                    // melee weapons
                    allowed_melee_weapons = melee_weapons.Where(x =>
                    {
                        return (loadout.Strength * 3 >= x.Strength * 2) // use 2h strength (2 handing gives 50% str bonus)
                            && (loadout.Dexterity >= x.Dexterity)
                            && (loadout.Intelligence >= x.Intelligence)
                            && (loadout.Faith >= x.Faith);
                    }).ToList(); // 2-handable weapons

                    if (App.DSOptions.RequireOneHandedStartingWeapons) // limit it further
                    {
                        allowed_melee_weapons = allowed_melee_weapons.Where(x =>
                        {
                            return loadout.Strength >= x.Strength; // limit to 1h strength
                        }).ToList();
                    }
                    // shields
                    allowed_shields = allowed_shields.Where(x =>
                    {
                        return (loadout.Strength >= x.Strength)
                            && (loadout.Dexterity >= x.Dexterity)
                            && (loadout.Intelligence >= x.Intelligence)
                            && (loadout.Faith >= x.Faith);
                    }).ToList(); // shield requires the strength
                    // spell tools
                    if (loadout.Type == Enums.DsrLoadoutType.Magic || loadout.Type == Enums.DsrLoadoutType.Miracle) // only bother for faith/int casters
                    {
                        allowed_spell_tools = spell_tools.Where(x =>
                        {
                            return (loadout.Strength * 3 >= x.Strength * 2) // use 2h strength (2 handing gives 50% str bonus)
                                && (loadout.Dexterity >= x.Dexterity)
                                && (loadout.Intelligence >= x.Intelligence)
                                && (loadout.Faith >= x.Faith);
                        }).ToList(); // 2-handable weapons

                        if (App.DSOptions.RequireOneHandedStartingWeapons) // limit it further
                        {
                            allowed_spell_tools = allowed_spell_tools.Where(x =>
                            {
                                return loadout.Strength >= x.Strength; // limit to 1h strength
                            }).ToList();
                        }
                    }
                }



                DarkSoulsItem? weapon = null;
                DarkSoulsItem? shield = null;
                DarkSoulsItem? sub_weapon= null;
                DarkSoulsItem? sub_shield = null;
                DarkSoulsItem? spell = null;
                DarkSoulsItem? thief_item = null;
                byte thief_item_quantity = 1;
                DarkSoulsItem? ammo = null;
                int ammotype = 1; // assume arrow
                int ammo_quantity = 99; // 99 for now. Make this an option


                var itemLots = new List<(int lotid, int item, int quantity)>(); // item lot updates for this loadout
                var desc_line_1 = "";
                var desc_line_2 = "";
                var desc_line_3 = "";
                var special_line = "";

                // Randomize spells for caster classes
                switch (loadout.Type)
                {   
                    case Enums.DsrLoadoutType.Magic:
                        var valid_sorceries = MiscHelper.GetSpells().Where(x => x.Name.StartsWith("Sorcery:")).ToList();
                        var allowed_sorceries = valid_sorceries;
                        if (!App.DSOptions.NoSpellStatRequirements) // if we have stat requirements, limit our list of spells
                            allowed_sorceries = valid_sorceries.Where(x => loadout.Intelligence > x.Intelligence).ToList();
                        // Then limit our spell list by user choice
                        if (App.DSOptions.StartingSorcery == 0) // default - soul arrow
                        {
                            allowed_sorceries = allowed_sorceries.Where(x => x.Name == "Sorcery: Soul Arrow").ToList();
                        }
                        if (App.DSOptions.StartingSorcery == 1) // any spell - we already have this list
                        {
                            ;
                        }
                        if (App.DSOptions.StartingSorcery == 2) // attack
                        {
                            allowed_sorceries = allowed_sorceries.Where(x => x.SpellCategory == Enums.SpellCategory.Attack).ToList();
                        }
                        spell = allowed_sorceries[random.Next(allowed_sorceries.Count)];
                        break;
                    case Enums.DsrLoadoutType.Miracle:
                        var valid_miracles = MiscHelper.GetSpells().Where(x => x.Name.StartsWith("Miracle:")).ToList();
                        var allowed_miracles = valid_miracles;
                        if (!App.DSOptions.NoSpellStatRequirements) // if we have stat requirements, limit our list of spells
                            allowed_miracles = valid_miracles.Where(x => loadout.Faith > x.Faith).ToList();
                        
                        if (!App.DSOptions.NoMiracleCovenantRequirements) // if we have covenant requirements, limit list of spells further
                        {
                            List<string> covenantMiracles = new List<string>(["Bountiful Sunlight", "Darkmoon Blade", "Soothing Sunlight", "Sunlight Spear"]);
                            allowed_miracles = allowed_miracles.Where(x => !covenantMiracles.Contains($"Miracle: {x.Name}")).ToList();
                        }
                        // Then limit our spell list by user choice
                        if (App.DSOptions.StartingMiracle == 0) // default - heal
                        {
                            allowed_miracles = allowed_miracles.Where(x => x.Name == "Miracle: Heal").ToList();
                        }
                        if (App.DSOptions.StartingMiracle == 1) // any miracle - we already have this list
                        {
                            ;
                        }
                        if (App.DSOptions.StartingMiracle == 3) // healing
                        {
                            allowed_miracles = allowed_miracles.Where(x => x.SpellCategory == Enums.SpellCategory.Heal).ToList();
                        }
                        spell = allowed_miracles[random.Next(allowed_miracles.Count)];
                        break;
                    case Enums.DsrLoadoutType.Pyromancy:
                        var valid_pyromancies = MiscHelper.GetSpells().Where(x => x.Name.StartsWith("Pyromancy:")).ToList();
                        // Then limit our spell list by user choice
                        if (App.DSOptions.StartingPyromancy == 0) // default - heal
                        {
                            valid_pyromancies = valid_pyromancies.Where(x => x.Name == "Pyromancy: Fireball").ToList();
                        }
                        if (App.DSOptions.StartingPyromancy == 1) // any miracle - we already have this list
                        {
                            ;
                        }
                        if (App.DSOptions.StartingPyromancy == 2) // Attack
                        {
                            valid_pyromancies = valid_pyromancies.Where(x => x.SpellCategory == Enums.SpellCategory.Attack).ToList();
                        }
                        spell = valid_pyromancies[random.Next(valid_pyromancies.Count)];
                        break;
                    default:
                        break;
                }

                if (spell != null)
                {
                    // no need to add spells to item lots - add directly to chara inits only
                    Array.Copy(BitConverter.GetBytes(spell.Id), 0, display_parambytes, CharaInitParam.SPELL_01, sizeof(int));
                    Array.Copy(BitConverter.GetBytes(spell.Id), 0, display_parambytes, CharaInitParam.ITEM_01, sizeof(int));
                    Array.Copy(BitConverter.GetBytes(spell.Id), 0, ingame_parambytes, CharaInitParam.SPELL_01, sizeof(int));
                    Array.Copy(BitConverter.GetBytes(spell.Id), 0, ingame_parambytes, CharaInitParam.ITEM_01, sizeof(int));
                    // add ammo to class description
                    special_line = $"{spell.Name}";
                }


                if (App.DSOptions.RandomizeStartingLoadouts)
                {
                    // Randomize equipment
                    switch (loadout.Type)
                    {
                        case Enums.DsrLoadoutType.Melee:
                            if (App.DSOptions.ExtraStartingWeaponForMeleeClasses)
                                sub_weapon = allowed_melee_weapons[random.Next(allowed_melee_weapons.Count)];
                                
                            if (loadout.Name == "Thief") // randomize thief item
                            {
                                var thief_items = new List<(byte quantity, String item_name)>([
                                    (99, "Throwing Knife"),
                                    (50, "Poison Throwing Knife"),
                                    (10, "Black Firebomb"),
                                    (20, "Firebomb"),
                                    (40, "Dung Pie"),
                                    (10, "Charcoal Pine Resin"),
                                    (10, "Gold Pine Resin"),
                                    (10, "Rotten Pine Resin"),
                                    (30, "Homeward Bone"),
                                    (20, "Green Blossom"),
                                    (10, "Elizabeth's Mushroom"),
                                    (15, "Blooming Purple Moss Clump")]);
                                var thief_item_entry = thief_items[random.Next(thief_items.Count)];
                                thief_item = MiscHelper.GetConsumables().Find(x => x.Name == thief_item_entry.item_name && x.Quantity == 1);
                                thief_item_quantity = thief_item_entry.quantity;
                                // give it in game and display (display doesn't work?)
                                Array.Copy(BitConverter.GetBytes(thief_item.Id), 0, display_parambytes, CharaInitParam.ITEM_01, sizeof(int));
                                Array.Copy(BitConverter.GetBytes(thief_item.Id), 0, ingame_parambytes, CharaInitParam.ITEM_01, sizeof(int));
                            }
                            break;
                        case Enums.DsrLoadoutType.Ranged:
                            var allowed_ranged_weapons = ranged_weapons;
                            if (!App.DSOptions.NoWeaponRequirements) // if there are weapon requirements, limit available weapons
                            {
                                allowed_ranged_weapons = ranged_weapons.Where(x =>
                                {
                                    return (loadout.Strength * 3 >= x.Strength * 2) // use 2h str - most ranged weapons require two hands anyway
                                        && (loadout.Dexterity >= x.Dexterity);
                                }).ToList();

                                if (App.DSOptions.RequireOneHandedStartingWeapons) // limit it further if required
                                {
                                    allowed_ranged_weapons = allowed_ranged_weapons.Where(x =>
                                    {
                                        return loadout.Strength >= x.Strength; // limit to 1h strength
                                    }).ToList();
                                }
                            }

                            sub_weapon = allowed_ranged_weapons[random.Next(allowed_ranged_weapons.Count)];
                            List<DarkSoulsItem> allowed_ammos = [];
                            
                            if (sub_weapon.Name.Contains(" Bow")) // non-crossbows, non-greatbows
                            {
                                allowed_ammos = MiscHelper.GetRangedWeapons().Where(x => x.Name.Contains("Arrow") && !x.Name.Contains("Dragonslayer") && !x.Name.Contains("Gough") && x.Quantity == 1).ToList();
                            }
                            else if (sub_weapon.Name.Contains("Greatbow"))
                            {
                                allowed_ammos = MiscHelper.GetRangedWeapons().Where(x => (x.Name.Contains("Dragonslayer") || x.Name.Contains("Gough")) && x.Quantity == 1).ToList();
                            }
                            else // crossbows
                            {
                                allowed_ammos = MiscHelper.GetRangedWeapons().Where(x => x.Name.Contains("Bolt") && x.Quantity == 1).ToList();
                                ammotype = 2; // bolt
                            }
                            ammo = allowed_ammos[random.Next(allowed_ammos.Count)];
                            break;
                        case Enums.DsrLoadoutType.Magic:
                            var catalysts = allowed_spell_tools.Where((x) => x.Name.Contains("Catalyst")).ToList();
                            sub_weapon = catalysts[random.Next(catalysts.Count)];
                            break;
                        case Enums.DsrLoadoutType.Miracle:
                            var talismans = allowed_spell_tools.Where((x) => x.Name.Contains("Talisman")).ToList();
                            sub_weapon = talismans[random.Next(talismans.Count)];
                            break;
                        case Enums.DsrLoadoutType.Pyromancy:
                            sub_weapon = spell_tools.Find(x => x.Name.Contains("Pyromancy Flame"));
                            break;
                        default:
                            break;
                    }
                    // add extra starting shields if option on
                    if (App.DSOptions.ExtraStartingShieldForAllClasses)
                        sub_shield = allowed_shields[random.Next(allowed_shields.Count)];

                    if (sub_weapon != null)
                    {
                        itemLots.Add((loadout.SubRightWeapon, sub_weapon.Id, 1)); // add to item lot
                        Array.Copy(BitConverter.GetBytes(sub_weapon.Id), 0, display_parambytes, CharaInitParam.SUBWEAPON_RIGHT, sizeof(int)); // add to char display
                    }
                    if (sub_shield != null)
                    {
                        itemLots.Add((loadout.SubLeftWeapon, sub_shield.Id, 1));
                        Array.Copy(BitConverter.GetBytes(sub_shield.Id), 0, display_parambytes, CharaInitParam.SUBWEAPON_LEFT, sizeof(int));
                    }

                    if (ammo != null)
                    {
                        itemLots.Add((loadout.Ammunition, ammo.Id, ammo_quantity)); // add ammunition to the character screen";
                                                                               // now add it to the item lot
                        if (ammotype == 1)
                            Array.Copy(BitConverter.GetBytes(ammo.Id), 0, display_parambytes, CharaInitParam.ARROW, sizeof(int));
                        else
                            Array.Copy(BitConverter.GetBytes(ammo.Id), 0, display_parambytes, CharaInitParam.BOLT, sizeof(int));
                        
                        // add ammo to class description
                        var ammo_item = App.AllItems.Find(x => x.Id == itemLots.Last().item);
                        special_line = $"{ammo_item.Name} x{ammo_quantity}";
                    }

                    if (thief_item != null)
                    {
                        ingame_parambytes[CharaInitParam.ITEMNUM_01] = thief_item_quantity;
                        Log.Logger.Debug($"Wrote to param {loadout.Id} at {CharaInitParam.ITEMNUM_01} value {thief_item_quantity}");
                        special_line = $"{thief_item.Name} x{thief_item_quantity}";
                    }
                    weapon = allowed_melee_weapons[random.Next(allowed_melee_weapons.Count)];
                    itemLots.Add((loadout.RightWeapon, weapon.Id, 1));
                    Array.Copy(BitConverter.GetBytes(weapon.Id), 0, display_parambytes, CharaInitParam.WEAPON_RIGHT, sizeof(int));

                    shield = allowed_shields[random.Next(allowed_shields.Count)];
                    itemLots.Add((loadout.LeftWeapon, shield.Id, 1));
                    Array.Copy(BitConverter.GetBytes(shield.Id), 0, display_parambytes, CharaInitParam.WEAPON_LEFT, sizeof(int));

                    //randomize armors
                    // randomize equipment
                    int head = BitConverter.ToInt32(display_parambytes, CharaInitParam.EQUIP_HEAD);
                    int body = BitConverter.ToInt32(display_parambytes, CharaInitParam.EQUIP_BODY);
                    int arms = BitConverter.ToInt32(display_parambytes, CharaInitParam.EQUIP_ARMS);
                    int legs = BitConverter.ToInt32(display_parambytes, CharaInitParam.EQUIP_LEGS);


                    if (head != 900000) // no head equipment
                    {
                        int new_head = head_armor[random.Next(head_armor.Count)].Id;
                        Array.Copy(BitConverter.GetBytes(new_head), 0, display_parambytes, CharaInitParam.EQUIP_HEAD, sizeof(int));
                        Array.Copy(BitConverter.GetBytes(new_head), 0, ingame_parambytes, CharaInitParam.EQUIP_HEAD, sizeof(int));
                    }
                    if (body != 901000) // no body equipment
                    {
                        int new_body = body_armor[random.Next(body_armor.Count)].Id;
                        Array.Copy(BitConverter.GetBytes(new_body), 0, display_parambytes, CharaInitParam.EQUIP_BODY, sizeof(int));
                        Array.Copy(BitConverter.GetBytes(new_body), 0, ingame_parambytes, CharaInitParam.EQUIP_BODY, sizeof(int));
                    }
                    if (arms != 902000) // no arms equipment
                    {
                        int new_arms = arms_armor[random.Next(arms_armor.Count)].Id;
                        Array.Copy(BitConverter.GetBytes(new_arms), 0, display_parambytes, CharaInitParam.EQUIP_ARMS, sizeof(int));
                        Array.Copy(BitConverter.GetBytes(new_arms), 0, ingame_parambytes, CharaInitParam.EQUIP_ARMS, sizeof(int));
                    }
                    if (legs != 903000) // no legs equipment
                    {
                        int new_legs = legs_armor[random.Next(legs_armor.Count)].Id;
                        Array.Copy(BitConverter.GetBytes(new_legs), 0, display_parambytes, CharaInitParam.EQUIP_LEGS, sizeof(int));
                        Array.Copy(BitConverter.GetBytes(new_legs), 0, ingame_parambytes, CharaInitParam.EQUIP_LEGS, sizeof(int));
                    }

                    // setup class description
                    var desc_strings = new List<string>([]);
                    if (weapon != null)
                        desc_strings.Add(weapon.Name);
                    if (sub_weapon != null)
                        desc_strings.Add(sub_weapon.Name);
                    if (shield != null)
                        desc_strings.Add(shield.Name);
                    if (sub_shield!= null)
                        desc_strings.Add(sub_shield.Name);
                    if (special_line != "")
                        desc_strings.Add(special_line);

                    if (desc_strings.Count == 5)
                    {
                        desc_line_1 = $"{desc_strings[0]}/{desc_strings[1]}";
                        desc_line_2 = $"{desc_strings[2]}/{desc_strings[3]}";
                        desc_line_3 = $"{desc_strings[4]}";
                    }
                    else if (desc_strings.Count == 4)
                    {
                        desc_line_1 = $"{desc_strings[0]}";
                        desc_line_2 = $"{desc_strings[1]}/{desc_strings[2]}";
                        desc_line_3 = $"{desc_strings[3]}";
                    }
                    else if (desc_strings.Count == 3)
                    {
                        desc_line_1 = $"{desc_strings[0]}";
                        desc_line_2 = $"{desc_strings[1]}";
                        desc_line_3 = $"{desc_strings[2]}";
                    }
                    else if (desc_strings.Count == 2)
                    {
                        desc_line_1 = $"{desc_strings[0]}";
                        desc_line_2 = $"{desc_strings[1]}";
                        desc_line_3 = "";
                    }
                    else
                        Log.Logger.Warning("Error building randomized starting loadout descriptions: unexpected count.");

                    // write class description
                    uint msgid = (uint)(loadout.Id + 132320 - 3000);
                    msgManStruct.UpdateMsg(msgid, $"{desc_line_1}\n{desc_line_2}\n{desc_line_3}");
                }
                else 
                {
                    if (special_line != "") // if only spells rando'd, set class description to them
                    {
                        desc_line_1 = special_line;

                        // write class description
                        uint msgid = (uint)(loadout.Id + 132320 - 3000);
                        msgManStruct.UpdateMsg(msgid, $"{desc_line_1}\n{desc_line_2}\n{desc_line_3}");
                    }
                    // else no rando done
                }

                // setup the "item lots" structure's info for all the items of this specific class
                var addeditems = 0;
                foreach (var item in itemLots)
                {
                    // if there isn't a spot for the item, give it with the "shield"
                    int lotid = item.lotid;
                    int baseid = item.lotid;
                    if (item.lotid == -1)
                    {
                        addeditems++;
                        baseid = loadout.LeftWeapon;
                        lotid = loadout.LeftWeapon + addeditems;
                    }
                    loadout_itemlots.Add((lotid, baseid, item.item, item.quantity));
                }

                // write array back to params
                Array.Copy(display_parambytes, 0, paramStruct.ParamBytes, charaInit_display.paramOffset, CharaInitParam.Size); // chara init for title screen
                Array.Copy(ingame_parambytes, 0, paramStruct.ParamBytes, charaInit_ingame.paramOffset, CharaInitParam.Size); // chara init for in-game (some data, e.g. spells)
                // write it to messages

            }

            if (App.DSOptions.RandomizeStartingGifts)
            {
                //var added_gifts = new List<KeyValuePair<long, string>>();
                var available_gifts = new List<(byte quantity, string gift_name, String item_name, String description)>([
                    // default items
                    (3, "Goddess's Blessing", "Divine Blessing", "(AP) Wow!\n3 divine blessings,\nwhat a steal!"),
                    (10, "Black Firebomb x10", "Black Firebomb", "(AP) A reliable favorite.\nInflict big ouchies."),
                    (1, "Twin Humanities", "Twin Humanities", "(AP) Two Humanities\nin one item."),
                    (1, "Binoculars", "Binoculars", "(AP) Enjoy the views!"),
                    (1, "Pendant", "Pendant", "(AP) Useless item."),
                    (1, "Tiny Being's Ring", "Tiny Being's Ring", "(AP) Ring belonging to\n just a little being,\n and it's its birthday."),
                    (1, "Old Witch's Ring", "Old Witch's Ring", "(AP) This ring grants\nyou slightly more HP."),
                    // added items
                    (99, "Throwing Knives x99", "Throwing Knife", "(AP) Throwing Knives."),
                    (50, "P.Throwing Knives x50", "Poison Throwing Knife", "(AP) Poison Throwing\nKnives."),
                    (20, "Firebomb x20", "Firebomb", "(AP) Firebombs.\nYou throw them,\nthey deal damage."),
                    (40, "Dung Pie x40", "Dung Pie", "(AP) Throw to inflict\nToxic on enemies."),
                    (10, "Charcoal P. Resin x10", "Charcoal Pine Resin", "(AP) Apply to weapon\nto deal fire damage."),
                    (10, "Gold P. Resin x10", "Gold Pine Resin", "(AP) Apply to weapon\nto deal lightning\ndamage."),
                    (10, "Rotten P. Resin x10", "Rotten Pine Resin", "(AP) Apply to weapon\nto apply poison buildup."),
                    (30, "Homeward Bone x30", "Homeward Bone", "(AP) Takes you home.\nSome consider this\nuseless, apparently."),
                    (20, "Green Blossom x20", "Green Blossom", "(AP) Grants increased\nstamina regeneration,\nfor a short time."),
                    (10, "Elizabeth's Mushroom x10", "Elizabeth's Mushroom", "(AP) Grants heatlh\nregeneration,for\na short time."),
                    (15, "B.P. Moss Clumps x15", "Blooming Purple Moss Clump", "(AP) Blooming Purple\nMoss Clumps - Cure\nmost ailments"),

                    (10, "Large Titanite Shard x10", "Large Titanite Shard", "(AP) Upgrade materials."),
                    (10, "Transient Curse x10", "Transient Curse", "(AP) Lets your\nweapon hit ghosts."),

                    (1, "Dragon Head Stone", "Dragon Head Stone", "(AP) Vow requirement\nremoval pending."),
                    (1, "Cloranthy Ring", "Cloranthy Ring", "(AP) Grants increased\nstamina regeneration"),
                    (1, "Wolf Ring", "Wolf Ring", "(AP) Increases Poise"),
                    (1, "Hornet Ring", "Hornet Ring", "(AP) Increased critical\ndamage."),
                    (1, "Hawk Ring", "Hawk Ring", "(AP) Increased bow range."),
                    (1, "Rusted Iron Ring", "Rusted Iron Ring", "(AP) Better movement\nthrough deep water\nand swamp."),
                    (1, "Sl. Dragoncrest Ring", "Slumbering Dragoncrest Ring", "(AP) Slumbering\nDragoncrest Ring\nSilences your footsteps.")
                ]);

                foreach (var gift in gifts)
                {
                    var charaInit_ingame = paramStruct.ParamEntries.Find(x => x.id == gift.Id);
                    // also update text

                    // get current param bytes
                    byte[] ingame_parambytes = new byte[CharaInitParam.Size];
                    Array.Copy(paramStruct.ParamBytes, charaInit_ingame.paramOffset, ingame_parambytes, 0, CharaInitParam.Size);

                    var gift_entry = available_gifts[random.Next(available_gifts.Count)];
                    Log.Logger.Warning($"Gift {gift.Name} => {gift_entry.item_name} x {gift_entry.quantity}");
                    available_gifts.Remove(gift_entry);
                    var gift_item = App.AllItems.Find(x => x.Name == gift_entry.item_name && x.Quantity == 1);
                    var item_id = gift_item.Id;
                    var item_cat = gift_item.Category;

                    // clear ring
                    Array.Copy(BitConverter.GetBytes(0), 0, ingame_parambytes, CharaInitParam.ACCESSORY_01, sizeof(int));
                    // clear item
                    Array.Copy(BitConverter.GetBytes(0), 0, ingame_parambytes, CharaInitParam.ITEM_01, sizeof(int));

                    if (gift_item.Category == Enums.DSItemCategory.Consumables)
                    {
                        // update the chara init item
                        Array.Copy(BitConverter.GetBytes(item_id), 0, ingame_parambytes, CharaInitParam.ITEM_01, sizeof(int));
                        ingame_parambytes[CharaInitParam.ITEMNUM_01] = gift_entry.quantity;
                    }
                    else if (gift_item.Category == Enums.DSItemCategory.Rings)
                    {
                        // update the chara init ring
                        Array.Copy(BitConverter.GetBytes(item_id), 0, ingame_parambytes, CharaInitParam.ACCESSORY_01, sizeof(int));
                    }
                    // write array back to params
                    Array.Copy(ingame_parambytes, 0, paramStruct.ParamBytes, charaInit_ingame.paramOffset, CharaInitParam.Size);

                    // write new gift name
                    uint msgid = (uint)(gift.Id + 132050 - 2400);
                    msgManStruct.UpdateMsg(msgid, gift_entry.gift_name);
                    // write gift description
                    uint descid = (uint)(gift.Id + 132350 - 2400);
                    msgManStruct.UpdateMsg(descid, gift_entry.description);
                    Log.Logger.Debug($"Added messages for gift {gift.Id} to replace {gift.Name} with {gift_entry.gift_name}");
                }
            }
            

            msgManStruct.UpdateMsg(401303, "New Game (AP)");
            msgManStruct.UpdateMsg(401311, $"DSAP {App.DSOptions?.VersionInfoString()}");


            msgManStruct.AddMsg(99999998, ""); // add dummy message to mark that we've been here
            msgManStruct.MsgEntries.Sort((x, y) => (x.id.CompareTo(y.id)));
            Log.Logger.Information($"Updated system text struct");
            MsgManHelper.WriteFromMsgManStruct(msgManStruct, 0x3e0); // write the gift names + system text updates

            byte[] parambytes = new byte[EquipParamWeapon.Size];
            // add a dummy item at 99999998 so that we can know we've been here.
            Array.Copy(BitConverter.GetBytes(-1), 0, parambytes, 0x80, sizeof(int)); // overwrite getitemflagid with -1, so it isn't used
            paramStruct.AddParam(99999998, parambytes, Encoding.ASCII.GetBytes("")); // mark that we've been here

            paramStruct.ParamEntries.Sort((x, y) => (x.id.CompareTo(y.id)));
            Log.Logger.Information($"Added 1 items to CharaInit struct and updated chars");

            ParamHelper.WriteFromParamSt(paramStruct, CharaInitParam.spOffset); // write the chara init params

            return true;
        }
        internal static bool AddInitItemLots()
        {
            // Read in the Param Structure
            // Modify it,
            // Then save it back
            bool reloadRequired = ParamHelper.ReadFromBytes(out ParamStruct<ItemLotParam> paramStruct,
                                                     ItemLotParam.spOffset,
                                                     (ps) => ps.ParamEntries.Last().id >= 99999990);
            if (!reloadRequired)
            {
                Log.Logger.Debug("Skipping reload of Item Lots");
                return false;
            }
            // if we are here, we are updating the params.

            var updlots = loadout_itemlots;
            byte[] parambytes = new byte[ItemLotParam.Size];
            int new_entries = 0;
            foreach (var newlot in updlots)
            {
                var foundlot = paramStruct.ParamEntries.Find((x) => x.id == newlot.baseid);
                if (foundlot.id != 0) // found it, so update it
                {
                    
                    if (newlot.id == foundlot.id) // based-on lot is the one to update
                    {
                        Log.Logger.Verbose($"updating item lot id {newlot.id}");
                        Array.Copy(BitConverter.GetBytes(newlot.itemid), 0, paramStruct.ParamBytes, foundlot.paramOffset + 0x0, sizeof(int)); // lotItemId01
                        paramStruct.ParamBytes[foundlot.paramOffset + 0x8a] = (byte) newlot.quantity; // lotItemNum01
                    }
                    else // add new entry based on the based-on lot
                    {
                        Log.Logger.Verbose($"Adding item lot id {newlot.id}, based on {newlot.baseid}");
                        new_entries++;
                        // copy in based on the based-on lot
                        Array.Copy(paramStruct.ParamBytes, foundlot.paramOffset, parambytes, 0, parambytes.Length);
                        Array.Copy(BitConverter.GetBytes(newlot.itemid), 0, parambytes, 0x0, sizeof(int)); // lotItemId01
                        parambytes[0x8a] = (byte)newlot.quantity; // lotItemNum01
                        paramStruct.AddParam((uint)newlot.id, parambytes, Encoding.ASCII.GetBytes("startitem"));
                    }
                }
            }

            // add a dummy item at 99999998 so that we can know we've been here.
            Array.Copy(BitConverter.GetBytes(-1), 0, parambytes, 0x80, sizeof(int)); // overwrite getitemflagid with -1, so it isn't used
            paramStruct.AddParam(99999998, parambytes, Encoding.ASCII.GetBytes("")); // mark that we've been here

            paramStruct.ParamEntries.Sort((x, y) => (x.id.CompareTo(y.id)));
            Log.Logger.Information($"Added {new_entries} items to ItemLotParams");

            ParamHelper.WriteFromParamSt(paramStruct, ItemLotParam.spOffset);

            return true;
        }
    }
}

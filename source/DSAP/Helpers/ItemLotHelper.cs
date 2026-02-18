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
    }
}

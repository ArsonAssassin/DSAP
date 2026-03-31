using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.Models;
using DSAP.Models;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
        /// Build a mapping of the location flag values in itemflags to the ItemLot that should come from there, for items in our own game.
        /// </summary>
        /// <details>
        /// This is used for replacing item lots in our own game.
        /// </details>
        /// <param name="resultMap">A dictionary mapping itemlot flags to "item lots".</param>
        /// <returns></returns>
        public static void BuildLotParamIdToLotMap(out Dictionary<int, ItemLot> resultMap,
            Dictionary<string, Tuple<int, string>> slotLocToItemUpgMap,
            Dictionary<long, ScoutedItemInfo> scoutedLocationInfo)
        {
            Dictionary<int, ItemLot> result = new Dictionary<int, ItemLot>();

            var itemflags = LocationHelper.GetItemLotFlags().Where((x) => x.IsEnabled).ToList();

            var addonitems = 0;

            int i = 0;
            foreach (var (k, v) in scoutedLocationInfo)
            {
                i++;
                int locId = ((int)k);
                string target = v.Player.Name;
                var lots = itemflags.Where(x => x.Id == locId);
                foreach (var lot in lots) /* found a location in our "item lots" */
                {
                    ItemLotItem newLotItem = new ItemLotItem { };
                    if (v.Player.Slot == App.Client.CurrentSession.ConnectionInfo.Slot) // it is us
                    {
                        /* Found an item of our own, located in our own game. 
                                 * Validate that it's in the itemflags we've been given, and find the matching item. */
                        if (App.AllItemsByApId.TryGetValue((int)v.ItemId, out var item))
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
                        else
                        {
                            Log.Logger.Error($"Item {i} not found for loc {locId} lotnull {lot == null}, {target} itemnull {item == null}");
                            App.Client.AddOverlayMessage($"Item {i} not found for loc {locId} lotnull {lot == null}, {target} itemnull {item == null}");
                            Log.Logger.Error($"Item at loc {locId} replaced with prism stone instead.");
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
                    if (!result.TryAdd(lot.ItemLotParamId, newitemlot))
                        addonitems++;
                    result[lot.ItemLotParamId].Items.Add(newLotItem);
                }
            }
            Log.Logger.Debug($"replacement dict size = {result.Count}");
            Log.Logger.Debug($" {addonitems} addonitems");


            /* Populate frampt chest with rubbish */
            const int frampt_base = 4000;
            /* Iterate over each pair of entries in the pair of lists */
            for (i = 0; i <= 76; i++)
            {
                /* for some reason these lots skip 9-ending flags */
                if (i % 10 == 9) 
                    continue;
                /* Skip estus flask + its upgrades */
                if (i >= 42 && i <= 50)
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
        public static void OverwriteItemLots(ParamStruct<ItemLotParam> paramStruct, Dictionary<int, ItemLot> itemLotIds)
        {
            int foundItems = 0;

            var tasks = new List<Task>();

            /* Reset the "number of itemlots placed" per id */
            foreach (var pair in itemLotIds)
            {
                pair.Value.numPlaced = 0;
            }
            foreach (var param_entry in paramStruct.ParamEntries)
            {
                var currentItemLotFlag = BitConverter.ToInt32(paramStruct.ParamBytes, (int)param_entry.paramOffset + 0x80);
                ItemLot newItemLot;
                //int olditem_id = BitConverter.ToInt32(paramStruct.ParamBytes, (int)param_entry.paramOffset + 0x0);
                //int olditem_cat = BitConverter.ToInt32(paramStruct.ParamBytes, (int)param_entry.paramOffset + 0x20);
                //var olditem = App.AllItems.Find(x => x.Id == olditem_id && (int)x.Category == olditem_cat);
                if (itemLotIds.TryGetValue((int)param_entry.id, out newItemLot))
                {
                    foundItems++;
                    // We found the correct item lot or are using the default, now let's overwrite it

                    /* Check if we still have items to replace this location with */
                    short replaceidx = newItemLot.numPlaced;
                    newItemLot.numPlaced++;
                    Log.Logger.Verbose($"Incremented lot id numplaced id={param_entry.id}, curr = {itemLotIds[(int)param_entry.id].numPlaced}");

                    if (newItemLot.numPlaced > newItemLot.Items.Count)
                    {
                        Log.Logger.Warning($"More items detected than are placable, for lot id={param_entry.id}");
                        App.Client.AddOverlayMessage($"More items detected than are placable, for lot id={param_entry.id}");
                        continue; /* don't place anything there */
                    }

                    try
                    {
                        WriteItemLot(paramStruct, param_entry, newItemLot, replaceidx);
                    }
                    catch (Exception e)
                    {
                        Log.Logger.Warning($"Overwrite Exception:{e.Message}, {replaceidx} lc {newItemLot.Items.Count}");
                        App.Client.AddOverlayMessage($"Overwrite Exception:{e.Message}, {replaceidx} lc {newItemLot.Items.Count}");
                    }

                    //Log.Logger.Verbose($"ItemLot id={param_entry.id} name={olditem?.Name} with GetItemFlagId {currentItemLotId} has been overwritten to give {newItemLot.Items[replaceidx].LotItemId} in {newItemLot.Items[replaceidx].LotItemCategory}.");
                    Log.Logger.Verbose($"ItemLot id={param_entry.id} with GetItemFlagId {currentItemLotFlag} has been overwritten to give {newItemLot.Items[replaceidx].LotItemId} in {newItemLot.Items[replaceidx].LotItemCategory}.");
                }
                else
                {
                    //Log.Logger.Verbose($"ItemLot id='{param_entry.id}' name={olditem?.Name} with GetItemFlagId {currentItemLotId} not overwritten.");
                    Log.Logger.Verbose($"ItemLot id='{param_entry.id}' with GetItemFlagId {currentItemLotFlag} not overwritten.");
                }
            }
            ///* Only if we are using Verbose logging, read in every ItemLotParam to print it out. */
            //if (Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Verbose))
            //{
            //    var itemlotparams = Memory.ReadObject<ItemLotParam>(currentAddress);
            //    Log.Logger.Verbose($"ilp '{i}'=" + itemlotparams.ToString());
            //}

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

            Log.Logger.Information($"{foundItems} items overwritten");
            App.Client.AddOverlayMessage($"{foundItems} items overwritten");

            if (foundItems == 0)
            {
                Log.Logger.Error($"Failed to overwrite items. Retry: restart game & client and reconnect");
                App.Client.AddOverlayMessage($"Failed to overwrite items. Retry: restart game & client and reconnect");
            }

        }

        private static void WriteItemLot(ParamStruct<ItemLotParam> paramStruct, (uint id, uint paramOffset, int strOffset) param_entry, ItemLot newItemLot, short replaceidx)
        {
            for (int j = 0; j < 8; j++)
            {
                OverwriteSingleItem(paramStruct, param_entry, newItemLot.Items[replaceidx], j);
            }
            
            //   Memory.Write(currentAddress + 0x80, newItemLot.GetItemFlagId);
            Array.Copy(BitConverter.GetBytes(newItemLot.CumulateNumFlagId), 0, paramStruct.ParamBytes, param_entry.paramOffset + 0x84, 4);
            paramStruct.ParamBytes[param_entry.paramOffset + 0x88] = newItemLot.CumulateNumMax;
            paramStruct.ParamBytes[param_entry.paramOffset + 0x89] = newItemLot.Rarity;

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
            Array.Copy(BitConverter.GetBytes(bitfield), 0, paramStruct.ParamBytes, param_entry.paramOffset + 0x92, sizeof(ushort));
        }
        public static void OverwriteSingleItem(ParamStruct<ItemLotParam> paramStruct, (uint id, uint paramOffset, int strOffset) param_entry, ItemLotItem newItemLot, int position)
        {
            Array.Copy(BitConverter.GetBytes(newItemLot.LotItemId), 0, paramStruct.ParamBytes, param_entry.paramOffset + 4 * position, sizeof(int));
            Array.Copy(BitConverter.GetBytes(newItemLot.LotItemCategory), 0, paramStruct.ParamBytes, param_entry.paramOffset + 0x20 + 4 * position, sizeof(int));
            Array.Copy(BitConverter.GetBytes((ushort)newItemLot.LotItemBasePoint), 0, paramStruct.ParamBytes, param_entry.paramOffset + 0x40 + 2 * position, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)newItemLot.CumulateLotPoint), 0, paramStruct.ParamBytes, param_entry.paramOffset + 0x50 + 2 * position, sizeof(ushort));
            paramStruct.ParamBytes[param_entry.paramOffset + 0x8A + position] = newItemLot.LotItemNum;
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
        /// Update start character loadouts and gifts, populate the "loadout specific item lots" (e.g. UA starter drops), and update descriptions.
        /// Since this updates system text for their descriptions, also update "New Game" text and add version info to top left of main menu in this function
        /// </summary>
        /// <returns></returns>
        internal static bool RandomizeStartingLoadouts()
        {
            // Read in Chara Init params
            bool reloadRequired = ParamHelper.ReadFromBytes(out ParamStruct<CharaInitParam> charaParamStruct,
                                                     CharaInitParam.spOffset,
                                                     (ps) => ps.ParamEntries.Last().id >= 99999990);
            if (!reloadRequired)
            {
                Log.Logger.Debug("Skipping reload of Chara Inits");
                return false;
            }
            // Read in system text FMGs
            bool reload2Required = MsgManHelper.ReadMsgManStruct(out MsgManStruct msgManStruct,
                                                     MsgManStruct.OFFSET_SYSTEM_TEXT,
                                                     (ps) => ps.MsgEntries.Last().id >= 99999990);
            if (!reload2Required)
            {
                Log.Logger.Warning("Warning: Could not reload gift item names.");
                //return false;
            }

            //msgManStruct.PrintAllFmgs();

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

            Random random = new Random(MiscHelper.HashSeed(App.Client.CurrentSession.RoomState.Seed) + App.Client.CurrentSession.ConnectionInfo.Slot);

            // For each class loadout slot, randomize it
            foreach (var loadout in loadouts)
            {
                var charaInit_display = charaParamStruct.ParamEntries.Find(x => x.id == loadout.Id);
                var charaInit_ingame = charaParamStruct.ParamEntries.Find(x => x.id == loadout.Id - 1000);

                // get current param bytes
                byte[] display_parambytes = new byte[CharaInitParam.Size];
                Array.Copy(charaParamStruct.ParamBytes, charaInit_display.paramOffset, display_parambytes, 0, CharaInitParam.Size);
                byte[] ingame_parambytes = new byte[CharaInitParam.Size];
                Array.Copy(charaParamStruct.ParamBytes, charaInit_ingame.paramOffset, ingame_parambytes, 0, CharaInitParam.Size);

                var allowed_melee_weapons = melee_weapons;
                var allowed_shields = shields;
                var allowed_spell_tools = spell_tools;

                allowed_melee_weapons = melee_weapons.Where(x => WeaponEquipable(loadout, x)).ToList();
                allowed_shields = shields.Where(x => ShieldEquipable(loadout, x)).ToList();
                allowed_spell_tools = spell_tools.Where(x => WeaponEquipable(loadout, x)).ToList();

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
                var special_line = "";

                // Get starter spell for the caster classes
                spell = GetStarterSpell(loadout, random);
                
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
                                var thief_items = MiscHelper.GetThiefItemsPool();
                                var thief_item_entry = thief_items[random.Next(thief_items.Count)];
                                thief_item = MiscHelper.GetConsumables().Find(x => x.Name == thief_item_entry.ItemName && x.Quantity == 1);
                                thief_item_quantity = thief_item_entry.Quantity;
                                // give it in game and display (display doesn't work?)
                                Array.Copy(BitConverter.GetBytes(thief_item.Id), 0, display_parambytes, CharaInitParam.ITEM_01, sizeof(int));
                                Array.Copy(BitConverter.GetBytes(thief_item.Id), 0, ingame_parambytes, CharaInitParam.ITEM_01, sizeof(int));
                            }
                            break;
                        case Enums.DsrLoadoutType.Ranged:
                            var allowed_ranged_weapons = ranged_weapons.Where(x => WeaponEquipable(loadout, x)).ToList();
                            
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
                        special_line = $"{ammo.Name} x{ammo_quantity}";
                    }

                    if (thief_item != null)
                    {
                        ingame_parambytes[CharaInitParam.ITEMNUM_01] = thief_item_quantity;
                        Log.Logger.Debug($"Wrote to param {loadout.Id} at {CharaInitParam.ITEMNUM_01} value {thief_item_quantity}");
                        special_line = $"{thief_item.Name} x{thief_item_quantity}";
                    }

                    // randomize "main" weapon
                    weapon = allowed_melee_weapons[random.Next(allowed_melee_weapons.Count)];
                    itemLots.Add((loadout.RightWeapon, weapon.Id, 1));
                    Array.Copy(BitConverter.GetBytes(weapon.Id), 0, display_parambytes, CharaInitParam.WEAPON_RIGHT, sizeof(int));

                    // randomize main shield
                    shield = allowed_shields[random.Next(allowed_shields.Count)];
                    itemLots.Add((loadout.LeftWeapon, shield.Id, 1));
                    Array.Copy(BitConverter.GetBytes(shield.Id), 0, display_parambytes, CharaInitParam.WEAPON_LEFT, sizeof(int));

                    //randomize armors/equipment
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
                    // done randomizing equipment

                    string desc_line = BuildClassDescString(weapon, sub_weapon, shield, sub_shield, special_line);

                    // write class description
                    uint msgid = (uint)(loadout.Id + 132320 - 3000);
                    msgManStruct.UpdateMsg(msgid, $"{desc_line}");
                }
                else 
                {
                    if (special_line != "") // if only spells rando'd, set class description to them
                    {
                        // write class description
                        uint msgid = (uint)(loadout.Id + 132320 - 3000);
                        msgManStruct.UpdateMsg(msgid, $"{special_line}");
                    }
                    // else no rando done, so no message update
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
                Array.Copy(display_parambytes, 0, charaParamStruct.ParamBytes, charaInit_display.paramOffset, CharaInitParam.Size); // chara init for title screen
                Array.Copy(ingame_parambytes, 0, charaParamStruct.ParamBytes, charaInit_ingame.paramOffset, CharaInitParam.Size); // chara init for in-game (some data, e.g. spells)
                // write it to messages

            } // end of foreach loadout loop

            if (App.DSOptions.RandomizeStartingGifts)
            {
                RandomizeGifts(charaParamStruct, msgManStruct, random);
            }

            // additional updates to interface
            msgManStruct.UpdateMsg(401303, "New Game (AP)");
            msgManStruct.UpdateMsg(401311, $"DSAP {App.DSOptions?.VersionInfoString()}");

            
            msgManStruct.AddMsg(99999998, "");  // add dummy message to mark that we've updated them
            msgManStruct.MsgEntries.Sort((x, y) => (x.id.CompareTo(y.id))); // sort for write
            MsgManHelper.WriteFromMsgManStruct(msgManStruct, MsgManStruct.OFFSET_SYSTEM_TEXT); // write the gift names + system text updates
            Log.Logger.Information($"Updated system text struct");

            // add a dummy chara init at 99999998 to know we've updated them
            byte[] parambytes = new byte[CharaInitParam.Size];
            charaParamStruct.AddParam(99999998, parambytes, Encoding.ASCII.GetBytes("")); // mark that we've been here

            charaParamStruct.ParamEntries.Sort((x, y) => (x.id.CompareTo(y.id)));
            Log.Logger.Information($"Added 1 items to CharaInit struct and updated chars");

            ParamHelper.WriteFromParamSt(charaParamStruct, CharaInitParam.spOffset); // write the chara init params

            return true;
        }

        private static bool ShieldEquipable(Loadout loadout, DarkSoulsItem x)
        {
            if (App.DSOptions.NoWeaponRequirements)
                return true;
            else
                return (loadout.Strength >= x.Strength) // use 1h strength
                && (loadout.Dexterity >= x.Dexterity)
                && (loadout.Intelligence >= x.Intelligence)
                && (loadout.Faith >= x.Faith);
        }

        private static bool WeaponEquipable(Loadout loadout, DarkSoulsItem x)
        {
            if (App.DSOptions.NoWeaponRequirements)
                return true;
            if (App.DSOptions.RequireOneHandedStartingWeapons)
                return (loadout.Strength >= x.Strength) // use 1h strength
                && (loadout.Dexterity >= x.Dexterity)
                && (loadout.Intelligence >= x.Intelligence)
                && (loadout.Faith >= x.Faith);
            else
                return (loadout.Strength * 3 >= x.Strength * 2) // use 2h strength (2 handing gives 50% str bonus)
                    && (loadout.Dexterity >= x.Dexterity)
                    && (loadout.Intelligence >= x.Intelligence)
                    && (loadout.Faith >= x.Faith);
        }

        private static string BuildClassDescString(DarkSoulsItem weapon, DarkSoulsItem? sub_weapon, DarkSoulsItem shield, DarkSoulsItem? sub_shield, string special_line)
        {
            var desc_line_1 = "";
            var desc_line_2 = "";
            var desc_line_3 = "";

            // setup class description
            var desc_strings = new List<string>([]);
            if (weapon != null)
                desc_strings.Add(weapon.Name);
            if (sub_weapon != null)
                desc_strings.Add(sub_weapon.Name);
            if (shield != null)
                desc_strings.Add(shield.Name);
            if (sub_shield != null)
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
            return $"{desc_line_1}\n{desc_line_2}\n{desc_line_3}";
        }

        // Given the read-in structs for chara init params and system/title menu msgs, update them to randomize gifts.
        // Assumes caller will write the structs back
        private static void RandomizeGifts(ParamStruct<CharaInitParam> charaParamStruct, MsgManStruct msgManStruct, Random random)
        {
            var gifts = MiscHelper.GetGiftParams();

            //var added_gifts = new List<KeyValuePair<long, string>>();
            var available_gifts = MiscHelper.GetGiftsPool();
            
            foreach (var gift in gifts)
            {
                var charaInit_ingame = charaParamStruct.ParamEntries.Find(x => x.id == gift.Id);
                // also update text

                // get current param bytes
                byte[] ingame_parambytes = new byte[CharaInitParam.Size];
                Array.Copy(charaParamStruct.ParamBytes, charaInit_ingame.paramOffset, ingame_parambytes, 0, CharaInitParam.Size);

                var gift_entry = available_gifts[random.Next(available_gifts.Count)];
                Log.Logger.Debug($"Gift {gift.Name} => {gift_entry.ItemName} x {gift_entry.Quantity}");
                available_gifts.Remove(gift_entry);
                var gift_item = App.AllItems.Find(x => x.Name == gift_entry.ItemName && x.Quantity == 1);
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
                    ingame_parambytes[CharaInitParam.ITEMNUM_01] = gift_entry.Quantity;
                }
                else if (gift_item.Category == Enums.DSItemCategory.Rings)
                {
                    // update the chara init ring
                    Array.Copy(BitConverter.GetBytes(item_id), 0, ingame_parambytes, CharaInitParam.ACCESSORY_01, sizeof(int));
                }
                // write array back to params
                Array.Copy(ingame_parambytes, 0, charaParamStruct.ParamBytes, charaInit_ingame.paramOffset, CharaInitParam.Size);

                // write new gift name
                uint msgid = (uint)(gift.Id + 132050 - 2400);
                msgManStruct.UpdateMsg(msgid, gift_entry.DisplayName);
                // write gift description
                uint descid = (uint)(gift.Id + 132350 - 2400);
                msgManStruct.UpdateMsg(descid, gift_entry.Description);
                Log.Logger.Debug($"Added messages for gift {gift.Id} to replace {gift.Name} with {gift_entry.DisplayName}");
            }
        }

        private static DarkSoulsItem? GetStarterSpell(Loadout loadout, Random random)
        {
            DarkSoulsItem spell = null;
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
            return spell;
        }

        internal static bool AddInitItemLots(ParamStruct<ItemLotParam> paramStruct, ref int new_entries)
        {
            var updlots = loadout_itemlots;
            byte[] parambytes = new byte[ItemLotParam.Size];
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
            paramStruct.FinalizeParams();
            return true;
        }
    }
}

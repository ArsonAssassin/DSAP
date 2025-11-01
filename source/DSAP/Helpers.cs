using Archipelago.Core.Models;
using Archipelago.Core.Util;
using DSAP.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Location = Archipelago.Core.Models.Location;
namespace DSAP
{
    public class Helpers
    {
        public static ulong GetBaseAddress()
        {
            var address = Memory.GetBaseAddress("DarkSoulsRemastered");
            if (address == 0)
            {
                Log.Debug("Could not find Base Address");
            }
            return (ulong)address;
        }
        public static ulong GetBaseAOffset()
        {
            var baseAddress = GetBaseAddress();
            byte[] pattern = { 0x8B, 0x76, 0x0C, 0x89, 0x35, 0x00, 0x00, 0x00, 0x00, 0x33, 0xC0 };
            string mask = "xxxxx????xx";
            IntPtr getBaseAAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

            int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getBaseAAddress + 3), 4), 0);
            IntPtr baseAAddress = getBaseAAddress + offset + 7;

            return (ulong)baseAAddress;
        }


        public static ulong GetFrpgNetManOffset()
        {
            try
            {
                var baseAddress = GetBaseAddress();
                Log.Debug($"Base address: 0x{baseAddress:X}");

                byte[] pattern = { 0x48, 0x83, 0x3d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8b, 0xf1 };
                string mask = "xxx????xxxx";

                IntPtr getfrpgNetManAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

                if (getfrpgNetManAddress == IntPtr.Zero)
                {
                    Log.Error("Failed to find the signature pattern for FrpgNetMan");
                    throw new Exception("Failed to find the signature pattern");
                }

                Log.Debug($"Found pattern at: 0x{getfrpgNetManAddress.ToInt64():X}");

                // Read the bytes at the pattern location to verify
                byte[] bytes = Memory.ReadByteArray((ulong)getfrpgNetManAddress, 11);
                Log.Debug($"Bytes at pattern: {BitConverter.ToString(bytes)}");

                // Read the 4-byte offset at position 3
                int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getfrpgNetManAddress + 3), 4), 0);
                Log.Debug($"Read offset value: 0x{offset:X}");

                // Try different pointer calculation methods
                IntPtr method1 = new IntPtr(getfrpgNetManAddress.ToInt64() + offset + 7);
                IntPtr method2 = new IntPtr(getfrpgNetManAddress.ToInt64() + 3 + 4 + offset);
                IntPtr method3 = new IntPtr(getfrpgNetManAddress.ToInt64() + offset + 8);

                ulong value1 = Memory.ReadULong((ulong)method1);
                ulong value2 = Memory.ReadULong((ulong)method2);
                ulong value3 = Memory.ReadULong((ulong)method3);

                Log.Debug($"Method 1 (offset+7): Address=0x{method1.ToInt64():X}, Value=0x{value1:X}");
                Log.Debug($"Method 2 (offset+3+4): Address=0x{method2.ToInt64():X}, Value=0x{value2:X}");
                Log.Debug($"Method 3 (offset+8): Address=0x{method3.ToInt64():X}, Value=0x{value3:X}");

                // Check if any method gives a likely valid pointer (usually in a specific memory range)
                if (value3 > 0x10000000 && value3 < 0x7FFFFFFFFFFF)
                {
                    Log.Debug("Using Method 3");
                    return value3;
                }

                // If we're here, all methods failed to produce a reasonable pointer
                Log.Error("Failed to resolve a valid FrpgNetMan pointer with any method");
                throw new Exception("Failed to get valid FrpgNetMan pointer");
            }
            catch (Exception ex)
            {
                Log.Error($"Error in GetFrpgNetManOffset: {ex.Message}");
                throw;
            }
        }

        public static ulong GetBaseBOffset()
        {
            var baseAddress = GetBaseAddress();
            byte[] pattern = { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x45, 0x33, 0xED, 0x48, 0x8B, 0xF1, 0x48, 0x85, 0xC0 };
            string mask = "xxx????xxxxxxxxx";
            IntPtr getBaseBAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

            if (getBaseBAddress == IntPtr.Zero)
            {
                throw new Exception("Failed to find the signature pattern");
            }

            int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getBaseBAddress + 3), 4), 0);

            IntPtr baseBAddress = new IntPtr(getBaseBAddress.ToInt64() + offset + 7);

            ulong pointerValue = Memory.ReadULong((ulong)baseBAddress);

            return pointerValue; 

        }
        //public static ulong GetBaseBOffset()
        //{
        //    var baseAddress = GetBaseAddress();
        //    byte[] pattern = { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x45, 0x33, 0xED, 0x48, 0x8B, 0xF1, 0x48, 0x85, 0xC0 };
        //    string mask = "xxx????xxxxxxxxx";
        //    IntPtr getBaseBAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

        //    int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getBaseBAddress + 3), 4), 0);
        //    IntPtr baseBAddress = (getBaseBAddress + 7)+ offset;

        //    return (ulong)baseBAddress;
        //}
        public static ulong GetBaseCOffset()
        {
            var baseAddress = GetBaseAddress();
            byte[] pattern = { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x28, 0x01, 0x66, 0x0F, 0x7F, 0x80, 0x00, 0x00, 0x00, 0x00, 0xC6, 0x80 };
            string mask = "xxx????xxxxxxx??xxxx";
            IntPtr getPFAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

            int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getPFAddress + 3), 4), 0);
            IntPtr progressionFlagsAddress = getPFAddress + offset + 7;

            return (ulong)progressionFlagsAddress;
        }
        public static ulong GetBaseXOffset()
        {
            var baseAddress = GetBaseAddress();
            byte[] pattern = { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x48, 0x39, 0x48, 0x68, 0x0f, 0x94, 0xc0, 0xc3 };
            string mask = "xxx????xxxxxxxx";
            IntPtr getBaseXAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

            uint offset = BitConverter.ToUInt32(Memory.ReadByteArray((ulong)(getBaseXAddress + 3), 4), 0);
            IntPtr baseXAddress = (nint)(getBaseXAddress + offset + 7);


            return (ulong)baseXAddress;
        }
        public static ulong GetChrBaseClassOffset()
        {
            var baseAddress = GetBaseAddress();       
            byte[] pattern = { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x45, 0x33, 0xED, 0x48, 0x8B, 0xF1, 0x48, 0x85, 0xC0 };
            string mask = "xxx????xxxxxxxxx";
            IntPtr getCBCAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

            int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getCBCAddress + 3), 4), 0);
            IntPtr chrBaseClassAddress = getCBCAddress + offset + 7;

            return (ulong)chrBaseClassAddress;
        }
        public static ulong GetProgressionFlagOffset()
        {
            var baseAddress = GetBaseAddress();
            byte[] pattern = { 0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x41, 0xB8, 0x01, 0x00, 0x00, 0x00, 0x44 };
            string mask = "xxx????xxxxxxx";
            IntPtr getPFAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

            int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getPFAddress + 3), 4), 0);
            IntPtr progressionFlagsAddress = getPFAddress + offset + 7;

            return (ulong)progressionFlagsAddress;
        }
        public static ulong GetSoloParamOffset()
        {
            var baseAddress = GetBaseAddress();
            byte[] pattern = { 0x4C, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x48, 0x63, 0xC9, 0x48, 0x8D, 0x04, 0xC9 };
            var mask = "xxx????xxxxxxx";
            IntPtr getSPAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

            int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getSPAddress + 3), 4), 0);
            IntPtr soloParamFlagsAddress = getSPAddress + offset + 7;
            return (ulong)soloParamFlagsAddress;
        }
        public static ulong GetEventFlagsOffset()
        {
            var baseAddress = GetBaseAddress();
            byte[] pattern = { 0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x99, 0x33, 0xC2, 0x45, 0x33, 0xC0, 0x2B, 0xC2, 0x8D, 0x50, 0xF6 };
            string mask = "xxx????xxxxxxxxxxx";
            IntPtr getEFAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

            int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getEFAddress + 3), 4), 0);
            IntPtr eventFlagsAddress = getEFAddress + offset + 7;

            return (ulong)(BitConverter.ToInt32(Memory.ReadFromPointer((ulong)eventFlagsAddress, 4, 2)));
        }
        internal static int GetPlayerHP()
        {
            return Memory.ReadInt(GetPlayerHPAddress());
        }
        internal static ulong GetPlayerHPAddress()
        {
            var baseB = GetBaseBOffset();
            var next = OffsetPointer(baseB, 0x10);
            var pointer = Memory.ReadULong(next);
            next = OffsetPointer(pointer, 0x14);
            return next;
        }
        private static ulong GetItemLotParamOffset()
        {
            var soloParams = GetSoloParamOffset();

            var foo = Memory.ReadULong(soloParams);
            var next = OffsetPointer(foo, 0x570);
            var foo2 = Memory.ReadULong(next);
            next = OffsetPointer(foo2, 0x38);
            var foo3 = Memory.ReadULong(next);
            return foo3;
        }
        private static ulong GetBonfireOffset()
        {
            var baseAddress = GetEventFlagsOffset();
            var baseBonfire = OffsetPointer(baseAddress, 0x5B);
            return baseBonfire;
        }
        public static List<ILocation> GetItemLotLocations()
        {
            List<ILocation> locations = new List<ILocation>();
            var lotFlags = GetItemLotFlags();
            var baseAddress = GetEventFlagsOffset();
            foreach (var lot in lotFlags)
            {
                locations.Add(new Location
                {
                    Name = lot.Name,
                    Address = baseAddress + GetEventFlagOffset(lot.Flag).Item1,
                    AddressBit = GetEventFlagOffset(lot.Flag).Item2,
                    Id = lot.Id,
                });
            }
            return locations;
        }
        public static List<ILocation> GetBossFlagLocations()
        {
            List<ILocation> locations = new List<ILocation>();
            var lotFlags = GetBossFlags();
            var baseAddress = GetEventFlagsOffset();
            foreach (var lot in lotFlags)
            {
                locations.Add(new Location
                {
                    Name = lot.Name,
                    Address = baseAddress + GetEventFlagOffset(lot.Flag).Item1,
                    AddressBit = GetEventFlagOffset(lot.Flag).Item2,
                    Id = lot.Id,
                });
            }
            return locations;
        }
        public static List<ILocation> GetBonfireFlagLocations()
        {
            List<ILocation> locations = new List<ILocation>();
            var lotFlags = GetBonfireFlags();
            var baseAddress = GetEventFlagsOffset();
            foreach (var lot in lotFlags)
            {
                locations.Add(new Location
                {
                    Name = lot.Name,
                    Id = lot.Id,
                    Address = lot.Flag,
                    AddressBit = lot.AddressBit
                });
            }
            return locations;
        }
        public static List<ILocation> GetDoorFlagLocations()
        {
            List<ILocation> locations = new List<ILocation>();
            var lotFlags = GetDoorFlags();
            var baseAddress = GetEventFlagsOffset();
            foreach (var lot in lotFlags)
            {
                locations.Add(new Location
                {
                    Name = lot.Name,
                    Address = baseAddress + GetEventFlagOffset(lot.Flag).Item1,
                    AddressBit = GetEventFlagOffset(lot.Flag).Item2,
                    Id = lot.Id,
                });
            }
            return locations;
        }
        public static List<ILocation> GetFogWallFlagLocations()
        {
            List<ILocation> locations = new List<ILocation>();
            var lotFlags = GetFogWallFlags();
            var baseAddress = GetEventFlagsOffset();
            foreach (var lot in lotFlags)
            {
                locations.Add(new Location
                {
                    Name = lot.Name,
                    Address = baseAddress + GetEventFlagOffset(lot.Flag).Item1,
                    AddressBit = GetEventFlagOffset(lot.Flag).Item2,
                    Id = lot.Id,
                });
            }
            return locations;
        }
        public static List<ILocation> GetMiscFlagLocations()
        {
            List<ILocation> locations = new List<ILocation>();
            var lotFlags = GetMiscFlags();
            var baseAddress = GetEventFlagsOffset();
            foreach (var lot in lotFlags)
            {
                locations.Add(new Location
                {
                    Name = lot.Name,
                    Address = baseAddress + GetEventFlagOffset(lot.Flag).Item1,
                    AddressBit = GetEventFlagOffset(lot.Flag).Item2,
                    Id = lot.Id,
                });
            }
            return locations;
        }
        public static ulong OffsetPointer(ulong ptr, int offset)
        {
            ushort offsetWithin4GB = (ushort)(ptr & 0xFFFF);
            ushort newOffset = (ushort)(offsetWithin4GB + offset);
            ulong newAddress = (ptr & 0xFFFF0000) | newOffset;
            return newAddress;
        }
        
        /* Build a mapping of the location flag values in eventflags to the ItemLot that should come from there, for items in our own game.
         * Then fill the rest of the eventflags locations with the default ItemLot ("Prism Stone").
         * This is used for replacing item lots in our own game. */
        public static Dictionary<int, ItemLot> BuildFlagToLotMap(List<EventFlag> eventflags)
        {
            Dictionary<int, ItemLot> result = new Dictionary<int, ItemLot>();

            var currentSlot = App.Client.CurrentSession.ConnectionInfo.Slot;
            var slotDataTask = App.Client.CurrentSession.DataStorage.GetSlotDataAsync(currentSlot);
            slotDataTask.Wait();
            var slotData = slotDataTask.Result;

            /* Get locationsId and locationsTarget into lists */
            List<int?> locationsIdList = new List<int?>();
            List<int?> locationsTargetList = new List<int?>();

            if (slotData.TryGetValue("locationsId", out object locationsId))
            {
                locationsIdList.AddRange(JsonConvert.DeserializeObject<int?[]>(locationsId.ToString()));
                if (slotData.TryGetValue("locationsTarget", out object locationsTarget))
                {
                    locationsTargetList.AddRange(JsonConvert.DeserializeObject<int?[]>(locationsTarget.ToString()));
                }
            }

            if (locationsIdList.Count == 0 || locationsTargetList.Count == 0
             || locationsIdList.Count != locationsTargetList.Count)
            {
                Log.Logger.Error($"Slot Info: Location and Item id count mismatch");
            }
            else
            {
                /* Iterate over each pair of entries in the pair of lists */
                for (int i = 0; i < locationsIdList.Count; i++)
                {
                    int? target = locationsTargetList[i];
                    int? locId = locationsIdList[i];

                    if (locId != null && target != null && target != 0)
                    {
                        /* Found an item of our own, located in our own game. 
                         * Validate that it's in the eventflags we've been given, and find the matching item. */
                        EventFlag? lot = eventflags.Find(x => x.Id == locId);
                        DarkSoulsItem? item = App.AllItems.Find(x => x.ApId == 11110000 + target);
                        if (lot != null && item != null)
                        {
                            Log.Logger.Verbose($"Item {i} at location id{locId}/flag={lot.Flag} ({lot.Name}) is {target}/{item.Id}({item.Name})");
                            var newitem = new ItemLotItem
                            {
                                CumulateLotPoint = 0,
                                CumulateReset = false,
                                EnableLuck = false,
                                GetItemFlagId = -1,
                                LotItemBasePoint = 100,
                                LotItemCategory = (int)item.Category,
                                LotItemNum = 1,
                                LotItemId = item.Id
                            };
                            /* If it's already in the mapping, add the item to the list of items in the existing lot */
                            if (result.ContainsKey(lot.Flag))
                                result[lot.Flag].Items.Add(newitem);
                            else
                            {
                                /* add the found location->item to the replacement dictionary */
                                var newitemlot = new ItemLot
                                {
                                    Rarity = 1,
                                    GetItemFlagId = -1,
                                    CumulateNumFlagId = -1,
                                    CumulateNumMax = 0,
                                    Items = new List<ItemLotItem>([newitem])
                                };
                                result.Add(lot.Flag, newitemlot);
                            }
                        }
                        else
                        {
                            Log.Logger.Verbose($"Item {i} {locId} lotnull {lot == null}, {target} itemnull {item == null}");
                        }

                    }
                }
            }
            Log.Logger.Debug($"replacement dict size = {result.Count}");

            /* Then, anything that is in this eventflags list, but isn't an item for our own world, replace with prism stones */

            /* prism stone lot */
            var defaultLot = new ItemLot()
            {
                Rarity = 1,
                GetItemFlagId = -1,
                CumulateNumFlagId = -1,
                CumulateNumMax = 0,
                Items = new List<ItemLotItem>()
                    {
                        new ItemLotItem
                        {
                            CumulateLotPoint = 0,
                            CumulateReset = false,
                            EnableLuck = false,
                            GetItemFlagId = -1,
                            LotItemBasePoint = 100,
                            LotItemCategory = (int)DSAP.Enums.DSItemCategory.Consumables,
                            LotItemNum = 1,
                            LotItemId = 370
                        }
                    }
            };

            HashSet<int> uniqueLots = new HashSet<int>();
            uniqueLots.UnionWith(eventflags.Select(x => x.Flag));
            Log.Logger.Verbose($"unique item lot count ={uniqueLots.Count}");

            Log.Logger.Verbose(string.Join(", ", uniqueLots));

            foreach (var lot in uniqueLots)
            {
                result.TryAdd(lot, defaultLot);
            }
            return result;
        }

        /* Build a mapping of the location id values in eventflags to the ItemLot that should come from there, for items in our own game.
         * This is used for detecting non-item lot conditions in our own game (like a door opening) and rewarding the player with that item.
         * This is a separate method from the above similar method for two reasons:
         *  1) It needs to fill in EventFlag.id as the key instead of EventFlag.flag, and
         *  2) It doesn't need the "prism stone" fallback */
        public static Dictionary<int, ItemLot> BuildIdToLotMap(List<EventFlag> eventflags)
        {
            Dictionary<int, ItemLot> result = new Dictionary<int, ItemLot>();

            var currentSlot = App.Client.CurrentSession.ConnectionInfo.Slot;
            var slotDataTask = App.Client.CurrentSession.DataStorage.GetSlotDataAsync(currentSlot);
            slotDataTask.Wait();
            var slotData = slotDataTask.Result;

            /* Get locationsId and locationsTarget into lists */
            List<int?> locationsIdList = new List<int?>();
            List<int?> locationsTargetList = new List<int?>();

            if (slotData.TryGetValue("locationsId", out object locationsId))
            {
                locationsIdList.AddRange(JsonConvert.DeserializeObject<int?[]>(locationsId.ToString()));
                if (slotData.TryGetValue("locationsTarget", out object locationsTarget))
                {
                    locationsTargetList.AddRange(JsonConvert.DeserializeObject<int?[]>(locationsTarget.ToString()));
                }
            }

            if (locationsIdList.Count == 0 || locationsTargetList.Count == 0
             || locationsIdList.Count != locationsTargetList.Count)
            {
                Log.Logger.Error($"Slot Info: Location and Item id count mismatch");
            }
            else
            {
                /* Iterate over each pair of entries in the pair of lists */
                for (int i = 0; i < locationsIdList.Count; i++)
                {
                    int? target = locationsTargetList[i];
                    int? locId = locationsIdList[i];

                    if (locId != null && target != null && target != 0)
                    {
                        /* Found an item of our own, located in our own game. 
                         * Validate that it's in the eventflags we've been given, and find the matching item. */

                        EventFlag? lot = eventflags.Find(x => x.Id == locId);
                        DarkSoulsItem? item = App.AllItems.Find(x => x.ApId == 11110000 + target);
                        if (lot != null && item != null)
                        {
                            Log.Logger.Verbose($"Item {i} at location id{locId}/flag={lot.Flag} ({lot.Name}) is {target}/{item.Id}({item.Name})");
                            var newitem = new ItemLotItem
                            {
                                CumulateLotPoint = 0,
                                CumulateReset = false,
                                EnableLuck = false,
                                GetItemFlagId = -1,
                                LotItemBasePoint = 100,
                                LotItemCategory = (int)item.Category,
                                LotItemNum = 1,
                                LotItemId = item.Id
                            };
                            /* If it's already in the mapping, add the item to the list of items in the existing lot */
                            if (result.ContainsKey(lot.Id))
                                result[lot.Id].Items.Add(newitem);
                            else
                            {
                                /* add the found location->item to the replacement dictionary */
                                var newitemlot = new ItemLot
                                {
                                    Rarity = 1,
                                    GetItemFlagId = -1,
                                    CumulateNumFlagId = -1,
                                    CumulateNumMax = 0,
                                    Items = new List<ItemLotItem>([newitem])
                                };
                                result.Add(lot.Id, newitemlot);
                            }
                        }
                        else
                        {
                            Log.Logger.Verbose($"Item {i} {locId} lotnull {lot == null}, {target} itemnull {item == null}");
                        }

                    }
                }
            }
            Log.Logger.Debug($"idToLotMap size = {result.Count}");

            /* Don't populate the rest of the flags with prism stones */
            return result;
        }
        public static List<ItemLot> GetItemLots()
        {
            List<ItemLot> itemLots = new List<ItemLot>();

            var startAddress = GetItemLotParamOffset();

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
        /* Overwrite all of the item lots at once. */
        public static void OverwriteItemLots(Dictionary<int, ItemLot> itemLotIds)
        {
            var startAddress = GetItemLotParamOffset();
            var dataOffset = Memory.ReadUInt(startAddress + 0x4);
            var rowCount = Memory.ReadUShort(startAddress + 0xA);
            var foundItems = 0;
            const int rowSize = 148; // Size of each ItemLotParam
            Log.Debug($"ItemParam list rowcount='{rowCount}'");
            var tasks = new List<Task>();

            for (int i = 0; i < rowCount; i++)
            {
                var currentAddress = startAddress + dataOffset + (ulong)(i * rowSize);
                var currentItemLotId = Memory.ReadInt(currentAddress + 0x80);  // GetItemFlagId is at offset 0x80

                    
                ItemLot newItemLot;
                if (itemLotIds.TryGetValue(currentItemLotId, out newItemLot))
                {

                    foundItems++;
                    // We found the correct item lot or are using the default, now let's overwrite it

                    /* Parallelize the writing of memory to many tasks for speed-up. */
                    tasks.Add(Task.Run(() =>
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            OverwriteSingleItem(currentAddress, newItemLot.Items[0], j);
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
                    }));
                        

                    Log.Verbose($"i='{i}' ItemLot with GetItemFlagId {currentItemLotId} has been overwritten at {currentAddress} to give {newItemLot.Items[0].LotItemId} in {newItemLot.Items[0].LotItemCategory}.");
                }
                else
                {
                    Log.Verbose($"i='{i}' ItemLot with GetItemFlagId {currentItemLotId} not overwritten.");
                }

            }

            Task.WaitAll(tasks.ToArray());

            Log.Information($"{foundItems} items overwritten");
        }
        public static void OverwriteSingleItem(ulong address, ItemLotItem newItemLot, int position)
        {

            //uint id =       Memory.ReadUInt(address + (ulong)(position * 4));
            //uint category = Memory.ReadUInt(address + 0x20 + (ulong)(position * 4));
            //uint itemnum = Memory.ReadUInt(address + 0x8A + (ulong)position);
            //uint itemflagid = Memory.ReadUInt(address + 0x60 + (ulong)(position * 4));
            //Log.Information($"id {id} cat {category} itemnum {itemnum} itemflag {itemflagid} overwritten");

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
            var allItems = GetAllItems();
            var item = allItems.FirstOrDefault(x => x.Id == lot.LotItemId);
            return item;
        }
        public static bool GetIsPlayerOnline()
        {
            var baseCOffset = GetBaseCOffset();
            ulong onlineFlagOffset = 0xB7D;

            var isOnline = Memory.ReadByte(baseCOffset + onlineFlagOffset) != 0;
            return isOnline;

        }
        public static List<Location> GetBossLocations()
        {
            var offset = GetProgressionFlagOffset();
            var bosses = GetBosses();
            var locations = new List<Location>();
            foreach (var b in bosses)
            {
                var location = new Location
                {
                    Id = b.LocationId,
                    Name = b.Name,
                    Address = offset + (ulong)b.Offset,
                    AddressBit = b.AddressBit
                };
                locations.Add(location);
            }
            return locations;
        }
        public static LastBonfire GetLastBonfire()
        {

            var baseC = GetBaseCOffset();
            var lastBonfireAddress = OffsetPointer(baseC, 0xB34);
            var lastBonfireId = Memory.ReadInt(lastBonfireAddress);
            //todo get last bonfire
            var list = GetLastBonfireList();
            var lastBonfire = list.FirstOrDefault(x => x.id == lastBonfireId);
            if (lastBonfire != null)
            {
                return lastBonfire;
            }
            return null;
        }
        public static async void MonitorLastBonfire(Action<LastBonfire> action)
        {
            var lastBonfire = GetLastBonfire();
            if (lastBonfire == null)
            {
                Log.Debug("No Last Bonfire found");
            }
            else Log.Debug($"Last bonfire was {lastBonfire.id}:{lastBonfire.name} ");
            while (true)
            {
                var currentLastBonfire = GetLastBonfire();
                if (currentLastBonfire != lastBonfire)
                {
                    Log.Debug("Last Bonfire Changed");
                    lastBonfire = currentLastBonfire;
                    action?.Invoke(currentLastBonfire);
                }
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
        public static List<DarkSoulsItem> GetConsumables()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Consumables.json");
            var list = JsonConvert.DeserializeObject<List<DarkSoulsItem>>(json);
            return list;
        }
        public static List<DarkSoulsItem> GetUpgradeMaterials()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.UpgradeMaterials.json");
            var list = JsonConvert.DeserializeObject<List<DarkSoulsItem>>(json);
            return list;
        }
        public static List<DarkSoulsItem> GetKeyItems()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.KeyItems.json");
            var list = JsonConvert.DeserializeObject<List<DarkSoulsItem>>(json);
            return list;
        }
        public static List<DarkSoulsItem> GetRings()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Rings.json");
            var list = JsonConvert.DeserializeObject<List<DarkSoulsItem>>(json);
            return list;
        }
        public static List<DarkSoulsItem> GetSpells()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Spells.json");
            var list = JsonConvert.DeserializeObject<List<DarkSoulsItem>>(json);
            return list;
        }
        public static List<DarkSoulsItem> GetShields()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Shields.json");
            var list = JsonConvert.DeserializeObject<List<DarkSoulsItem>>(json);
            return list;
        }
        public static List<DarkSoulsItem> GetTraps()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Traps.json");
            var list = JsonConvert.DeserializeObject<List<DarkSoulsItem>>(json);
            return list;
        }
        public static List<DarkSoulsItem> GetRangedWeapons()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.RangedWeapons.json");
            var list = JsonConvert.DeserializeObject<List<DarkSoulsItem>>(json);
            return list;
        }
        public static List<DarkSoulsItem> GetMeleeWeapons()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.MeleeWeapons.json");
            var list = JsonConvert.DeserializeObject<List<DarkSoulsItem>>(json);
            return list;
        }
        public static List<DarkSoulsItem> GetArmor()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Armor.json");
            var list = JsonConvert.DeserializeObject<List<DarkSoulsItem>>(json);
            return list;
        }
        public static List<DarkSoulsItem> GetSpellTools()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.SpellTools.json");
            var list = JsonConvert.DeserializeObject<List<DarkSoulsItem>>(json);
            return list;
        }
        public static List<DarkSoulsItem> GetUsableItems()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.UsableItems.json");
            var list = JsonConvert.DeserializeObject<List<DarkSoulsItem>>(json);
            return list;
        }
        public static List<ItemLotFlag> GetItemLotFlags()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.ItemLots.json");
            var list = JsonConvert.DeserializeObject<List<ItemLotFlag>>(json);
            return list;
        }
        public static List<BossFlag> GetBossFlags()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.BossFlags.json");
            var list = JsonConvert.DeserializeObject<List<BossFlag>>(json);
            return list;
        }
        public static List<BonfireFlag> GetBonfireFlags()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Bonfires.json");
            var list = JsonConvert.DeserializeObject<List<BonfireFlag>>(json);
            return list;
        }
        public static List<DoorFlag> GetDoorFlags()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Doors.json");
            var list = JsonConvert.DeserializeObject<List<DoorFlag>>(json);
            return list;
        }
        public static List<FogWallFlag> GetFogWallFlags()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.FogWalls.json");
            var list = JsonConvert.DeserializeObject<List<FogWallFlag>>(json);
            return list;
        }
        public static List<EventFlag> GetMiscFlags()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.MiscFlags.json");
            var list = JsonConvert.DeserializeObject<List<EventFlag>>(json);
            return list;
        }
        public static List<LastBonfire> GetLastBonfireList()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.LastBonfire.json");
            var list = JsonConvert.DeserializeObject<List<LastBonfire>>(json);
            return list;
        }
        public static List<DarkSoulsItem> GetAllItems()
        {
            var results = new List<DarkSoulsItem>();

            results = results.Concat(GetConsumables()).ToList();
            results = results.Concat(GetKeyItems()).ToList();
            results = results.Concat(GetRings()).ToList();
            results = results.Concat(GetUpgradeMaterials()).ToList();
            results = results.Concat(GetSpells()).ToList();
            results = results.Concat(GetShields()).ToList();
            results = results.Concat(GetRangedWeapons()).ToList();
            results = results.Concat(GetSpellTools()).ToList();
            results = results.Concat(GetUsableItems()).ToList();
            results = results.Concat(GetMeleeWeapons()).ToList();
            results = results.Concat(GetArmor()).ToList();
            results = results.Concat(GetTraps()).ToList();

            return results;
        }
        public static ulong FlagToOffset(EventFlag flag)
        {
            var offset = GetEventFlagOffset(flag.Flag).Item1;
            return offset;
        }
        public bool IsInGame()
        {
            throw new NotImplementedException();
        }

        public static (ulong, int) GetEventFlagOffset(int eventFlag)
        {
            string idString = eventFlag.ToString("D8");
            int tail = Int32.Parse(idString.Substring(5, 3));

            uint fourByteMask = 0x80000000 >> (tail % 32);
            int significantByte = 0;
            if ((fourByteMask & 0x000000FF) != 0) significantByte = 0;
            else if ((fourByteMask & 0x0000FF00) != 0) significantByte = 1;
            else if ((fourByteMask & 0x00FF0000) != 0) significantByte = 2;
            else if ((fourByteMask & 0xFF000000) != 0) significantByte = 3;

            int bitMask = BitOperations.TrailingZeroCount((fourByteMask >> significantByte * 8) & 0xFF);
            var offset = GetPrimaryOffsetFromFlagId(idString);
            offset += GetSecondaryOffsetFromFlagId(idString);
            offset += Int32.Parse(idString.Substring(4, 1)) * 128;
            offset += (tail - (tail % 32)) / 8;

            ulong addressOffser = Convert.ToUInt64(offset + significantByte);

            return (addressOffser, bitMask);
        }

        private static int GetPrimaryOffsetFromFlagId(string eventFlag)
        {
            return eventFlag.Substring(0, 1) switch
            {
                "0" => 0x00000,
                "1" => 0x00500,
                "5" => 0x05F00,
                "6" => 0x0B900,
                "7" => 0x11300,
                _ => throw new ArgumentException("Cannot get primary offset for GetItemFlagId: " + eventFlag),
            };
        }

        private static int GetSecondaryOffsetFromFlagId(string eventFlag)
        {
            var num = eventFlag.Substring(1, 3) switch
            {
                "000" => 00,
                "100" => 01,
                "101" => 02,
                "102" => 03,
                "110" => 04,
                "120" => 05,
                "121" => 06,
                "130" => 07,
                "131" => 08,
                "132" => 09,
                "140" => 10,
                "141" => 11,
                "150" => 12,
                "151" => 13,
                "160" => 14,
                "170" => 15,
                "180" => 16,
                "181" => 17,
                _ => throw new ArgumentException("Cannot get secondary offset for GetItemFlagId: " + eventFlag),
            };
            return num * 1280;
        }
        public static List<int> GetStarterGearIds()
        {
            return new List<int>
            {
                51810100,
                51810110,
                51810120,
                51810130,
                51810140,
                51810150,
                51810160,
                51810170,
                51810180,
                51810190,
                51810200,
                51810210,
                51810220,
                51810220,
                51810230,
                51810240,
                51810250,
                51810260,
                51810270,
                51810280,
                51810290,
                51810300,
                51810310,
                51810320,
                51810330,
            };
        }
        public static List<Boss> GetBosses()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Bosses.json");
            var list = JsonConvert.DeserializeObject<List<Boss>>(json);
            return list;
        }
        public static string OpenEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string file = reader.ReadToEnd();
                return file;
            }
        }
        public static byte[] GetItemCommand()
        {

            byte[] x = [0xBA, 0x00, 0x00, 0x00, 0x10, 0x41, 0xB9, 0x01, 0x00, 0x00, 0x00, 0x41, 0xB8, 0x28, 0x70, 0x08, 0x00, 0x41, 0xBC, 0xFE, 0xFE, 0xFE, 0xFE, 0x48, 0xA1, 0x30, 0xA5, 0xC8, 0x41, 0x01, 0x00, 0x00, 0x00, 0xC6, 0x44, 0x24, 0x38, 0x01, 0x40, 0x88, 0x7C, 0x24, 0x30, 0xC6, 0x44, 0x24, 0x28, 0x01, 0x4C, 0x8B, 0x78, 0x10, 0xC6, 0x44, 0x24, 0x20, 0x01, 0x49, 0x8D, 0x8F, 0x80, 0x02, 0x00, 0x00, 0x48, 0x83, 0xEC, 0x38, 0x49, 0xBE, 0xE0, 0x79, 0x74, 0x40, 0x01, 0x00, 0x00, 0x00, 0x41, 0xFF, 0xD6, 0x48, 0x83, 0xC4, 0x38, 0xC3];
            return x;
        }


        public static byte[] GetItemWithMessage()
        {
            byte[] x = new byte[] {
        0x8B, 0x15, 0x3E, 0x00, 0x00, 0x00,               // mov edx,[itemdata] 
        0x48, 0xA1, 0x30, 0xA5, 0xc8, 0x41, 0x01, 0x00, 0x00, 0x00,  // mov rax,[ChrBaseClass]
        0x44, 0x8B, 0x0D, 0x31, 0x00, 0x00, 0x00,        // mov r9d,[itemdata+4]
        0x44, 0x8B, 0x05, 0x2E, 0x00, 0x00, 0x00,        // mov r8d,[itemdata+8]
        0x4C, 0x8B, 0x78, 0x10,                           // mov r15,[rax+10]
        0x49, 0x8D, 0x8F, 0x80, 0x02, 0x00, 0x00,        // lea rcx,[r15+280]
        0x48, 0x83, 0xEC, 0x38,                           // sub rsp,38
        0xFF, 0x15, 0x02, 0x00, 0x00, 0x00, 0xEB, 0x08,  // call ItemGetAddr
        0xE0, 0x79, 0x74, 0x40, 0x01, 0x00, 0x00, 0x00,  // ItemGetAddr placeholder
        0x48, 0x83, 0xC4, 0x38,                           // add rsp,38
        0xC3,                                             // ret
        0x90, 0x90,                                       // nops for alignment
        0x00, 0x00, 0x00, 0x00,                          // item category placeholder
        0x00, 0x00, 0x00, 0x00,                          // item quantity placeholder
        0x00, 0x00, 0x00, 0x00,                          // item id placeholder
        0xFF, 0xFF, 0xFF, 0xFF                           // item durability
    };

            // Replace ChrBaseClass address
            Array.Copy(BitConverter.GetBytes(GetChrBaseClassOffset()), 0, x, 8, 8);

            return x;
        }
                /*  ----------Code To Emulate--------------
            
            mov rcx,[BaseB]
            mov edx,1
            sub rsp,38
            call 0x1404867e0
            add rsp,38
            ret

        */
        /*  ----------Homeward Bone injected ASM--------------
            0:  48 c7 c1 78 56 34 12    mov    rcx,0x12345678
            7:  00
            8:  ba 01 00 00 00          mov    edx,0x1
            d:  49 be e0 67 48 40 01    movabs r14,0x1404867e0
            14: 00 00 00
            17: 48 83 ec 38             sub    rsp,0x38
            1b: 41 ff d6                call   r14
            1e: 48 83 c4 38             add    rsp,0x38
            22: c3                      ret 
         */
        public static byte[] HomewardBone()
        {
            byte[] x = new byte[] {
                    0x48, 0xC7, 0xC1, 0x78, 0x56, 0x34, 0x12,
                    0xBA, 0x01, 0x00, 0x00, 0x00,
                    0x49, 0xBE, 0xE0, 0x67, 0x48, 0x40, 0x01, 0x00, 0x00, 0x00,
                    0x48, 0x83, 0xEC, 0x38,
                    0x41, 0xFF, 0xD6,
                    0x48, 0x83, 0xC4, 0x38,
                    0xC3

            };

            return x;
        }

    }
}

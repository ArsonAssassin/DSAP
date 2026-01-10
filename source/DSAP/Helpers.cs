using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Archipelago.Core.Util.GPS;
using DSAP.Models;
using Serilog;
using Silk.NET.OpenGL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Location = Archipelago.Core.Models.Location;
namespace DSAP
{
    public class Helpers
    {   
        /* aka GameDataMan */
        private static AoBHelper BaseBAoB = new AoBHelper("BaseB",
                [0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x45, 0x33, 0xED, 0x48, 0x8B, 0xF1, 0x48, 0x85, 0xC0],
                "xxx????xxxxxxxxx", 3, 4);
        /* worlddataman? */
        private static AoBHelper BaseEAoB = new AoBHelper("BaseE",
                [0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x88, 0x98, 0x0B, 0x00, 0x00, 0x8B, 0x41, 0x3C, 0xC3],
                "xxx????xxxxxxxxxxx", 3, 4);
        /* AKA "WorldChrManImp" */
        private static AoBHelper BaseXAoB = new AoBHelper("BaseX",
                [0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x48, 0x39, 0x48, 0x68, 0x0f, 0x94, 0xc0, 0xc3],
                "xxx????xxxxxxxx", 3, 4);
        /* aka 141c8adc0 */
        private static AoBHelper EmkAoB = new AoBHelper("EmkHead",
                [0x48, 0x89, 0x05, 0x00, 0x00, 0x00, 0x00, 0xeb, 0x0b, 0x48, 0xc7, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8b, 0x5c, 0x24, 0x50],
                "xxx????xxxxx????xxxxxxxxx", 3, 4);

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
        

        public static ulong GetBaseAddress()
        {
            var address = Memory.GetBaseAddress("DarkSoulsRemastered");
            if (address == 0)
            {
                Log.Logger.Debug("Could not find Base Address");
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
                Log.Logger.Debug($"Base address: 0x{baseAddress:X}");

                byte[] pattern = { 0x48, 0x83, 0x3d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8b, 0xf1 };
                string mask = "xxx????xxxx";

                IntPtr getfrpgNetManAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

                if (getfrpgNetManAddress == IntPtr.Zero)
                {
                    Log.Logger.Error("Failed to find the signature pattern for FrpgNetMan");
                    throw new Exception("Failed to find the signature pattern");
                }

                Log.Logger.Debug($"Found pattern at: 0x{getfrpgNetManAddress.ToInt64():X}");

                // Read the bytes at the pattern location to verify
                byte[] bytes = Memory.ReadByteArray((ulong)getfrpgNetManAddress, 11);
                Log.Logger.Debug($"Bytes at pattern: {BitConverter.ToString(bytes)}");

                // Read the 4-byte offset at position 3
                int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getfrpgNetManAddress + 3), 4), 0);
                Log.Logger.Debug($"Read offset value: 0x{offset:X}");

                // Try different pointer calculation methods
                IntPtr method1 = new IntPtr(getfrpgNetManAddress.ToInt64() + offset + 7);
                IntPtr method2 = new IntPtr(getfrpgNetManAddress.ToInt64() + 3 + 4 + offset);
                IntPtr method3 = new IntPtr(getfrpgNetManAddress.ToInt64() + offset + 8);

                ulong value1 = Memory.ReadULong((ulong)method1);
                ulong value2 = Memory.ReadULong((ulong)method2);
                ulong value3 = Memory.ReadULong((ulong)method3);

                Log.Logger.Debug($"Method 1 (offset+7): Address=0x{method1.ToInt64():X}, Value=0x{value1:X}");
                Log.Logger.Debug($"Method 2 (offset+3+4): Address=0x{method2.ToInt64():X}, Value=0x{value2:X}");
                Log.Logger.Debug($"Method 3 (offset+8): Address=0x{method3.ToInt64():X}, Value=0x{value3:X}");

                // Check if any method gives a likely valid pointer (usually in a specific memory range)
                if (value3 > 0x10000000 && value3 < 0x7FFFFFFFFFFF)
                {
                    Log.Logger.Debug("Using Method 3");
                    return value3;
                }

                // If we're here, all methods failed to produce a reasonable pointer
                Log.Logger.Error("Failed to resolve a valid FrpgNetMan pointer with any method");
                throw new Exception("Failed to get valid FrpgNetMan pointer");
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error in GetFrpgNetManOffset: {ex.Message}");
                throw;
            }
        }

        public static ulong GetBaseBAddress()
        {
            return (ulong)BaseBAoB.Address;
        }
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
        public static ulong GetBaseEAddress()
        {
            IntPtr baseE = BaseEAoB.Address;
            return (ulong)baseE;

        }
        public static ulong GetBaseXAddress()
        {
            IntPtr baseX = BaseXAoB.Address;
            return (ulong)baseX;
            
        }
        public static ulong GetEmkHeadAddress()
        {
            IntPtr emkHeadPtr = EmkAoB.Address;
            return (ulong)emkHeadPtr;

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
            Log.Logger.Verbose($"getpf={getPFAddress}");
            Log.Logger.Verbose($"getpf offset={offset}");
            Log.Logger.Verbose($"pf @ ={progressionFlagsAddress}");

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
            var baseB = GetBaseBAddress();
            var next = OffsetPointer(baseB, 0x10);
            var pointer = Memory.ReadULong(next);
            next = OffsetPointer(pointer, 0x14);
            return next;
        }
        /// <summary>
        /// Get the HP address to which writing will actually update the player's HP (for deathlink).
        /// </summary>
        /// <returns>The address, or 0 if any pointer value along the chain was 0.</returns>
        internal static ulong GetPlayerWritableHPAddress()
        {
            var baseX = GetBaseXAddress();
            if (baseX != 0)
            {
                var next = OffsetPointer(baseX, 0x68);
                var pointer = Memory.ReadULong(next);
                if (pointer != 0)
                {
                    next = OffsetPointer(pointer, 0x3e8);
                    return next;
                }
            }
            return 0;
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
        public static bool ReadBonfireFlag(string name)
        {
            string result = "";
            var lotFlags = GetBonfireFlagLocations();
            Location thisLoc = (Location)lotFlags.FirstOrDefault(x => x.Name == name);

            ulong Address = thisLoc.Address;
            int AddressBit = thisLoc.AddressBit;

            int wholebyte = Memory.ReadByte(Address);
            int bit = 0;
            if ((wholebyte & (1 << AddressBit)) != 0)
                bit = 1;
            Log.Logger.Debug($"Read Bonfire Flag for {name} at {Address}/0x{Address.ToString("X")} = {wholebyte}, bit [{AddressBit}]={bit}");
            return bit == 1;
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
            var offset = GetProgressionFlagOffset();
            var baseAddress = (ulong)Memory.ReadInt(offset);
            var baseAddress2 = (ulong)Memory.ReadInt(baseAddress);
            Log.Logger.Verbose($"bfloc offset={offset},0x{offset.ToString("X")}");
            Log.Logger.Verbose($"bfloc baseadd={baseAddress},0x{baseAddress.ToString("X")}");
            Log.Logger.Verbose($"bfloc baseadd2={baseAddress2},0x{baseAddress2.ToString("X")}");

            foreach (var lot in lotFlags)
            {
                locations.Add(new Location
                {
                    Name = lot.Name,
                    Address = baseAddress2 + lot.Offset,
                    AddressBit = lot.AddressBit,
                    Id = lot.Id
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
        public static ulong OffsetPointer(ulong ptr, uint offset)
        {
            ulong newAddress = ptr;
            return ptr + (ulong)offset;
        }

        /// <summary>
        /// Build a mapping of the location flag values in eventflags to the ItemLot that should come from there, for items in our own game.
        /// </summary>
        /// <details>
        /// This is used for replacing item lots in our own game.
        /// </details>
        /// <param name="eventflags">A list of location flags of which items will be found.</param>
        /// <param name="resultMap">A dictionary mapping itemlot flags to "item lots".</param>
        /// <param name="specialResultMap">A dictionary mapping itemlot flags to "special items" (e.g. events, traps). </param>
        /// <returns></returns>
        public static void BuildFlagToLotMap(out Dictionary<int, ItemLot> resultMap,
            out Dictionary<int, ItemLot> specialResultMap,
            List<EventFlag> eventflags,
            Dictionary<string, object> slotData,
            Dictionary<string, Tuple<int, string>> slotLocToItemUpgMap)
        {
            Dictionary<int, ItemLot> result = new Dictionary<int, ItemLot>();
            Dictionary<int, ItemLot> specialResult = new Dictionary<int, ItemLot>();

            var addonitems = 0;

            /* Get locationsId and locationsTarget into lists */
            List<int?> locationsIdList = new List<int?>();
            List<int> locationsTargetList = new List<int>();

            if (slotData.TryGetValue("locationsId", out object locationsId))
            {
                locationsIdList.AddRange(JsonSerializer.Deserialize<int?[]>(locationsId.ToString()));
                if (slotData.TryGetValue("locationsTarget", out object locationsTarget))
                {
                    locationsTargetList.AddRange(JsonSerializer.Deserialize<int[]>(locationsTarget.ToString()));
                }
            }

            if (locationsIdList.Count == 0 || locationsTargetList.Count == 0
             || locationsIdList.Count != locationsTargetList.Count)
            {
                Log.Logger.Error("Slot Info: Location and Item id count mismatch, cannot overwrite items.");
                App.Client.AddOverlayMessage("Slot Info: Location and Item id count mismatch, cannot overwrite items.");
            }
            else
            {
                /* Iterate over each pair of entries in the pair of lists */
                for (int i = 0; i < locationsIdList.Count; i++)
                {
                    int target = locationsTargetList[i];
                    int? locId = locationsIdList[i];
                    if (locId != null) /* full list of locations in our game */
                    {
                        EventFlag? lot = eventflags.Find(x => x.Id == locId);
                        if (lot != null) /* found a location in our "item lots" */
                        {
                            ItemLotItem newLotItem = new ItemLotItem { };
                            if (target != 0) /* found an item in our game  */
                            {
                                /* Found an item of our own, located in our own game. 
                                 * Validate that it's in the eventflags we've been given, and find the matching item. */
                                DarkSoulsItem? item = App.AllItems.Find(x => x.ApId == 11110000 + target);
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
                                                repitem = UpgradeItem(repitem, itemupg.Item2);
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
                                        LotItemNum = 1,
                                        LotItemId = repitem.Id
                                    };

                                    if (item.Category == Enums.DSItemCategory.DsrEvent || item.Category == Enums.DSItemCategory.Trap)
                                    {
                                        Log.Logger.Debug($"Item at loc {locId} detected as {item.Name} in category {item.Category} - replaced with prism stone.");
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
                                        
                                        newLotItem = prismStoneLotItem;
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
                            else /* item not in own game, put a prism stone instead */
                            {
                                Log.Logger.Verbose($"Item {i} target = {target}");
                                newLotItem = prismStoneLotItem;
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
                }
            }
            Log.Logger.Debug($"replacement dict size = {result.Count}");
            Log.Logger.Debug($" {addonitems} addonitems");


            /* Populate frampt chest with rubbish */
            const int frampt_base = 50004000;
            /* Iterate over each pair of entries in the pair of lists */
            for (int i = 0; i <= 69; i++)
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

            /* Then, anything that is in this eventflags list, but wasn't an AP location sent to us, replace with prism stones */
            //Dictionary<int, int> addedItems = [];
            //foreach (var flag in eventflags.Where(x => !result.ContainsKey(x.Flag)).Select(x => x.Flag))
            //{
            //    addedItems.TryAdd(flag, 0);
            //    addedItems[flag] += 1;
            //}
            //foreach (var pair in addedItems)
            //{
            //    int flag = pair.Key;
            //    result.TryAdd(pair.Key, new ItemLot()
            //    {
            //        Rarity = 1,
            //        GetItemFlagId = -1,
            //        CumulateNumFlagId = -1,
            //        CumulateNumMax = 0,
            //        Items = []
            //    });
            //    for (int i = 0; i < pair.Value; i++)
            //    {
            //        result[flag].Items.Add(prismStoneLotItem);
            //    }
            //    Log.Logger.Verbose($"item lot {flag} added, count = {result[flag].Items.Count} items");
            //}
            specialResultMap = specialResult;
            resultMap = result;
            return;
        }

        /// <summary>
        /// Build a mapping of the location id values in eventflags to the ItemLot that should come from there, for items in our own game.
        /// </summary>
        /// <details>
        /// This is used for detecting non-item lot conditions in our own game(like a door opening) and rewarding the player with that item.
        /// This is a separate method from the above similar method for one main reason:
        ///    1) It needs to fill in EventFlag.id as the key instead of EventFlag.flag, so we can search by the "AP location id" instead of the DSR flag
        /// </details>
        /// <param name="eventflags">A list of location flags of which items will be found.</param>
        /// <returns></returns>
        public static Dictionary<int, ItemLot> BuildIdToLotMap(List<EventFlag> eventflags, Dictionary<string, object> slotData, Dictionary<string, Tuple<int, string>> slotLocToItemUpgMap)
        {
            Dictionary<int, ItemLot> result = new Dictionary<int, ItemLot>();

            /* Get locationsId and locationsTarget into lists */
            List<int?> locationsIdList = new List<int?>();
            List<int?> locationsTargetList = new List<int?>();

            if (slotData.TryGetValue("locationsId", out object locationsId))
            {
                locationsIdList.AddRange(JsonSerializer.Deserialize<int?[]>(locationsId.ToString()));
                if (slotData.TryGetValue("locationsTarget", out object locationsTarget))
                {
                    locationsTargetList.AddRange(JsonSerializer.Deserialize<int?[]>(locationsTarget.ToString()));
                }
            }

            if (locationsIdList.Count == 0 || locationsTargetList.Count == 0
             || locationsIdList.Count != locationsTargetList.Count)
            {
                Log.Logger.Error("Slot Info: Location and Item id count mismatch, cannot overwrite items.");
                App.Client.AddOverlayMessage("Slot Info: Location and Item id count mismatch, cannot overwrite items.");
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
                            DarkSoulsItem repitem = item;
                            if (item.Category == Enums.DSItemCategory.AnyWeapon)
                            {
                                Log.Logger.Verbose($"Attempting to upgrade item: {App.Client.CurrentSession.ConnectionInfo.Slot}:{lot.Id} ({item.Name})");
                                if (App.DSOptions.UpgradedWeaponsPercentage > 0
                                    && slotLocToItemUpgMap.TryGetValue($"{App.Client.CurrentSession.ConnectionInfo.Slot}:{lot.Id}", out var itemupg))
                                {
                                    if (itemupg.Item1 == item.ApId) // if item apid matches
                                        repitem = UpgradeItem(repitem, itemupg.Item2);
                                    else
                                    {
                                        Log.Logger.Error($"Item upgrade error: '{itemupg.Item1}' != '{item.ApId}', for item {item.Name} at {lot.Name}.");
                                        App.Client.AddOverlayMessage($"Item upgrade error: '{itemupg.Item1}' != '{item.ApId}', for item {item.Name} at {lot.Name}.");
                                    }
                                }
                            }

                            Log.Logger.Verbose($"Item {i} at location id{locId}/flag={lot.Flag} ({lot.Name}) is {target}/{repitem.Id}({repitem.Name})");
                            ItemLotItem newitem = new ItemLotItem
                            {
                                CumulateLotPoint = 0,
                                CumulateReset = false,
                                EnableLuck = false,
                                GetItemFlagId = -1,
                                LotItemBasePoint = 100,
                                LotItemCategory = (int)repitem.Category,
                                LotItemNum = 1,
                                LotItemId = repitem.Id
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
        /// <summary>
        /// Build a mapping of the slot:locationid key to itemid:upg value based on info stored in slotdata from the server.
        /// </summary>
        /// <details>
        /// This is used later when replacing or receiving items - to know if they should be ugpraded.
        /// This is needed because we don't have an individual ApId per possible upgrade, but it is deterministic from the generate.
        /// </details>
        /// <returns></returns>
        public static Dictionary<string, Tuple<int, string>> BuildSlotLocationToItemUpgMap(Dictionary<string, object> slotData, int currentSlot)
        {
            Dictionary<string, Tuple<int, string>> result = [];
            
            if (App.DSOptions.ApworldCompare("0.0.20.0") < 0) /* apworld is < 0.0.20.0, which introduces weapon upgrades */
            {
                Log.Logger.Warning($"Apworld version too low, skipping weapon upgrade mapping.");
                return result;
            }

            if (App.DSOptions.UpgradedWeaponsPercentage == 0)
            {
                Log.Logger.Information($"Upgraded weapon percentage detected as 0, skipping weapon upgrades.");
                return result;
            }

            /* Get itemsAddress, itemsId, and itemsUpgrades into lists */
            List<string> itemsAddress = new List<string?>();
            List<int?> itemsId = new List<int?>();
            List<string?> itemsUpgrades = new List<string?>();

            try
            {
                if (slotData.TryGetValue("itemsAddress", out object itemsAddress_temp))
                {
                    itemsAddress.AddRange(JsonSerializer.Deserialize<string?[]>(itemsAddress_temp.ToString()));
                    if (slotData.TryGetValue("itemsId", out object itemsId_temp))
                    {
                        itemsId.AddRange(JsonSerializer.Deserialize<int?[]>(itemsId_temp.ToString()));
                        if (slotData.TryGetValue("itemsUpgrades", out object itemsUpgrades_temp))
                        {
                            itemsUpgrades.AddRange(JsonSerializer.Deserialize<string[]>(itemsUpgrades_temp.ToString()));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error($"exception creating upg map: {e.Message} {e.ToString()}");
            }

            if (itemsAddress.Count == 0 || itemsId.Count == 0 || itemsUpgrades.Count == 0
             || itemsAddress.Count != itemsId.Count || itemsAddress.Count != itemsUpgrades.Count)
            {
                Log.Logger.Error("Cannot map item upgrades: itemsAddress, itemsId, itemsUpgrades count mismatch.");
                Log.Logger.Error($"{itemsAddress.Count},{itemsId.Count},{itemsUpgrades.Count}");
                App.Client.AddOverlayMessage("Cannot map item upgrades: itemsAddress, itemsId, itemsUpgrades count mismatch.");
                App.Client.AddOverlayMessage($"{itemsAddress.Count},{itemsId.Count},{itemsUpgrades.Count}");
            }
            else
            {
                /* Iterate over each pair of entries in the pair of lists */
                for (int i = 0; i < itemsAddress.Count; i++)
                {
                    string address = itemsAddress[i];
                    int? id = itemsId[i];
                    string? upgrade = itemsUpgrades[i];

                    /* skip it if there's no item id, no upgrade info, and it doesn't have a location address */
                    if (id.HasValue && upgrade != null && !address.EndsWith(":None")) 
                    {
                        /* Now processing each potential items with upgrades */
                        /* key = address ("1:4000") */
                        /* value = item id , upgrade (8000, "Magic:5")*/
                        result[address] = new (id.Value, upgrade);
                    }
                }
            }
            Log.Logger.Debug($"upgdict size = {result.Count}");

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
        /// <summary>
        ///  Overwrite all of the item lots at once. 
        /// </summary>
        /// <param name="itemLotIds">A map of location ids to item Lots that replace them</param>
        public static void OverwriteItemLots(Dictionary<int, ItemLot> itemLotIds)
        {
            var startAddress = GetItemLotParamOffset();
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
        public static bool SetLastBonfireToFS()
        {
            var baseCoff = GetBaseCOffset();
            if (baseCoff != 0)
            {
                var baseC = (ulong)Memory.ReadInt(baseCoff);
                if (baseC != 0)
                {
                    var lastBonfireAddress = OffsetPointer(baseC, 0xB34);
                    const int firelinkShrine_id = 1020980;
                    Memory.Write(lastBonfireAddress, firelinkShrine_id);
                    return true;
                }
            }
            return false;
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
                Log.Logger.Debug("No Last Bonfire found");
            }
            else Log.Logger.Debug($"Last bonfire was {lastBonfire.id}:{lastBonfire.name} ");
            while (true)
            {
                var currentLastBonfire = GetLastBonfire();
                if (currentLastBonfire != lastBonfire)
                {
                    Log.Logger.Debug("Last Bonfire Changed");
                    lastBonfire = currentLastBonfire;
                    action?.Invoke(currentLastBonfire);
                }
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
        public static List<DarkSoulsItem> GetConsumables()
        {
            

            var json = OpenEmbeddedResource("DSAP.Resources.Consumables.json");
            var list = System.Text.Json.JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetUpgradeMaterials()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.UpgradeMaterials.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetEmbers()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Embers.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetKeyItems()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.KeyItems.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetRings()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Rings.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetSpells()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Spells.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetShields()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Shields.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetTraps()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Traps.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetDsrEventItems()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.DsrEvents.json");
            var list = JsonSerializer.Deserialize<List<DsrEvent>>(json, GetJsonOptions());
            List<DarkSoulsItem> newlist = list.Select(x => new DarkSoulsItem()
            {
                Name = x.Itemname,
                Id = x.Itemid, // ap id of event item
                StackSize = 1,
                UpgradeType = Enums.ItemUpgrade.None,
                Category = Enums.DSItemCategory.DsrEvent,
                ApId = x.Itemid, // ap id of event item
            }
            ).ToList();
            return newlist;
        }
        public static List<EmkController> GetDsrEventEmks()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.DsrEvents.json");
            var list = JsonSerializer.Deserialize<List<DsrEvent>>(json, GetJsonOptions());
            List<EmkController> newlist = list.Select(x => new EmkController(x.Locname, x.Type,
                x.Eventid, x.Eventslot, x.Itemid)).ToList();
            return newlist;
        }
        public static List<DarkSoulsItem> GetRangedWeapons()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.RangedWeapons.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetMeleeWeapons()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.MeleeWeapons.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetArmor()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Armor.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetSpellTools()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.SpellTools.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static DarkSoulsItem UpgradeItem(DarkSoulsItem item, string itemupg, bool log = false)
        {
            if (itemupg != null)
            {
                Dictionary<String, int> infusionmap = new Dictionary<string, int>
                {
                    {"Normal", 0},
                    {"Crystal", 1},
                    {"Lightning", 2},
                    {"Raw", 3},
                    {"Magic", 4},
                    {"Enchanted", 5},
                    {"Divine", 6},
                    {"Occult", 7},
                    {"Fire", 8},
                    {"Chaos", 9},
                };

                string[] tokens = itemupg.Split(':');
                if (tokens.Count() == 2)
                {
                    var infusionMod= infusionmap[tokens[0]];
                    var lvl = Int32.Parse(tokens[1]);
                    DarkSoulsItem newitem = new DarkSoulsItem
                    {
                        Name = item.Name,
                        Id = item.Id + lvl + 100 * infusionMod,
                        StackSize = item.StackSize,
                        UpgradeType = item.UpgradeType,
                        Category = item.Category,
                        ApId = item.ApId
                    };
                    if (log)
                    {
                        Log.Logger.Information($"Upgraded item {item.Name} to {itemupg}");
                        App.Client.AddOverlayMessage($"Upgraded item {item.Name} to {itemupg}");
                    }
                        
                    
                    return newitem;
                }
            }
            Log.Logger.Error($"Error upgrading item {item.Name}");
            App.Client.AddOverlayMessage($"Error upgrading item {item.Name}");

            return item;
        }
        public static List<DarkSoulsItem> GetUsableItems()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.UsableItems.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<ItemLotFlag> GetItemLotFlags()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.ItemLots.json");
            var list = JsonSerializer.Deserialize<List<ItemLotFlag>>(json, GetJsonOptions());
            return list;
        }
        public static List<BossFlag> GetBossFlags()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.BossFlags.json");
            var list = JsonSerializer.Deserialize<List<BossFlag>>(json, GetJsonOptions());
            return list;
        }
        public static List<BonfireFlag> GetBonfireFlags()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Bonfires.json");
            var list = JsonSerializer.Deserialize<List<BonfireFlag>>(json, GetJsonOptions());
            return list;
        }
        public static List<DoorFlag> GetDoorFlags()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Doors.json");
            var list = JsonSerializer.Deserialize<List<DoorFlag>>(json, GetJsonOptions());
            return list;
        }
        public static List<FogWallFlag> GetFogWallFlags()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.DsrEvents.json");
            List <Enums.DsrEventType> fogwalltypes = [Enums.DsrEventType.FOGWALL, Enums.DsrEventType.BOSSFOGWALL, Enums.DsrEventType.EARLYFOGWALL];
            var list = JsonSerializer.Deserialize<List<DsrEvent>>(json, GetJsonOptions()).Where(x => fogwalltypes.Contains(x.Type));
            List<FogWallFlag> newlist = list.Select(x => new FogWallFlag()
            {
                Name = x.Locname,
                Id = x.Locid, // apid of event location
                Flag = x.Flag
            }
            ).ToList();
            return newlist;
        }
        public static List<EventFlag> GetMiscFlags()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.MiscFlags.json");
            var list = JsonSerializer.Deserialize<List<EventFlag>>(json, GetJsonOptions());
            return list;
        }
        public static List<LastBonfire> GetLastBonfireList()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.LastBonfire.json");
            var list = JsonSerializer.Deserialize<List<LastBonfire>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetAllItems()
        {
            var results = new List<DarkSoulsItem>();

            results = results.Concat(GetConsumables()).ToList();
            results = results.Concat(GetKeyItems()).ToList();
            results = results.Concat(GetRings()).ToList();
            results = results.Concat(GetUpgradeMaterials()).ToList();
            results = results.Concat(GetEmbers()).ToList();
            results = results.Concat(GetSpells()).ToList();
            results = results.Concat(GetShields()).ToList();
            results = results.Concat(GetRangedWeapons()).ToList();
            results = results.Concat(GetSpellTools()).ToList();
            results = results.Concat(GetUsableItems()).ToList();
            results = results.Concat(GetMeleeWeapons()).ToList();
            results = results.Concat(GetArmor()).ToList();
            results = results.Concat(GetTraps()).ToList();
            results = results.Concat(GetDsrEventItems()).ToList();

            return results;
        }
        public static ulong FlagToOffset(EventFlag flag)
        {
            var offset = GetEventFlagOffset(flag.Flag).Item1;
            return offset;
        }
        public static bool IsInGame()
        {
            if (getIngameTime() != 0)
                return true;
            return false;
        }        
        public static uint getIngameTime()
        {
            var baseB = GetBaseBAddress();
            if (baseB != 0)
            {
                var next = OffsetPointer(baseB, 0xA4);
                return Memory.ReadUInt(next);
            }
            return 0; 
        }
        public static PositionData GetPosition()
        {
            Log.Logger.Debug("Getting position");
            var PositionData = new PositionData();
            if (IsInGame())
            {
                // map = worldnumber + area number. e.g. 10 + 02 => m10_02 = firelink shrine
                ulong eoffset = GetBaseEAddress();
                if (eoffset != 0)
                {
                    uint worldnumber = GetWorldNumber();
                    uint areanumber = GetAreaNumber();
                    Log.Logger.Debug($"Position update: got w/a {worldnumber} {areanumber}");
                    if (worldnumber > 9 && worldnumber < 19 && areanumber >= 0 && areanumber < 3)
                    {
                        PositionData.MapId = (int)(1000000 * worldnumber + 10000 * areanumber);
                        Log.Logger.Debug($"Got position: {PositionData.MapId}");
                        return PositionData;
                    }
                }
            }
            PositionData.MapId = App.Client.GPSHandler?.MapId ?? 0;
            Log.Logger.Debug($"Got position: {PositionData.MapId} (no update)");
            return PositionData;
        }
        public static uint GetWorldNumber(ulong eOffset = 0) // E + A23
        {
            if (eOffset == 0)
                eOffset = GetBaseEAddress();
            if (eOffset != 0)
            {
                var next = OffsetPointer(eOffset, 0xA23);
                return Memory.ReadByte(next);
            }
            return 0;
        }
        public static uint GetAreaNumber(ulong eOffset = 0) // E + A22
        {
            if (eOffset == 0)
                eOffset = GetBaseEAddress();
            if (eOffset != 0)
            {
                var next = OffsetPointer(eOffset, 0xA22);
                return Memory.ReadByte(next);
            }
            return 0;
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
            var list = JsonSerializer.Deserialize<List<Boss>>(json);
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
        protected internal static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions();
        }

        static HashSet<Tuple<int, int>> PrevEvents = [];
        static ulong prevEventHeadPtr = ulong.MinValue;
        static ulong prevEventHead = ulong.MinValue;
        internal static void CheckEventsList()
        {
            HashSet<Tuple<int, int>> ExistingEvents = [];

            ulong eventhead_ptr = GetEmkHeadAddress();
            if (eventhead_ptr != prevEventHeadPtr)
                Log.Logger.Debug($"eventheadptr changed from {prevEventHeadPtr.ToString("X")} to {eventhead_ptr.ToString("X")}");
            ulong eventhead = Memory.ReadULong((ulong)eventhead_ptr);
            if (eventhead != prevEventHead)
                Log.Logger.Debug($"eventhead changed from {prevEventHead.ToString("X")} to {eventhead.ToString("X")}");
            prevEventHead = eventhead;
            prevEventHeadPtr = eventhead_ptr;

            // read every event into the hashset
            // detect number of differences
            int numevents = 0;
            for (ulong thisEmk = eventhead; thisEmk != 0; thisEmk = Memory.ReadULong((ulong)thisEmk + 0x68))
            {

                numevents++;
                if (numevents > 2000)
                {
                    Log.Logger.Warning($"c events:{numevents}");
                    Log.Logger.Warning($"thisemk = :{thisEmk.ToString("X")}");
                    break;
                }

                int eventid = Memory.ReadInt(thisEmk + 0x30);
                int eventslot = Memory.ReadByte(thisEmk + 0x34);

                ExistingEvents.Add(new Tuple<int,int>(eventid, eventslot));
            }

            if (!ExistingEvents.SetEquals(PrevEvents))
            {
                bool printedfirst = false;
                int eventsRemoved = 0;
                Tuple<int, int> lastemk = new Tuple<int, int>(0,0);
                foreach (Tuple<int,int> emk in PrevEvents)
                {
                    if (!ExistingEvents.Contains(emk))
                    {
                        eventsRemoved++;
                        if (!printedfirst)
                        {
                            Log.Logger.Debug($"(F) - {emk.Item1}:{emk.Item2}");
                            printedfirst = true;
                        }
                        lastemk = emk;
                    }
                }
                if (eventsRemoved > 1)
                {
                    Log.Logger.Debug($"(L) - {lastemk.Item1}:{lastemk.Item2}");
                }
                printedfirst = false;
                int eventsAdded = 0;
                lastemk = new Tuple<int, int>(0, 0);
                foreach (Tuple<int, int> emk in ExistingEvents)
                {
                    if (!PrevEvents.Contains(emk))
                    {
                        eventsAdded++;
                        if (!printedfirst)
                        {
                            Log.Logger.Debug($"(F) + {emk.Item1}:{emk.Item2}");
                            printedfirst = true;
                        }
                        lastemk = emk;
                    }
                }
                if (eventsAdded > 1)
                {
                    Log.Logger.Debug($"(L) + {lastemk.Item1}:{lastemk.Item2}");
                }
                Log.Logger.Debug($"Events from {PrevEvents.Count} to {ExistingEvents.Count} > - {eventsRemoved} + {eventsAdded}");
                
                PrevEvents = new HashSet<Tuple<int, int>>(ExistingEvents);
            }
        }

        static uint cached_mapid3;
        internal static void ManageEventsList(List<EmkController> emkControllers)
        {
            try
            {
                Log.Logger.Verbose("running eventlist");

                if (emkControllers.Count == 0)
                {
                    return;
                }
                if (!IsInGame())
                    return;
                Dictionary<Tuple<int, int>, EmkController> emkdict = [];
                List<EmkController> addingEmks = [];

                foreach (EmkController emk in emkControllers)
                {
                    /* If player doesn't have key, or we've "saved ptr", examine it in list */
                    if (!emk.HasKey || emk.Saved_Ptr != 0)
                    {
                        emkdict[new Tuple<int, int>(emk.Eventid, emk.Eventslot)] = emk;
                    }
                }

                uint mapid3 = 0; // 3 digit map code
                uint wnum = GetWorldNumber();
                uint anum = GetAreaNumber();
                if (wnum > 0)
                    mapid3 = 10 * wnum + anum;
                if (mapid3 != cached_mapid3)
                    Log.Logger.Verbose($"mapid={mapid3}, w={wnum}, a={anum}");
                cached_mapid3 = mapid3;

                ulong eventhead_ptr = GetEmkHeadAddress();
                if (eventhead_ptr == 0)
                {
                    Log.Logger.Verbose($"eventheadptr is null");
                    ReleaseEvents(emkControllers);
                    return;
                }
                ulong eventhead = Memory.ReadULong((ulong)eventhead_ptr);
                if (eventhead == 0)
                {
                    Log.Logger.Verbose($"eventhead is null");
                    ReleaseEvents(emkControllers);
                    return;
                }

                if (emkdict.Count != 0)
                {
                    //Log.Logger.Information("Emks found, Managing event list");

                    // prevptr is address of a ptr to the "current emk"
                    // When "current emk" is pulled off list, it 
                    ulong prevptr = eventhead_ptr;
                    int numevents = 0;
                    // check every event for if it is in the list
                    for (ulong thisEmk = eventhead; thisEmk != 0; thisEmk = Memory.ReadULong((ulong)thisEmk + 0x68))
                    {
                        bool updatedEmk = true;
                        while (thisEmk != null && updatedEmk) // loop as long as we are pulling off events
                        {
                            updatedEmk = false;
                            numevents++;
                            if (numevents > 2000) // sanity check
                            {
                                Log.Logger.Warning($"m events:{numevents}");
                                Log.Logger.Warning($"thisemk = :{thisEmk.ToString("X")}");
                                break;
                            }

                            int eventid = Memory.ReadInt(thisEmk + 0x30);
                            int eventslot = Memory.ReadByte(thisEmk + 0x34);
                            var t = new Tuple<int, int>(eventid, eventslot);
                            if (emkdict.ContainsKey(t))
                            {
                                EmkController emk = emkdict[t];
                                if (!emk.HasKey) /* Player doesn't have key -> pull it */
                                {
                                    // Only pull it if we're in the relevant map. This is to do less "pulls" in general!
                                    if (emk.MapId3 == mapid3) /* Compare current mapid to event's valid mapid */
                                    {
                                        ulong nextptr = Memory.ReadULong(thisEmk + 0x68);
                                        Memory.Write(prevptr, nextptr);
                                        emk.Saved_Ptr = thisEmk;
                                        emk.Deactivated = true;
                                        thisEmk = nextptr;
                                        updatedEmk = true;
                                        Log.Logger.Debug($"Pulled event: {emk.Name} at {emk.Saved_Ptr.ToString("X")}");
                                    }
                                }
                                else /* Player has event's key, but we found it in list? Destroy our "old" version, and stop interfering. */
                                {
                                    emk.Saved_Ptr = 0;
                                    emk.Deactivated = false;
                                    Log.Logger.Debug($"Un-pulled event: {emk.Name} at {emk.Saved_Ptr.ToString("X")}");
                                }
                            }
                        }
                        if (thisEmk == null) // reached end of list
                            break;
                        prevptr = thisEmk + 0x68; // save address of previous node's last spot when we move on.
                    }
                }

                foreach (EmkController emk in emkControllers)
                {
                    /* If we have a saved ptr that we need to re-insert */
                    if (emk.HasKey && emk.Saved_Ptr != 0)
                    {
                        /* If we're in the map for the event */
                        if (emk.MapId3 == mapid3) /* Compare current mapid to event's valid mapid */
                        {
                            addingEmks.Add(emk);
                            Log.Logger.Debug($"Re-adding event: {emk.Name} at {emk.Saved_Ptr.ToString("X")}");
                        }
                    }
                }

                if (addingEmks.Count > 0)
                {
                    var firstEmk = addingEmks.First();
                    var lastEmk = addingEmks.Last();

                    if (addingEmks.Count > 1)
                    {
                        // point our saved events each to the next one in sequence, creating a "saved event sequence"
                        for (var i = 0; i < addingEmks.Count - 1; i++)
                        {
                            Memory.Write(addingEmks[i].Saved_Ptr + 0x68, addingEmks[i + 1].Saved_Ptr);
                        }
                    }
                    eventhead_ptr = GetEmkHeadAddress();
                    if (eventhead_ptr == 0)
                    {
                        Log.Logger.Warning($"eventheadptr is null");
                        ReleaseEvents(emkControllers);
                        return;
                    }
                    eventhead = Memory.ReadULong((ulong)eventhead_ptr);
                    if (eventhead == 0)
                    {
                        Log.Logger.Warning($"eventhead is null");
                        ReleaseEvents(emkControllers);
                        return;
                    }
                    // point last saved event to event head
                    Memory.Write(lastEmk.Saved_Ptr + 0x68, eventhead);
                    // make our first saved event the head of the event list.
                    Memory.Write(eventhead_ptr, firstEmk.Saved_Ptr);

                    // clear all the event saved ptrs
                    foreach (var emk in addingEmks)
                    {
                        emk.Deactivated = false;
                        emk.Saved_Ptr = 0;
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Exception in manageevents: {ex.Message}\n{ex.InnerException}\n{ex.Source}");
            }
        }
        // Build a list of event controllers, which we use to lock events until player has received the items.
        // We only add events to the list if their items are in the multiworld.
        internal static List<EmkController> BuildEmkControllers(Dictionary<string, object> slotData)
        {
            List<EmkController> result = [];

            if (App.DSOptions.ApworldCompare("0.0.20.1") < 0) /* apworld is < 0.0.20.1, which introduces events */
            {
                Log.Logger.Warning($"Apworld version too low, skipping fog wall lock processing.");
                return result;
            }

            List<int?> itemsId = [];
            try
            {
                if (slotData.TryGetValue("itemsId", out object itemsId_temp))
                {
                    itemsId.AddRange(JsonSerializer.Deserialize<int?[]>(itemsId_temp.ToString()));
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error($"exception creating fog map: {e.Message} {e.ToString()}");
            }
            var events = GetDsrEventEmks();
            foreach (var item in itemsId)
            {
                EmkController? newemk = events.Find(x => x.ApId == item);
                if (newemk != null)
                {
                    Log.Logger.Debug($"Adding {newemk.Name} to list. Id:slot={newemk.Eventid}:{newemk.Eventslot}");
                    result.Add(newemk);
                }
            }

            return result;
        }
        // Clear the saved ptrs of our list of "EmkControllers", because we detected there being no events in the list.
        static void ReleaseEvents(List<EmkController> emkControllers)
        {
            int num_released = 0;
            foreach (var controller in emkControllers)
            {
                if (controller.Saved_Ptr != 0 || controller.Deactivated == true)
                {
                    controller.Saved_Ptr = 0;
                    controller.Deactivated = false;
                }
            }
            if (num_released > 0)
                Log.Logger.Debug($"Released all emks, {num_released} controllers affected.");
        }
    }
}

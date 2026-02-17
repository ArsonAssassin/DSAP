using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Archipelago.Core.Util.GPS;
using Archipelago.Core.Util.Hook;
using Archipelago.MultiClient.Net.Models;
using DSAP.Models;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Location = Archipelago.Core.Models.Location;
namespace DSAP
{
    public class Helpers
    {
        private static readonly object _memAllocLock = new object();
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
        private static AoBHelper SoloParamAob = new AoBHelper("SoloParam",
                [ 0x4C, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x48, 0x63, 0xC9, 0x48, 0x8D, 0x04, 0xC9 ],
                "xxx????xxxxxxx", 3, 4);
        
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
            var foo = SoloParamAob.Address;
            Log.Logger.Verbose($"solo param location {foo.ToString("X")}");
            var next = OffsetPointer(((ulong)foo), 0x570);
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
        private static List<ILocation> CachedItemLotLocations = null;
        public static List<ILocation> GetItemLotLocations()
        {
            if (CachedItemLotLocations != null)
                return CachedItemLotLocations;

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
            CachedItemLotLocations = locations;
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
                Id = x.Dsrid, // dsr id of event item
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
            byte[] x = new byte[] {
                0x41, 0xb9, 0x00, 0x00, 0x00, 0x00,       //mov         r9d,0x0          // amount
                0x41, 0xb8, 0x00, 0x00, 0x00, 0x00,       //mov         r8d,0x0          // itemid
                0xba, 0x00, 0x00, 0x00, 0x00,             //mov         edx,0x0          // category
                0x48, 0xa1, 0x30, 0xa5, 0xc8, 0x41, 0x01, //movabs      rax,[0x141c8a530]
                0x00, 0x00, 0x00,
                0x48, 0x85, 0xc0,                         //TEST        RAX,RAX
                0x74, 0x31,                               //JZ          0x31  (BadRaX)
                0x4c, 0x8b, 0x78, 0x10,                   //mov         r15,[rax+0x10]
                0x49, 0x8d, 0x8f, 0x80, 0x02, 0x00, 0x00, //lea         rcx,[r15+0x280]
                0x48, 0x83, 0xec, 0x38,                   //sub         rsp,0x38
                0x49, 0xbe, 0xe0, 0x79, 0x74, 0x40, 0x01, //movabs      r14,0x1407479E0  // addItemToInventory()
                0x00, 0x00, 0x00,
                0x41, 0xff, 0xd6,                         //call        r14
                0x48, 0x83, 0xc4, 0x38,                   //add         rsp,0x38

                0x48, 0xb8,                               //movabs rax,0x1234567812345678 // replace with resultArea
                0x78, 0x56, 0x34, 0x12,  // target operand -> result area (8 bytes)
                0x78, 0x56, 0x34, 0x12,
                0xc7, 0x00, 0x00, 0x00, 0x00, 0x00,        //mov DWORD PTR[rax],0x00000000
                0xc3,                                     //RET 
                // BadRAX:
                0x48, 0xb8,                               //movabs rax,0x1234567812345678 // replace with resultArea
                0x78, 0x56, 0x34, 0x12,  // target operand -> result area (8 bytes)
                0x78, 0x56, 0x34, 0x12,
                0xc7, 0x00, 0xff, 0xff, 0xff, 0xff,        //mov DWORD PTR[rax],0xffffffff
                0xc3,                                     //RET 

            };
            return x;
        }


        public static byte[] GetItemWithMessageCommand()
        {
            // could use additional validation
            // - check 0x141c891a8 / ItemGetMenuManImpl before the addItemToInventory,
            // - check 0x141c8a530 / GameDataMan for null before it is dereferenced
            byte[] x = new byte[] {
                0x41, 0xb9, 0x00, 0x00, 0x00, 0x00,       //mov         r9d,0x0          // amount
                0x41, 0xb8, 0x00, 0x00, 0x00, 0x00,       //mov         r8d,0x0          // itemid
                0xba, 0x00, 0x00, 0x00, 0x00,             //mov         edx,0x0          // category
                0x48, 0xa1, 0x30, 0xa5, 0xc8, 0x41, 0x01, //movabs      rax,[0x141c8a530]
                0x00, 0x00, 0x00,
                0x48, 0x85, 0xc0,                         //TEST        RAX,RAX
                0x0f, 0x84, 0xda, 0x00, 0x00, 0x00,       //JZ          +218 / 0xDA (BadRAX)
                0x4c, 0x8b, 0x78, 0x10,                   //mov         r15,[rax+0x10]
                0x49, 0x8d, 0x8f, 0x80, 0x02, 0x00, 0x00, //lea         rcx,[r15+0x280]
                0x48, 0x83, 0xec, 0x38,                   //sub         rsp,0x38
                0x49, 0xbe, 0xe0, 0x79, 0x74, 0x40, 0x01, //movabs      r14,0x1407479E0  // addItemToInventory()
                0x00, 0x00, 0x00,
                0x41, 0xff, 0xd6,                         //call        r14
                0x48, 0x83, 0xc4, 0x38,                   //add         rsp,0x38

                0x41, 0xb9, 0x00, 0x00, 0x00, 0x00,       //mov         r9d,0x0          // amount
                0x41, 0xb8, 0x00, 0x00, 0x00, 0x00,       //mov         r8d,0x0          // itemid
                0xba, 0x00, 0x00, 0x00, 0x00,             //mov         edx,0x0          // category
                0x48, 0xb9, 0xa8, 0x91, 0xc8, 0x41, 0x01, //movabs      rcx,0x141c891a8 // ItemGetMenuMan 
                0x00, 0x00, 0x00,
                0x48, 0x8b, 0x09,                         //mov         rcx,QWORD PTR [rcx]
                0x48, 0x83, 0xec, 0x64,                   //sub         rsp,0x64

                0x40, 0x53,                               //PUSH        RBX
                0x4c, 0x8b, 0xd9,                         //MOV         R11,RCX
                0x33, 0xc0,                               //XOR         EAX,EAX
                0x48, 0x85, 0xc9,                         //TEST        RCX,RCX
                0x0f, 0x84, 0x99, 0x00, 0x00, 0x00,       //JZ          +153 / 0x99 (BadRCX)
                0x48, 0x8b, 0x49, 0x10,                   //MOV         RCX,qword ptr [RCX + 0x10]
                0x8b, 0xda,                               //MOV         EBX,EDX
                0x48, 0x85, 0xc9,                         //TEST        RCX,RCX
                0x74, 0x0c,                               //JZ          0x0c
                0x4c, 0x8b, 0x11,                         //MOV         R10,qword ptr [RCX]
                0x48, 0x8b, 0xc1,                         //MOV         RAX,RCX
                0x4d, 0x89, 0x53, 0x10,                   //MOV         qword ptr [R11 + 0x10],R10
                0xeb, 0x34,                               //JMP         0x34
                0x49, 0x8b, 0x4b, 0x08,                   //MOV         RCX,qword ptr [R11 + 0x8]
                0x48, 0x85, 0xc9,                         //TEST        RCX,RCX
                0x74, 0x42,                               //JZ          0x42
                0x48, 0x8b, 0xc1,                         //MOV         RAX,RCX
                0x48, 0x8b, 0x09,                         //MOV         RCX,qword ptr [RCX]
                0x48, 0x85, 0xc9,                         //TEST        RCX,RCX
                0x74, 0x18,                               //JZ          0x18
                0x48, 0x8b, 0xd0,                         //MOV         RDX,RAX
                0x48, 0x8b, 0xc1,                         //MOV         RAX,RCX
                0x48, 0x8b, 0x09,                         //MOV         RCX,qword ptr [RCX]
                0x48, 0x85, 0xc9,                         //TEST        RCX,RCX
                0x75, 0xf2,                               //JNZ         0xf2
                0x48, 0x85, 0xd2,                         //TEST        RDX,RDX
                0x74, 0x05,                               //JZ          0x8
                0x48, 0x89, 0x0a,                         //MOV         qword ptr [RDX],RCX
                0xeb, 0x08,                               //JMP         0x8
                0x49, 0xc7, 0x43,                         //MOV         qword ptr [R11 + 0x8],0x0
                0x08, 0x00, 0x00,
                0x00, 0x00,
                0x48, 0x85, 0xc0,                         //TEST        RAX,RAX
                0x74, 0x12,                               //JZ          0x12
                0x48, 0xc7, 0x00,                         //MOV         qword ptr [RAX],0x0
                0x00, 0x00, 0x00, 0x00,
                0x89, 0x58, 0x08,                         //MOV         dword ptr [RAX + 0x8],EBX
                0x44, 0x89, 0x40, 0x0c,                   //MOV         dword ptr [RAX + 0xc],R8D
                0x44, 0x89, 0x48, 0x10,                   //MOV         dword ptr [RAX + 0x10],R9D
                0x49, 0x8b, 0x4b, 0x08,                   //MOV         RCX,qword ptr [R11 + 0x8]
                0x48, 0x89, 0x08,                         //MOV         qword ptr [RAX],RCX
                0xb9, 0x2c, 0x01,                         //MOV         ECX,0x12c
                0x00, 0x00,
                0x49, 0x89, 0x43, 0x08,                   //MOV         qword ptr [R11 + 0x8],RAX
                0x5b,                                     //POP         RBX
                
                0x48, 0x83, 0xc4, 0x64,                   //add         rsp,0x64
                0x48, 0xb8,                               //movabs rax,0x1234567812345678
                0x78, 0x56, 0x34, 0x12,
                0x78, 0x56, 0x34, 0x12,
                0xc7, 0x00, 0x00, 0x00, 0x00, 0x00,       //mov DWORD PTR[rax],0x00000000
                0xc3,                                     //RET 
                // BadRAX:
                0x48, 0xb8,                               //movabs rax,0x1234567812345678 // replace with resultArea
                0x78, 0x56, 0x34, 0x12,  // target operand -> result area (8 bytes)
                0x78, 0x56, 0x34, 0x12,
                0xc7, 0x00, 0xff, 0xff, 0xff, 0xff,        //mov DWORD PTR[rax],0xffffffff
                0xc3,                                     //RET 
                // BadRCX:
                0x5b,                                     //POP         RBX
                0x48, 0x83, 0xc4, 0x64,                   //add         rsp,0x64
                0x48, 0xb8,                               //movabs rax,0x1234567812345678
                0x78, 0x56, 0x34, 0x12,
                0x78, 0x56, 0x34, 0x12,
                0xc7, 0x00, 0x00, 0x00, 0x00, 0x00,       //mov DWORD PTR[rax],0x00000000
                0xc3,                                     //RET 

            };
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
                Log.Logger.Verbose($"running eventlist for {emkControllers.Count} emks");

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

            if (App.DSOptions.ApworldCompare("0.0.21.0") < 0) /* apworld is < 0.0.21.0, which introduces events */
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
                    Log.Logger.Verbose($"Adding {newemk.Name} to list. Id:slot={newemk.Eventid}:{newemk.Eventslot}");
                    result.Add(newemk);
                }
            }

            return result;
        }
        // Clear the saved ptrs of our list of "EmkControllers", because we detected there being no events in the list.
        public static void ReleaseEvents(List<EmkController> emkControllers)
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

        // Eventflag   Offset
        // 960-967   = 123
        // 968-975   = 122
        // 976-983   = 121
        // 984-991   = 120
        // 992-999   = 127
        // 1000-1007 = 131
        // 1008-1015 = 130
        // 1016-1023 = 129
        // 1024-1031 = 128
        // -> 3 bytes free, offset 124-126. Use [960]+1-2 for seed hash, [960]+3 for SaveId.
        // This gap happens again every 1000 flags (until 9k), for each map's flags, in each category of flags
        // -> use [1960]+1-3 for slot id
        private static ulong GetSaveIdAddress()
        {
            var initoff = Helpers.GetEventFlagsOffset();
            int flag = 960;
            var off = Helpers.GetEventFlagOffset(flag).Item1 + 3; // 3rd byte after this one
            // here we have 3 bytes of memory available.
            Log.Logger.Debug($"saveid address = {(off + initoff).ToString("X")}");
            return off + initoff;
        }
        private static ulong GetSaveSeedAddress()
        {
            var initoff = Helpers.GetEventFlagsOffset();
            int flag = 960;
            var off = Helpers.GetEventFlagOffset(flag).Item1 + 1; // 1st and 2nd byte after this one
            // here we have 3 bytes of memory available.
            Log.Logger.Debug($"Seed address = {(off + initoff).ToString("X")}");
            return off + initoff;
        }
        private static ulong GetSaveSlotAddress()
        {
            var initoff = Helpers.GetEventFlagsOffset();
            int flag = 1960;
            var off = Helpers.GetEventFlagOffset(flag).Item1 + 1; // Up to 3 bytes
            // here we have 3 bytes of memory available.
            Log.Logger.Debug($"Slot address = {(off + initoff).ToString("X")}");
            return off + initoff;
        }
        internal static byte GetSavedSaveId()
        {
            ulong address = GetSaveIdAddress();
            return Memory.ReadByte(address);
        }

        internal static void SetSavedSaveId(byte newsaveid)
        {
            ulong address = GetSaveIdAddress();
            Memory.Write(address, newsaveid);
        }
        internal static ushort GetSavedSeedHash()
        {
            ulong address = GetSaveSeedAddress();
            return Memory.ReadUShort(address);
        }
        internal static void SetSavedSeedHash(ushort seedhash)
        {
            ulong address = GetSaveSeedAddress();
            Memory.Write(address, seedhash);
        }
        internal static ushort HashSeed(string seed)
        {
            uint result = 31719121;
            foreach (char c in seed)
            {
                result ^= c;
                result = UInt32.RotateLeft(result, 11);
            }
            return (ushort)(result % 65000 + 1); // ensure it is a short, but non-zero.
        }
        internal static ushort GetSavedSlot()
        {
            ulong address = GetSaveSlotAddress();
            return Memory.ReadUShort(address);
        }
        internal static void SetSavedSlot(ushort slot)
        {
            ulong address = GetSaveSlotAddress();
            Memory.Write(address, slot);
        }
        public static void ListItemLots()
        {
            var startAddress = GetItemLotParamOffset();
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
            var startAddress = GetItemLotParamOffset();

            var dataOffset = Memory.ReadUInt(startAddress + 0x4);
            var rowCount = Memory.ReadUShort(startAddress + 0xA);
            var rowSize = 148;

            var paramTableBytes = Memory.ReadByteArray(startAddress + (ulong)(12 * rowCount), 0x30);
            var itemlotflag = GetItemLotFlags().Find(x => x.Name.ToLower().Contains("well"));

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
                var itemLot = ReadItemLot(currentAddress);
                if (itemLot.GetItemFlagId == itemlotflag.Flag)
                {
                    OverwriteSingleItem(currentAddress, experimentalLotItem, 0);
                }
            }
            Location loc = (Location)Helpers.GetItemLotLocations().Find(x => x.Name.ToLower().Contains("well"));
            SetEventFlag(itemlotflag.Flag, 0);
            return;
        }
        public static void ChangePrismStoneText()
        {
            var item = GetAllItems().Find(x => x.Name.ToLower().Contains("prism stone"));
            uint itemid = (uint)item.Id;
            //uint itemid = 9014;

            ulong MsgMan = Memory.ReadULong(0x141c7e3e8);
            ulong GoodsMsgsStart = Memory.ReadULong(MsgMan + 0x380);
            ulong GoodsCaptionMsgsStart = Memory.ReadULong(MsgMan + 0x378);
            ulong GoodsInfoMsgStart = Memory.ReadULong(MsgMan + 0x328);
            ulong itemNameStrLoc = FindMsg(GoodsMsgsStart, itemid);
            ulong itemCaptionStrLoc = FindMsg(GoodsCaptionMsgsStart, itemid);
            ulong itemInfoStrLoc = FindMsg(GoodsInfoMsgStart, itemid);

            UpdateItemText(itemNameStrLoc, 100, "AP Item\0");
            UpdateItemText(itemCaptionStrLoc, 100, "This is an item that belongs to another world...\0");
            UpdateItemText(itemInfoStrLoc, 500, "*narrator voice* We're not sure how this got here. \nBest hold on to it. \n\nJust in case.\0");

            ulong equipGoodsParamResCap = Memory.ReadULong((ulong)(SoloParamAob.Address + 0xF0));
            //upgradeGoods(equipGoodsParamResCap);
            //AddMsgs(9015, new List<string>() { "AP Item From Player 2's world" });
            return;
        }

        private static bool upgradeGoods(List<KeyValuePair<long, string>> addedEntries)
        {
            ulong resCap = Memory.ReadULong((ulong)(SoloParamAob.Address + 0xF0));
            uint old_buffer_size = Memory.ReadUInt(resCap + 0x30);
            ulong old_buffer = Memory.ReadULong(resCap + 0x38);
            ushort old_buffer_num_entries = Memory.ReadUShort(old_buffer + 0xA);

            /* first, read highest numbered param in list */
            uint highest_id = Memory.ReadUInt(old_buffer + (ulong)(0x30 + ((old_buffer_num_entries - 1) * 0xc)));
            if (addedEntries.First().Key <= highest_id)
            {
                Log.Logger.Warning($"Warning: Highest id in params detected as {highest_id}, >= one of our entries.");
                Log.Logger.Warning($"Checking if params and msgs have already been updated...");
                
                uint old_end_buffer_offset = old_buffer_size + (uint)(0x8 * old_buffer_num_entries) + 0x10 + 0xf;
                ulong old_desc_area_loc = old_buffer + old_end_buffer_offset;
                DescArea old_desc_area = Memory.ReadObject<DescArea>(old_desc_area_loc);
                Log.Logger.Debug("Read object: " + old_desc_area.ToString());
                bool requires_reload = ValidateDescArea(old_desc_area);
                if (requires_reload)
                {
                    //int intermediate_buffer_size = old_desc_area.FullAllocLength;
                    ulong intermediate_buffer_loc = old_buffer;
                    // desc size 4, full alloc length 4, old address 8, old length 4, seed hash 4, slot 4
                    // reset old buffer values
                    old_buffer = old_desc_area.OldAddress;
                    old_buffer_size = (uint)old_desc_area.OldLength;
                    old_buffer_num_entries = Memory.ReadUShort(old_buffer + 0xA);

                    /* Switch out the pointer so deallocated area isn't accessed by the game */
                    Memory.Write(resCap + 0x30, old_buffer_size);
                    Memory.Write(resCap + 0x38, old_buffer);

                    // dealloc previously swapped-in area
                    Memory.FreeMemory((nint)(intermediate_buffer_loc - 0x10));
                    Log.Logger.Warning("Reloading EquipGoodsParams");
                }
                else
                {
                    Log.Logger.Information("EquipGoodsParams replacement not needed - skipping");
                    return false;
                }

            }

            uint old_buffer_string_offset = Memory.ReadUInt(old_buffer + 0x0);
            ushort old_buffer_params_offset = Memory.ReadUShort(old_buffer + 0x4);

            ushort new_entries = (ushort)addedEntries.Count();

            uint goods_param_size = 0x5c;
            ushort new_buffer_num_entries = (ushort)(old_buffer_num_entries + new_entries);

            ushort new_buffer_params_offset = (ushort)(old_buffer_params_offset + (0xc * new_entries));
            uint new_buffer_string_offset = (ushort)(old_buffer_string_offset + ((0xc + goods_param_size) * new_entries));
            uint addl_str_length = (uint)addedEntries.Aggregate(0, (total, x) => total + x.Value.Length + 1);
            uint new_endtable_size = (uint)(0x8 * new_buffer_num_entries);

            uint new_buffer_size = (uint)(old_buffer_size + addl_str_length + (0xc + goods_param_size) * new_entries);
            uint new_buffer_alloc_size = (uint)(new_buffer_size + (0x8 * new_buffer_num_entries) + 0x10 + 0xf + DescArea.size); // ensure enough for the binary search table and the prologue

            ulong new_allocated_buffer = 0;
            lock (_memAllocLock)
            {
                new_allocated_buffer = (ulong)Memory.AllocateAbove(new_buffer_alloc_size);
            }
            
            ulong new_buffer = new_allocated_buffer + 0x10;
            Log.Logger.Information($"Allocated {new_buffer_alloc_size} bytes at {new_allocated_buffer.ToString("X")}");
            Log.Logger.Information($"Overwrite EquipParamGoods @ {old_buffer.ToString("X")} to {new_buffer.ToString("X")}");


            /* Then, copy the header + pointer structs */
            byte[] basebytes = Memory.ReadByteArray(old_buffer, old_buffer_params_offset);
            Memory.WriteByteArray(new_buffer, basebytes);
            /* Then, copy the params */
            uint old_buffer_params_length = (uint)(goods_param_size * old_buffer_num_entries);
            byte[] basebytes2 = Memory.ReadByteArray(old_buffer + old_buffer_params_offset, (int)old_buffer_params_length);
            Memory.WriteByteArray(new_buffer + new_buffer_params_offset, basebytes2);
            /* Then, copy the strings */
            uint old_buffer_strings_length = old_buffer_size - old_buffer_string_offset;
            byte[] basebytes3 = Memory.ReadByteArray(old_buffer + old_buffer_string_offset, (int)old_buffer_strings_length);
            Memory.WriteByteArray(new_buffer + new_buffer_string_offset, basebytes3);

            /* old buffer ends on the last string - last null terminator (shift-jis) */
            byte[] parambytes = Memory.ReadByteArray(old_buffer + old_buffer_params_offset, (int)goods_param_size);
            parambytes[0x36] = 99; // max num
            parambytes[0x3a] = 1; // goods type = key
            parambytes[0x3b] = 0; // ref category = like key
            parambytes[0x3e] = 0; // use animation = 0
            // Is Only One?
            // Is Deposit?
            uint new_string_loc = (uint)(new_buffer + new_buffer_string_offset + old_buffer_strings_length);

            // fix old entries' offsets
            for (uint i = 0; i < old_buffer_num_entries; i++)
            {
                uint currloc = (uint)(new_buffer + 0x30 + i * 0xc);
                uint poff = Memory.ReadUInt(currloc + 0x4);
                uint soff = Memory.ReadUInt(currloc + 0x8);
                poff = (uint)(poff + (0xc * new_entries));
                soff = soff + (0xc + goods_param_size) * new_entries;
                Memory.Write(currloc + 0x4, poff);
                Memory.Write(currloc + 0x8, soff);
            }

            /* then add the new pointer structs, params, and strings, and pointers to each. */
            for (uint i = 0; i < new_entries; i++)
            {
                var entry = addedEntries.ToArray()[i];
                byte[] stringbytes = Encoding.ASCII.GetBytes($"{entry.Value}\0");
                uint newid = (uint)entry.Key;
                // set sort bytes in param based on id - not sure if this is grabbing top or bottom 2 bytes!! But filling all 4 put the items at the top instead.
                byte[] idbytes = BitConverter.GetBytes(newid);
                parambytes[0x1c] = idbytes[0];
                parambytes[0x1d] = idbytes[1];
                //parambytes[0x1e] = idbytes[2];
                //parambytes[0x1f] = idbytes[3];
                byte[] iconbytes = BitConverter.GetBytes((short)2042);
                parambytes[0x2c] = iconbytes[0];
                parambytes[0x2d] = iconbytes[1];
                parambytes[0x45] |= (byte)(0x30); // turn on isDrop and isDeposit bits

                uint currloc = (uint)(new_buffer + old_buffer_params_offset + i * 0xc);
                Memory.Write(currloc + 0x0, newid);

                uint new_param_loc = (uint)(new_buffer + new_buffer_params_offset + (old_buffer_num_entries + i) * goods_param_size);
                Memory.WriteByteArray(new_param_loc, parambytes);
                Memory.Write(currloc + 0x4, new_param_loc-new_buffer);

                Memory.WriteByteArray(new_string_loc, stringbytes);
                Memory.Write(currloc + 0x8, new_string_loc - new_buffer);
                new_string_loc += (uint)stringbytes.Length;
            }
            Log.Logger.Information($"Added {new_entries} items to EquipParamGoods from {addedEntries.First().Key} to {addedEntries.Last().Key}");

            ulong post_string_loc = new_string_loc;
            ulong saved_len = post_string_loc - new_buffer;
            /* Then fix up the offsets */
            Memory.Write(new_buffer + 0x0, new_buffer_string_offset);
            Memory.Write(new_buffer + 0x4, new_buffer_params_offset);
            Memory.Write(new_buffer + 0xA, new_buffer_num_entries);

            //Memory.Write(new_allocated_buffer, new_buffer_size);
            Memory.Write(new_allocated_buffer, saved_len);

            // copy over the endtable
            ulong new_endtable_loc = new_buffer + ((saved_len + 0xf) & 0xFFFFFFFFFFFFFFF0);
            ulong old_endtable_loc = old_buffer + ((old_buffer_size + 0xf) & 0xFFFFFFFFFFFFFFF0);
            byte[] old_endtable = Memory.ReadByteArray(old_endtable_loc, 8*old_buffer_num_entries);
            Memory.WriteByteArray(new_endtable_loc, old_endtable);
            // add our new entries to the endtable (binary search table)
            for (uint i = 0; i < new_entries; i++)
            {
                var entry = addedEntries.ToArray()[i];
                uint newid = (uint)entry.Key;
                uint curr_endtable_loc = (uint)(new_endtable_loc + 8 * (i + old_buffer_num_entries));
                Memory.Write(curr_endtable_loc, newid);
                Memory.Write(curr_endtable_loc + 0x4, old_buffer_num_entries + i);
            }
            // end of data

            // add desc area to end
            uint end_buffer_offset = new_buffer_size + (uint)(0x8 * new_buffer_num_entries) + 0x10 + 0xf;
            ulong desc_area_loc = new_buffer + end_buffer_offset;

            var seedHash = HashSeed(App.Client.CurrentSession.RoomState.Seed);
            var slot = App.Client.CurrentSession.ConnectionInfo.Slot;

            var new_desc_area = new DescArea((int)new_buffer_alloc_size, old_buffer, (int)old_buffer_size, seedHash, slot);
            Memory.WriteObject<DescArea>(desc_area_loc, new_desc_area);
            // end of data + metadata


            /* Then switch out the pointer */
            Memory.Write(resCap + 0x38, new_buffer);
            Memory.Write(resCap + 0x30, saved_len);
            return true;
        }

        private static bool ValidateDescArea(DescArea descArea)
        {
            bool requires_reload = false;
            if (descArea.DescSize >= DescArea.size)
            {
                int old_slot = descArea.Slot;
                
                if (descArea.SeedHash != HashSeed(App.Client.CurrentSession.RoomState.Seed)) // different seed
                {
                    if (IsInGame())
                    {
                        App.Client.AddOverlayMessage($"Error - check the client log");
                        Log.Logger.Error("Different seed detected than your previous connection to Archipelago.");
                        Log.Logger.Error("However, you are loaded into a save. Try again while not loaded in.");
                        return false;
                    }
                    else
                    {
                        Log.Logger.Information("Different seed detected than your previous load. Resetting area");
                        return true;
                    }
                        
                    
                }
                else if (old_slot != App.Client.CurrentSession.ConnectionInfo.Slot) // different slot
                {
                    if (IsInGame())
                    {
                        App.Client.AddOverlayMessage($"Error - check the client log");
                        Log.Logger.Error("Different slotdetected than your previous connection to Archipelago.");
                        Log.Logger.Error("However, you are loaded into a save. Try again while not loaded in.");
                        return false;
                    }
                    else
                    {
                        Log.Logger.Information("Different seed detected than your previous load. Resetting area");
                        return true;
                    }   
                }
                else // seed and slot checked out fine. Looks good, no need to reload.
                {
                    return false;
                }
            }
            else // desc area too small
            {
                Log.Logger.Error("No version detected on equip goods params. A different mod is probably interfering with our items.");
                Log.Logger.Error("Try running without other mods.");
                return false;
            }
        }

        private static void AddMsgs(uint offset, List<KeyValuePair<long, string>> instrings, string msgsName)
        {
            ulong MsgMan = Memory.ReadULong(0x141c7e3e8);

            ulong old_buffer = Memory.ReadULong(MsgMan + offset);
            ulong old_buffer_size = Memory.ReadUInt(old_buffer + 0x4);
            ulong old_buffer_num_spanmaps = Memory.ReadUShort(old_buffer + 0xc);
            // structure of buffer:
            // string end/size@ 0x04
            // num of span maps 0x0c
            // # stroff entries 0x10
            // stroff table  at 0x14
            // span maps start  0x1c
            // span maps have <offset> <min> <max>. Offset is # of entries into string offset table
            // after span maps is string offset table
            // There are (number of items, = 0x110 vanilla) in string offset table
            // Then there are the strings
            // New buffer will need added:
            //  1. (optionally) +c bytes for another map,
            //  2. 0x4 bytes per added id in the string offset table
            //  3. [n] bytes for each string including null char, in Unicode
            // New buffer will need updated:
            //     (optionally) num of span maps
            //     string offset at 0x14
            //     span map entry if not adding a new one
            //     each string offset entry
            //     end of strings/size of all

            // get highest entry in span maps table
            // check its id against known ids, if higher do desc area validation
            long highest_id = Memory.ReadUInt(old_buffer + 0x1c + 0xc * (old_buffer_num_spanmaps - 1) + 0x8);
            Log.Logger.Verbose($"msgs highest id = {highest_id}");
            if (highest_id >= instrings.First().Key) // conflict
            {
                // validate desc area
                // If it's no good, reset it
                ulong desc_area_loc = old_buffer + old_buffer_size;
                DescArea old_desc_area = Memory.ReadObject<DescArea>(desc_area_loc);
                Log.Logger.Debug("Read object: " + old_desc_area.ToString() + " from " + desc_area_loc.ToString("X"));
                bool update_required = ValidateDescArea(old_desc_area);
                if (update_required)
                {
                    ulong intermediate_buffer_loc = old_buffer; // save "swapped-in area" ptr
                    // reset old buffer values
                    old_buffer = old_desc_area.OldAddress;
                    old_buffer_size = (ulong)old_desc_area.OldLength;
                    old_buffer_num_spanmaps = Memory.ReadUShort(old_buffer + 0xc);

                    /* Switch out the pointer so deallocated area isn't accessed by the game */
                    Memory.Write(MsgMan + offset, old_buffer);

                    // dealloc previously swapped-in area
                    Memory.FreeMemory((nint)(intermediate_buffer_loc));
                    Log.Logger.Information($"Reloading {msgsName} text changes");
                }
                else
                {
                    Log.Logger.Information($"{msgsName} text update not needed - skipping");
                    return;
                }
            }

            ulong old_buffer_num_stroff_entries = Memory.ReadUShort(old_buffer + 0x10);
            ulong old_buffer_stroff_start_offset = Memory.ReadUInt(old_buffer + 0x14);

            ulong new_entries = (ulong)(instrings.Last().Key - instrings.First().Key + 1);
            ulong total_String_size = 0;
            ulong new_buffer = 0;
            ulong new_buffer_stroff_start_offset = old_buffer_stroff_start_offset + 0xc;
            ulong old_buffer_string_start_offset = old_buffer_stroff_start_offset + (old_buffer_num_stroff_entries * 4);
            ulong new_buffer_string_start_offset = old_buffer_string_start_offset + 0xc + 4 * new_entries;
            foreach (var entry in instrings)
            {
                total_String_size += (ulong)Encoding.Unicode.GetBytes(entry.Value).Length;
            }
            if (new_entries < (ulong)instrings.Count)
                total_String_size += (ulong)instrings.Count - new_entries;
            //calculate size
            ulong new_buffer_size = old_buffer_size + 0xc + 0x4 * new_entries + total_String_size;
            ulong new_buffer_total_size = old_buffer_size + 0xc + 0x4 * new_entries + total_String_size + (ulong)DescArea.size;

            lock (_memAllocLock)
            {
                new_buffer = (ulong)Memory.AllocateAbove((uint)new_buffer_total_size);
            }
            //Log.Logger.Information($"Allocated {new_buffer_total_size} bytes at {new_buffer.ToString("X")}");
            Log.Logger.Information($"Updating {msgsName} text @ {old_buffer.ToString("X")} to {new_buffer.ToString("X")}");

            // first, copy over header & old maps
            byte[] basebytes = Memory.ReadByteArray(old_buffer, (int)(0x1c + old_buffer_num_spanmaps * 0xc));
            Memory.WriteByteArray(new_buffer, basebytes);
            // Then, copy over existing string offset table

            //ulong new_buffer_stroff_start_offset = old_buffer_stroff_start_offset + 0xc;
                

            byte[] basebytes2 = Memory.ReadByteArray(old_buffer + old_buffer_stroff_start_offset, (int)(old_buffer_num_stroff_entries * 4));
            Memory.WriteByteArray(new_buffer + new_buffer_stroff_start_offset, basebytes2);
            // Then, copy over existing strings
            byte[] basebytes3 = Memory.ReadByteArray(old_buffer + old_buffer_string_start_offset, (int)(old_buffer_size - old_buffer_string_start_offset));
            Memory.WriteByteArray(new_buffer + new_buffer_string_start_offset, basebytes3);

            // add new span map
            ulong new_spanmap_loc = new_buffer + old_buffer_stroff_start_offset; // old buffer stroffs started where this would
            Memory.Write(new_spanmap_loc, (int)old_buffer_num_stroff_entries); // next str off index will be the next available number (0 indexed) - aka current max
            Memory.Write(new_spanmap_loc + 0x4, (int)instrings.First().Key);
            Memory.Write(new_spanmap_loc + 0x8, (int)instrings.Last().Key);

            // Correct bad string offsets in table - increase by 0xc for the new spanmap, and 0x4 for each new string
            for (uint i = 0; i < old_buffer_num_stroff_entries; i++)
            {
                ulong stroff_loc = new_buffer + new_buffer_stroff_start_offset + 4 * i;
                uint stroff_val = Memory.ReadUInt(stroff_loc);
                if (stroff_val != 0)
                {
                    stroff_val += (uint)(0xc + (new_entries * 4));
                    Memory.Write(stroff_loc, stroff_val);
                }
            }
            // point to end of last old string
            ulong curr_end_loc = new_buffer + new_buffer_string_start_offset + (old_buffer_size - old_buffer_string_start_offset);
            ulong end_of_stroffs = new_buffer + new_buffer_stroff_start_offset + (4 * old_buffer_num_stroff_entries);
            for (uint i = 0; i < new_entries; i++)
            {
                ulong curr_stroff_loc = end_of_stroffs + 4 * i;
                Memory.Write(curr_stroff_loc, (int)(curr_end_loc - new_buffer)); // point stroff entry to string position
                                                                                    // Then write the string
                byte[] ba = Encoding.Unicode.GetBytes("\0");
                if (instrings.Any(x => x.Key == instrings.First().Key + i))
                    ba = Encoding.Unicode.GetBytes(instrings.Find(x => x.Key == instrings.First().Key + i).Value);
                Memory.WriteByteArray(curr_end_loc, ba);
                curr_end_loc += (ulong)ba.Length;
            }
            // end of data here
            // add desc area
            var seedHash = HashSeed(App.Client.CurrentSession.RoomState.Seed);
            var slot = App.Client.CurrentSession.ConnectionInfo.Slot;
            var new_desc_area = new DescArea((int)new_buffer_total_size, old_buffer, (int)old_buffer_size, seedHash, slot);
            Memory.WriteObject<DescArea>(curr_end_loc, new_desc_area);
            Log.Logger.Verbose($"new Desc Area written to {curr_end_loc.ToString("X")}");
            // end here

            // fix up header area
            Memory.Write(new_buffer + 0x4, curr_end_loc - new_buffer);
            Memory.Write(new_buffer + 0xc, old_buffer_num_spanmaps + 1);
            Memory.Write(new_buffer + 0x10, old_buffer_num_stroff_entries + new_entries);
            Memory.Write(new_buffer + 0x14, new_buffer_stroff_start_offset);
            
            /* Then switch out the pointer */
            Memory.Write(MsgMan + offset, new_buffer);
        }
        private static void UpdateItemText(ulong strloc, int len, string newstring)
        {
            if (strloc == 0)
            {
                Log.Logger.Information($"strloc = {strloc}"); return;
            }
            byte[] ba = Memory.ReadByteArray(strloc, len);
            string su16 = Encoding.Unicode.GetString(ba);
            string[] sub16 = su16.Split("\0");
            Log.Logger.Information($"Padding to {sub16[0].Length} bytes");
            int available_space = sub16[0].Length;
            string newptxt = newstring;
            if (newstring.Length > available_space) 
                newptxt = newstring.Substring(0, sub16[0].Length);

            byte[] newba = Encoding.Unicode.GetBytes(newptxt);
            Memory.WriteByteArray(strloc, newba);
            Log.Logger.Information($"String found: {su16}, \n@{strloc.ToString("X")}");
            Log.Logger.Information($"Wrote string {newptxt}");
        }

        private static ulong FindMsg(ulong MsgsStart, uint id)
        {
            ulong GoodsMsgsStrTableOffset = Memory.ReadULong(MsgsStart + 0x14);
            ushort GoodsMsgsCompareEntries = Memory.ReadUShort(MsgsStart + 0xc);
            ulong GoodsMsgsCompareStart = MsgsStart + 0x1c;
            uint compareEntrySize = 0xc;
            for (uint curridx = 0; curridx < GoodsMsgsCompareEntries; curridx++)
            {
                ulong currentry = GoodsMsgsCompareStart + (compareEntrySize * curridx);
                uint low = Memory.ReadUInt(currentry + 0x4);
                uint high = Memory.ReadUInt(currentry + 0x8);
                if (low <= id && id <= high)
                {
                    uint baseoffset = Memory.ReadUInt(currentry + 0x0);
                    uint idoffset = id - low;
                    uint strEntryOffset = 4 * (idoffset + baseoffset);

                    ulong itemstroffset = Memory.ReadUInt(MsgsStart + GoodsMsgsStrTableOffset + strEntryOffset);
                    ulong itemstrloc = MsgsStart + itemstroffset;
                    return itemstrloc;
                }
            }
            return 0;
        }
        private static void SetEventFlag(int flagnum, byte newvalue)
        {
            var baseAddress = Helpers.GetEventFlagsOffset();
            Location newloc = new Location()
            {
                Address = baseAddress + Helpers.GetEventFlagOffset(flagnum).Item1,
                AddressBit = Helpers.GetEventFlagOffset(flagnum).Item2
            };
            Memory.WriteBit(newloc.Address, newloc.AddressBit, newvalue == 0 ? false : true);
            return;
        }

        internal static async Task AddAPItems(Dictionary<long, ScoutedItemInfo> scoutedLocationInfo)
        {
            List<KeyValuePair<long, ScoutedItemInfo>> addedEntries = scoutedLocationInfo.Where((e) => e.Value.Player.Slot != App.Client.CurrentSession.ConnectionInfo.Slot).ToList();
            //addedEntries.Sort((a, b) => a.Key.CompareTo(b.Key));

            var added_names = addedEntries.Select(x => new KeyValuePair<long, string>(x.Key, $"{x.Value.Player}'s {x.Value.ItemDisplayName}\0")).ToList();
            var added_captions = addedEntries.Select(x => new KeyValuePair<long, string>(x.Key, BuildItemCaption(x))).ToList();

            var added_emk_names = GetDsrEventItems().Select(x => new KeyValuePair<long, string>(x.Id, $"{x.Name}\0"));
            var added_emk_captions = GetDsrEventItems().Select(x => new KeyValuePair<long, string>(x.Id, BuildDsrEventItemCaption()));

            added_names.AddRange(added_emk_names);
            added_captions.AddRange(added_emk_captions);

            added_names.Sort((a, b) => a.Key.CompareTo(b.Key));
            added_captions.Sort((a, b) => a.Key.CompareTo(b.Key));

            var watch = System.Diagnostics.Stopwatch.StartNew();

            // add items
            bool do_replacements = upgradeGoods(added_names);

            var tasks = new List<Task>
                {
                Task.Run(() => { AddMsgs(0x380, added_names, "Item Names"); }), // names
                Task.Run(() => { AddMsgs(0x378, added_captions, "Item Captions"); }), // captions
                Task.Run(() => { AddMsgs(0x328, added_captions, "Item Descriptions"); }), // info
                };
            await Task.WhenAll(tasks);

            watch.Stop();
            Log.Logger.Information($"Finished adding new items params + msg text, took {watch.ElapsedMilliseconds}ms");
            App.Client.AddOverlayMessage($"Finished adding new items params + msg text, took {watch.ElapsedMilliseconds}ms");

            var local_ap_keys = added_emk_names.ToList();
            local_ap_keys.Sort((a,b) =>  a.Key.CompareTo(b.Key));
            // add item removal hook. Filter only things that are remote items; min = after last fogwall/local ap key, max = last ap key
            AddAPItemHook(local_ap_keys.Last().Key + 1, added_names.Last().Key);
        }

        private static void AddAPItemHook(long min, long max)
        {
            ulong target_func_start = 0x1407479E0;
            byte[] replaced_instructions = Memory.ReadByteArray(target_func_start, 14);
            ulong replacement_func_start_addr = (ulong)Memory.Allocate(1000, Memory.PAGE_EXECUTE_READWRITE);

            var jmpstub = new byte[]
            {
                0xff, 0x25, 0x00, 0x00, 0x00, 0x00,       //jmp    QWORD PTR [rip+0x0]        # 6 <_main+0x6>
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // target address
                // then the address to jump to (8 bytes)
            };
            Array.Copy(BitConverter.GetBytes(replacement_func_start_addr), 0, jmpstub, 6, 8); // target address

            //CMP r9d,0x12345678
            //JL OVER
            //CMP r9d,0x12345678
            //JG OVER
            // RET and 5 nops (could be replaced with mov r9d,<value>)
            // OVER (label)
            // 14 nops (replaced with source 14 bytes overwritten by jmp instruction)
            //  jmp        qword[rip+0]
            // <return address>
            var new_instructions = new byte[]
            {
                0x41, 0x81, 0xf8, 0x78, 0x56, 0x34, 0x12,    // cmp r9d,0x12345678
                0x7c, 0x0f,                                  // jl     OVER
                0x41, 0x81, 0xf8, 0x78, 0x56, 0x34, 0x12,    // cmp    r9d,0x12345678
                0x7f, 0x06,                                  // jg     OVER
                0xc3, 0x90, 0x90, 0x90, 0x90, 0x90,          // ret and 5 nops
                //0x41, 0xb8, 0x72, 0x01, 0x00, 0x00,          // mov    r9d,0x172 (dec 370)
                // OVER (label)
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,    // 14 nops -> get replaced with source 14 bytes
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                0xff, 0x25, 0x00, 0x00, 0x00, 0x00,          // jmp    QWORD PTR [rip+0x8]
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // jmp's target address
            };

            Array.Copy(BitConverter.GetBytes(min), 0, new_instructions, 3, 4); // min
            Array.Copy(BitConverter.GetBytes(max), 0, new_instructions, 12, 4); // max
            Array.Copy(replaced_instructions, 0, new_instructions, 24, 14); // replaced_instructions
            Array.Copy(BitConverter.GetBytes(target_func_start + 14), 0, new_instructions, 44, 8); // target address


            Memory.WriteByteArray(replacement_func_start_addr, new_instructions); // write new instructions into its hook area
            Memory.WriteByteArray(target_func_start, jmpstub); // write jmp stub (e.g. "create hook")
        }

        internal static string BuildItemCaption(KeyValuePair<long, ScoutedItemInfo> item)
        {
            const byte progression = 0b001;
            const byte useful = 0b010;
            const byte trap = 0b100;
            string item_type = "normal";
            if (((byte)item.Value.Flags) == 0b001) item_type = "Progression";
            if (((byte)item.Value.Flags) == 0b010) item_type = "Useful";
            if (((byte)item.Value.Flags) == 0b100) item_type = "Trap";
            return $"A {item_type} Archipelago item for {item.Value.Player}'s {item.Value.ItemGame}.\0";
        }
        internal static string BuildDsrEventItemCaption()
        {
            return "A boon from another world. Makes a fog wall passable.\0";
        }

        internal static bool CanPopupItems()
        {
            ulong ItemGetMenuMan = Memory.ReadULong(0x141c891a8);
            if (ItemGetMenuMan == 0) 
                return false;

            ulong unused_node_queue = Memory.ReadULong(ItemGetMenuMan + 0x10);
            ulong valid_node_queue = Memory.ReadULong(ItemGetMenuMan + 0x8);
            if (unused_node_queue == 0 && valid_node_queue == 0)
                return false;

            return true;
        }
    }
}

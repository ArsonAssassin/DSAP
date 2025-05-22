using Archipelago.Core.Models;
using Archipelago.Core.Util;
using DSAP.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private static readonly Dictionary<string, OffsetParams> Signatures = new Dictionary<string, OffsetParams>
        {
            { "BaseA", new OffsetParams(
                new byte[] { 0x8B, 0x76, 0x0C, 0x89, 0x35, 0x00, 0x00, 0x00, 0x00, 0x33, 0xC0 },
                "xxxxx????xx",
                5, true, 7) },

            { "FrpgNetMan", new OffsetParams(
                new byte[] { 0x48, 0x83, 0x3d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8b, 0xf1 },
                "xxx????xxxx",
                3, true, 8, 0x1000000) },

            { "BaseB", new OffsetParams(
                new byte[] { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x45, 0x33, 0xED, 0x48, 0x8B, 0xF1, 0x48, 0x85, 0xC0 },
                "xxx????xxxxxxxxx",
                3, true, 7, 0x1000000) },

            { "BaseC", new OffsetParams(
                new byte[] { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x28, 0x01, 0x66, 0x0F, 0x7F, 0x80, 0x00, 0x00, 0x00, 0x00, 0xC6, 0x80 },
                "xxx????xxxxxxx??xxxx",
                3, true, 7) },

            { "BaseX", new OffsetParams(
                new byte[] { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x48, 0x39, 0x48, 0x68, 0x0f, 0x94, 0xc0, 0xc3 },
                "xxx????xxxxxxxx",
                3, true, 7) },

            { "ChrBaseClass", new OffsetParams(
                new byte[] { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x45, 0x33, 0xED, 0x48, 0x8B, 0xF1, 0x48, 0x85, 0xC0 },
                "xxx????xxxxxxxxx",
                3, true, 7) },

            { "ProgressionFlag", new OffsetParams(
                new byte[] { 0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x41, 0xB8, 0x01, 0x00, 0x00, 0x00, 0x44 },
                "xxx????xxxxxxx",
                3, true, 7) },

            { "SoloParam", new OffsetParams(
                new byte[] { 0x4C, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x48, 0x63, 0xC9, 0x48, 0x8D, 0x04, 0xC9 },
                "xxx????xxxxxxx",
                3, true, 7) },

            { "EventFlags", new OffsetParams(
                new byte[] { 0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x99, 0x33, 0xC2, 0x45, 0x33, 0xC0, 0x2B, 0xC2, 0x8D, 0x50, 0xF6 },
                "xxx????xxxxxxxxxxx",
                3, true, 7) }
        };

        public static ulong FindAddressBySignature(OffsetParams signature)
        {
            try
            {
                var baseAddress = GetBaseAddress();

                IntPtr foundAddress = Memory.FindSignature(
                    (nint)baseAddress,
                    signature.SearchSize,
                    signature.Pattern,
                    signature.Mask);

                if (foundAddress == IntPtr.Zero)
                {
                    Log.Error($"Failed to find signature pattern");
                    throw new Exception($"Failed to find signature pattern");
                }

                Log.Debug($"Found pattern at: 0x{foundAddress.ToInt64():X}");

                // Read the bytes at the pattern location to verify
                byte[] bytes = Memory.ReadByteArray((ulong)foundAddress, signature.Mask.Length);
                Log.Debug($"Bytes at pattern: {BitConverter.ToString(bytes)}");

                if (!signature.AddRelativeOffset)
                {
                    // Just return the found address
                    return (ulong)foundAddress;
                }

                // Read the offset at the specified position
                int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(foundAddress + signature.OffsetPosition), 4), 0);
                Log.Debug($"Read offset value: 0x{offset:X}");

                // Calculate the final address
                IntPtr finalAddress = new IntPtr(foundAddress.ToInt64() + offset + signature.FinalAddressOffset);
                Log.Debug($"Final calculated address: 0x{finalAddress.ToInt64():X}");

                ulong result = (ulong)finalAddress;

                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Error in FindAddressBySignature: {ex.Message}");
                throw;
            }
        }
        public static ulong GetAddressBySignatureName(string signatureName)
        {
            if (!Signatures.TryGetValue(signatureName, out var signature))
            {
                throw new ArgumentException($"Signature '{signatureName}' not found in the signatures dictionary");
            }

            return FindAddressBySignature(signature);
        }

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
            return Memory.ReadULong(GetAddressBySignatureName("BaseA"));
        }


        public static ulong GetFrpgNetManOffset()
        {
            return Memory.ReadULong(GetAddressBySignatureName("FrpgNetMan"));
        }

        public static ulong GetBaseBOffset()
        {
            return Memory.ReadULong(GetAddressBySignatureName("BaseB"));
        }
        public static Dictionary<int, BonfireState> GetBonfireStates()
        {
            Dictionary<int, BonfireState> bonfireStates = new Dictionary<int, BonfireState>();

            try
            {
                // Get the FrpgNetMan pointer
                var frpgNetManOffset = GetFrpgNetManOffset();

                // Navigate to the bonfire database
                var netBonfireDbAddress = Archipelago.Core.Util.Helpers.ResolvePointer(frpgNetManOffset, 0x00, 0xb68);

                var foo = Memory.ReadULong(frpgNetManOffset);
                var bar = Memory.ReadULong(foo);
                var baz = Memory.ReadULong(bar);

                var foo2 = Memory.ReadULong(foo + 0xb68);
                var bar2 = Memory.ReadULong(bar + 0xb68);
                var baz2 = Memory.ReadULong(baz + 0xb68);

                // Get the first element at offset 0x28
                var elementAddress = Memory.ReadULong(netBonfireDbAddress + 0x28);


                var foo3 = Memory.ReadULong(foo2 + 0x28);
                var bar3 = Memory.ReadULong(bar2 + 0x28);
                var baz3 = Memory.ReadULong(baz2 + 0x28);

                if (elementAddress == 0) return bonfireStates;

                // Now follow the same traversal pattern as the original code
                for (var i = 0; i < 100; i++)
                {
                    // First dereference element at offset 0x0 (like element = element.CreatePointerFromAddress(0x0))
                    elementAddress = Memory.ReadULong(elementAddress);

                    var foo4 = Memory.ReadULong(foo3);
                    var bar4 = Memory.ReadULong(bar3);
                    var baz4 = Memory.ReadULong(baz3);
                    if (elementAddress == 0) break;

                    // Get the bonfire item at offset 0x10 (like netBonfireDbItem = element.CreatePointerFromAddress(0x10))
                    var netBonfireDbItemAddress = Memory.ReadULong(elementAddress + 0x10);
                    if (netBonfireDbItemAddress == 0) break;

                    // Read the bonfire ID and state
                    var bonfireId = Memory.ReadInt(netBonfireDbItemAddress + 0x8);
                    var bonfireState = Memory.ReadInt(netBonfireDbItemAddress + 0xC);

                    // Add to dictionary if not already present
                    if (!bonfireStates.ContainsKey(bonfireId))
                    {
                        bonfireStates[bonfireId] = (BonfireState)bonfireState;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error in GetBonfireStates: {ex.Message}");
            }

            return bonfireStates;
        }
        public static ulong ResolvePointerChain(ulong baseAddress, params int[] offsets)
        {
            ulong currentAddress = baseAddress;

            foreach (int offset in offsets.Take(offsets.Length - 1))
            {
                currentAddress = Memory.ReadULong(currentAddress + (ulong)offset);
                if (currentAddress == 0)
                {
                    Log.Error($"Null pointer encountered while resolving pointer chain at offset 0x{offset:X}");
                    return 0;
                }
            }

            // Add the final offset without dereferencing
            if (offsets.Length > 0)
            {
                currentAddress += (ulong)offsets.Last();
            }

            return currentAddress;
        }
        public static ulong GetBaseCOffset()
        {
            return Memory.ReadULong(GetAddressBySignatureName("BaseC"));
        }
        public static ulong GetBaseXOffset()
        {
            return GetAddressBySignatureName("BaseX");
        }
        public static ulong GetChrBaseClassOffset()
        {
            return Memory.ReadULong(GetAddressBySignatureName("ChrBaseClass"));
        }
        public static ulong GetProgressionFlagOffset()
        {
            return Memory.ReadULong(GetAddressBySignatureName("ProgressionFlag"));
        }
        public static ulong GetSoloParamOffset()
        {
            return Memory.ReadULong(GetAddressBySignatureName("SoloParam"));
        }
        public static ulong GetEventFlagsOffset()
        {
            return Memory.ReadULong(GetAddressBySignatureName("EventFlags"));
        //    return (ulong)(BitConverter.ToInt32(Memory.ReadFromPointer(addressPointer, 4, 2)));
        }
        internal static int GetPlayerHP()
        {
            return Memory.ReadInt(GetPlayerHPAddress());
        }
        internal static ulong GetPlayerHPAddress()
        {
            var baseB = GetBaseBOffset();
            return ResolvePointerChain(baseB, 0x10, 0x14);
        }
        private static ulong GetItemLotParamOffset()
        {
            var soloParams = GetSoloParamOffset();
            return ResolvePointerChain(soloParams, 0x0, 0x570, 0x38);
        }
        private static ulong GetBonfireOffset()
        {
            var baseAddress = GetEventFlagsOffset();
            var baseBonfire = OffsetPointer(baseAddress, 0x5B);
            return baseBonfire;
        }
        public static List<Location> GetItemLotLocations()
        {
            List<Location> locations = new List<Location>();
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
        public static List<Location> GetBossFlagLocations()
        {
            List<Location> locations = new List<Location>();
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
        public static List<Location> GetBonfireFlagLocations()
        {
            List<Location> locations = new List<Location>();
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
        public static List<Location> GetDoorFlagLocations()
        {
            List<Location> locations = new List<Location>();
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
        public static List<Location> GetFogWallFlagLocations()
        {
            List<Location> locations = new List<Location>();
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
        public static List<Location> GetMiscFlagLocations()
        {
            List<Location> locations = new List<Location>();
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
        public static void OverwriteItemLot(int itemLotId, ItemLot newItemLot)
        {
            var startAddress = GetItemLotParamOffset();
            var dataOffset = Memory.ReadUInt(startAddress + 0x4);
            var rowCount = Memory.ReadUShort(startAddress + 0xA);
            const int rowSize = 148; // Size of each ItemLotParam

            for (int i = 0; i < rowCount; i++)
            {
                var currentAddress = startAddress + dataOffset + (ulong)(i * rowSize);
                var currentItemLotId = Memory.ReadInt(currentAddress + 0x80);  // GetItemFlagId is at offset 0x80

                if (currentItemLotId == itemLotId)
                {
                    // We found the correct item lot, now let's overwrite it

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

                    Log.Verbose($"ItemLot with GetItemFlagId {itemLotId} has been overwritten.");
                    
                }
            }

            Log.Verbose($"ItemLot with GetItemFlagId {itemLotId} not found.");
        }
        public static void OverwriteSingleItem(ulong address, ItemLotItem newItemLot, int position)
        {
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

    }
}

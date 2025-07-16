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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
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
        private static ulong ItemLotParamOffset = 0;

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
            return Memory.ReadULong(Memory.ReadULong(GetAddressBySignatureName("EventFlags")));
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
        public static ulong GetItemLotParamOffset()
        {
            if (ItemLotParamOffset != 0)
            {
                return ItemLotParamOffset;
            }
            var soloParams = GetSoloParamOffset();
            ItemLotParamOffset = ResolvePointerChain(soloParams, 0x570, 0x38, 0x0);
            return ItemLotParamOffset;
        }
        public static ulong GetPlayerGameDataOffset()
        {
            return ResolvePointerChain(0x141C8A530, new int[] { 0x0, 0xD10});
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
                    NibblePosition = NibblePosition.Lower,
                    CheckType = LocationCheckType.Bit,
                    Category = "ItemLot",
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
                    NibblePosition = NibblePosition.Lower,
                    CheckType = LocationCheckType.Bit,
                    Category = "BossEvent",

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
                    AddressBit = lot.AddressBit,
                    NibblePosition = NibblePosition.Lower,
                    CheckType = LocationCheckType.Bit,
                    Category = "Bonfire",
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
                    NibblePosition = NibblePosition.Lower,
                    CheckType = LocationCheckType.Bit,
                    Category = "Door",
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
                    NibblePosition = NibblePosition.Lower,
                    CheckType = LocationCheckType.Bit,
                    Category = "FogWall",
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
                    NibblePosition = NibblePosition.Lower,
                    CheckType = LocationCheckType.Bit,
                    Category = "MiscFlag"
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
        public static Dictionary<int, ItemLot> GetItemLots()
        {
            List<ItemLotParamStruct> itemLots = new List<ItemLotParamStruct>();
            Dictionary<int, ItemLot> itemLotLookup = new Dictionary<int, ItemLot>();
            var startAddress = GetItemLotParamOffset();

            var dataOffset = Memory.ReadUInt(startAddress + 0x4);
            var rowCount = Memory.ReadUShort(startAddress + 0xA);
            var sizeOfStruct = Marshal.SizeOf(typeof(ItemLotParamStruct));

            List<ItemLotParamStruct> itemLotParams = Memory.ReadStructs<ItemLotParamStruct>(startAddress + dataOffset, rowCount);

            for (int i = 0; i < itemLotParams.Count; i++)
            {
                ItemLotParamStruct p = itemLotParams[i];
                itemLots.Add(p);
                itemLotLookup.TryAdd(p.LotOverallGetItemFlagId, new ItemLot(p, startAddress + dataOffset + (ulong)i * (ulong)sizeOfStruct));

            }
            return itemLotLookup;
        }
        public static void OverwriteItemLot(ItemLot oldItemLot, ItemLotParamStruct newItemLot)
        {
            newItemLot.LotOverallGetItemFlagId = oldItemLot.itemLotParam.LotOverallGetItemFlagId;
            newItemLot.GetItemFlagIds[0] = oldItemLot.itemLotParam.GetItemFlagIds[0];
            Memory.WriteStruct<ItemLotParamStruct>(oldItemLot.startAddress, newItemLot);
        }
        public static void OverwriteSingleItem(ulong address, int position, ItemLotParamStruct newItemLot, int itemNumber)
        {
            Memory.Write(address + (ulong)(position * 4), newItemLot.LotItemIds[itemNumber]);
            Memory.Write(address + 0x20 + (ulong)(position * 4), newItemLot.LotItemCategories[itemNumber]);
            Memory.Write(address + 0x40 + (ulong)(position * 2), (ushort)newItemLot.LotItemBasePoints[itemNumber]);
            Memory.Write(address + 0x50 + (ulong)(position * 2), (ushort)newItemLot.CumulateLotPoints[itemNumber]);
            //Memory.Write(address + 0x60 + (ulong)(position * 4), newItemLot.Items[j].GetItemFlagId);
            Memory.WriteByte(address + 0x8A + (ulong)position, newItemLot.LotItemNums[itemNumber]);
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
        public static DarkSoulsItem CreateItemFromLot(ItemLotParamStruct lot, int itemNumber)
        {
            var allItems = GetAllItems();
            var item = allItems.FirstOrDefault(x => x.Id == lot.LotItemIds[itemNumber]);
            return item;
        }
        public static bool GetIsPlayerOnline()
        {
            var baseCOffset = GetBaseCOffset();
            ulong onlineFlagOffset = 0xB7D;

            var isOnline = Memory.ReadByte(baseCOffset + onlineFlagOffset) != 0;
            return isOnline;

        }
        public static bool GetIsPlayerInGame()
        {
            ulong playerGameData = GetPlayerGameDataOffset();
            return Memory.ReadByte(playerGameData + 0x120) != 0;
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
                    AddressBit = b.AddressBit,
                    NibblePosition = NibblePosition.Lower,
                    CheckType = LocationCheckType.Bit,
                    Category = "BossProg"
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

        /*---------GetItemCommand Injected ASM-------------------------
            0:  ba 00 00 00 10          mov    edx,0x10000000
            5:  41 b9 01 00 00 00       mov    r9d,0x1
            b:  41 b8 28 70 08 00       mov    r8d,0x87028
            11: 41 bc fe fe fe fe       mov    r12d,0xfefefefe
            17: 48 a1 30 a5 c8 41 01    movabs rax,ds:0x141c8a530
            1e: 00 00 00
            21: c6 44 24 38 01          mov    BYTE PTR [rsp+0x38],0x1
            26: 40 88 7c 24 30          mov    BYTE PTR [rsp+0x30],dil
            2b: c6 44 24 28 01          mov    BYTE PTR [rsp+0x28],0x1
            30: 4c 8b 78 10             mov    r15,QWORD PTR [rax+0x10]
            34: c6 44 24 20 01          mov    BYTE PTR [rsp+0x20],0x1
            39: 49 8d 8f 80 02 00 00    lea    rcx,[r15+0x280]
            40: 48 83 ec 38             sub    rsp,0x38
            44: 49 be e0 79 74 40 01    movabs r14,0x1407479e0
            4b: 00 00 00
            4e: 41 ff d6                call   r14
            51: 48 83 c4 38             add    rsp,0x38
            55: c3                      ret 
         */
        public static byte[] GetItemCommand()
        {

            byte[] x = [0xBA, 0x00, 0x00, 0x00, 0x10, 0x41, 0xB9, 0x01, 0x00, 0x00, 0x00, 0x41, 0xB8, 0x28, 0x70, 0x08, 0x00, 0x41, 0xBC, 0xFE, 0xFE, 0xFE, 0xFE, 0x48, 0xA1, 0x30, 0xA5, 0xC8, 0x41, 0x01, 0x00, 0x00, 0x00, 0xC6, 0x44, 0x24, 0x38, 0x01, 0x40, 0x88, 0x7C, 0x24, 0x30, 0xC6, 0x44, 0x24, 0x28, 0x01, 0x4C, 0x8B, 0x78, 0x10, 0xC6, 0x44, 0x24, 0x20, 0x01, 0x49, 0x8D, 0x8F, 0x80, 0x02, 0x00, 0x00, 0x48, 0x83, 0xEC, 0x38, 0x49, 0xBE, 0xE0, 0x79, 0x74, 0x40, 0x01, 0x00, 0x00, 0x00, 0x41, 0xFF, 0xD6, 0x48, 0x83, 0xC4, 0x38, 0xC3];
            return x;
        }

        /*---------GetItemWithMessage Injected ASM-----------------------------------
            0:  8b 15 3e 00 00 00       mov    edx,DWORD PTR [rip+0x3e]        # 0x44
            6:  48 a1 30 a5 c8 41 01    movabs rax,ds:0x141c8a530
            d:  00 00 00
            10: 44 8b 0d 31 00 00 00    mov    r9d,DWORD PTR [rip+0x31]        # 0x48
            17: 44 8b 05 2e 00 00 00    mov    r8d,DWORD PTR [rip+0x2e]        # 0x4c
            1e: 4c 8b 78 10             mov    r15,QWORD PTR [rax+0x10]
            22: 49 8d 8f 80 02 00 00    lea    rcx,[r15+0x280]
            29: 48 83 ec 38             sub    rsp,0x38
            2d: ff 15 02 00 00 00       call   QWORD PTR [rip+0x2]        # 0x35
            33: eb 08                   jmp    0x3d
            35: e0 79                   loopne 0xb0
            37: 74 40                   je     0x79
            39: 01 00                   add    DWORD PTR [rax],eax
            3b: 00 00                   add    BYTE PTR [rax],al
            3d: 48 83 c4 38             add    rsp,0x38
            41: c3                      ret
            42: 90                      nop
            43: 90                      nop
            44: 00 00                   add    BYTE PTR [rax],al
            46: 00 00                   add    BYTE PTR [rax],al
            48: 00 00                   add    BYTE PTR [rax],al
            4a: 00 00                   add    BYTE PTR [rax],al
            4c: 00 00                   add    BYTE PTR [rax],al
            4e: 00 00                   add    BYTE PTR [rax],al
            50: ff                      (bad)
            51: ff                      (bad)
            52: ff                      (bad)
            53: ff                      .byte 0xff 
         */
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


        /*  ----------Code To Emulate--------------
            
            1403fe4ef 8b d0           MOV        EDX,ItemCategory
            1403fe4f1 44 8b ce        MOV        R9D,ItemCount
            1403fe4f4 44 8b c3        MOV        R8D,ItemId
            1403fe4f7 48 8b cf        MOV        RCX,RDI
            1403fe4fa e8 91 a7        CALL       SetupItemPickupMenuWithoutPickup

        */
        /*  ----------ItemPickupDialogWithoutPickup injected ASM--------------
            0:  8b 15 32 00 00 00       mov    edx,DWORD PTR [rip+0x32]        # 0x38
            6:  44 8b 0d 2f 00 00 00    mov    r9d,DWORD PTR [rip+0x2f]        # 0x3c
            d:  44 8b 05 2c 00 00 00    mov    r8d,DWORD PTR [rip+0x2c]        # 0x40
            14: 48 a1 a8 91 c8 41 01    movabs rax,ds:0x141c891a8
            1b: 00 00 00
            1e: 48 89 c1                mov    rcx,rax
            21: 48 83 ec 38             sub    rsp,0x38
            25: 49 be 90 8c 72 40 01    movabs r14,0x140728c90
            2c: 00 00 00
            2f: 41 ff d6                call   r14
            32: 48 83 c4 38             add    rsp,0x38
            36: c3                      ret
            37: 90                      nop
            38: 00 00                   add    BYTE PTR [rax],al
            3a: 00 00                   add    BYTE PTR [rax],al
            3c: 00 00                   add    BYTE PTR [rax],al
            3e: 00 00                   add    BYTE PTR [rax],al
            40: 00 00                   add    BYTE PTR [rax],al
            42: 00 00                   add    BYTE PTR [rax],al 
         */
        public static byte[] ItemPickupDialogWithoutPickup()
        {
            byte[] x = new byte[] {
                0x8B, 0x15, 0x32, 0x00, 0x00, 0x00,
                0x44, 0x8B, 0x0D, 0x2F, 0x00, 0x00, 0x00,
                0x44, 0x8B, 0x05, 0x2C, 0x00, 0x00, 0x00,
                0x48, 0xA1, 0xA8, 0x91, 0xC8, 0x41, 0x01,
                0x00, 0x00, 0x00,
                0x48, 0x89, 0xC1,
                0x48, 0x83, 0xEC, 0x38,
                0x49, 0xBE, 0x90, 0x8C, 0x72, 0x40, 0x01, 0x00, 0x00, 0x00,
                0x41, 0xFF, 0xD6,
                0x48, 0x83, 0xC4, 0x38,
                0xC3,
                0x90,
                0x00,0x00, 0x00, 0x00,
                0x00,0x00, 0x00, 0x00,
                0x00,0x00, 0x00, 0x00,
                };

            return x;
        }

        /*  ----------InjectItemPickupDialogSwitch injected ASM
            0:  41 81 f8 72 01 00 00    cmp    r8d,0x172
            7:  74 0d                   je     16 <skip_dialog>
            9:  48 83 ec 38             sub    rsp,0x38
            d:  e8 00 00 00 00          call   12 <_main+0x12>
            12: 48 83 c4 38             add    rsp,0x38
            0000000000000016 <skip_dialog>:
            16: c3                      ret 
         */
        public static byte[] InjectItemPickupDialogSwitch()
        {
            byte[] x = new byte[] {
                0x41, 0x81, 0xF8, 0x72, 0x01, 0x00, 0x00,
                0x74, 0x0D,
                0x48, 0x83, 0xEC, 0x38,
                0xE8, 0x92, 0xA7, 0x32, 0x00,
                0x48, 0x83, 0xC4, 0x38,
                0xC3,
            };

            return x;
        }

        internal static ulong GetItemPickupDialog()
        {
            return Helpers.ResolvePointerChain(0x141c88d98, new int[] { 0x0, 0x12C });
        }
        internal static bool GetIsItemPickupDialogVisible()
        {
            return Memory.ReadByte(GetItemPickupDialog()) != 0;
        }

        internal static InjectedString SetItemPickupText(DarkSoulsItem item)
        {
            //base struct of all text messages in the game including dialog and item text
            ulong MsgMan = 0x141c7e3e8;
            /*
             * offset Goods Table       0x380
             * offset Ring Table        0x390
             * offset Weapon Table      0x3A0
             * offset Armor Table       0x3B0
             */
            int messageManOffset;
            switch (item.Category)
            {
                case Enums.DSItemCategory.Armor: 
                    messageManOffset = 0x3B0; 
                    break;
                case Enums.DSItemCategory.MeleeWeapons:
                    messageManOffset = 0x3A0;
                    break;
                case Enums.DSItemCategory.Rings:
                    messageManOffset = 0x390;
                    break;
                case Enums.DSItemCategory.Consumables:
                    messageManOffset = 0x380;
                    break;
                default: 
                    messageManOffset = 0x380;
                    break;
            }
            ulong itemPickupDialogTable = ResolvePointerChain(MsgMan, new int[] { 0x0, messageManOffset, 0x0 });
            //See Func 0x14053df10 for how this Message Man struct is used.
            //This value at this address is an offset from the base of the table that tells us how far to offset to get to the correct string.
            uint offsetOfBaseOfStringOffsetTable = Memory.ReadUInt(itemPickupDialogTable + 0x14);
            
            //This is the offset of the Map From Item Ids to String Ids, the map starts here.
            ulong baseOfItemIdToStringMap = itemPickupDialogTable + 0x1C;

            uint sizeOfItemIdToStringIndexMap = Memory.ReadUInt(itemPickupDialogTable + 0xC);


            //In the prism stone example this value is 6A, this is the index in the string offset table (mult by 4)
            //that we look for the string offset for the prism stone string.

            //the first 4 byte value in each entry in this map is the index in the string offset table

            //the second 4 byte value in each entry in this map is the starting item id 
            //(for example there are prism stones for each different color of prism stone and they all need to be named the same text)
            //more concretely each weapon and armor upgrade and infusion had a separate id/upgrade number so for example the Striaght Sword has a base id
            //as well as a series of offset ids marking each smith upgrade as well as each infused upgrade as well as each smithed and infused upgrade.
            //the items are grouped first by their base id, then infusion type, then upgrade level.

            //the third 4 byte value in each entry in this map is the the last id in the bucket associated with the string table offset

            byte[] byteArray = Memory.ReadByteArray(baseOfItemIdToStringMap, (int) sizeOfItemIdToStringIndexMap * 0xC);

            uint[] intArray = new uint[byteArray.Length / 4];
            Buffer.BlockCopy(byteArray, 0, intArray, 0, byteArray.Length);
            uint offset = 0x6A;
            bool found = false;
            for (int i = 0; i < sizeOfItemIdToStringIndexMap - 1; i++)
            {
                int index = i * 3;
                if(item.Id >= intArray[index + 1] && item.Id <= intArray[index + 2])
                {
                    found = true;
                    offset = intArray[index] + ((uint)item.Id - intArray[index + 1]);
                    break;
                }
            }
            
            uint indexOfItemInStringOffsetTable = offset * 0x4;
            
            //The value at this address is the offset from the base of the table to find the string
            ulong stringOffsetLoc = itemPickupDialogTable + offsetOfBaseOfStringOffsetTable + indexOfItemInStringOffsetTable;
            ulong itemStringOffset = Memory.ReadUInt(stringOffsetLoc);

            String itemName = item.Name;
            
            //We need padding of at least 2 Zeros to end a unicode string, unicode strings have zeros between each character.
            uint unicodeStringLength = ((uint)itemName.Length + 1) * 2;
            
            //Allocate space for an injected string above the start of the table but below the
            //transition from the 32 bit address space to the 64 bit address space.
            //On original harware this was unlikely a constraint, but we cannot overflow to the correct address
            //given the 4 byte offset that we can inject.
            IntPtr injectedStringLoc = Memory.AllocateAbove(unicodeStringLength);
            uint injectedStringAddress = (uint)injectedStringLoc.ToInt32();
            uint offsetToInjectedString = injectedStringAddress - (uint)itemPickupDialogTable;
            Memory.WriteString((ulong)injectedStringAddress, itemName, Archipelago.Core.Util.Enums.Endianness.Little, Encoding.Unicode);
            Memory.Write(stringOffsetLoc, offsetToInjectedString);
            InjectedString result = new InjectedString(itemName, injectedStringLoc, stringOffsetLoc, itemStringOffset);
            return result;
        }

        internal static void FreeItemPickupText(InjectedString injectedString)
        {
            Memory.Write(injectedString.stringOffsetLoc, injectedString.originalStringOffset);
            Memory.FreeMemory(injectedString.injectedStringLoc);
        }
    }
}

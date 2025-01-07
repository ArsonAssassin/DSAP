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
        public static ulong GetBaseAddress()
        {
            var address = Memory.GetBaseAddress("DarkSoulsRemastered");
            if (address == 0)
            {
                Log.Debug("Could not find Base Address");
            }
            return (ulong)address;
        }
        public static ulong GetBaseCOffset()
        {
            var baseAddress = GetBaseAddress();
            // byte[] pattern = { 48 8B 05 xx xx xx xx 0F 28 01 66 0F 7F 80 xx xx 00 00 C6 80 }
            byte[] pattern = { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x28, 0x01, 0x66, 0x0F, 0x7F, 0x80, 0x00, 0x00, 0x00, 0x00, 0xC6, 0x80 };
            string mask = "xxx????xxxxxxx??xxxx";
            //   byte[] pattern = { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x45, 0x33, 0xED, 0x48, 0x8B, 0xF1, 0x48, 0x85, 0xC0 };
            //  string mask = "xxx????xxxxxxxxxx";
            IntPtr getPFAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

            int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getPFAddress + 3), 4), 0);
            IntPtr progressionFlagsAddress = getPFAddress + offset + 7;

            return (ulong)progressionFlagsAddress;
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

            return results;
        }
        public static ulong FlagToOffset(EventFlag flag)
        {
            var offset = GetEventFlagOffset(flag.Flag).Item1;
            return offset;
        }
        public static void WriteToFile(string fileName, object content)
        {
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"AP_DarkSoulsRemastered", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
            using (var streamWriter = new StreamWriter(fileStream))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                var serializer = new JsonSerializer();

                serializer.Serialize(jsonWriter, content);
            }
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
    }
}

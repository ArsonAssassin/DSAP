using Archipelago.Core.Models;
using Archipelago.Core.Util;
using DSAP.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;

namespace DSAP
{
    public class Helpers
    {
        public static ulong GetBaseAddress()
        {
            var address = (ulong)Memory.GetBaseAddress("DarkSoulsRemastered");
            if (address == 0)
            {
                Console.WriteLine("Could not find Base Address");
            }
            return address;
        }
        public static ulong GetBaseCOffset()
        {
            var baseAddress = GetBaseAddress();
            byte[] pattern = { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x45, 0x33, 0xED, 0x48, 0x8B, 0xF1, 0x48, 0x85, 0xC0 };
            string mask = "xxx????xxxxxxxxxx";
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
        private static ulong GetItemLotFlagsOffset()
        {
            var baseAddress = GetBaseAddress();
            byte[] pattern = { 0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x99, 0x33, 0xC2, 0x45, 0x33, 0xC0, 0x2B, 0xC2, 0x8D, 0x50, 0xF6 };
            string mask = "xxx????xxxxxxxxxxx";
            IntPtr getILFAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

            int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getILFAddress + 3), 4), 0);
            IntPtr itemLotFlagsAddress = getILFAddress + offset + 7;

            return (ulong)(BitConverter.ToInt32(Memory.ReadFromPointer((ulong)itemLotFlagsAddress, 4, 2)));
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
        public static List<Location> GetItemLotLocations()
        {
            List<Location> locations = new List<Location>();
            var lotFlags = GetItemLotFlags();
            var baseAddress = GetItemLotFlagsOffset();
            foreach (var lot in lotFlags)
            {
                locations.Add(new Location
                {
                    Name = lot.Name,
                    Address = baseAddress + GetEventFlagOffset(lot.Id).Item1,
                    AddressBit = GetEventFlagOffset(lot.Id).Item2,
                    Id = lot.Id,                    
                });
            }
            return locations;
        }
        private static ulong OffsetPointer(ulong ptr, int offset)
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

            var lotFlags = GetItemLotFlags();

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
                    LotItemId = Memory.ReadInt(currentAddress + (ulong)(i * 4)),
                    LotItemCategory = Memory.ReadInt(currentAddress + 0x20 + (ulong)(i * 4)),
                    LotItemBasePoint = Memory.ReadUShort(currentAddress + 0x40 + (ulong)(i * 2)),
                    CumulateLotPoint = Memory.ReadUShort(currentAddress + 0x50 + (ulong)(i * 2)),
                    GetItemFlagId = Memory.ReadInt(currentAddress + 0x60 + (ulong)(i * 4)),
                    LotItemNum = Memory.ReadByte(startAddress + 0x8A + (ulong)i),                    
                    EnableLuck = bitArray.Get(i),
                    CumulateReset = bitArray.Get(i + 8)
                };
                currentAddress += 0x04;
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
                        if (j < newItemLot.Items.Count)
                        {
                            // Write actual item data
                            Memory.Write(currentAddress + (ulong)(j * 4), newItemLot.Items[j].LotItemId);
                            Memory.Write(currentAddress + 0x20 + (ulong)(j * 4), newItemLot.Items[j].LotItemCategory);
                            Memory.Write(currentAddress + 0x40 + (ulong)(j * 2), (ushort)newItemLot.Items[j].LotItemBasePoint);
                            Memory.Write(currentAddress + 0x50 + (ulong)(j * 2), (ushort)newItemLot.Items[j].CumulateLotPoint);
                            Memory.Write(currentAddress + 0x60 + (ulong)(j * 4), newItemLot.Items[j].GetItemFlagId);
                            Memory.WriteByte(currentAddress + 0x8A + (ulong)j, newItemLot.Items[j].LotItemNum);
                        }
                        else
                        {
                            // Fill remaining slots with default values
                            Memory.Write(currentAddress + (ulong)(j * 4), 0);  // LotItemId
                            Memory.Write(currentAddress + 0x20 + (ulong)(j * 4), 0);  // LotItemCategory
                            Memory.Write(currentAddress + 0x40 + (ulong)(j * 2), (ushort)0);  // LotItemBasePoint
                            Memory.Write(currentAddress + 0x50 + (ulong)(j * 2), (ushort)0);  // CumulateLotPoint
                            Memory.Write(currentAddress + 0x60 + (ulong)(j * 4), 0);  // GetItemFlagId
                            Memory.WriteByte(currentAddress + 0x8A + (ulong)j, 0);  // LotItemNum
                        }
                    }

                    Memory.Write(currentAddress + 0x80, newItemLot.GetItemFlagId);
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

                    Console.WriteLine($"ItemLot with GetItemFlagId {itemLotId} has been overwritten.");
                    return;
                }
            }

            Console.WriteLine($"ItemLot with GetItemFlagId {itemLotId} not found.");
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

            var isOnline = Memory.ReadByte(baseCOffset +  onlineFlagOffset) != 0;
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
        public static List<ItemLotFlag> GetItemLotFlags()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.ItemLots.json");
            var list = JsonConvert.DeserializeObject<List<ItemLotFlag>>(json);
            return list;
        }
        public static List<DarkSoulsItem> GetAllItems()
        {
            var results = new List<DarkSoulsItem>();

            results.Concat(GetConsumables());
            results.Concat(GetKeyItems());
            results.Concat(GetRings());
            results.Concat(GetUpgradeMaterials());
            return results;
        }
        public static ulong FlagToOffset(ItemLotFlag flag)
        {
            var offset = GetEventFlagOffset(flag.Id).Item1;
            return offset;
        }

        private static (ulong, int) GetEventFlagOffset(int eventFlag)
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
        public static int ApIdToDsId(int dsId)
        {
            return dsId - 11110000;
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

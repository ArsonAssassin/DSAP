using Archipelago.Core.Models;
using Archipelago.Core.Util;
using DSAP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAP
{
    public static class Temp
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
        public static ulong GetBonfireBaseOffset()
        {
            var baseAddress = GetBaseAddress();
            byte[] pattern = { 0x48, 0x83, 0x3d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8b, 0xf1 };
            string mask = "xxx????xxxx";
            nint getBonfireAddress = Memory.FindSignature((nint)baseAddress, 0x1000000, pattern, mask);

            int offset = BitConverter.ToInt32(Memory.ReadByteArray((ulong)(getBonfireAddress + 3), 4), 0);
            nint bonfireAddress = getBonfireAddress + offset + 8;

            return (ulong)bonfireAddress;
        }
        public static ulong GetBonfireOffset()
        {
            var address = GetBonfireBaseOffset();
            var foo = Memory.ReadULong(address);
            var next = Helpers.OffsetPointer(foo, 0xb68);
            var foo2 = Memory.ReadULong(next);
            next = Helpers.OffsetPointer(foo2, 0x28);
            var foo3 = Memory.ReadULong(next);
            next = Helpers.OffsetPointer(foo3, 0x10);
            return next;
            return address;
        }
        public static List<EventFlag> GetBonfireFlags()
        {
            var json = Helpers.OpenEmbeddedResource("DSAP.Resources.temp.json");
            var list = JsonConvert.DeserializeObject<List<EventFlag>>(json);
            return list;
        }
        public static List<Location> GetBonfireLocations()
        {
            List<Location> locations = new List<Location>();
            var lotFlags = GetBonfireFlags();
            var baseAddress = GetBonfireOffset();
            foreach (var lot in lotFlags)
            {
                locations.Add(new Location
                {
                    Name = lot.Name,
                    Address = baseAddress + Helpers.GetEventFlagOffset(lot.Flag).Item1,
                    AddressBit = Helpers.GetEventFlagOffset(lot.Flag).Item2,
                    Id = lot.Id,
                });
            }
            return locations;
        }
    }
}

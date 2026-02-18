using Archipelago.Core.Models;
using Archipelago.Core.Util;
using DSAP.Models;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DSAP.Helpers
{
    internal class LocationHelper
    {
        #region Location Helpers
        private static List<ILocation> CachedItemLotLocations = null;
        public static List<ILocation> GetItemLotLocations()
        {
            if (CachedItemLotLocations != null)
                return CachedItemLotLocations;

            List<ILocation> locations = new List<ILocation>();
            var lotFlags = GetItemLotFlags();
            var baseAddress = AddressHelper.GetEventFlagsOffset();
            foreach (var lot in lotFlags)
            {
                locations.Add(new Location
                {
                    Name = lot.Name,
                    Address = baseAddress + AddressHelper.GetEventFlagOffset(lot.Flag).Item1,
                    AddressBit = AddressHelper.GetEventFlagOffset(lot.Flag).Item2,
                    Id = lot.Id,
                });
            }
            CachedItemLotLocations = locations;
            return locations;
        }
        public static List<ILocation> GetBossFlagLocations()
        {
            List<ILocation> locations = new List<ILocation>();
            var lotFlags = GetBossFlags();
            var baseAddress = AddressHelper.GetEventFlagsOffset();
            foreach (var lot in lotFlags)
            {
                locations.Add(new Location
                {
                    Name = lot.Name,
                    Address = baseAddress + AddressHelper.GetEventFlagOffset(lot.Flag).Item1,
                    AddressBit = AddressHelper.GetEventFlagOffset(lot.Flag).Item2,
                    Id = lot.Id,
                });
            }
            return locations;
        }
        public static List<ILocation> GetBonfireFlagLocations()
        {
            List<ILocation> locations = new List<ILocation>();
            var lotFlags = GetBonfireFlags();
            var offset = AddressHelper.GetProgressionFlagOffset();
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
            var baseAddress = AddressHelper.GetEventFlagsOffset();
            foreach (var lot in lotFlags)
            {
                locations.Add(new Location
                {
                    Name = lot.Name,
                    Address = baseAddress + AddressHelper.GetEventFlagOffset(lot.Flag).Item1,
                    AddressBit = AddressHelper.GetEventFlagOffset(lot.Flag).Item2,
                    Id = lot.Id,
                });
            }
            return locations;
        }
        public static List<ILocation> GetFogWallFlagLocations()
        {
            List<ILocation> locations = new List<ILocation>();
            var lotFlags = GetFogWallFlags();
            var baseAddress = AddressHelper.GetEventFlagsOffset();
            foreach (var lot in lotFlags)
            {
                locations.Add(new Location
                {
                    Name = lot.Name,
                    Address = baseAddress + AddressHelper.GetEventFlagOffset(lot.Flag).Item1,
                    AddressBit = AddressHelper.GetEventFlagOffset(lot.Flag).Item2,
                    Id = lot.Id,
                });
            }
            return locations;
        }
        public static List<ILocation> GetMiscFlagLocations()
        {
            List<ILocation> locations = new List<ILocation>();
            var lotFlags =  GetMiscFlags();
            var baseAddress = AddressHelper.GetEventFlagsOffset();
            foreach (var lot in lotFlags)
            {
                locations.Add(new Location
                {
                    Name = lot.Name,
                    Address = baseAddress + AddressHelper.GetEventFlagOffset(lot.Flag).Item1,
                    AddressBit = AddressHelper.GetEventFlagOffset(lot.Flag).Item2,
                    Id = lot.Id,
                });
            }
            return locations;
        }
        public static List<Location> GetBossLocations()
        {
            var offset = AddressHelper.GetProgressionFlagOffset();
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
        public static List<Boss> GetBosses()
        {
            var json = MiscHelper.OpenEmbeddedResource("DSAP.Resources.Bosses.json");
            var list = JsonSerializer.Deserialize<List<Boss>>(json, MiscHelper.GetJsonOptions());
            return list;
        }
        #endregion Location Helpers
        #region Flag Helpers
        public static bool ReadBonfireFlag(string name)
        {
            string result = "";
            var lotFlags = LocationHelper.GetBonfireFlagLocations();
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

        public static List<ItemLotFlag> GetItemLotFlags()
        {
            var json = MiscHelper.OpenEmbeddedResource("DSAP.Resources.ItemLots.json");
            var list = JsonSerializer.Deserialize<List<ItemLotFlag>>(json, MiscHelper.GetJsonOptions());
            return list;
        }
        public static List<BossFlag> GetBossFlags()
        {
            var json = MiscHelper.OpenEmbeddedResource("DSAP.Resources.BossFlags.json");
            var list = JsonSerializer.Deserialize<List<BossFlag>>(json, MiscHelper.GetJsonOptions());
            return list;
        }
        public static List<BonfireFlag> GetBonfireFlags()
        {
            var json = MiscHelper.OpenEmbeddedResource("DSAP.Resources.Bonfires.json");
            var list = JsonSerializer.Deserialize<List<BonfireFlag>>(json, MiscHelper.GetJsonOptions());
            return list;
        }
        public static List<DoorFlag> GetDoorFlags()
        {
            var json = MiscHelper.OpenEmbeddedResource("DSAP.Resources.Doors.json");
            var list = JsonSerializer.Deserialize<List<DoorFlag>>(json, MiscHelper.GetJsonOptions());
            return list;
        }
        public static List<FogWallFlag> GetFogWallFlags()
        {
            var json = MiscHelper.OpenEmbeddedResource("DSAP.Resources.DsrEvents.json");
            List<Enums.DsrEventType> fogwalltypes = [Enums.DsrEventType.FOGWALL, Enums.DsrEventType.BOSSFOGWALL, Enums.DsrEventType.EARLYFOGWALL];
            var list = JsonSerializer.Deserialize<List<DsrEvent>>(json, MiscHelper.GetJsonOptions()).Where(x => fogwalltypes.Contains(x.Type));
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
            var json = MiscHelper.OpenEmbeddedResource("DSAP.Resources.MiscFlags.json");
            var list = JsonSerializer.Deserialize<List<EventFlag>>(json, MiscHelper.GetJsonOptions());
            return list;
        }

        public static ulong FlagToOffset(EventFlag flag)
        {
            var offset = AddressHelper.GetEventFlagOffset(flag.Flag).Item1;
            return offset;
        }

        public static void SetEventFlag(int flagnum, byte newvalue)
        {
            var baseAddress = AddressHelper.GetEventFlagsOffset();
            Location newloc = new Location()
            {
                Address = baseAddress + AddressHelper.GetEventFlagOffset(flagnum).Item1,
                AddressBit = AddressHelper.GetEventFlagOffset(flagnum).Item2
            };
            Memory.WriteBit(newloc.Address, newloc.AddressBit, newvalue == 0 ? false : true);
            return;
        }
        #endregion
    }
}

using Archipelago.Core.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DSAP.Helpers
{
    public class AddressHelper
    {
        /* aka GameDataMan */
        public static AoBHelper BaseBAoB = new AoBHelper("BaseB",
                [0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x45, 0x33, 0xED, 0x48, 0x8B, 0xF1, 0x48, 0x85, 0xC0],
                "xxx????xxxxxxxxx", 3, 4);
        /* worlddataman? */
        public static AoBHelper BaseEAoB = new AoBHelper("BaseE",
                [0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x88, 0x98, 0x0B, 0x00, 0x00, 0x8B, 0x41, 0x3C, 0xC3],
                "xxx????xxxxxxxxxxx", 3, 4);
        /* AKA "WorldChrManImp" */
        public static AoBHelper BaseXAoB = new AoBHelper("BaseX",
                [0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x48, 0x39, 0x48, 0x68, 0x0f, 0x94, 0xc0, 0xc3],
                "xxx????xxxxxxxx", 3, 4);
        /* aka 141c8adc0 */
        public static AoBHelper EmkAoB = new AoBHelper("EmkHead",
                [0x48, 0x89, 0x05, 0x00, 0x00, 0x00, 0x00, 0xeb, 0x0b, 0x48, 0xc7, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8b, 0x5c, 0x24, 0x50],
                "xxx????xxxxx????xxxxxxxxx", 3, 4);
        public static AoBHelper SoloParamAob = new AoBHelper("SoloParam",
                [0x4C, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x48, 0x63, 0xC9, 0x48, 0x8D, 0x04, 0xC9],
                "xxx????xxxxxxx", 3, 4);
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
        internal static ulong GetPlayerHPAddress()
        {
            var baseB = GetBaseBAddress();
            var next = MiscHelper.OffsetPointer(baseB, 0x10);
            var pointer = Memory.ReadULong(next);
            next = MiscHelper.OffsetPointer(pointer, 0x14);
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
                var next = MiscHelper.OffsetPointer(baseX, 0x68);
                var pointer = Memory.ReadULong(next);
                if (pointer != 0)
                {
                    next = MiscHelper.OffsetPointer(pointer, 0x3e8);
                    return next;
                }
            }
            return 0;
        }
        public static ulong GetItemLotParamOffset()
        {
            var foo = SoloParamAob.Address;
            Log.Logger.Verbose($"solo param location {foo:X}");
            var next = MiscHelper.OffsetPointer(((ulong)foo), 0x570);
            var foo2 = Memory.ReadULong(next);
            next = MiscHelper.OffsetPointer(foo2, 0x38);
            var foo3 = Memory.ReadULong(next);
            return foo3;
        }
        private static ulong GetBonfireOffset()
        {
            var baseAddress = GetEventFlagsOffset();
            var baseBonfire = MiscHelper.OffsetPointer(baseAddress, 0x5B);
            return baseBonfire;
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
        public static ulong GetSaveIdAddress()
        {
            var initoff = AddressHelper.GetEventFlagsOffset();
            int flag = 960;
            var off = AddressHelper.GetEventFlagOffset(flag).Item1 + 3; // 3rd byte after this one
            // here we have 3 bytes of memory available.
            Log.Logger.Debug($"saveid address = {(off + initoff):X}");
            return off + initoff;
        }
        public static ulong GetSaveSeedAddress()
        {
            var initoff = AddressHelper.GetEventFlagsOffset();
            int flag = 960;
            var off = AddressHelper.GetEventFlagOffset(flag).Item1 + 1; // 1st and 2nd byte after this one
            // here we have 3 bytes of memory available.
            Log.Logger.Debug($"Seed address = {(off + initoff):X}");
            return off + initoff;
        }
        public static ulong GetSaveSlotAddress()
        {
            var initoff = AddressHelper.GetEventFlagsOffset();
            int flag = 1960;
            var off = AddressHelper.GetEventFlagOffset(flag).Item1 + 1; // Up to 3 bytes
            // here we have 3 bytes of memory available.
            Log.Logger.Debug($"Slot address = {(off + initoff):X}");
            return off + initoff;
        }

    }
}

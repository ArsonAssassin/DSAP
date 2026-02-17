using Archipelago.Core.Util;
using Serilog;

namespace DSAP.Models
{
    // desc size 4, full alloc length 4, old address 8, old length 4, seed hash 4, slot 4
    internal class DescArea
    {

        [MemoryOffset(0x00)]
        public int DescSize { get; set; }
        [MemoryOffset(0x04)]
        public int FullAllocLength { get; set; }
        [MemoryOffset(0x08)]
        public ulong OldAddress { get; set; }
        [MemoryOffset(0x10)]
        public int OldLength { get; set; }
        [MemoryOffset(0x14)]
        public int SeedHash { get; set; }
        [MemoryOffset(0x18)]
        public int Slot { get; set; }

        public static int size = 0x1c;

        public DescArea(int fullAllocLength, ulong oldAddress, int oldLength, int seedHash, int slot)
        {
            FullAllocLength = fullAllocLength;
            OldAddress = oldAddress;
            OldLength = oldLength;
            SeedHash = seedHash;
            Slot = slot;
            DescSize = size;
            Log.Logger.Debug("Created Desc Area: " + ToString());
        }
        public DescArea()
        {
            DescSize = size;
        }

        override public string ToString()
        {
            string result = $"{{\"size\":{DescSize}, \"FullAllocLength\":{FullAllocLength}, \"OldAddress\":{OldAddress.ToString("X")}, \"OldLength\":{OldLength}, \"SeedHash\":{SeedHash}, \"Slot\":{Slot} }}";
            return result;
        }
    }
}

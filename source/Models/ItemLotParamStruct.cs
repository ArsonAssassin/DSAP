using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DSAP.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 148)]
    public class ItemLotParamStruct
    {
        // Per-item properties (Structure of Arrays)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public int[] LotItemIds; // 8 items * 4 bytes = 32 bytes

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public int[] LotItemCategories; // 8 items * 4 bytes = 32 bytes

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public ushort[] LotItemBasePoints; // 8 items * 2 bytes = 16 bytes

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public ushort[] CumulateLotPoints; // 8 items * 2 bytes = 16 bytes

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public int[] GetItemFlagIds; // Flag ID for each item; 8 items * 4 bytes = 32 bytes

        // Lot-specific properties (as read into the 'lot' object)
        public int LotOverallGetItemFlagId; // 4 bytes

        public int LotCumulateNumFlagId; // 4 bytes

        public byte LotCumulateNumMax; // 1 byte

        public byte LotRarity; // 1 byte

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] LotItemNums; // 8 items * 1 byte = 8 bytes

        // Bitfields for EnableLuck and CumulateReset (for all 8 items)
        public byte EnableLuckBits; // Each bit (0-7) corresponds to EnableLuck for item 0-7

        public byte CumulateResetBits; // Each bit (0-7) corresponds to CumulateReset for item 0-7
                                       // (as per bitArray.Get(i+8) logic)

        public ItemLotParamStruct()
        {

            LotItemIds = new int[8];
            LotItemCategories = new int[8];
            LotItemBasePoints = new ushort[8];
            CumulateLotPoints = new ushort[8];
            GetItemFlagIds = new int[8];
            LotOverallGetItemFlagId = 0;
            LotCumulateNumFlagId = 0;
            LotCumulateNumMax = 0;
            LotRarity = 0;
            LotItemNums = new byte[8];
            EnableLuckBits = 0;
            CumulateResetBits = 0;
        }
    }
}

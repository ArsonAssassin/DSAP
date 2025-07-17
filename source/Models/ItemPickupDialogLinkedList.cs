using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DSAP.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 24)]
    public struct LinkedListItemData
    {
        // 8 bytes: Pointer or ID to the previous item in the logical linked list sequence
        public ulong PreviousItemInLL;

        // 4 bytes: Category identifier for the item
        public uint ItemCategory;

        // 4 bytes: Unique code or identifier for the item
        public uint ItemCode;

        // 4 bytes: Count or quantity of the item
        public uint ItemCount;

        // 4 bytes: An unknown integer field
        public uint UnknownField;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 240)]
    public struct ItemPickupDialogLinkedList
    {
        // 8 bytes: Virtual Function Table pointer
        public ulong Vft;

        // 8 bytes: Pointer or ID to the last element in the linked list that we want to read
        public ulong LastElementLinkedList;

        // 8 bytes: Pointer or ID to the next element that would be allocated in the linked list
        public ulong NextAllocationInLL;

        // 8 bytes: Pointer or ID to the start of the entire linked list
        public ulong StartOfLL;

        // 8 bytes: Unc
        public ulong Unknown1;

        // 8 bytes: Unc
        public ulong Unknown2;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public LinkedListItemData[] Items;


        public ItemPickupDialogLinkedList()
        {
            Items = new LinkedListItemData[8];
        }
    }
}

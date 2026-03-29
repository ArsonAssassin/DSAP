using Archipelago.Core.Util;
using DSAP.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSAP.Models
{
    public class MsgManStruct
    {
        const int HEADER_SIZE = 0x1c;
        public const int OFFSET_SYSTEM_TEXT = 0x3e0;
        public const int OFFSET_ITEM_NAMES = 0x380;
        public const int OFFSET_ITEM_CAPTIONS = 0x378;
        public const int OFFSET_ITEM_DESCRIPTIONS = 0x328;
        public int BufferSize { get; set; }
        public ulong BufferLoc { get; set; }
        public byte[] AllBytes { get; set; }
        public byte[] StringBytes { get; set; }
        public List<byte> NewStringBytes { get; set; }
        public List<(uint id, int stringOffset)> MsgEntries { get; set; }
        internal DescArea DescArea { get; set; }
        public MsgManStruct()
        {
        }
        public void ReadFromBytes(ulong bufferLoc, Func<MsgManStruct,bool> isUsedCondition)
        {
            BufferLoc = bufferLoc;
            ulong BufferSize = Memory.ReadUInt(bufferLoc + 0x4);
            ushort old_buffer_num_spanmaps = Memory.ReadUShort(bufferLoc + 0xc);
            MsgEntries = [];
            DescArea = null;
            NewStringBytes = [];
            AllBytes = Memory.ReadByteArray(BufferLoc, (int)BufferSize);
            // structure of buffer:
            // string end/size@ 0x04
            // num of span maps 0x0c
            // # stroff entries 0x10
            // stroff table  at 0x14
            // span maps start  0x1c
            // span maps have <offset> <min> <max>. Offset is # of entries into string offset table
            // after span maps is string offset table
            // There are (number of items, = 0x110 vanilla) in string offset table
            // Then there are the strings
            // New buffer will need added:
            //  1. (optionally) +c bytes for another map,
            //  2. 0x4 bytes per added id in the string offset table
            //  3. [n] bytes for each string including null char, in Unicode
            // New buffer will need updated:
            //     (optionally) num of span maps
            //     string offset at 0x14
            //     span map entry if not adding a new one
            //     each string offset entry
            //     end of strings/size of all

            ushort num_entries = BitConverter.ToUInt16(AllBytes, 0x10);
            ushort StringOffsetTableOffset = BitConverter.ToUInt16(AllBytes, 0x14);

            // read in the spans
            var spans = new List<(int StrOffIndex, uint startid, uint endid)>();
            for (int i = 0; i < old_buffer_num_spanmaps; i++)
            {
                int ent_offset = HEADER_SIZE + (i * 0xc);
                int ent_index = BitConverter.ToInt32(AllBytes, ent_offset);
                uint ent_startid = BitConverter.ToUInt32(AllBytes, ent_offset + 4);
                uint ent_endid = BitConverter.ToUInt32(AllBytes, ent_offset + 8);
                spans.Add((ent_index, ent_startid, ent_endid));
            }

            int strings_start = StringOffsetTableOffset + (4 * num_entries);

            // read them into our MsgEntries list
            foreach (var span in spans)
            {
                // make an entry for each part of each span
                for (uint i = span.startid; i <= span.endid; i++)
                {
                    int stroff_index = span.StrOffIndex + (int)(i - span.startid);
                    int string_offset = BitConverter.ToInt32(AllBytes, StringOffsetTableOffset + (4 * stroff_index));
                    string_offset -= strings_start; // normalize for size of strings table
                    MsgEntries.Add((i, string_offset));
                }
            }

            uint stringlength = (uint)BufferSize - (uint)strings_start;
            // create and fill StringBytes
            StringBytes = new byte[stringlength];
            Array.Copy(AllBytes, strings_start, StringBytes, 0, stringlength);

            if (isUsedCondition(this))
            {
                int desc_offset = (int)BufferSize;
                ulong desc_area_loc = BufferLoc + (ulong)desc_offset;
                Log.Logger.Debug($"Reading desc from {desc_area_loc:X} at offset {desc_offset}");
                DescArea = Memory.ReadObject<DescArea>(desc_area_loc);
            }
        }

        internal void AddMsg(uint newid, string newString)
        {
            int stringOffset = StringBytes.Length + NewStringBytes.Count; // get index of "current end" of strings
            NewStringBytes.AddRange(Encoding.Unicode.GetBytes(newString + '\0')); // queue our new string to "strings" section
            MsgEntries.Add(((uint)newid, stringOffset));
        }
        internal void UpdateMsg(uint updid, string newString)
        {
            int stringOffset = StringBytes.Length + NewStringBytes.Count; // get index of "current end" of strings
            NewStringBytes.AddRange(Encoding.Unicode.GetBytes(newString + '\0')); // queue our new string to "strings" section
            var entryIndex = MsgEntries.FindIndex(x => (uint)x.id == updid);

            MsgEntries[entryIndex] = (updid, stringOffset);
        }
        internal byte[] GenerateWriteArray(out int shortLength)
        {

            // create new strings byte array
            byte[] newStringBytes = new byte[StringBytes.Length + NewStringBytes.Count];
            Array.Copy(StringBytes, newStringBytes, StringBytes.Length);
            Array.Copy(NewStringBytes.ToArray(), 0, newStringBytes, 0 + StringBytes.Length, NewStringBytes.ToArray().Length);
            StringBytes = newStringBytes;
            this.NewStringBytes = [];

            byte[] StringOffsetTableBytes = new byte[4 * MsgEntries.Count];

            var spans = new List<(int StrOffIndex, uint startid, uint endid)>();
            int j = 0;
            int lastspan = -1;

            foreach (var entry in MsgEntries) {
                if (spans.Count > 0 && spans[lastspan].endid + 1 == entry.id) // if this is at the end of span
                    spans[lastspan] = (spans[lastspan].StrOffIndex, spans[lastspan].startid, entry.id); // add it to span
                else
                {
                    spans.Add((j, entry.id, entry.id));
                    lastspan++;
                }
                j++;
            }

            // create new entries byte array
            byte[] SpansBytes = new byte[12 * spans.Count];

            // get offsets for filling in stroff table

            int num_spans = spans.Count;
            int num_entries = MsgEntries.Count;
            uint string_offset_table_offset = (uint)(HEADER_SIZE + 0xc * num_spans);
            uint strings_start = (uint)(string_offset_table_offset + 0x4 * num_entries);
            uint strings_end = (uint)(strings_start + StringBytes.Length);

            
            // fill the Span maps table with all the spans pointing to each id's index into the <string offset table>
            Log.Logger.Debug($"Span maps count = {num_spans}, size = 0x{SpansBytes.Length:X}");
            for (int i = 0; i < spans.Count; i++)
            {
                Array.Copy(BitConverter.GetBytes(spans[i].StrOffIndex), 0, SpansBytes, (12 * i), sizeof(uint));
                Array.Copy(BitConverter.GetBytes(spans[i].startid), 0, SpansBytes, (12 * i) + 4, sizeof(uint));
                Array.Copy(BitConverter.GetBytes(spans[i].endid), 0, SpansBytes, (12 * i) + 8, sizeof(uint));
            }
            // fill in String Offset table with offsets into the Strings array
            Log.Logger.Debug($"stroffs count = {MsgEntries.Count}, size = {(MsgEntries.Count * 0x4):X}");
            for (int i = 0; i < MsgEntries.Count; i++)
            {
                var offset = 0;
                if (MsgEntries[i].stringOffset >= 0)
                    offset = (int)(MsgEntries[i].stringOffset + strings_start);
                Array.Copy(BitConverter.GetBytes(offset), 0, StringOffsetTableBytes, (4 * i), sizeof(uint));
            }

            byte[] header = new byte[HEADER_SIZE]; // 1c bytes of header

            int size = HEADER_SIZE + SpansBytes.Length + StringOffsetTableBytes.Length + StringBytes.Length;
            
            // fill header
            Array.Copy(AllBytes, 0, header, 0, HEADER_SIZE);

            // string end/size@ 0x04
            // num of span maps 0x0c
            // # stroff entries 0x10
            // stroff table  at 0x14
            // span maps start  0x1c

            
            Array.Copy(BitConverter.GetBytes(size), 0, header, 0x4, sizeof(uint));
            Array.Copy(BitConverter.GetBytes(num_spans), 0, header, 0xc, sizeof(int));
            Array.Copy(BitConverter.GetBytes(num_entries), 0, header, 0x10, sizeof(int));
            Array.Copy(BitConverter.GetBytes(string_offset_table_offset), 0, header, 0x14, sizeof(int));

            uint desc_offset = (uint)size;
            Log.Logger.Debug($"new desc area offset: {desc_offset}");
            int total_size = size + DescArea.size;

            var seedHash = MiscHelper.HashSeed(App.Client.CurrentSession.RoomState.Seed);
            var slot = App.Client.CurrentSession.ConnectionInfo.Slot;
            if (DescArea == null) // if this is first update, generate desc area
            {
                DescArea = new DescArea(total_size, BufferLoc, BufferSize, seedHash, slot);
            }
            else // if it isn't, update desc area instead (bufferLoc and Size always point to original params)
            {
                DescArea.FullAllocLength = total_size;
                DescArea.SeedHash = seedHash;
                DescArea.Slot = slot;
            }

            // finaly, copy it all in.
            byte[] result = new byte[total_size];
            Array.Copy(header, 0, result, 0x0, HEADER_SIZE);
            Array.Copy(SpansBytes, 0, result, HEADER_SIZE, SpansBytes.Length);
            Array.Copy(StringOffsetTableBytes, 0, result, string_offset_table_offset, StringOffsetTableBytes.Length);
            Array.Copy(StringBytes, 0, result, strings_start, StringBytes.Length);
            Array.Copy(DescArea.GetBytes(), 0, result, desc_offset, DescArea.GetBytes().Length);
            
            shortLength = size;
            return result;
        }
    }
}

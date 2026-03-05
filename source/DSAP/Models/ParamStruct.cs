using Archipelago.Core.Util;
using DSAP.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSAP.Models
{
    public class ParamStruct<ParamT> where ParamT : IParam, new()
    {
        const int PROLOGUE_SIZE = 0x10;
        const int HEADER_SIZE = 0x30;
        public int BufferSize { get; set; }
        public ulong BufferLoc { get; set; }
        public byte[] AllBytes { get; set; }
        public byte[] ParamBytes { get; set; }
        public List<byte> AddedParamBytes { get; set; }
        public byte[] StringBytes { get; set; }
        public StringBuilder newStrings { get; set; }
        public List<(uint id, uint paramOffset, int strOffset)> ParamEntries { get; set; }
        private Type type { get; set; }
        internal DescArea DescArea { get; set; }
        public ParamStruct()
        {
        }
        public void ReadFromBytes(ulong bufferLoc, int bufferSize, Func<ParamStruct<ParamT>, bool> isUsedCondition)
        {
            BufferLoc = bufferLoc;
            BufferSize = bufferSize;
            ParamEntries = [];
            AddedParamBytes = [];
            DescArea = null;
            newStrings = new StringBuilder("");
            type = typeof(ParamT);
            AllBytes = Memory.ReadByteArray(BufferLoc - 0x10, (int)BufferSize + 0x10);
            int base_offset = 0x10;
            int string_offset = BitConverter.ToInt32(AllBytes, base_offset + 0x0);
            ushort paramsOffset = BitConverter.ToUInt16(AllBytes, base_offset + 0x4);
            ushort num_entries = BitConverter.ToUInt16(AllBytes, base_offset + 0xA);

            for (int i = 0; i < num_entries; i++)
            {
                int ent_offset = base_offset + 0x30 + (i * 0xc);
                uint ent_id = BitConverter.ToUInt32(AllBytes, ent_offset);
                uint ent_param_offset = BitConverter.ToUInt32(AllBytes, ent_offset + 4) - paramsOffset;
                int ent_string_offset = BitConverter.ToInt32(AllBytes, ent_offset + 8) - string_offset;
                ParamEntries.Add((ent_id, ent_param_offset, ent_string_offset));
            }
            uint endOfParamsOffset = (uint)string_offset;
            uint paramsLength = endOfParamsOffset - paramsOffset;
            // create and fill ParamBytes
            ParamBytes = new byte[paramsLength];
            Array.Copy(AllBytes, base_offset + paramsOffset, ParamBytes, 0, paramsLength);


            uint endOfStringsOffset = BitConverter.ToUInt32(AllBytes, 0);
            uint stringlength = endOfStringsOffset - (uint)string_offset;
            // create and fill StringBytes
            StringBytes = new byte[stringlength];
            Array.Copy(AllBytes, base_offset + string_offset, StringBytes, 0, stringlength);

            if (isUsedCondition(this))
            {
                int desc_offset = BufferSize + (0x8 * num_entries) + 0xf;
                ulong desc_area_loc = BufferLoc + (ulong)desc_offset;
                Log.Logger.Information($"Reading desc from {desc_area_loc} at offset {desc_offset}");
                DescArea = Memory.ReadObject<DescArea>(desc_area_loc);
            }
        }

        internal void AddParam(uint newid, byte[] newParamBytes, byte[] stringBytes)
        {
            uint paramOffset = (uint)ParamBytes.Length + (uint)AddedParamBytes.Count; // get index of "current end" of param bytes
            AddedParamBytes.AddRange(newParamBytes); // queue our new bytes to add to "params" section
            int stringOffset = StringBytes.Length + newStrings.ToString().Length; // get index of "current end" of strings
            newStrings.Append(Encoding.ASCII.GetString(stringBytes)); // queue our new string to "strings" section
            ParamEntries.Add(((uint)newid, paramOffset, stringOffset));
        }
        internal byte[] GenerateWriteArray(out int shortLength)
        {
            // create new entries byte array
            byte[] EntriesBytes = new byte[12 * ParamEntries.Count];

            // create new params byte array
            byte[] newParamBytes = new byte[ParamBytes.Length + AddedParamBytes.Count];
            Array.Copy(ParamBytes, newParamBytes, ParamBytes.Length);
            Array.Copy(AddedParamBytes.ToArray(), 0, newParamBytes, 0 + ParamBytes.Length, AddedParamBytes.ToArray().Length);
            ParamBytes = newParamBytes;
            AddedParamBytes = [];

            // create new strings byte array
            byte[] newStringBytes = new byte[StringBytes.Length + Encoding.ASCII.GetBytes(newStrings.ToString()).Length];
            Array.Copy(StringBytes, newStringBytes, StringBytes.Length);
            Array.Copy(Encoding.ASCII.GetBytes(newStrings.ToString()), 0, newStringBytes, 0 + StringBytes.Length, Encoding.ASCII.GetBytes(newStrings.ToString()).Length);
            StringBytes = newStringBytes;
            newStrings = new StringBuilder("");

            // get offsets for filling in EntriesBytes
            ushort num_entries = (ushort)ParamEntries.Count;
            uint params_offset = (uint)(HEADER_SIZE + EntriesBytes.Length);
            uint string_offset = (uint)(params_offset + ParamBytes.Length);
            uint endTable_offset = (uint)((string_offset + StringBytes.Length + 0xF) & 0xFFFFFFF0); // round to quadword boundary

            Log.Logger.Debug($"ParamEntries count = {ParamEntries.Count}, size = {EntriesBytes.Length}");
            for (int i = 0; i < ParamEntries.Count; i++)
            {
                Array.Copy(BitConverter.GetBytes(ParamEntries[i].id), 0, EntriesBytes, (12 * i), sizeof(uint));
                Array.Copy(BitConverter.GetBytes(params_offset + ParamEntries[i].paramOffset), 0, EntriesBytes, (12 * i) + 4, sizeof(uint));
                int stroffset = (int)string_offset + ParamEntries[i].strOffset;
                if (ParamEntries[i].strOffset < 0)
                    stroffset = 0; // write a 0 if element didn't have a string offset originally.
                Array.Copy(BitConverter.GetBytes(stroffset), 0, EntriesBytes, (12 * i) + 8, sizeof(uint));
            }

            
            byte[] prologue = new byte[PROLOGUE_SIZE]; // 10 bytes before area
            byte[] header = new byte[HEADER_SIZE]; // 30 bytes of header

            int regular_size = HEADER_SIZE + EntriesBytes.Length + ParamBytes.Length + StringBytes.Length;
            
            // fill prologue -> offset of end table from "start"
            Array.Copy(BitConverter.GetBytes((ulong)regular_size), prologue, sizeof(ulong));
            // fill header
            Array.Copy(AllBytes, PROLOGUE_SIZE, header, 0, HEADER_SIZE);


            Array.Copy(BitConverter.GetBytes(string_offset), 0, header, 0x0, sizeof(uint));
            Array.Copy(BitConverter.GetBytes(params_offset), 0, header, 0x4, sizeof(uint));
            Array.Copy(BitConverter.GetBytes(num_entries), 0, header, 0xA, sizeof(ushort));

            byte[] endTable = new byte[8 * ParamEntries.Count]; // binary search table after strings
            for (int i = 0; i < ParamEntries.Count; i++)
            {
                Array.Copy(BitConverter.GetBytes(ParamEntries[i].id), 0, endTable, (8 * i), sizeof(uint));
                Array.Copy(BitConverter.GetBytes(i), 0, endTable, (8 * i) + 4, sizeof(uint));
            }

            uint desc_offset = (uint)(prologue.Length + regular_size + 0xf + endTable.Length);
            Log.Logger.Information($"new desc area offset: {desc_offset}");
            int total_size = PROLOGUE_SIZE + regular_size + 0xf + endTable.Length + DescArea.size;

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
            Array.Copy(prologue, 0, result, 0, PROLOGUE_SIZE);
            Array.Copy(header, 0, result, PROLOGUE_SIZE + 0x0, HEADER_SIZE);
            Array.Copy(EntriesBytes, 0, result, PROLOGUE_SIZE + HEADER_SIZE, EntriesBytes.Length);
            Array.Copy(ParamBytes, 0, result, PROLOGUE_SIZE + params_offset, ParamBytes.Length);
            Array.Copy(StringBytes, 0, result, PROLOGUE_SIZE + string_offset, StringBytes.Length);
            Array.Copy(endTable, 0, result, PROLOGUE_SIZE + endTable_offset, endTable.Length);
            Array.Copy(DescArea.GetBytes(), 0, result, desc_offset, DescArea.GetBytes().Length);
            
            shortLength = regular_size;
            return result;
        }
    }
}

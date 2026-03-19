using Archipelago.Core.Util;
using DSAP.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSAP.Helpers
{
    internal class MsgManHelper
    {
        // return = whether reload is required
        public static bool ReadMsgManStruct(out MsgManStruct result, int msgManOffset, Func<MsgManStruct, bool> isUsedCondition)
        {
            result = new MsgManStruct();


            ulong MsgMan = Memory.ReadULong(0x141c7e3e8);
            ulong BufferLoc = Memory.ReadULong(MsgMan + (ulong)msgManOffset);
            
            result.ReadFromBytes(BufferLoc, isUsedCondition);
            if (result.DescArea != null)
            {
                bool reloadRequired = MiscHelper.ValidateDescArea(result.DescArea, "Gift Names");
                if (!reloadRequired)
                {
                    return false;
                }
                Log.Logger.Warning($"overwriting Gift Names");
                result.ReadFromBytes(result.DescArea.OldAddress, isUsedCondition);
            }
            return true;
        }
        public static bool WriteFromMsgManStruct(MsgManStruct input, int msgManOffset)
        {
            ulong MsgMan = Memory.ReadULong(0x141c7e3e8);
            ulong oldBufferLoc = Memory.ReadULong(MsgMan + (ulong)msgManOffset);
            int oldBufferSize = Memory.ReadInt(oldBufferLoc + 0x4);
            bool hadOldUpdatedArea = false;
            // if "desc area" exists, then we're reloading the new updated area. Set up for later Free().
            if (input.DescArea != null)
            {
                hadOldUpdatedArea = true;
                oldBufferSize = input.DescArea.FullAllocLength;
                oldBufferLoc = input.BufferLoc;
            }

            byte[] newBytes = input.GenerateWriteArray(out int shortLength);
            ulong allocArea = (ulong)Memory.Allocate((uint)newBytes.Length);
            Log.Logger.Debug($"Allocated {newBytes.Length:X} bytes at {allocArea:X}");

            Memory.WriteByteArray(allocArea, newBytes);
            ulong newBufferLoc = allocArea; // get past prologue

            
            Log.Logger.Debug($"Overwrite Item Names @ {oldBufferLoc:X} to {allocArea:X}");

            /* Then switch out the pointer */
            Memory.Write(MsgMan + (ulong)msgManOffset, newBufferLoc);

            if (hadOldUpdatedArea)
            {
                Memory.FreeMemory((nint)oldBufferLoc);
                Log.Logger.Debug($"Free old item names @ {oldBufferLoc:X}");
            }
            return true;
        }
    }
}

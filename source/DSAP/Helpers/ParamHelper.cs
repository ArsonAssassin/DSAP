using Archipelago.Core.Util;
using DSAP.Models;
using Serilog;
using System;

namespace DSAP.Helpers
{
    internal class ParamHelper
    {
        // return = whether reload is required
        public static bool ReadFromBytes<ParamT>(out ParamStruct<ParamT> result, int soloParamOffset, Func<ParamStruct<ParamT>, bool> isUsedCondition) where ParamT : IParam, new()
        {
            result = new ParamStruct<ParamT>();

            ulong ResCapLoc = Memory.ReadULong((ulong)(AddressHelper.SoloParamAob.Address + soloParamOffset));
            int BufferSize = (int)Memory.ReadUInt(ResCapLoc + 0x30);
            ulong BufferLoc = Memory.ReadULong(ResCapLoc + 0x38);

            result.ReadFromBytes(BufferLoc, BufferSize, isUsedCondition);
            if (result.DescArea != null)
            {
                bool reloadRequired = MiscHelper.ValidateDescArea(result.DescArea, typeof(ParamT).Name);
                if (!reloadRequired)
                {
                    return false;
                }
                Log.Logger.Warning($"overwriting {typeof(ParamT).Name}");
                result.ReadFromBytes(result.DescArea.OldAddress, result.DescArea.OldLength, isUsedCondition);
            }
            return true;
        }
        public static bool WriteFromParamSt<ParamT>(ParamStruct<ParamT> input, int soloParamOffset) where ParamT : IParam, new()
        {
            ulong resCapLoc = Memory.ReadULong((ulong)(AddressHelper.SoloParamAob.Address + soloParamOffset));
            int oldBufferSize = (int)Memory.ReadUInt(resCapLoc + 0x30);
            ulong oldBufferLoc = Memory.ReadULong(resCapLoc + 0x38);
            bool hadOldUpdatedArea = false;
            // if "desc area" exists, then we're reloading the new updated area. Set up for later Free().
            if (input.DescArea != null)
            {
                hadOldUpdatedArea = true;
                oldBufferSize = input.DescArea.FullAllocLength;
                oldBufferLoc = input.BufferLoc - 0x10;
            }

            byte[] newBytes = input.GenerateWriteArray(out int shortLength);
            ulong allocArea = (ulong)Memory.Allocate((uint)newBytes.Length);
            Log.Logger.Debug($"Allocated {newBytes.Length.ToString("X")} bytes at {allocArea.ToString("X")}");

            Memory.WriteByteArray(allocArea, newBytes);
            ulong newBufferLoc = allocArea + 0x10; // get past prologue

            
            Log.Logger.Debug($"Overwrite {typeof(ParamT).Name} @ {oldBufferLoc.ToString("X")} to {allocArea.ToString("X")}");

            /* Then switch out the pointer */
            Memory.Write(resCapLoc + 0x38, newBufferLoc);
            Memory.Write(resCapLoc + 0x30, shortLength);

            if (hadOldUpdatedArea)
            {
                Memory.FreeMemory((nint)oldBufferLoc);
                Log.Logger.Debug($"Free old {typeof(ParamT).Name} @ {oldBufferLoc.ToString("X")}");
            }
            return true;
        }
    }
}

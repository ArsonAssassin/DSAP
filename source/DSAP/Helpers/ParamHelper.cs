using Archipelago.Core.AvaloniaGUI.Logging;
using Archipelago.Core.Util;
using DSAP.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAP.Helpers
{
    internal class ParamHelper
    {
        public const int EquipParamGoods_offset = 0xf0;
        // return = whether reload is required
        public static bool ReadFromBytes<ParamT>(out ParamSt<ParamT> result, int soloParamOffset) where ParamT : Param, new()
        {
            result = new ParamSt<ParamT>();

            ulong ResCapLoc = Memory.ReadULong((ulong)(AddressHelper.SoloParamAob.Address + soloParamOffset));
            int BufferSize = (int)Memory.ReadUInt(ResCapLoc + 0x30);
            ulong BufferLoc = Memory.ReadULong(ResCapLoc + 0x38);

            result.ReadFromBytes(BufferLoc, BufferSize);
            if (result.DescArea != null)
            {
                bool reloadRequired = MiscHelper.ValidateDescArea(result.DescArea, "EquipParamGoods");
                if (!reloadRequired)
                {
                    return false;
                }
                Log.Logger.Warning($"overwriting EquipParamGoods");
                result.ReadFromBytes(result.DescArea.OldAddress, result.DescArea.OldLength);
            }
            return true;
        }
        public static bool WriteFromParamSt<ParamT>(ParamSt<ParamT> input, int soloParamOffset) where ParamT : Param, new()
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

            byte[] newBytes = input.generateWriteArray(out int shortLength);
            ulong allocArea = (ulong)Memory.Allocate((uint)newBytes.Length);
            Log.Logger.Debug($"Allocated {newBytes.Length.ToString("X")} bytes at {allocArea.ToString("X")}");

            Memory.WriteByteArray(allocArea, newBytes);
            ulong newBufferLoc = allocArea + 0x10; // get past prologue

            
            Log.Logger.Debug($"Overwrite EquipParamGoods @ {oldBufferLoc.ToString("X")} to {allocArea.ToString("X")}");

            /* Then switch out the pointer */
            Memory.Write(resCapLoc + 0x38, newBufferLoc);
            Memory.Write(resCapLoc + 0x30, newBytes.Length);

            if (hadOldUpdatedArea)
            {
                Memory.FreeMemory((nint)oldBufferLoc);
                Log.Logger.Debug($"Free old EquipParamGoods @ {oldBufferLoc.ToString("X")}");
            }
            return true;
        }
    }
}

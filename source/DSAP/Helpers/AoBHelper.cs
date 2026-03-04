using Archipelago.Core.Util;
using Serilog;
using System;

namespace DSAP.Helpers
{
    public class AoBHelper
    {
        public AoBHelper(string key, byte[] pattern, string mask, ushort operandOffset, ushort operandLength)
        {
            Key = key;
            Pattern = pattern;
            Mask = mask;
            OperandOffset = operandOffset;
            OperandLength = operandLength;
            _pointer = nint.Zero;
        }
        public string Key;
        public byte[] Pattern;
        public string Mask;

        ushort OperandOffset;
        ushort OperandLength;
        private nint _pointer;
        private nint Pointer
        {
            /* Get the position of the singleton/global pointer in memory */
            get
            {
                /* Cache the result. This looks in memory that is static after program load so it will not change. */
                if (_pointer == nint.Zero)
                {
                    /* First, AoB search. This finds an instruction that references the global pointer with relative addressing */
                    nint baseAddress = (nint)AddressHelper.GetBaseAddress();
                    nint result = Memory.FindSignature(baseAddress, 0x1000000, Pattern, Mask);
                    Log.Logger.Debug($"{Key}_aob found: 0x{result.ToInt64().ToString("X")}");
                    if (result != nint.Zero)
                    {
                        /* Get the relative offset */
                        uint offset = BitConverter.ToUInt32(Memory.ReadByteArray((ulong)(result + OperandOffset), OperandLength), 0);
                        if (offset != 0)
                        {
                            Log.Logger.Debug($"{Key}_offset found: 0x{offset.ToString("X")}");
                            /* Instruction position + instruction size + relative offset = global pointer position */
                            nint globalPtr = (nint)(result + offset + (OperandOffset + OperandLength));
                            Log.Logger.Debug($"{Key}_ptr found: 0x{globalPtr.ToInt64().ToString("X")}");
                            _pointer = globalPtr; /* cache the result */
                        }
                        else
                        {
                            Log.Logger.Warning($"{Key}_offset not found: 0x{offset.ToString("X")}");
                        }
                    }
                    else
                    {
                        Log.Logger.Error($"{Key}_aob not found: 0x{result.ToInt64().ToString("X")}");
                    }
                }
                return _pointer;
            }
            set
            {
                _pointer = value;
            }
        }
        /* Dereference the global pointer - this can't be cached. */
        public nint Address
        { 
            get 
            {
                if (Pointer != nint.Zero)
                {
                    ulong result = Memory.ReadULong((ulong)_pointer);
                    //Log.Logger.Warning($"aob {Key} @ {_pointer}");
                    return (nint)result;
                }
                return nint.Zero;
            } 
        }
    }
}

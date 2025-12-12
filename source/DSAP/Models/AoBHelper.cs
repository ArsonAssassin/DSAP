using Archipelago.Core.Util;
using Serilog;
using System;

namespace DSAP.Models
{
    class AoBHelper
    {
        public AoBHelper(string key, byte[] pattern, string mask)
        {
            Key = key;
            Pattern = pattern;
            Mask = mask;
            _address = IntPtr.Zero;
        }
        public string Key;
        public byte[] Pattern;
        public string Mask;
        private IntPtr _address;
        public IntPtr Address
        {
            get
            {
                /* Cache the AoB search result. This looks in memory that is static after program load so it will not change. */
                if (_address == IntPtr.Zero)
                {
                    IntPtr baseAddress = (IntPtr)Helpers.GetBaseAddress();
                    IntPtr result = Memory.FindSignature(baseAddress, 0x1000000, this.Pattern, this.Mask);
                    _address = result;
                    Log.Logger.Debug($"{Key}_aob set: {result.ToInt64()}/0x{result.ToInt64().ToString("X")}");
                }
                return _address;
            }
            set
            {
                _address = value;
            }
        }
    }
}

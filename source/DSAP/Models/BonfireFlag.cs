using Archipelago.Core.Json;
using Archipelago.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DSAP.Models
{
    public class BonfireFlag : EventFlag
    {
        [JsonConverter(typeof(HexToUIntConverter))]
        public uint Offset { get; set; }
        public int AddressBit { get;set; }
        public new ulong Flag 
        {
            get 
            {
                return Helpers.OffsetPointer(Helpers.GetEventFlagsOffset(), (int)Offset); 
            }
        }
    }
}

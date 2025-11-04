using Archipelago.Core.Util;
using System.Text.Json.Serialization;
using Archipelago.Core.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAP.Models
{
    public class Boss
    {
        public string Name { get; set; }
        [JsonConverter(typeof(HexToUIntConverter))]
        public uint Offset { get; set; }
        public int AddressBit { get; set; }
        public int LocationId { get; set; }
    }
}

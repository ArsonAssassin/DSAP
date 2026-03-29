using Archipelago.Core.Json;
using System.Text.Json.Serialization;

namespace DSAP.Models
{
    public class BonfireWarp : EventFlag
    {
        [JsonConverter(typeof(HexToUIntConverter))]
        public uint Offset { get; set; }
        public int AddressBit { get;set; }
        public int Flag { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int DsrId { get; set; }
    }
}

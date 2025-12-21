using System.Text.Json.Serialization;
using static DSAP.Enums;

namespace DSAP.Models
{
    public class DsrEvent : EventFlag
    {
        public int Eventid { get; set; }
        public int Eventslot {  get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DsrEventType Type { get; set; }
    }
}

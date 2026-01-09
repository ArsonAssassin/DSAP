using System.Text.Json.Serialization;
using static DSAP.Enums;

namespace DSAP.Models
{
    public class DsrEvent
    {
        public string Name { get; set; }
        public int Itemid { get; set; }
        public int Flag { get; set; }
        public int Locid { get; set; }
        public int Eventid { get; set; }
        public int Eventslot {  get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DsrEventType Type { get; set; }
    }
}

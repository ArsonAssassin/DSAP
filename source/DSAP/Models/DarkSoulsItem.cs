using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static DSAP.Enums;

namespace DSAP.Models
{
    public class DarkSoulsItem
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int StackSize { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemUpgrade UpgradeType { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public DSItemCategory Category { get; set; }
        public int ApId { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAP.Models
{
    public class ItemLot
    {
        public List<ItemLotItem> Items { get; set; }
        public int GetItemFlagId { get; set; }
        public int CumulateNumFlagId { get; set; }
        public byte CumulateNumMax { get; set; }
        public byte Rarity { get; set; }

        public short numPlaced { get; set; } = 0;

    }
}

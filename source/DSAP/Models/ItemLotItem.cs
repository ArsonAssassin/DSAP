using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAP.Models
{
    public class ItemLotItem
    {
        public int LotItemId { get; set; }
        public int LotItemCategory { get; set; }
        public ushort LotItemBasePoint { get; set; }
        public ushort CumulateLotPoint { get; set; }
        public int GetItemFlagId { get; set; }
        public byte LotItemNum { get; set; }
        public bool EnableLuck {  get; set; }
        public bool CumulateReset { get; set; }
    }
}

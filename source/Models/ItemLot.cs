using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DSAP.Models
{
    public class ItemLot
    {
        public ItemLotParamStruct itemLotParam;
        public ulong startAddress;

        public ItemLot(ItemLotParamStruct itemLotParam, ulong startAddress)
        {
            this.itemLotParam = itemLotParam;
            this.startAddress = startAddress;
        }
    }
}

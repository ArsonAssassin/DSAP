using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAP.Models
{
    internal class EquipGoodsParam : Param
    {
        public static uint Size { get; set; } = 0x4c;
        public EquipGoodsParam() : base()
        {
        }
    }
}

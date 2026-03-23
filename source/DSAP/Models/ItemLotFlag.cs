using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAP.Models
{
    public class ItemLotFlag : EventFlag
    {
        public bool IsEnabled { get; set; } = true;
        public int ItemLotParamId { get; set; } = 0;
    }
}

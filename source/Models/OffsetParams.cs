using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAP.Models
{
    public class OffsetParams
    {
        public byte[] Pattern { get; set; }
        public string Mask { get; set; }
        public int OffsetPosition { get; set; }
        public bool AddRelativeOffset { get; set; }

        public int FinalAddressOffset { get; set; }

        public int SearchSize { get; set; }

        public OffsetParams(
            byte[] pattern,
            string mask,
            int offsetPosition = 3,
            bool addRelativeOffset = true,
            int finalAddressOffset = 7,
            int searchSize = 0x1000000)
        {
            Pattern = pattern;
            Mask = mask;
            OffsetPosition = offsetPosition;
            AddRelativeOffset = addRelativeOffset;
            FinalAddressOffset = finalAddressOffset;
            SearchSize = searchSize;
        }
    }
}

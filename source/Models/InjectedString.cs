using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DSAP.Models
{
    public class InjectedString
    {
        public String text;

        public IntPtr injectedStringLoc;

        public ulong stringOffsetLoc;

        public ulong originalStringOffset;

        public InjectedString(string text, IntPtr injectedStringLoc, ulong stringOffsetLoc, ulong originalStringOffset)
        {
            this.text = text;
            this.injectedStringLoc = injectedStringLoc;
            this.stringOffsetLoc = stringOffsetLoc;
            this.originalStringOffset = originalStringOffset;
        }
    }
}

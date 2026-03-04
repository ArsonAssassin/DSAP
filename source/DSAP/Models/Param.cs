using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DSAP.Models
{
    public abstract class Param
    {
        public virtual uint Size { get; set; }

        public Param()
        {
        }
    }
}

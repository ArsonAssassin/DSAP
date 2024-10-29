using Archipelago.Core.Util;
using Archipelago.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAP
{
    public class DarkSoulsClient : IGameClient
    {
        public bool IsConnected { get; set; }
        public int ProcId { get; set; }
        public string ProcessName { get; set; }
        public DarkSoulsClient()
        {
            ProcessName = "DarkSoulsRemastered";
            ProcId = Memory.GetProcIdFromExe(ProcessName);
        }
        public bool Connect()
        {
            Console.WriteLine($"Connecting to {ProcessName}");
            if (ProcId == 0)
            {
                Console.WriteLine($"{ProcessName} not found.");
                return false;
            }
            IsConnected = true;
            return true;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.Core;
using Archipelago.Core.Util;

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
            ProcId = Helpers.Bootstrap(ProcessName);
        }
        public bool Connect()
        {
            Console.WriteLine($"Connecting to {ProcessName}");            
            if (ProcId == 0)
            {
                Console.WriteLine($"{ProcessName} not found.");
                Console.WriteLine("Press any key to exit.");
                Console.Read();
                System.Environment.Exit(0);
                return false;
            }
            return true;
        }

    }
}

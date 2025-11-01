using Archipelago.Core.Util;
using Archipelago.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

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
        }
        public bool Connect()
        {
            ProcId = Memory.GetProcIdFromExe(ProcessName);

            if (ProcId == 0)
            {
                Log.Error($"{ProcessName} not found.");
                IsConnected = false;
                return false;
            }
            /* Log first connection */
            if (!IsConnected)
                Log.Information($"Connecting to {ProcessName}");
            IsConnected = true;
            return true;
        }

    }
}

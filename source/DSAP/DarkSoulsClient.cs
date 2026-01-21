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
        public int ProcId { get; set; } = 0;
        public List<int> ProcIds { get; set; } = [];
        public string ProcessName { get; set; }
        public DarkSoulsClient()
        {
            ProcessName = "DarkSoulsRemastered";
        }
        public bool Connect()
        {
            try
            {
                ProcIds = Memory.GetProcIdsFromExe(ProcessName);
            }
            catch
            {
                Log.Error($"{ProcessName} not found.");
                IsConnected = false;
                return false;
            }

            if (ProcIds.Count == 0)
            {
                Log.Error($"{ProcessName} not found.");
                IsConnected = false;
                return false;
            }

            if (ProcIds.Count == 1)
                ProcId = ProcIds[0];

            // If there are multiple, wait for user to choose.
            if (ProcIds.Count > 1 && ProcId == 0)
            {
                Log.Error($"Multiple instance of DSR detected. Choose one by typing /pid value. PID list:");
                foreach (int id in ProcIds)
                {
                    Log.Error($"{id}");
                }

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

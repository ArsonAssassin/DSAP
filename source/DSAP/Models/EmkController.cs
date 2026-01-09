using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DSAP.Enums;

namespace DSAP.Models
{
    public class EmkController
    {
        public bool Deactivated; // Whether the event has been "deactivated"
        public string Name {  get; set; }
        public DsrEventType Type { get; set; }
        public int ApId { get; set; } // AP item id that clears this event lock
        public int Eventid { get; set; }
        public int Eventslot { get; set; }
        public int MapId3 { get; set; } // 3-digit map+area id
        public ulong Saved_Ptr { get; set; } // pointer to the saved event, if it has been detached.
        public bool HasKey { get; set; } // whether player has the "key" to the event, and to stop "locking" it

        public EmkController(string name, DsrEventType type, int eventid, int eventslot, int apid)
        {
            Deactivated = false;
            HasKey = false;
            Name = name;
            Type = type;
            Eventid = eventid;
            Eventslot = eventslot;
            Saved_Ptr = 0;
            ApId = apid;
            // Save the 2nd-4th digits of the event number.
            // If we eventually add map-agnostic events, or multi-map events (e.g. darkroot garden elevator),
            // then make this an explicit list of maps defined in the json instead.
            MapId3 = (Eventid / 10000) % 1000; 

        }
        public void Unlock()
        {
            HasKey = true;
            Log.Logger.Debug($"Unlocking Event <{Name}> id:slot <{Eventid}:{Eventslot}>");
        }
    }
}

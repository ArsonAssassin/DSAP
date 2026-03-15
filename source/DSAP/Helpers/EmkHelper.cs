using Archipelago.Core.Util;
using DSAP.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DSAP.Helpers
{
    internal class EmkHelper
    {
        static HashSet<Tuple<int, int>> PrevEvents = [];
        static ulong prevEventHeadPtr = ulong.MinValue;
        static ulong prevEventHead = ulong.MinValue;
        internal static void CheckEventsList()
        {
            HashSet<Tuple<int, int>> ExistingEvents = [];

            ulong eventhead_ptr = AddressHelper.GetEmkHeadAddress();
            int numevents = 0;
            if (eventhead_ptr != prevEventHeadPtr)
                Log.Logger.Debug($"eventheadptr changed from {prevEventHeadPtr:X} to {eventhead_ptr:X}");
            if (eventhead_ptr != 0)
            {
                ulong eventhead = Memory.ReadULong((ulong)eventhead_ptr);
                if (eventhead != prevEventHead)
                    Log.Logger.Debug($"eventhead changed from {prevEventHead:X} to {eventhead:X}");
                prevEventHead = eventhead;
                prevEventHeadPtr = eventhead_ptr;

                // read every event into the hashset
                // detect number of differences => numevents
                for (ulong thisEmk = eventhead; thisEmk != 0; thisEmk = Memory.ReadULong((ulong)thisEmk + 0x68))
                {

                    numevents++;
                    if (numevents > 2000)
                    {
                        Log.Logger.Warning($"c events:{numevents}");
                        Log.Logger.Warning($"thisemk = :{thisEmk:X}");
                        break;
                    }

                    int eventid = Memory.ReadInt(thisEmk + 0x30);
                    int eventslot = Memory.ReadByte(thisEmk + 0x34);

                    ExistingEvents.Add(new Tuple<int, int>(eventid, eventslot));
                }
            }
            

            if (!ExistingEvents.SetEquals(PrevEvents))
            {
                bool printedfirst = false;
                int eventsRemoved = 0;
                Tuple<int, int> lastemk = new Tuple<int, int>(0, 0);
                foreach (Tuple<int, int> emk in PrevEvents)
                {
                    if (!ExistingEvents.Contains(emk))
                    {
                        eventsRemoved++;
                        if (!printedfirst)
                        {
                            Log.Logger.Debug($"(F) - {emk.Item1}:{emk.Item2}");
                            printedfirst = true;
                        }
                        lastemk = emk;
                    }
                }
                if (eventsRemoved > 1)
                {
                    Log.Logger.Debug($"(L) - {lastemk.Item1}:{lastemk.Item2}");
                }
                printedfirst = false;
                int eventsAdded = 0;
                lastemk = new Tuple<int, int>(0, 0);
                foreach (Tuple<int, int> emk in ExistingEvents)
                {
                    if (!PrevEvents.Contains(emk))
                    {
                        eventsAdded++;
                        if (!printedfirst)
                        {
                            Log.Logger.Debug($"(F) + {emk.Item1}:{emk.Item2}");
                            printedfirst = true;
                        }
                        lastemk = emk;
                    }
                }
                if (eventsAdded > 1)
                {
                    Log.Logger.Debug($"(L) + {lastemk.Item1}:{lastemk.Item2}");
                }
                Log.Logger.Debug($"Events from {PrevEvents.Count} to {ExistingEvents.Count} > - {eventsRemoved} + {eventsAdded}");

                PrevEvents = new HashSet<Tuple<int, int>>(ExistingEvents);
            }
        }

        static uint cached_mapid3;
        internal static void ManageEventsList(List<EmkController> emkControllers)
        {
            try
            {
                Log.Logger.Verbose($"running eventlist for {emkControllers.Count} emks");

                if (emkControllers.Count == 0)
                {
                    return;
                }
                if (!MiscHelper.IsInGame())
                    return;
                Dictionary<Tuple<int, int>, EmkController> emkdict = [];
                List<EmkController> addingEmks = [];

                foreach (EmkController emk in emkControllers)
                {
                    /* If player doesn't have key, or we've "saved ptr", examine it in list */
                    if (!emk.HasKey || emk.Saved_Ptr != 0)
                    {
                        emkdict[new Tuple<int, int>(emk.Eventid, emk.Eventslot)] = emk;
                    }
                }

                uint mapid3 = 0; // 3 digit map code
                uint wnum = MiscHelper.GetWorldNumber();
                uint anum = MiscHelper.GetAreaNumber();
                if (wnum > 0)
                    mapid3 = 10 * wnum + anum;
                if (mapid3 != cached_mapid3)
                    Log.Logger.Verbose($"mapid={mapid3}, w={wnum}, a={anum}");
                cached_mapid3 = mapid3;

                ulong eventhead_ptr = AddressHelper.GetEmkHeadAddress();
                if (eventhead_ptr == 0)
                {
                    Log.Logger.Verbose($"eventheadptr is null");
                    ReleaseEvents(emkControllers);
                    return;
                }
                ulong eventhead = Memory.ReadULong((ulong)eventhead_ptr);
                if (eventhead == 0)
                {
                    Log.Logger.Verbose($"eventhead is null");
                    ReleaseEvents(emkControllers);
                    return;
                }

                if (emkdict.Count != 0)
                {
                    //Log.Logger.Information("Emks found, Managing event list");

                    // prevptr is address of a ptr to the "current emk"
                    // When "current emk" is pulled off list, it 
                    ulong prevptr = eventhead_ptr;
                    int numevents = 0;
                    // check every event for if it is in the list
                    for (ulong thisEmk = eventhead; thisEmk != 0; thisEmk = Memory.ReadULong((ulong)thisEmk + 0x68))
                    {
                        bool updatedEmk = true;
                        while (thisEmk != 0 && updatedEmk) // loop as long as we are pulling off events
                        {
                            updatedEmk = false;
                            numevents++;
                            if (numevents > 2000) // sanity check
                            {
                                Log.Logger.Warning($"m events:{numevents}");
                                Log.Logger.Warning($"thisemk = :{thisEmk:X}");
                                break;
                            }

                            int eventid = Memory.ReadInt(thisEmk + 0x30);
                            int eventslot = Memory.ReadByte(thisEmk + 0x34);
                            var t = new Tuple<int, int>(eventid, eventslot);
                            if (emkdict.ContainsKey(t))
                            {
                                EmkController emk = emkdict[t];
                                if (!emk.HasKey) /* Player doesn't have key -> pull it */
                                {
                                    // Only pull it if we're in the relevant map. This is to do less "pulls" in general!
                                    if (emk.MapId3 == mapid3) /* Compare current mapid to event's valid mapid */
                                    {
                                        ulong nextptr = Memory.ReadULong(thisEmk + 0x68);
                                        Memory.Write(prevptr, nextptr);
                                        emk.Saved_Ptr = thisEmk;
                                        emk.Deactivated = true;
                                        thisEmk = nextptr;
                                        updatedEmk = true;
                                        Log.Logger.Debug($"Pulled event: {emk.Name} at {emk.Saved_Ptr:X}");
                                    }
                                }
                                else /* Player has event's key, but we found it in list? Destroy our "old" version, and stop interfering. */
                                {
                                    emk.Saved_Ptr = 0;
                                    emk.Deactivated = false;
                                    Log.Logger.Debug($"Un-pulled event: {emk.Name} at {emk.Saved_Ptr:X}");
                                }
                            }
                        }
                        if (thisEmk == 0) // reached end of list
                            break;
                        prevptr = thisEmk + 0x68; // save address of previous node's last spot when we move on.
                    }
                }

                foreach (EmkController emk in emkControllers)
                {
                    /* If we have a saved ptr that we need to re-insert */
                    if (emk.HasKey && emk.Saved_Ptr != 0)
                    {
                        /* If we're in the map for the event */
                        if (emk.MapId3 == mapid3) /* Compare current mapid to event's valid mapid */
                        {
                            addingEmks.Add(emk);
                            Log.Logger.Debug($"Re-adding event: {emk.Name} at {emk.Saved_Ptr:X}");
                        }
                    }
                }

                if (addingEmks.Count > 0)
                {
                    var firstEmk = addingEmks.First();
                    var lastEmk = addingEmks.Last();

                    if (addingEmks.Count > 1)
                    {
                        // point our saved events each to the next one in sequence, creating a "saved event sequence"
                        for (var i = 0; i < addingEmks.Count - 1; i++)
                        {
                            Memory.Write(addingEmks[i].Saved_Ptr + 0x68, addingEmks[i + 1].Saved_Ptr);
                        }
                    }
                    eventhead_ptr = AddressHelper.GetEmkHeadAddress();
                    if (eventhead_ptr == 0)
                    {
                        Log.Logger.Warning($"eventheadptr is null");
                        ReleaseEvents(emkControllers);
                        return;
                    }
                    eventhead = Memory.ReadULong((ulong)eventhead_ptr);
                    if (eventhead == 0)
                    {
                        Log.Logger.Warning($"eventhead is null");
                        ReleaseEvents(emkControllers);
                        return;
                    }
                    // point last saved event to event head
                    Memory.Write(lastEmk.Saved_Ptr + 0x68, eventhead);
                    // make our first saved event the head of the event list.
                    Memory.Write(eventhead_ptr, firstEmk.Saved_Ptr);

                    // clear all the event saved ptrs
                    foreach (var emk in addingEmks)
                    {
                        emk.Deactivated = false;
                        emk.Saved_Ptr = 0;
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Exception in manageevents: {ex.Message}\n{ex.InnerException}\n{ex.Source}");
            }
        }
        // Build a list of event controllers, which we use to lock events until player has received the items.
        // We only add events to the list if their items are in the multiworld.
        internal static List<EmkController> BuildEmkControllers(Dictionary<string, object> slotData)
        {
            List<EmkController> result = [];

            if (App.DSOptions.ApworldCompare("0.0.21.0") < 0) /* apworld is < 0.0.21.0, which introduces events */
            {
                Log.Logger.Warning($"Apworld version too low, skipping fog wall lock processing.");
                return result;
            }

            List<int?> itemsId = [];
            try
            {
                if (slotData.TryGetValue("itemsId", out object itemsId_temp))
                {
                    itemsId.AddRange(JsonSerializer.Deserialize<int?[]>(itemsId_temp.ToString()));
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error($"exception creating fog map: {e.Message} {e.ToString()}");
            }
            var events = MiscHelper.GetDsrEventEmks();
            foreach (var item in itemsId)
            {
                EmkController? newemk = events.Find(x => x.ApId == item);
                if (newemk != null)
                {
                    Log.Logger.Verbose($"Adding {newemk.Name} to list. Id:slot={newemk.Eventid}:{newemk.Eventslot}");
                    result.Add(newemk);
                }
            }

            return result;
        }
        // Clear the saved ptrs of our list of "EmkControllers", because we detected there being no events in the list.
        public static void ReleaseEvents(List<EmkController> emkControllers)
        {
            int num_released = 0;
            foreach (var controller in emkControllers)
            {
                if (controller.Saved_Ptr != 0 || controller.Deactivated == true)
                {
                    controller.Saved_Ptr = 0;
                    controller.Deactivated = false;
                }
            }
            if (num_released > 0)
                Log.Logger.Debug($"Released all emks, {num_released} controllers affected.");
        }
    }
}

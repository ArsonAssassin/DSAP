using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.Models;
using DSAP.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAP.Helpers
{
    public class ApItemInjectorHelper
    {
        private static readonly object _memAllocLock = new object();
        internal static async Task AddAPItems(Dictionary<long, ScoutedItemInfo> scoutedLocationInfo)
        {
            List<KeyValuePair<long, ScoutedItemInfo>> addedEntries = scoutedLocationInfo.Where((e) => e.Value.Player.Slot != App.Client.CurrentSession.ConnectionInfo.Slot).ToList();
            //addedEntries.Sort((a, b) => a.Key.CompareTo(b.Key));

            var added_names = addedEntries.Select(x => new KeyValuePair<long, string>(x.Key, $"{x.Value.Player}'s {x.Value.ItemDisplayName}\0")).ToList();
            var added_captions = addedEntries.Select(x => new KeyValuePair<long, string>(x.Key, BuildItemCaption(x))).ToList();

            var added_emk_names = MiscHelper.GetDsrEventItems().Select(x => new KeyValuePair<long, string>(x.Id, $"{x.Name}\0"));
            var added_emk_captions = MiscHelper.GetDsrEventItems().Select(x => new KeyValuePair<long, string>(x.Id, BuildDsrEventItemCaption()));

            added_names.AddRange(added_emk_names);
            added_captions.AddRange(added_emk_captions);

            added_names.Sort((a, b) => a.Key.CompareTo(b.Key));
            added_captions.Sort((a, b) => a.Key.CompareTo(b.Key));

            var watch = System.Diagnostics.Stopwatch.StartNew();

            // add items
            bool do_replacements = upgradeGoods(added_names);

            var tasks = new List<Task>
                {
                Task.Run(() => { AddMsgs(0x380, added_names, "Item Names"); }), // names
                Task.Run(() => { AddMsgs(0x378, added_captions, "Item Captions"); }), // captions
                Task.Run(() => { AddMsgs(0x328, added_captions, "Item Descriptions"); }), // info
                };
            await Task.WhenAll(tasks);

            watch.Stop();
            Log.Logger.Information($"Finished adding new items params + msg text, took {watch.ElapsedMilliseconds}ms");
            App.Client.AddOverlayMessage($"Finished adding new items params + msg text, took {watch.ElapsedMilliseconds}ms");

            var local_ap_keys = added_emk_names.ToList();
            local_ap_keys.Sort((a, b) => a.Key.CompareTo(b.Key));
            // add item removal hook. Filter only things that are remote items; min = after last fogwall/local ap key, max = last ap key
            AddAPItemHook(local_ap_keys.Last().Key + 1, added_names.Last().Key);
        }

        private static void AddAPItemHook(long min, long max)
        {
            ulong target_func_start = 0x1407479E0;
            byte[] replaced_instructions = Memory.ReadByteArray(target_func_start, 14);
            ulong replacement_func_start_addr = (ulong)Memory.Allocate(1000, Memory.PAGE_EXECUTE_READWRITE);

            var jmpstub = new byte[]
            {
                0xff, 0x25, 0x00, 0x00, 0x00, 0x00,       //jmp    QWORD PTR [rip+0x0]        # 6 <_main+0x6>
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // target address
                // then the address to jump to (8 bytes)
            };
            Array.Copy(BitConverter.GetBytes(replacement_func_start_addr), 0, jmpstub, 6, 8); // target address

            //CMP r9d,0x12345678
            //JL OVER
            //CMP r9d,0x12345678
            //JG OVER
            // RET and 5 nops (could be replaced with mov r9d,<value>)
            // OVER (label)
            // 14 nops (replaced with source 14 bytes overwritten by jmp instruction)
            //  jmp        qword[rip+0]
            // <return address>
            var new_instructions = new byte[]
            {
                0x41, 0x81, 0xf8, 0x78, 0x56, 0x34, 0x12,    // cmp r9d,0x12345678
                0x7c, 0x0f,                                  // jl     OVER
                0x41, 0x81, 0xf8, 0x78, 0x56, 0x34, 0x12,    // cmp    r9d,0x12345678
                0x7f, 0x06,                                  // jg     OVER
                0xc3, 0x90, 0x90, 0x90, 0x90, 0x90,          // ret and 5 nops
                //0x41, 0xb8, 0x72, 0x01, 0x00, 0x00,          // mov    r9d,0x172 (dec 370)
                // OVER (label)
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,    // 14 nops -> get replaced with source 14 bytes
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                0xff, 0x25, 0x00, 0x00, 0x00, 0x00,          // jmp    QWORD PTR [rip+0x8]
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // jmp's target address
            };

            Array.Copy(BitConverter.GetBytes(min), 0, new_instructions, 3, 4); // min
            Array.Copy(BitConverter.GetBytes(max), 0, new_instructions, 12, 4); // max
            Array.Copy(replaced_instructions, 0, new_instructions, 24, 14); // replaced_instructions
            Array.Copy(BitConverter.GetBytes(target_func_start + 14), 0, new_instructions, 44, 8); // target address


            Memory.WriteByteArray(replacement_func_start_addr, new_instructions); // write new instructions into its hook area
            Memory.WriteByteArray(target_func_start, jmpstub); // write jmp stub (e.g. "create hook")
        }

        internal static string BuildItemCaption(KeyValuePair<long, ScoutedItemInfo> item)
        {
            const byte progression = 0b001;
            const byte useful = 0b010;
            const byte trap = 0b100;
            string item_type = "normal";
            if (((byte)item.Value.Flags) == 0b001) item_type = "Progression";
            if (((byte)item.Value.Flags) == 0b010) item_type = "Useful";
            if (((byte)item.Value.Flags) == 0b100) item_type = "Trap";
            return $"A {item_type} Archipelago item for {item.Value.Player}'s {item.Value.ItemGame}.\0";
        }
        internal static string BuildDsrEventItemCaption()
        {
            return "A boon from another world. Makes a fog wall passable.\0";
        }


        private static bool upgradeGoods(List<KeyValuePair<long, string>> addedEntries)
        {
            ulong resCap = Memory.ReadULong((ulong)(AddressHelper.SoloParamAob.Address + 0xF0));
            uint old_buffer_size = Memory.ReadUInt(resCap + 0x30);
            ulong old_buffer = Memory.ReadULong(resCap + 0x38);
            ushort old_buffer_num_entries = Memory.ReadUShort(old_buffer + 0xA);

            /* first, read highest numbered param in list */
            uint highest_id = Memory.ReadUInt(old_buffer + (ulong)(0x30 + ((old_buffer_num_entries - 1) * 0xc)));
            if (addedEntries.First().Key <= highest_id)
            {
                Log.Logger.Warning($"Warning: Highest id in params detected as {highest_id}, >= one of our entries.");
                Log.Logger.Warning($"Checking if params and msgs have already been updated...");

                uint old_end_buffer_offset = old_buffer_size + (uint)(0x8 * old_buffer_num_entries) + 0x10 + 0xf;
                ulong old_desc_area_loc = old_buffer + old_end_buffer_offset;
                DescArea old_desc_area = Memory.ReadObject<DescArea>(old_desc_area_loc);
                Log.Logger.Debug("Read object: " + old_desc_area.ToString());
                bool requires_reload = ValidateDescArea(old_desc_area);
                if (requires_reload)
                {
                    //int intermediate_buffer_size = old_desc_area.FullAllocLength;
                    ulong intermediate_buffer_loc = old_buffer;
                    // desc size 4, full alloc length 4, old address 8, old length 4, seed hash 4, slot 4
                    // reset old buffer values
                    old_buffer = old_desc_area.OldAddress;
                    old_buffer_size = (uint)old_desc_area.OldLength;
                    old_buffer_num_entries = Memory.ReadUShort(old_buffer + 0xA);

                    /* Switch out the pointer so deallocated area isn't accessed by the game */
                    Memory.Write(resCap + 0x30, old_buffer_size);
                    Memory.Write(resCap + 0x38, old_buffer);

                    // dealloc previously swapped-in area
                    Memory.FreeMemory((nint)(intermediate_buffer_loc - 0x10));
                    Log.Logger.Warning("Reloading EquipGoodsParams");
                }
                else
                {
                    Log.Logger.Information("EquipGoodsParams replacement not needed - skipping");
                    return false;
                }

            }

            uint old_buffer_string_offset = Memory.ReadUInt(old_buffer + 0x0);
            ushort old_buffer_params_offset = Memory.ReadUShort(old_buffer + 0x4);

            ushort new_entries = (ushort)addedEntries.Count();

            uint goods_param_size = 0x5c;
            ushort new_buffer_num_entries = (ushort)(old_buffer_num_entries + new_entries);

            ushort new_buffer_params_offset = (ushort)(old_buffer_params_offset + (0xc * new_entries));
            uint new_buffer_string_offset = (ushort)(old_buffer_string_offset + ((0xc + goods_param_size) * new_entries));
            uint addl_str_length = (uint)addedEntries.Aggregate(0, (total, x) => total + x.Value.Length + 1);
            uint new_endtable_size = (uint)(0x8 * new_buffer_num_entries);

            uint new_buffer_size = (uint)(old_buffer_size + addl_str_length + (0xc + goods_param_size) * new_entries);
            uint new_buffer_alloc_size = (uint)(new_buffer_size + (0x8 * new_buffer_num_entries) + 0x10 + 0xf + DescArea.size); // ensure enough for the binary search table and the prologue

            ulong new_allocated_buffer = 0;
            lock (_memAllocLock)
            {
                new_allocated_buffer = (ulong)Memory.AllocateAbove(new_buffer_alloc_size);
            }

            ulong new_buffer = new_allocated_buffer + 0x10;
            Log.Logger.Information($"Allocated {new_buffer_alloc_size} bytes at {new_allocated_buffer.ToString("X")}");
            Log.Logger.Information($"Overwrite EquipParamGoods @ {old_buffer.ToString("X")} to {new_buffer.ToString("X")}");


            /* Then, copy the header + pointer structs */
            byte[] basebytes = Memory.ReadByteArray(old_buffer, old_buffer_params_offset);
            Memory.WriteByteArray(new_buffer, basebytes);
            /* Then, copy the params */
            uint old_buffer_params_length = (uint)(goods_param_size * old_buffer_num_entries);
            byte[] basebytes2 = Memory.ReadByteArray(old_buffer + old_buffer_params_offset, (int)old_buffer_params_length);
            Memory.WriteByteArray(new_buffer + new_buffer_params_offset, basebytes2);
            /* Then, copy the strings */
            uint old_buffer_strings_length = old_buffer_size - old_buffer_string_offset;
            byte[] basebytes3 = Memory.ReadByteArray(old_buffer + old_buffer_string_offset, (int)old_buffer_strings_length);
            Memory.WriteByteArray(new_buffer + new_buffer_string_offset, basebytes3);

            /* old buffer ends on the last string - last null terminator (shift-jis) */
            byte[] parambytes = Memory.ReadByteArray(old_buffer + old_buffer_params_offset, (int)goods_param_size);
            parambytes[0x36] = 99; // max num
            parambytes[0x3a] = 1; // goods type = key
            parambytes[0x3b] = 0; // ref category = like key
            parambytes[0x3e] = 0; // use animation = 0
            // Is Only One?
            // Is Deposit?
            uint new_string_loc = (uint)(new_buffer + new_buffer_string_offset + old_buffer_strings_length);

            // fix old entries' offsets
            for (uint i = 0; i < old_buffer_num_entries; i++)
            {
                uint currloc = (uint)(new_buffer + 0x30 + i * 0xc);
                uint poff = Memory.ReadUInt(currloc + 0x4);
                uint soff = Memory.ReadUInt(currloc + 0x8);
                poff = (uint)(poff + (0xc * new_entries));
                soff = soff + (0xc + goods_param_size) * new_entries;
                Memory.Write(currloc + 0x4, poff);
                Memory.Write(currloc + 0x8, soff);
            }

            /* then add the new pointer structs, params, and strings, and pointers to each. */
            for (uint i = 0; i < new_entries; i++)
            {
                var entry = addedEntries.ToArray()[i];
                byte[] stringbytes = Encoding.ASCII.GetBytes($"{entry.Value}\0");
                uint newid = (uint)entry.Key;
                // set sort bytes in param based on id - not sure if this is grabbing top or bottom 2 bytes!! But filling all 4 put the items at the top instead.
                byte[] idbytes = BitConverter.GetBytes(newid);
                parambytes[0x1c] = idbytes[0];
                parambytes[0x1d] = idbytes[1];
                //parambytes[0x1e] = idbytes[2];
                //parambytes[0x1f] = idbytes[3];
                byte[] iconbytes = BitConverter.GetBytes((short)2042);
                parambytes[0x2c] = iconbytes[0];
                parambytes[0x2d] = iconbytes[1];
                parambytes[0x45] |= (byte)(0x30); // turn on isDrop and isDeposit bits

                uint currloc = (uint)(new_buffer + old_buffer_params_offset + i * 0xc);
                Memory.Write(currloc + 0x0, newid);

                uint new_param_loc = (uint)(new_buffer + new_buffer_params_offset + (old_buffer_num_entries + i) * goods_param_size);
                Memory.WriteByteArray(new_param_loc, parambytes);
                Memory.Write(currloc + 0x4, new_param_loc - new_buffer);

                Memory.WriteByteArray(new_string_loc, stringbytes);
                Memory.Write(currloc + 0x8, new_string_loc - new_buffer);
                new_string_loc += (uint)stringbytes.Length;
            }
            Log.Logger.Information($"Added {new_entries} items to EquipParamGoods from {addedEntries.First().Key} to {addedEntries.Last().Key}");

            ulong post_string_loc = new_string_loc;
            ulong saved_len = post_string_loc - new_buffer;
            /* Then fix up the offsets */
            Memory.Write(new_buffer + 0x0, new_buffer_string_offset);
            Memory.Write(new_buffer + 0x4, new_buffer_params_offset);
            Memory.Write(new_buffer + 0xA, new_buffer_num_entries);

            //Memory.Write(new_allocated_buffer, new_buffer_size);
            Memory.Write(new_allocated_buffer, saved_len);

            // copy over the endtable
            ulong new_endtable_loc = new_buffer + ((saved_len + 0xf) & 0xFFFFFFFFFFFFFFF0);
            ulong old_endtable_loc = old_buffer + ((old_buffer_size + 0xf) & 0xFFFFFFFFFFFFFFF0);
            byte[] old_endtable = Memory.ReadByteArray(old_endtable_loc, 8 * old_buffer_num_entries);
            Memory.WriteByteArray(new_endtable_loc, old_endtable);
            // add our new entries to the endtable (binary search table)
            for (uint i = 0; i < new_entries; i++)
            {
                var entry = addedEntries.ToArray()[i];
                uint newid = (uint)entry.Key;
                uint curr_endtable_loc = (uint)(new_endtable_loc + 8 * (i + old_buffer_num_entries));
                Memory.Write(curr_endtable_loc, newid);
                Memory.Write(curr_endtable_loc + 0x4, old_buffer_num_entries + i);
            }
            // end of data

            // add desc area to end
            uint end_buffer_offset = new_buffer_size + (uint)(0x8 * new_buffer_num_entries) + 0x10 + 0xf;
            ulong desc_area_loc = new_buffer + end_buffer_offset;

            var seedHash = MiscHelper.HashSeed(App.Client.CurrentSession.RoomState.Seed);
            var slot = App.Client.CurrentSession.ConnectionInfo.Slot;

            var new_desc_area = new DescArea((int)new_buffer_alloc_size, old_buffer, (int)old_buffer_size, seedHash, slot);
            Memory.WriteObject<DescArea>(desc_area_loc, new_desc_area);
            // end of data + metadata


            /* Then switch out the pointer */
            Memory.Write(resCap + 0x38, new_buffer);
            Memory.Write(resCap + 0x30, saved_len);
            return true;
        }

        private static bool ValidateDescArea(DescArea descArea)
        {
            bool requires_reload = false;
            if (descArea.DescSize >= DescArea.size)
            {
                int old_slot = descArea.Slot;

                if (descArea.SeedHash != MiscHelper.HashSeed(App.Client.CurrentSession.RoomState.Seed)) // different seed
                {
                    if (MiscHelper.IsInGame())
                    {
                        App.Client.AddOverlayMessage($"Error - check the client log");
                        Log.Logger.Error("Different seed detected than your previous connection to Archipelago.");
                        Log.Logger.Error("However, you are loaded into a save. Try again while not loaded in.");
                        return false;
                    }
                    else
                    {
                        Log.Logger.Information("Different seed detected than your previous load. Resetting area");
                        return true;
                    }


                }
                else if (old_slot != App.Client.CurrentSession.ConnectionInfo.Slot) // different slot
                {
                    if (MiscHelper.IsInGame())
                    {
                        App.Client.AddOverlayMessage($"Error - check the client log");
                        Log.Logger.Error("Different slotdetected than your previous connection to Archipelago.");
                        Log.Logger.Error("However, you are loaded into a save. Try again while not loaded in.");
                        return false;
                    }
                    else
                    {
                        Log.Logger.Information("Different seed detected than your previous load. Resetting area");
                        return true;
                    }
                }
                else // seed and slot checked out fine. Looks good, no need to reload.
                {
                    return false;
                }
            }
            else // desc area too small
            {
                Log.Logger.Error("No version detected on equip goods params. A different mod is probably interfering with our items.");
                Log.Logger.Error("Try running without other mods.");
                return false;
            }
        }

        private static void AddMsgs(uint offset, List<KeyValuePair<long, string>> instrings, string msgsName)
        {
            ulong MsgMan = Memory.ReadULong(0x141c7e3e8);

            ulong old_buffer = Memory.ReadULong(MsgMan + offset);
            ulong old_buffer_size = Memory.ReadUInt(old_buffer + 0x4);
            ulong old_buffer_num_spanmaps = Memory.ReadUShort(old_buffer + 0xc);
            // structure of buffer:
            // string end/size@ 0x04
            // num of span maps 0x0c
            // # stroff entries 0x10
            // stroff table  at 0x14
            // span maps start  0x1c
            // span maps have <offset> <min> <max>. Offset is # of entries into string offset table
            // after span maps is string offset table
            // There are (number of items, = 0x110 vanilla) in string offset table
            // Then there are the strings
            // New buffer will need added:
            //  1. (optionally) +c bytes for another map,
            //  2. 0x4 bytes per added id in the string offset table
            //  3. [n] bytes for each string including null char, in Unicode
            // New buffer will need updated:
            //     (optionally) num of span maps
            //     string offset at 0x14
            //     span map entry if not adding a new one
            //     each string offset entry
            //     end of strings/size of all

            // get highest entry in span maps table
            // check its id against known ids, if higher do desc area validation
            long highest_id = Memory.ReadUInt(old_buffer + 0x1c + 0xc * (old_buffer_num_spanmaps - 1) + 0x8);
            Log.Logger.Verbose($"msgs highest id = {highest_id}");
            if (highest_id >= instrings.First().Key) // conflict
            {
                // validate desc area
                // If it's no good, reset it
                ulong desc_area_loc = old_buffer + old_buffer_size;
                DescArea old_desc_area = Memory.ReadObject<DescArea>(desc_area_loc);
                Log.Logger.Debug("Read object: " + old_desc_area.ToString() + " from " + desc_area_loc.ToString("X"));
                bool update_required = ValidateDescArea(old_desc_area);
                if (update_required)
                {
                    ulong intermediate_buffer_loc = old_buffer; // save "swapped-in area" ptr
                    // reset old buffer values
                    old_buffer = old_desc_area.OldAddress;
                    old_buffer_size = (ulong)old_desc_area.OldLength;
                    old_buffer_num_spanmaps = Memory.ReadUShort(old_buffer + 0xc);

                    /* Switch out the pointer so deallocated area isn't accessed by the game */
                    Memory.Write(MsgMan + offset, old_buffer);

                    // dealloc previously swapped-in area
                    Memory.FreeMemory((nint)(intermediate_buffer_loc));
                    Log.Logger.Information($"Reloading {msgsName} text changes");
                }
                else
                {
                    Log.Logger.Information($"{msgsName} text update not needed - skipping");
                    return;
                }
            }

            ulong old_buffer_num_stroff_entries = Memory.ReadUShort(old_buffer + 0x10);
            ulong old_buffer_stroff_start_offset = Memory.ReadUInt(old_buffer + 0x14);

            ulong new_entries = (ulong)(instrings.Last().Key - instrings.First().Key + 1);
            ulong total_String_size = 0;
            ulong new_buffer = 0;
            ulong new_buffer_stroff_start_offset = old_buffer_stroff_start_offset + 0xc;
            ulong old_buffer_string_start_offset = old_buffer_stroff_start_offset + (old_buffer_num_stroff_entries * 4);
            ulong new_buffer_string_start_offset = old_buffer_string_start_offset + 0xc + 4 * new_entries;
            foreach (var entry in instrings)
            {
                total_String_size += (ulong)Encoding.Unicode.GetBytes(entry.Value).Length;
            }
            if (new_entries < (ulong)instrings.Count)
                total_String_size += (ulong)instrings.Count - new_entries;
            //calculate size
            ulong new_buffer_size = old_buffer_size + 0xc + 0x4 * new_entries + total_String_size;
            ulong new_buffer_total_size = old_buffer_size + 0xc + 0x4 * new_entries + total_String_size + (ulong)DescArea.size;

            lock (_memAllocLock)
            {
                new_buffer = (ulong)Memory.AllocateAbove((uint)new_buffer_total_size);
            }
            //Log.Logger.Information($"Allocated {new_buffer_total_size} bytes at {new_buffer.ToString("X")}");
            Log.Logger.Information($"Updating {msgsName} text @ {old_buffer.ToString("X")} to {new_buffer.ToString("X")}");

            // first, copy over header & old maps
            byte[] basebytes = Memory.ReadByteArray(old_buffer, (int)(0x1c + old_buffer_num_spanmaps * 0xc));
            Memory.WriteByteArray(new_buffer, basebytes);
            // Then, copy over existing string offset table

            //ulong new_buffer_stroff_start_offset = old_buffer_stroff_start_offset + 0xc;


            byte[] basebytes2 = Memory.ReadByteArray(old_buffer + old_buffer_stroff_start_offset, (int)(old_buffer_num_stroff_entries * 4));
            Memory.WriteByteArray(new_buffer + new_buffer_stroff_start_offset, basebytes2);
            // Then, copy over existing strings
            byte[] basebytes3 = Memory.ReadByteArray(old_buffer + old_buffer_string_start_offset, (int)(old_buffer_size - old_buffer_string_start_offset));
            Memory.WriteByteArray(new_buffer + new_buffer_string_start_offset, basebytes3);

            // add new span map
            ulong new_spanmap_loc = new_buffer + old_buffer_stroff_start_offset; // old buffer stroffs started where this would
            Memory.Write(new_spanmap_loc, (int)old_buffer_num_stroff_entries); // next str off index will be the next available number (0 indexed) - aka current max
            Memory.Write(new_spanmap_loc + 0x4, (int)instrings.First().Key);
            Memory.Write(new_spanmap_loc + 0x8, (int)instrings.Last().Key);

            // Correct bad string offsets in table - increase by 0xc for the new spanmap, and 0x4 for each new string
            for (uint i = 0; i < old_buffer_num_stroff_entries; i++)
            {
                ulong stroff_loc = new_buffer + new_buffer_stroff_start_offset + 4 * i;
                uint stroff_val = Memory.ReadUInt(stroff_loc);
                if (stroff_val != 0)
                {
                    stroff_val += (uint)(0xc + (new_entries * 4));
                    Memory.Write(stroff_loc, stroff_val);
                }
            }
            // point to end of last old string
            ulong curr_end_loc = new_buffer + new_buffer_string_start_offset + (old_buffer_size - old_buffer_string_start_offset);
            ulong end_of_stroffs = new_buffer + new_buffer_stroff_start_offset + (4 * old_buffer_num_stroff_entries);
            for (uint i = 0; i < new_entries; i++)
            {
                ulong curr_stroff_loc = end_of_stroffs + 4 * i;
                Memory.Write(curr_stroff_loc, (int)(curr_end_loc - new_buffer)); // point stroff entry to string position
                                                                                 // Then write the string
                byte[] ba = Encoding.Unicode.GetBytes("\0");
                if (instrings.Any(x => x.Key == instrings.First().Key + i))
                    ba = Encoding.Unicode.GetBytes(instrings.Find(x => x.Key == instrings.First().Key + i).Value);
                Memory.WriteByteArray(curr_end_loc, ba);
                curr_end_loc += (ulong)ba.Length;
            }
            // end of data here
            // add desc area
            var seedHash = MiscHelper.HashSeed(App.Client.CurrentSession.RoomState.Seed);
            var slot = App.Client.CurrentSession.ConnectionInfo.Slot;
            var new_desc_area = new DescArea((int)new_buffer_total_size, old_buffer, (int)old_buffer_size, seedHash, slot);
            Memory.WriteObject<DescArea>(curr_end_loc, new_desc_area);
            Log.Logger.Verbose($"new Desc Area written to {curr_end_loc.ToString("X")}");
            // end here

            // fix up header area
            Memory.Write(new_buffer + 0x4, curr_end_loc - new_buffer);
            Memory.Write(new_buffer + 0xc, old_buffer_num_spanmaps + 1);
            Memory.Write(new_buffer + 0x10, old_buffer_num_stroff_entries + new_entries);
            Memory.Write(new_buffer + 0x14, new_buffer_stroff_start_offset);

            /* Then switch out the pointer */
            Memory.Write(MsgMan + offset, new_buffer);
        }
        private static void UpdateItemText(ulong strloc, int len, string newstring)
        {
            if (strloc == 0)
            {
                Log.Logger.Information($"strloc = {strloc}"); return;
            }
            byte[] ba = Memory.ReadByteArray(strloc, len);
            string su16 = Encoding.Unicode.GetString(ba);
            string[] sub16 = su16.Split("\0");
            Log.Logger.Information($"Padding to {sub16[0].Length} bytes");
            int available_space = sub16[0].Length;
            string newptxt = newstring;
            if (newstring.Length > available_space)
                newptxt = newstring.Substring(0, sub16[0].Length);

            byte[] newba = Encoding.Unicode.GetBytes(newptxt);
            Memory.WriteByteArray(strloc, newba);
            Log.Logger.Information($"String found: {su16}, \n@{strloc.ToString("X")}");
            Log.Logger.Information($"Wrote string {newptxt}");
        }

        private static ulong FindMsg(ulong MsgsStart, uint id)
        {
            ulong GoodsMsgsStrTableOffset = Memory.ReadULong(MsgsStart + 0x14);
            ushort GoodsMsgsCompareEntries = Memory.ReadUShort(MsgsStart + 0xc);
            ulong GoodsMsgsCompareStart = MsgsStart + 0x1c;
            uint compareEntrySize = 0xc;
            for (uint curridx = 0; curridx < GoodsMsgsCompareEntries; curridx++)
            {
                ulong currentry = GoodsMsgsCompareStart + (compareEntrySize * curridx);
                uint low = Memory.ReadUInt(currentry + 0x4);
                uint high = Memory.ReadUInt(currentry + 0x8);
                if (low <= id && id <= high)
                {
                    uint baseoffset = Memory.ReadUInt(currentry + 0x0);
                    uint idoffset = id - low;
                    uint strEntryOffset = 4 * (idoffset + baseoffset);

                    ulong itemstroffset = Memory.ReadUInt(MsgsStart + GoodsMsgsStrTableOffset + strEntryOffset);
                    ulong itemstrloc = MsgsStart + itemstroffset;
                    return itemstrloc;
                }
            }
            return 0;
        }

        public static void ChangePrismStoneText()
        {
            var item = MiscHelper.GetAllItems().Find(x => x.Name.ToLower().Contains("prism stone"));
            uint itemid = (uint)item.Id;
            //uint itemid = 9014;

            ulong MsgMan = Memory.ReadULong(0x141c7e3e8);
            ulong GoodsMsgsStart = Memory.ReadULong(MsgMan + 0x380);
            ulong GoodsCaptionMsgsStart = Memory.ReadULong(MsgMan + 0x378);
            ulong GoodsInfoMsgStart = Memory.ReadULong(MsgMan + 0x328);
            ulong itemNameStrLoc = FindMsg(GoodsMsgsStart, itemid);
            ulong itemCaptionStrLoc = FindMsg(GoodsCaptionMsgsStart, itemid);
            ulong itemInfoStrLoc = FindMsg(GoodsInfoMsgStart, itemid);

            UpdateItemText(itemNameStrLoc, 100, "AP Item\0");
            UpdateItemText(itemCaptionStrLoc, 100, "This is an item that belongs to another world...\0");
            UpdateItemText(itemInfoStrLoc, 500, "*narrator voice* We're not sure how this got here. \nBest hold on to it. \n\nJust in case.\0");

            ulong equipGoodsParamResCap = Memory.ReadULong((ulong)(AddressHelper.SoloParamAob.Address + 0xF0));
            //upgradeGoods(equipGoodsParamResCap);
            //AddMsgs(9015, new List<string>() { "AP Item From Player 2's world" });
            return;
        }
    }
}

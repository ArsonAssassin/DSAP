using Archipelago.Core.Util;
using DSAP.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSAP.Helpers
{
    internal class ParamHelper
    {
        private static bool testAddItemLot() // List<KeyValuePair<long, string>> addedEntries)
        {
            // Read in the Param Structure
            // Modify it,
            // Then save it back
            bool reloadRequired = ParamHelper.ReadFromBytes(out ParamStruct<ItemLotParam> paramStruct,
                                                     ItemLotParam.spOffset,
                                                     (ps) => ps.ParamEntries.Last().id >= 99999990);
            if (!reloadRequired)
            {
                Log.Logger.Debug("Skipping reload of Item Lots");
                return false;
            }
            // if we are here, we are updating the params.

            List<KeyValuePair<long, string>> addedEntries = [];
            addedEntries.Add(new KeyValuePair<long, string>(1810261, "Handaxe 2"));
            ushort new_entries = (ushort)addedEntries.Count();

            // Get first entry's Param (e.g. White Sign Soapstone), use it as basis for new params.
            byte[] parambytes = new byte[ItemLotParam.Size];
            var copyentry = paramStruct.ParamEntries.Find((x) => x.id == 1810260);
            Array.Copy(paramStruct.ParamBytes, copyentry.paramOffset, parambytes, 0, parambytes.Length);

            Array.Copy(BitConverter.GetBytes(2000000), 0, parambytes, 0x0, sizeof(int)); // make it an arrow
            parambytes[0x8a] = 98; // set to 98 arrows

            // For each new item, "Add Item" to ParamSt
            for (uint i = 0; i < new_entries; i++)
            {
                var entry = addedEntries.ToArray()[i];
                uint newid = (uint)entry.Key;
                byte[] stringbytes = Encoding.ASCII.GetBytes($"{entry.Value}\0");
                // set sort bytes in param based on id - not sure if this is grabbing top or bottom 2 bytes!! But filling all 4 put the items at the top instead.
                byte[] idbytes = BitConverter.GetBytes(newid);
                // This will add the item to the array, and append its string to the NewString buffer
                paramStruct.AddParam(newid, parambytes, stringbytes);
            }

            // add a dummy item at 99999998 so that we can know we've been here.
            Array.Copy(BitConverter.GetBytes(-1), 0, parambytes, 0x80, sizeof(int)); // overwrite getitemflagid with -1, so it isn't used
            paramStruct.AddParam(99999998, parambytes, Encoding.ASCII.GetBytes("")); // mark that we've been here

            paramStruct.ParamEntries.Sort((x, y) => (x.id.CompareTo(y.id)));
            Log.Logger.Information($"Added {new_entries} items to ItemLotParams from {addedEntries.First().Key} to {addedEntries.Last().Key}");

            ParamHelper.WriteFromParamSt(paramStruct, ItemLotParam.spOffset);

            return true;
        }
        internal static bool RemoveWeaponRequirements()
        {
            // Read in the Param Structure
            // Modify it,
            // Then save it back
            bool reloadRequired = ParamHelper.ReadFromBytes(out ParamStruct<EquipParamWeapon> paramStruct,
                                                     EquipParamWeapon.spOffset,
                                                     (ps) => ps.ParamEntries.Last().id >= 99999990);
            if (!reloadRequired)
            {
                Log.Logger.Debug("Skipping reload of Weapons");
                return false;
            }
            // if we are here, we are updating the params.

            // Get first entry's Param (e.g. White Sign Soapstone), use it as basis for new params.
            byte[] parambytes = new byte[EquipParamWeapon.Size];
            var copyentry = paramStruct.ParamEntries.Find((x) => x.id == 100000);
            Array.Copy(paramStruct.ParamBytes, copyentry.paramOffset, parambytes, 0, parambytes.Length);

            Array.Copy(BitConverter.GetBytes(2000000), 0, parambytes, 0x0, sizeof(int)); // make it an arrow
            parambytes[0x8a] = 98; // set to 98 arrows

            // For each existing item, modify required str/dex/int/faith
            for (int i = 0; i < paramStruct.ParamEntries.Count; i++)
            {
                var ent = paramStruct.ParamEntries[i];
                paramStruct.ParamBytes[ent.paramOffset + 0xed] = 0; // str
                paramStruct.ParamBytes[ent.paramOffset + 0xee] = 0; // dex
                paramStruct.ParamBytes[ent.paramOffset + 0xef] = 0; // int
                paramStruct.ParamBytes[ent.paramOffset + 0xf0] = 0; // faith
            }

            // add a dummy item at 99999998 so that we can know we've been here.
            Array.Copy(BitConverter.GetBytes(-1), 0, parambytes, 0x80, sizeof(int)); // overwrite getitemflagid with -1, so it isn't used
            paramStruct.AddParam(99999998, parambytes, Encoding.ASCII.GetBytes("")); // mark that we've been here

            paramStruct.ParamEntries.Sort((x, y) => (x.id.CompareTo(y.id)));
            Log.Logger.Information($"Added 1 items to EquipParamWeapon struct and removed stat requirements");

            ParamHelper.WriteFromParamSt(paramStruct, EquipParamWeapon.spOffset);

            return true;
        }
        internal static bool RemoveSpellRequirements()
        {
            // Read in the Param Structure
            // Modify it,
            // Then save it back
            bool reloadRequired = ParamHelper.ReadFromBytes(out ParamStruct<MagicParam> paramStruct,
                                                     MagicParam.spOffset,
                                                     (ps) => ps.ParamEntries.Last().id >= 99999990);
            if (!reloadRequired)
            {
                Log.Logger.Debug("Skipping reload of Magic");
                return false;
            }

            // For each existing spell item, modify required int/faith
            for (int i = 0; i < paramStruct.ParamEntries.Count; i++)
            {
                var ent = paramStruct.ParamEntries[i];
                // if modifying int and faith requirements
                paramStruct.ParamBytes[ent.paramOffset + MagicParam.Int_Requirement] = 0;   // int
                paramStruct.ParamBytes[ent.paramOffset + MagicParam.Faith_Requirement] = 0; // faith
                
                // if removing vow restrictions
                paramStruct.ParamBytes[ent.paramOffset + MagicParam.VOW_00_07] = 0xff;   // spell usable while in no covenant and first 7
                paramStruct.ParamBytes[ent.paramOffset + MagicParam.VOW_08_15] = 0xff;   // spell usable while in any of the other covenants
            }

            // Get first entry's Param (e.g. White Sign Soapstone), use it as basis for new params.
            byte[] parambytes = new byte[MagicParam.Size];

            // add a dummy item at 99999998 so that we can know we've been here.
            Array.Copy(BitConverter.GetBytes(-1), 0, parambytes, 0, sizeof(int)); // overwrite getitemflagid with -1, so it isn't used
            paramStruct.AddParam(99999998, parambytes, Encoding.ASCII.GetBytes("")); // mark that we've been here

            paramStruct.ParamEntries.Sort((x, y) => (x.id.CompareTo(y.id)));
            Log.Logger.Information($"Added 1 items to EquipParamWeapon struct and removed stat requirements");

            ParamHelper.WriteFromParamSt(paramStruct, MagicParam.spOffset);

            return true;
        }
        internal static bool TestMoveChange(int field, int value)
        {
            // Read in the Param Structure
            // Modify it,
            // Then save it back
            bool reloadRequired = ParamHelper.ReadFromBytes(out ParamStruct<MoveParam> paramStruct,
                                                     MoveParam.spOffset,
                                                     (ps) => ps.ParamEntries.Last().id >= 99999990);
            if (!reloadRequired)
            {
                Log.Logger.Debug("Skipping reload of Moves");
                return false;
            }
            // if we are here, we are updating the params.

            var updentry = paramStruct.ParamEntries.Find((x) => x.id == 103); // get dog moveset
            Memory.Write(paramStruct.BufferLoc + (ulong)(paramStruct.ParamEntries.Count * 12 + 0x30) + (ulong)field, value);

            // Get first entry's Param (e.g. White Sign Soapstone), use it as basis for new params.
            //byte[] parambytes = new byte[MoveParam.Size];
            //var copyentry = paramStruct.ParamEntries.Find((x) => x.id == 103); // get dog moveset
            //Array.Copy(paramStruct.ParamBytes, copyentry.paramOffset, parambytes, 0, parambytes.Length);

            //// For each existing item, modify required str/dex/int/faith
            //for (int i = 0; i < paramStruct.ParamEntries.Count; i++)
            //{
            //    var ent = paramStruct.ParamEntries[i];
            //    if (ent.id > 20)
            //        break;
            //    Array.Copy(parambytes, 0, paramStruct.ParamBytes, ent.paramOffset, MoveParam.Size); // copy moveset over taret
            //}

            // add a dummy item at 99999998 so that we can know we've been here.
            //paramStruct.AddParam(99999998, parambytes, Encoding.ASCII.GetBytes("")); // mark that we've been here

            //paramStruct.ParamEntries.Sort((x, y) => (x.id.CompareTo(y.id)));
            //Log.Logger.Information($"Added 1 items to MoveParam struct and changed player moveset");

            //ParamHelper.WriteFromParamSt(paramStruct, MoveParam.spOffset);

            return true;
        }


        // return = whether reload is required
        public static bool ReadFromBytes<ParamT>(out ParamStruct<ParamT> result, int soloParamOffset, Func<ParamStruct<ParamT>, bool> isUsedCondition) where ParamT : IParam, new()
        {
            result = new ParamStruct<ParamT>();

            ulong ResCapLoc = Memory.ReadULong((ulong)(AddressHelper.SoloParamAob.Address + soloParamOffset));
            if (ResCapLoc == 0)
            {
                Log.Logger.Error($"Could not find params at offset {soloParamOffset}");
            }
            int BufferSize = (int)Memory.ReadUInt(ResCapLoc + 0x30);
            ulong BufferLoc = Memory.ReadULong(ResCapLoc + 0x38);

            result.ReadFromBytes(BufferLoc, BufferSize, isUsedCondition);
            if (result.DescArea != null)
            {
                bool reloadRequired = MiscHelper.ValidateDescArea(result.DescArea, typeof(ParamT).Name);
                if (!reloadRequired)
                {
                    return false;
                }
                Log.Logger.Warning($"overwriting {typeof(ParamT).Name}");
                result.ReadFromBytes(result.DescArea.OldAddress, result.DescArea.OldLength, isUsedCondition);
            }
            return true;
        }
        public static bool WriteFromParamSt<ParamT>(ParamStruct<ParamT> input, int soloParamOffset) where ParamT : IParam, new()
        {
            ulong resCapLoc = Memory.ReadULong((ulong)(AddressHelper.SoloParamAob.Address + soloParamOffset));
            int oldBufferSize = (int)Memory.ReadUInt(resCapLoc + 0x30);
            ulong oldBufferLoc = Memory.ReadULong(resCapLoc + 0x38);
            bool hadOldUpdatedArea = false;
            // if "desc area" exists, then we're reloading the new updated area. Set up for later Free().
            if (input.DescArea != null)
            {
                hadOldUpdatedArea = true;
                oldBufferSize = input.DescArea.FullAllocLength;
                oldBufferLoc = input.BufferLoc - 0x10;
            }

            byte[] newBytes = input.GenerateWriteArray(out int shortLength);
            ulong allocArea = (ulong)Memory.Allocate((uint)newBytes.Length);
            Log.Logger.Debug($"Allocated {newBytes.Length:X} bytes at {allocArea:X}");

            Memory.WriteByteArray(allocArea, newBytes);
            ulong newBufferLoc = allocArea + 0x10; // get past prologue

            
            Log.Logger.Debug($"Overwrite {typeof(ParamT).Name} @ {oldBufferLoc:X} to {allocArea:X}");

            /* Then switch out the pointer */
            Memory.Write(resCapLoc + 0x38, newBufferLoc);
            Memory.Write(resCapLoc + 0x30, shortLength);

            if (hadOldUpdatedArea)
            {
                Memory.FreeMemory((nint)oldBufferLoc);
                Log.Logger.Debug($"Free old {typeof(ParamT).Name} @ {oldBufferLoc:X}");
            }
            return true;
        }
    }
}

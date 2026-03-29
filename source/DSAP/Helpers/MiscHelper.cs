using Archipelago.Core.Util;
using Archipelago.Core.Util.GPS;
using DSAP.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Location = Archipelago.Core.Models.Location;
namespace DSAP.Helpers
{
    public class MiscHelper
    {
        /// <summary>
        /// Determine if a reload of this area should be done
        /// </summary>
        /// <param name="descArea"></param>
        /// <param name="checkArea"></param>
        /// <returns>whether reload is required</returns>
        internal static bool ValidateDescArea(DescArea descArea, string checkArea)
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
                Log.Logger.Error($"Unknown metadata size detected on {checkArea}. A different mod may be interfering.");
                Log.Logger.Error("Try restarting DSR without other mods.");
                return false;
            }
        }
        internal static int GetPlayerHP()
        {
            return Memory.ReadInt(AddressHelper.GetPlayerHPAddress());
        }

        public static ulong OffsetPointer(ulong ptr, uint offset)
        {
            ulong newAddress = ptr;
            return ptr + (ulong)offset;
        }

        /// <summary>
        /// Build a mapping of the slot:locationid key to itemid:upg value based on info stored in slotdata from the server.
        /// </summary>
        /// <details>
        /// This is used later when replacing or receiving items - to know if they should be ugpraded.
        /// This is needed because we don't have an individual ApId per possible upgrade, but it is deterministic from the generate.
        /// </details>
        /// <returns></returns>
        public static Dictionary<string, Tuple<int, string>> BuildSlotLocationToItemUpgMap(Dictionary<string, object> slotData, int currentSlot)
        {
            Dictionary<string, Tuple<int, string>> result = [];
            
            if (App.DSOptions.ApworldCompare("0.0.20.0") < 0) /* apworld is < 0.0.20.0, which introduces weapon upgrades */
            {
                Log.Logger.Warning($"Apworld version too low, skipping weapon upgrade mapping.");
                return result;
            }

            if (App.DSOptions.UpgradedWeaponsPercentage == 0)
            {
                Log.Logger.Information($"Upgraded weapon percentage detected as 0, skipping weapon upgrades.");
                return result;
            }

            /* Get itemsAddress, itemsId, and itemsUpgrades into lists */
            List<string> itemsAddress = new List<string?>();
            List<int?> itemsId = new List<int?>();
            List<string?> itemsUpgrades = new List<string?>();

            try
            {
                if (slotData.TryGetValue("itemsAddress", out object itemsAddress_temp))
                {
                    itemsAddress.AddRange(JsonSerializer.Deserialize<string?[]>(itemsAddress_temp.ToString()));
                    if (slotData.TryGetValue("itemsId", out object itemsId_temp))
                    {
                        itemsId.AddRange(JsonSerializer.Deserialize<int?[]>(itemsId_temp.ToString()));
                        if (slotData.TryGetValue("itemsUpgrades", out object itemsUpgrades_temp))
                        {
                            itemsUpgrades.AddRange(JsonSerializer.Deserialize<string[]>(itemsUpgrades_temp.ToString()));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error($"exception creating upg map: {e.Message} {e.ToString()}");
            }

            if (itemsAddress.Count == 0 || itemsId.Count == 0 || itemsUpgrades.Count == 0
             || itemsAddress.Count != itemsId.Count || itemsAddress.Count != itemsUpgrades.Count)
            {
                Log.Logger.Error("Cannot map item upgrades: itemsAddress, itemsId, itemsUpgrades count mismatch.");
                Log.Logger.Error($"{itemsAddress.Count},{itemsId.Count},{itemsUpgrades.Count}");
                App.Client.AddOverlayMessage("Cannot map item upgrades: itemsAddress, itemsId, itemsUpgrades count mismatch.");
                App.Client.AddOverlayMessage($"{itemsAddress.Count},{itemsId.Count},{itemsUpgrades.Count}");
            }
            else
            {
                /* Iterate over each pair of entries in the pair of lists */
                for (int i = 0; i < itemsAddress.Count; i++)
                {
                    string address = itemsAddress[i];
                    int? id = itemsId[i];
                    string? upgrade = itemsUpgrades[i];

                    /* skip it if there's no item id, no upgrade info, and it doesn't have a location address */
                    if (id.HasValue && upgrade != null && !address.EndsWith(":None")) 
                    {
                        /* Now processing each potential items with upgrades */
                        /* key = address ("1:4000") */
                        /* value = item id , upgrade (8000, "Magic:5")*/
                        result[address] = new (id.Value, upgrade);
                    }
                }
            }
            Log.Logger.Debug($"upgdict size = {result.Count}");

            return result;
        }


        public static bool GetIsPlayerOnline()
        {
            var baseCOffset = AddressHelper.GetBaseCOffset();
            ulong onlineFlagOffset = 0xB7D;

            var isOnline = Memory.ReadByte(baseCOffset + onlineFlagOffset) != 0;
            return isOnline;

        }
        public static bool SetLastBonfireTo(Enums.Bonfires bonfireId)
        {
            var baseCoff = AddressHelper.GetBaseCOffset();
            if (baseCoff != 0)
            {
                var baseC = Memory.ReadULong(baseCoff);
                if (baseC != 0)
                {
                    var lastBonfireAddress = OffsetPointer(baseC, 0xB34);
                    Memory.Write(lastBonfireAddress, (int)bonfireId);
                    return true;
                }
            }
            return false;
        }
        public static LastBonfire GetLastBonfire()
        {

            var baseC = AddressHelper.GetBaseCOffset();
            var lastBonfireAddress = OffsetPointer(baseC, 0xB34);
            var lastBonfireId = Memory.ReadInt(lastBonfireAddress);
            //todo get last bonfire
            var list = GetLastBonfireList();
            var lastBonfire = list.FirstOrDefault(x => x.id == lastBonfireId);
            if (lastBonfire != null)
            {
                return lastBonfire;
            }
            return null;
        }
        public static async void MonitorLastBonfire(Action<LastBonfire> action)
        {
            var lastBonfire = GetLastBonfire();
            if (lastBonfire == null)
            {
                Log.Logger.Debug("No Last Bonfire found");
            }
            else Log.Logger.Debug($"Last bonfire was {lastBonfire.id}:{lastBonfire.name} ");
            while (true)
            {
                var currentLastBonfire = GetLastBonfire();
                if (currentLastBonfire != lastBonfire)
                {
                    Log.Logger.Debug("Last Bonfire Changed");
                    lastBonfire = currentLastBonfire;
                    action?.Invoke(currentLastBonfire);
                }
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
        public static List<LastBonfire> GetLastBonfireList()
        {
            var json = MiscHelper.OpenEmbeddedResource("DSAP.Resources.LastBonfire.json");
            var list = JsonSerializer.Deserialize<List<LastBonfire>>(json, MiscHelper.GetJsonOptions());
            return list;
        }
        public static List<Loadout> GetLoadouts()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Loadouts.json");
            var list = JsonSerializer.Deserialize<List<Loadout>>(json, MiscHelper.GetJsonOptions());
            return list;
        }
        public static List<EventFlag> GetGiftParams()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.GiftParams.json");
            var list = JsonSerializer.Deserialize<List<EventFlag>>(json, MiscHelper.GetJsonOptions());
            return list;
        }
        public static List<Gift> GetGiftsPool()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.GiftsPool.json");
            var list = JsonSerializer.Deserialize<List<Gift>>(json, MiscHelper.GetJsonOptions());
            return list;
        }
        public static List<ThiefItem> GetThiefItemsPool()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.ThiefItemsPool.json");
            var list = JsonSerializer.Deserialize<List<ThiefItem>>(json, MiscHelper.GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetConsumables()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Consumables.json");
            var list = System.Text.Json.JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetUpgradeMaterials()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.UpgradeMaterials.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetEmbers()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Embers.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetKeyItems()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.KeyItems.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetRings()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Rings.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetSpells()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Spells.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetShields()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Shields.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetTraps()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Traps.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetBonfireWarpItems()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Bonfires.json");
            var list = JsonSerializer.Deserialize<List<BonfireWarp>>(json, GetJsonOptions());
            List<DarkSoulsItem> newlist = list.Where(x => x.ItemId != 0).Select(x => new DarkSoulsItem()
            {
                Name = x.ItemName,
                Id = x.DsrId, // dsr id of warp unlock item
                StackSize = 1,
                UpgradeType = Enums.ItemUpgrade.None,
                Category = Enums.DSItemCategory.BonfireWarp,
                ApId = x.ItemId, // ap id of event item
            }).ToList();
            return newlist;
        }
        public static List<BonfireWarp> GetBonfireWarpInfos()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Bonfires.json");
            var list = JsonSerializer.Deserialize<List<BonfireWarp>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetDsrEventItems()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.DsrEvents.json");
            var list = JsonSerializer.Deserialize<List<DsrEvent>>(json, GetJsonOptions());
            List<DarkSoulsItem> newlist = list.Select(x => new DarkSoulsItem()
            {
                Name = x.Itemname,
                Id = x.Dsrid, // dsr id of event item
                StackSize = 1,
                UpgradeType = Enums.ItemUpgrade.None,
                Category = Enums.DSItemCategory.DsrEvent,
                ApId = x.Itemid, // ap id of event item
            }
            ).ToList();
            return newlist;
        }
        public static List<EmkController> GetDsrEventEmks()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.DsrEvents.json");
            var list = JsonSerializer.Deserialize<List<DsrEvent>>(json, GetJsonOptions());
            List<EmkController> newlist = list.Select(x => new EmkController(x.Locname, x.Type,
                x.Eventid, x.Eventslot, x.Itemid)).ToList();
            return newlist;
        }
        public static List<DarkSoulsItem> GetRangedWeapons()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.RangedWeapons.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetMeleeWeapons()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.MeleeWeapons.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetArmor()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.Armor.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetSpellTools()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.SpellTools.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static DarkSoulsItem UpgradeItem(DarkSoulsItem item, string itemupg, bool log = false)
        {
            if (itemupg != null)
            {
                Dictionary<String, int> infusionmap = new Dictionary<string, int>
                {
                    {"Normal", 0},
                    {"Crystal", 1},
                    {"Lightning", 2},
                    {"Raw", 3},
                    {"Magic", 4},
                    {"Enchanted", 5},
                    {"Divine", 6},
                    {"Occult", 7},
                    {"Fire", 8},
                    {"Chaos", 9},
                };

                string[] tokens = itemupg.Split(':');
                if (tokens.Count() == 2)
                {
                    var infusionMod= infusionmap[tokens[0]];
                    var lvl = Int32.Parse(tokens[1]);
                    DarkSoulsItem newitem = new DarkSoulsItem
                    {
                        Name = item.Name,
                        Id = item.Id + lvl + 100 * infusionMod,
                        StackSize = item.StackSize,
                        UpgradeType = item.UpgradeType,
                        Category = item.Category,
                        ApId = item.ApId
                    };
                    if (log)
                    {
                        Log.Logger.Information($"Upgraded item {item.Name} to {itemupg}");
                        App.Client.AddOverlayMessage($"Upgraded item {item.Name} to {itemupg}");
                    }
                        
                    
                    return newitem;
                }
            }
            Log.Logger.Error($"Error upgrading item {item.Name}");
            App.Client.AddOverlayMessage($"Error upgrading item {item.Name}");

            return item;
        }
        public static List<DarkSoulsItem> GetUsableItems()
        {
            var json = OpenEmbeddedResource("DSAP.Resources.UsableItems.json");
            var list = JsonSerializer.Deserialize<List<DarkSoulsItem>>(json, GetJsonOptions());
            return list;
        }
        public static List<DarkSoulsItem> GetAllItems()
        {
            var results = new List<DarkSoulsItem>();

            results = results.Concat(GetConsumables()).ToList();
            results = results.Concat(GetKeyItems()).ToList();
            results = results.Concat(GetRings()).ToList();
            results = results.Concat(GetUpgradeMaterials()).ToList();
            results = results.Concat(GetEmbers()).ToList();
            results = results.Concat(GetSpells()).ToList();
            results = results.Concat(GetShields()).ToList();
            results = results.Concat(GetRangedWeapons()).ToList();
            results = results.Concat(GetSpellTools()).ToList();
            results = results.Concat(GetUsableItems()).ToList();
            results = results.Concat(GetMeleeWeapons()).ToList();
            results = results.Concat(GetArmor()).ToList();
            results = results.Concat(GetTraps()).ToList();
            results = results.Concat(GetDsrEventItems()).ToList();
            results = results.Concat(GetBonfireWarpItems()).ToList();

            return results;
        }
        public static bool IsInGame()
        {
            if (getIngameTime() != 0)
                return true;
            return false;
        }        
        public static uint getIngameTime()
        {
            var baseB = AddressHelper.GetBaseBAddress();
            if (baseB != 0)
            {
                var next = OffsetPointer(baseB, 0xA4);
                return Memory.ReadUInt(next);
            }
            return 0; 
        }
        public static PositionData GetPosition()
        {
            Log.Logger.Debug("Getting position");
            var PositionData = new PositionData();
            if (IsInGame())
            {
                // map = worldnumber + area number. e.g. 10 + 02 => m10_02 = firelink shrine
                ulong eoffset = AddressHelper.GetBaseEAddress();
                if (eoffset != 0)
                {
                    uint worldnumber = GetWorldNumber();
                    uint areanumber = GetAreaNumber();
                    Log.Logger.Debug($"Position update: got w/a {worldnumber} {areanumber}");
                    if (worldnumber > 9 && worldnumber < 19 && areanumber >= 0 && areanumber < 3)
                    {
                        PositionData.MapId = (int)(1000000 * worldnumber + 10000 * areanumber);
                        Log.Logger.Debug($"Got position: {PositionData.MapId}");
                        return PositionData;
                    }
                }
            }
            PositionData.MapId = App.Client.GPSHandler?.MapId ?? 0;
            Log.Logger.Debug($"Got position: {PositionData.MapId} (no update)");
            return PositionData;
        }
        public static uint GetWorldNumber(ulong eOffset = 0) // E + A23
        {
            if (eOffset == 0)
                eOffset = AddressHelper.GetBaseEAddress();
            if (eOffset != 0)
            {
                var next = OffsetPointer(eOffset, 0xA23);
                return Memory.ReadByte(next);
            }
            return 0;
        }
        public static uint GetAreaNumber(ulong eOffset = 0) // E + A22
        {
            if (eOffset == 0)
                eOffset = AddressHelper.GetBaseEAddress();
            if (eOffset != 0)
            {
                var next = OffsetPointer(eOffset, 0xA22);
                return Memory.ReadByte(next);
            }
            return 0;
        }
        public static string OpenEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string file = reader.ReadToEnd();
                return file;
            }
        }
        public static byte[] GetItemCommand()
        {
            byte[] x = new byte[] {
                0x41, 0xb9, 0x00, 0x00, 0x00, 0x00,       //mov         r9d,0x0          // amount
                0x41, 0xb8, 0x00, 0x00, 0x00, 0x00,       //mov         r8d,0x0          // itemid
                0xba, 0x00, 0x00, 0x00, 0x00,             //mov         edx,0x0          // category
                0x48, 0xa1, 0x30, 0xa5, 0xc8, 0x41, 0x01, //movabs      rax,[0x141c8a530]
                0x00, 0x00, 0x00,
                0x48, 0x85, 0xc0,                         //TEST        RAX,RAX
                0x74, 0x31,                               //JZ          0x31  (BadRaX)
                0x4c, 0x8b, 0x78, 0x10,                   //mov         r15,[rax+0x10]
                0x49, 0x8d, 0x8f, 0x80, 0x02, 0x00, 0x00, //lea         rcx,[r15+0x280]
                0x48, 0x83, 0xec, 0x38,                   //sub         rsp,0x38
                0x49, 0xbe, 0xe0, 0x79, 0x74, 0x40, 0x01, //movabs      r14,0x1407479E0  // addItemToInventory()
                0x00, 0x00, 0x00,
                0x41, 0xff, 0xd6,                         //call        r14
                0x48, 0x83, 0xc4, 0x38,                   //add         rsp,0x38

                0x48, 0xb8,                               //movabs rax,0x1234567812345678 // replace with resultArea
                0x78, 0x56, 0x34, 0x12,  // target operand -> result area (8 bytes)
                0x78, 0x56, 0x34, 0x12,
                0xc7, 0x00, 0x00, 0x00, 0x00, 0x00,        //mov DWORD PTR[rax],0x00000000
                0xc3,                                     //RET 
                // BadRAX:
                0x48, 0xb8,                               //movabs rax,0x1234567812345678 // replace with resultArea
                0x78, 0x56, 0x34, 0x12,  // target operand -> result area (8 bytes)
                0x78, 0x56, 0x34, 0x12,
                0xc7, 0x00, 0xff, 0xff, 0xff, 0xff,        //mov DWORD PTR[rax],0xffffffff
                0xc3,                                     //RET 

            };
            return x;
        }


        public static byte[] GetItemWithMessageCommand()
        {
            // could use additional validation
            // - check 0x141c891a8 / ItemGetMenuManImpl before the addItemToInventory,
            // - check 0x141c8a530 / GameDataMan for null before it is dereferenced
            byte[] x = new byte[] {
                0x41, 0xb9, 0x00, 0x00, 0x00, 0x00,       //mov         r9d,0x0          // amount
                0x41, 0xb8, 0x00, 0x00, 0x00, 0x00,       //mov         r8d,0x0          // itemid
                0xba, 0x00, 0x00, 0x00, 0x00,             //mov         edx,0x0          // category
                0x48, 0xa1, 0x30, 0xa5, 0xc8, 0x41, 0x01, //movabs      rax,[0x141c8a530]
                0x00, 0x00, 0x00,
                0x48, 0x85, 0xc0,                         //TEST        RAX,RAX
                0x0f, 0x84, 0xda, 0x00, 0x00, 0x00,       //JZ          +218 / 0xDA (BadRAX)
                0x4c, 0x8b, 0x78, 0x10,                   //mov         r15,[rax+0x10]
                0x49, 0x8d, 0x8f, 0x80, 0x02, 0x00, 0x00, //lea         rcx,[r15+0x280]
                0x48, 0x83, 0xec, 0x38,                   //sub         rsp,0x38
                0x49, 0xbe, 0xe0, 0x79, 0x74, 0x40, 0x01, //movabs      r14,0x1407479E0  // addItemToInventory()
                0x00, 0x00, 0x00,
                0x41, 0xff, 0xd6,                         //call        r14
                0x48, 0x83, 0xc4, 0x38,                   //add         rsp,0x38

                0x41, 0xb9, 0x00, 0x00, 0x00, 0x00,       //mov         r9d,0x0          // amount
                0x41, 0xb8, 0x00, 0x00, 0x00, 0x00,       //mov         r8d,0x0          // itemid
                0xba, 0x00, 0x00, 0x00, 0x00,             //mov         edx,0x0          // category
                0x48, 0xb9, 0xa8, 0x91, 0xc8, 0x41, 0x01, //movabs      rcx,0x141c891a8 // ItemGetMenuMan 
                0x00, 0x00, 0x00,
                0x48, 0x8b, 0x09,                         //mov         rcx,QWORD PTR [rcx]
                0x48, 0x83, 0xec, 0x64,                   //sub         rsp,0x64

                0x40, 0x53,                               //PUSH        RBX
                0x4c, 0x8b, 0xd9,                         //MOV         R11,RCX
                0x33, 0xc0,                               //XOR         EAX,EAX
                0x48, 0x85, 0xc9,                         //TEST        RCX,RCX
                0x0f, 0x84, 0x99, 0x00, 0x00, 0x00,       //JZ          +153 / 0x99 (BadRCX)
                0x48, 0x8b, 0x49, 0x10,                   //MOV         RCX,qword ptr [RCX + 0x10]
                0x8b, 0xda,                               //MOV         EBX,EDX
                0x48, 0x85, 0xc9,                         //TEST        RCX,RCX
                0x74, 0x0c,                               //JZ          0x0c
                0x4c, 0x8b, 0x11,                         //MOV         R10,qword ptr [RCX]
                0x48, 0x8b, 0xc1,                         //MOV         RAX,RCX
                0x4d, 0x89, 0x53, 0x10,                   //MOV         qword ptr [R11 + 0x10],R10
                0xeb, 0x34,                               //JMP         0x34
                0x49, 0x8b, 0x4b, 0x08,                   //MOV         RCX,qword ptr [R11 + 0x8]
                0x48, 0x85, 0xc9,                         //TEST        RCX,RCX
                0x74, 0x42,                               //JZ          0x42
                0x48, 0x8b, 0xc1,                         //MOV         RAX,RCX
                0x48, 0x8b, 0x09,                         //MOV         RCX,qword ptr [RCX]
                0x48, 0x85, 0xc9,                         //TEST        RCX,RCX
                0x74, 0x18,                               //JZ          0x18
                0x48, 0x8b, 0xd0,                         //MOV         RDX,RAX
                0x48, 0x8b, 0xc1,                         //MOV         RAX,RCX
                0x48, 0x8b, 0x09,                         //MOV         RCX,qword ptr [RCX]
                0x48, 0x85, 0xc9,                         //TEST        RCX,RCX
                0x75, 0xf2,                               //JNZ         0xf2
                0x48, 0x85, 0xd2,                         //TEST        RDX,RDX
                0x74, 0x05,                               //JZ          0x8
                0x48, 0x89, 0x0a,                         //MOV         qword ptr [RDX],RCX
                0xeb, 0x08,                               //JMP         0x8
                0x49, 0xc7, 0x43,                         //MOV         qword ptr [R11 + 0x8],0x0
                0x08, 0x00, 0x00,
                0x00, 0x00,
                0x48, 0x85, 0xc0,                         //TEST        RAX,RAX
                0x74, 0x12,                               //JZ          0x12
                0x48, 0xc7, 0x00,                         //MOV         qword ptr [RAX],0x0
                0x00, 0x00, 0x00, 0x00,
                0x89, 0x58, 0x08,                         //MOV         dword ptr [RAX + 0x8],EBX
                0x44, 0x89, 0x40, 0x0c,                   //MOV         dword ptr [RAX + 0xc],R8D
                0x44, 0x89, 0x48, 0x10,                   //MOV         dword ptr [RAX + 0x10],R9D
                0x49, 0x8b, 0x4b, 0x08,                   //MOV         RCX,qword ptr [R11 + 0x8]
                0x48, 0x89, 0x08,                         //MOV         qword ptr [RAX],RCX
                0xb9, 0x2c, 0x01,                         //MOV         ECX,0x12c
                0x00, 0x00,
                0x49, 0x89, 0x43, 0x08,                   //MOV         qword ptr [R11 + 0x8],RAX
                0x5b,                                     //POP         RBX
                
                0x48, 0x83, 0xc4, 0x64,                   //add         rsp,0x64
                0x48, 0xb8,                               //movabs rax,0x1234567812345678
                0x78, 0x56, 0x34, 0x12,
                0x78, 0x56, 0x34, 0x12,
                0xc7, 0x00, 0x00, 0x00, 0x00, 0x00,       //mov DWORD PTR[rax],0x00000000
                0xc3,                                     //RET 
                // BadRAX:
                0x48, 0xb8,                               //movabs rax,0x1234567812345678 // replace with resultArea
                0x78, 0x56, 0x34, 0x12,  // target operand -> result area (8 bytes)
                0x78, 0x56, 0x34, 0x12,
                0xc7, 0x00, 0xff, 0xff, 0xff, 0xff,        //mov DWORD PTR[rax],0xffffffff
                0xc3,                                     //RET 
                // BadRCX:
                0x5b,                                     //POP         RBX
                0x48, 0x83, 0xc4, 0x64,                   //add         rsp,0x64
                0x48, 0xb8,                               //movabs rax,0x1234567812345678
                0x78, 0x56, 0x34, 0x12,
                0x78, 0x56, 0x34, 0x12,
                0xc7, 0x00, 0x00, 0x00, 0x00, 0x00,       //mov DWORD PTR[rax],0x00000000
                0xc3,                                     //RET 

            };
            return x;
        }
        /*  ----------Code To Emulate--------------

    mov rcx,[BaseB]
    mov edx,1
    sub rsp,38
    call 0x1404867e0
    add rsp,38
    ret

*/
        /*  ----------Homeward Bone injected ASM--------------
            0:  48 c7 c1 78 56 34 12    mov    rcx,0x12345678
            7:  00
            8:  ba 01 00 00 00          mov    edx,0x1
            d:  49 be e0 67 48 40 01    movabs r14,0x1404867e0
            14: 00 00 00
            17: 48 83 ec 38             sub    rsp,0x38
            1b: 41 ff d6                call   r14
            1e: 48 83 c4 38             add    rsp,0x38
            22: c3                      ret 
         */
        public static byte[] HomewardBone()
        {
            byte[] x = new byte[] {
                    0x48, 0xC7, 0xC1, 0x78, 0x56, 0x34, 0x12,
                    0xBA, 0x01, 0x00, 0x00, 0x00,
                    0x49, 0xBE, 0xE0, 0x67, 0x48, 0x40, 0x01, 0x00, 0x00, 0x00,
                    0x48, 0x83, 0xEC, 0x38,
                    0x41, 0xFF, 0xD6,
                    0x48, 0x83, 0xC4, 0x38,
                    0xC3

            };

            return x;
        }
        protected internal static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions();
        }
        
        internal static byte GetSavedSaveId()
        {
            ulong address = AddressHelper.GetSaveIdAddress();
            return Memory.ReadByte(address);
        }

        internal static void SetSavedSaveId(byte newsaveid)
        {
            ulong address = AddressHelper.GetSaveIdAddress();
            Memory.Write(address, newsaveid);
        }
        internal static ushort GetSavedSeedHash()
        {
            ulong address = AddressHelper.GetSaveSeedAddress();
            return Memory.ReadUShort(address);
        }
        internal static void SetSavedSeedHash(ushort seedhash)
        {
            ulong address = AddressHelper.GetSaveSeedAddress();
            Memory.Write(address, seedhash);
        }
        internal static ushort HashSeed(string seed)
        {
            uint result = 31719121;
            foreach (char c in seed)
            {
                result ^= c;
                result = UInt32.RotateLeft(result, 11);
            }
            return (ushort)(result % 65000 + 1); // ensure it is a short, but non-zero.
        }
        internal static ushort GetSavedSlot()
        {
            ulong address = AddressHelper.GetSaveSlotAddress();
            return Memory.ReadUShort(address);
        }
        internal static void SetSavedSlot(ushort slot)
        {
            ulong address = AddressHelper.GetSaveSlotAddress();
            Memory.Write(address, slot);
        }
        
        internal static bool CanPopupItems()
        {
            ulong ItemGetMenuMan = Memory.ReadULong(0x141c891a8);
            if (ItemGetMenuMan == 0) 
                return false;

            ulong unused_node_queue = Memory.ReadULong(ItemGetMenuMan + 0x10);
            ulong valid_node_queue = Memory.ReadULong(ItemGetMenuMan + 0x8);
            if (unused_node_queue == 0 && valid_node_queue == 0)
                return false;

            return true;
        }

        internal static void TeleportIfPlayerHasKilled(string tpCommand, string bossName, string bonfireName, Enums.Bonfires bonfireId)
        {
            // Check if player has killed the boss. If so, teleport them to the Oolacile Sanctuary.
            var lotFlags = LocationHelper.GetBossFlags();
            var baseAddress = AddressHelper.GetEventFlagsOffset();
            BossFlag bossFlag = lotFlags.Find((x) => x.Name.Contains(bossName));
            var bossLoc = new Location
            {
                Name = bossFlag.Name,
                Address = baseAddress + AddressHelper.GetEventFlagOffset(bossFlag.Flag).Item1,
                AddressBit = AddressHelper.GetEventFlagOffset(bossFlag.Flag).Item2,
                Id = bossFlag.Id,
            };
            if (bossLoc.Check())
            {
                if (SetLastBonfireTo(bonfireId))
                {
                    App.HomewardBoneCommand();
                    Log.Logger.Information($"DLC teleport - player sent to {bonfireName}.");
                    App.Client.AddOverlayMessage($"DLC teleport - player sent to {bonfireName}.");
                }
            }
            else
            {
                Log.Logger.Information($"{tpCommand} teleport failed - player has not killed {bossName}.");
                App.Client.AddOverlayMessage($"{tpCommand} teleport failed - player has not killed {bossName}.");
            }
        }
    }
}

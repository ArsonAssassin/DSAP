
using Archipelago.Core;
using Archipelago.Core.MauiGUI;
using Archipelago.Core.MauiGUI.Models;
using Archipelago.Core.MauiGUI.ViewModels;
using Archipelago.Core.Models;
using Archipelago.Core.Traps;
using Archipelago.Core.Util;
using Archipelago.Core.Util.Overlay;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using DSAP.Models;
using Newtonsoft.Json;
using Serilog;
using SharpDX;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using static DSAP.Enums;
using Color = Microsoft.Maui.Graphics.Color;
using Location = Archipelago.Core.Models.Location;
namespace DSAP
{
    public partial class App : Application
    {
        static MainPageViewModel Context;
        private DeathLinkService _deathlinkService;

        public static ArchipelagoClient Client { get; set; }
        public static List<DarkSoulsItem> AllItems { get; set; }
        private static readonly object _lockObject = new object();
        private bool IsHandlingDeathlink = false;
        private static List<InjectedString> injectedStrings = new List<InjectedString>();
        public App()
        {
            InitializeComponent();

            Context = new MainPageViewModel();
            Context.ConnectClicked += Context_ConnectClicked;
            Context.UnstuckVisible = true;
            Context.CommandReceived += (e, a) =>
            {
                Client?.SendMessage(a.Command);
            };
            MainPage = new MainPage(Context);
            Context.ConnectButtonEnabled = true;
        }

        private async void Context_UnstuckClicked(object? sender, EventArgs e)
        {
            //var bonfireStates = Helpers.GetBonfireStates();
            //Log.Logger.Information(JsonConvert.SerializeObject(bonfireStates));
            var originalLots = Helpers.GetItemLots();
            await RemoveItems();
            var overwrittenLots = Helpers.GetItemLots();

            if(originalLots == overwrittenLots)
            {
                Log.Error("Overwriting itemlots failed.");
            }
        }

        public static void AddItem(int category, int id, int quantity)
        {
            var command = Helpers.GetItemCommand();
            //Set item category
            Array.Copy(BitConverter.GetBytes(category), 0, command, 0x1, 4);
            //Set item quantity
            Array.Copy(BitConverter.GetBytes(quantity), 0, command, 0x7, 4);
            //set item id
            Array.Copy(BitConverter.GetBytes(id), 0, command, 0xD, 4);

            var result = Memory.ExecuteCommand(command);
        }
        public static void AddItemWithMessage(int category, int id, int quantity)
        {
            var command = Helpers.GetItemWithMessage();

            // Set item category (at offset 0x3F)
            Array.Copy(BitConverter.GetBytes(category), 0, command, 0x3F, 4);

            // Set item quantity (at offset 0x43)
            Array.Copy(BitConverter.GetBytes(quantity), 0, command, 0x43, 4);

            // Set item id (at offset 0x47)
            Array.Copy(BitConverter.GetBytes(id), 0, command, 0x47, 4);

            var result = Memory.ExecuteCommand(command);
        }

        public static bool ItemPickupDialogWithoutPickup(int category, int id, int quantity)
        {
            // Tested this method of displaying messages with a 100 back to back triggers and it does not crash the game
            ulong itemPickupDialogManImpl = Helpers.ResolvePointerChain(0x141C891A8, new int[] { 0x0, 0x0 });
            ItemPickupDialogLinkedList itemPickupLL = Memory.ReadStruct<ItemPickupDialogLinkedList>(itemPickupDialogManImpl);
            ulong currIdxOfLastElement = (itemPickupLL.NextAllocationInLL - itemPickupLL.StartOfLL) / 0x18;

            if(currIdxOfLastElement >= 5)
            {
                return false;
            }

            LinkedListItemData itemData = itemPickupLL.Items[currIdxOfLastElement];
            itemData.ItemCategory = (uint)category;
            itemData.ItemCode = (uint)id;
            itemData.ItemCount = (uint)quantity;
            itemData.PreviousItemInLL = itemPickupLL.StartOfLL + ((currIdxOfLastElement-1) * 0x18);
            if(currIdxOfLastElement == 0)
            {
                itemData.PreviousItemInLL = 0;
            }
            itemPickupLL.Items[currIdxOfLastElement] = itemData;
            itemPickupLL.NextAllocationInLL += 0x18;
            itemPickupLL.LastElementLinkedList = itemPickupLL.NextAllocationInLL - 0x18;

            Memory.WriteStruct<ItemPickupDialogLinkedList>(itemPickupDialogManImpl, itemPickupLL);
            return true;
        }

        /// <summary>
        /// This command triggers the anti debugger occasionally use ItemPickupDialogWithoutPickup instead
        /// <summary>
        public static void ItemPickupDialogWithoutPickupCommand(int category, int id, int quantity)
        {
            var command = Helpers.ItemPickupDialogWithoutPickup();

            // Set item category (at offset 0x3F)
            Array.Copy(BitConverter.GetBytes(category), 0, command, 0x38, 4);

            // Set item quantity (at offset 0x43)
            Array.Copy(BitConverter.GetBytes(quantity), 0, command, 0x3C, 4);

            // Set item id (at offset 0x47)
            Array.Copy(BitConverter.GetBytes(id), 0, command, 0x40, 4);

            var result = Memory.ExecuteCommand(command);
        }

        public static void RemoveItemPickupDialogSetupFunction()
        { 
            long itemPickupDialogSetupFunction = 0x140728c90;
            var command = Helpers.InjectItemPickupDialogSwitch();
            long address = 0x1400003F0;
            int destinationIndex = 0x12;
            long offsetToItemPickupSetupFunction = itemPickupDialogSetupFunction - (address + destinationIndex);
            byte[] offsetInjectedFunctionBytes = BitConverter.GetBytes((int)offsetToItemPickupSetupFunction);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(offsetInjectedFunctionBytes);
            }
            Array.Copy(offsetInjectedFunctionBytes, 0, command, destinationIndex - 0x4, 4);
            Memory.WriteByteArray((ulong)address, command);

            long itemPickupDialogSetupFunctionCall = 0x1403fe4fa;
            long offset = address - (itemPickupDialogSetupFunctionCall + 0x5);
            byte[] injectedFuncitonCall = new byte[5];
            injectedFuncitonCall[0] = 0xE8;
            byte[] offsetBytes = BitConverter.GetBytes((int)offset);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(offsetBytes);
            }
            Array.Copy(offsetBytes, 0, injectedFuncitonCall, 1, 4);
            Memory.WriteByteArray((ulong)itemPickupDialogSetupFunctionCall, injectedFuncitonCall);

        }

        public static bool IsValidPointer(ulong address)
        {
            try
            {
                Memory.ReadByte(address);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async void Context_ConnectClicked(object? sender, ConnectClickedEventArgs e)
        {
            Context.ConnectButtonEnabled = false;
            Log.Logger.Information("Connecting...");
            if (Client != null)
            {
                Client.Connected -= OnConnected;
                Client.Disconnected -= OnDisconnected;
                Client.ItemReceived -= Client_ItemReceived;
                Client.MessageReceived -= Client_MessageReceived;
                Context.UnstuckClicked -= Context_UnstuckClicked;
                //if (_deathlinkService != null)
                //{
                //    _deathlinkService.OnDeathLinkReceived -= _deathlinkService_OnDeathLinkReceived;
                //    _deathlinkService = null;
                //}
                Client.CancelMonitors();
            }
            DarkSoulsClient client = new DarkSoulsClient();
            var connected = client.Connect();
            if (!connected)
            {
                Log.Logger.Error("Dark Souls not running, open Dark Souls before connecting!");
                Context.ConnectButtonEnabled = true;
                return;
            }

            Client = new ArchipelagoClient(client);

            AllItems = Helpers.GetAllItems();
            Client.Connected += OnConnected;
            Client.Disconnected += OnDisconnected;
            var isOnline = Helpers.GetIsPlayerOnline();
            if (isOnline)
            {
                Log.Logger.Warning("YOU ARE PLAYING ONLINE. THIS APPLICATION WILL NOT PROCEED.");
                Context.ConnectButtonEnabled = true;
                return;
            }

            await Client.Connect(e.Host, "Dark Souls Remastered");

            Client.ItemReceived += Client_ItemReceived;
            Client.MessageReceived += Client_MessageReceived;
            Context.UnstuckClicked += Context_UnstuckClicked;

            await Client.Login(e.Slot, !string.IsNullOrWhiteSpace(e.Password) ? e.Password : null);

            Client.IntializeOverlayService(new WindowsOverlayService());

            //if (Client.Options.ContainsKey("enable_deathlink") && (bool)Client.Options["enable_deathlink"])
            //{
            //    _deathlinkService = Client.EnableDeathLink();
            //    _deathlinkService.OnDeathLinkReceived += _deathlinkService_OnDeathLinkReceived;
            //    Memory.MonitorAddressForAction<int>(Helpers.GetPlayerHPAddress(), () => SendDeathlink(_deathlinkService), (health) => Helpers.GetPlayerHP() <= 0);
            //}



            var bossLocations = Helpers.GetBossFlagLocations();
            var itemLocations = Helpers.GetItemLotLocations();
            var bonfireLocations = Helpers.GetBonfireFlagLocations();
            var doorLocations = Helpers.GetDoorFlagLocations();
            var fogWallLocations = Helpers.GetFogWallFlagLocations();
            var miscLocations = Helpers.GetMiscFlagLocations();

            var goalLocation = bossLocations.First(x => x.Name.Contains("Lord of Cinder"));
            Memory.MonitorAddressBitForAction(goalLocation.Address, goalLocation.AddressBit, () => Client.SendGoalCompletion());
            

            Client.MonitorLocations(bossLocations);
            Client.MonitorLocations(itemLocations);
            Client.MonitorLocations(bonfireLocations);
            Client.MonitorLocations(doorLocations);
            // Client.MonitorLocations(fogWallLocations);
            Client.MonitorLocations(miscLocations);

            //Helpers.MonitorLastBonfire((lastBonfire) =>
            //{
            //    Log.Logger.Debug($"Rested at bonfire: {lastBonfire.id}:{lastBonfire.name}");
            //});
            CleanUpItemPickupText();
            Memory.MonitorAddressByteChangeForAction(Helpers.GetItemPickupDialog(), 0x1, 0x0, () => CleanUpItemPickupText());

            RemoveItems();
            RemoveItemPickupDialogSetupFunction();

            Context.ConnectButtonEnabled = true;


        }

        private void CleanUpItemPickupText()
        {
            foreach (InjectedString injString in injectedStrings)
            {
                Helpers.FreeItemPickupText(injString);
            }
            injectedStrings = new List<InjectedString>();
        }

        //private void SendDeathlink(DeathLinkService _deathlinkService)
        //{
        //    if (!IsHandlingDeathlink)
        //    {
        //        Log.Logger.Information("Sending Deathlink. RIP.");
        //        _deathlinkService.SendDeathLink(new DeathLink(Client.CurrentSession.Players.ActivePlayer.Name));
        //    }

        //    //Restart deathlink when player is alive again
        //    Memory.MonitorAddressForAction<int>(Helpers.GetPlayerHPAddress(), 
        //        () => {
        //            IsHandlingDeathlink = false;
        //            Memory.MonitorAddressForAction<int>(Helpers.GetPlayerHPAddress(),
        //                () => SendDeathlink(_deathlinkService),
        //                (health) => Helpers.GetPlayerHP() <= 0);
        //            },
        //        (health) => Helpers.GetPlayerHP() > 0);
        //}
        //private void _deathlinkService_OnDeathLinkReceived(DeathLink deathLink)
        //{
        //    Log.Logger.Information("Deathlink received. RIP.");
        //    IsHandlingDeathlink = true;
        //    Memory.Write(Helpers.GetPlayerHPAddress(), 0);
        //}

        private void Client_MessageReceived(object? sender, Archipelago.Core.Models.MessageReceivedEventArgs e)
        {
            if (e.Message.Parts.Any(x => x.Text == "[Hint]: "))
            {
                LogHint(e.Message);
            }
            Log.Logger.Information(JsonConvert.SerializeObject(e.Message));
            Client.AddOverlayMessage(e.Message.ToString(), TimeSpan.FromSeconds(10));
        }

        private static async Task RemoveItems()
        {
            var lots = Helpers.GetItemLots();
            var lotFlags = Helpers.GetItemLotFlags();

            //Helpers.WriteToFile("itemLots.json", lots);

            var replacementLot = new ItemLotParamStruct
            {
                LotRarity = 1,
                LotOverallGetItemFlagId = -1,
                LotCumulateNumFlagId = -1,
                LotCumulateNumMax = 0,
            };
            replacementLot.CumulateResetBits = 0;
            replacementLot.EnableLuckBits = 0;
            replacementLot.CumulateLotPoints[0] = 0;
            replacementLot.GetItemFlagIds[0] = -1;
            replacementLot.LotItemBasePoints[0] = 100;
            replacementLot.LotItemCategories[0] = (int)DSItemCategory.Consumables;
            replacementLot.LotItemNums[0] = 1;
            replacementLot.LotItemIds[0] = 370;

            for (int i = 0; i < lotFlags.Count; i++)
            {
                if (lotFlags[i].IsEnabled)
                {
                    ItemLot lot = lots.GetValueOrDefault(lotFlags[i].Flag);
                    Helpers.OverwriteItemLot(lot, replacementLot);
                }
            }
            Log.Logger.Information("Finished overwriting items");
        }
        private static void Client_ItemReceived(object? sender, ItemReceivedEventArgs e)
        {
            LogItem(e.Item);
            int itemAPId = (int) e.Item.Id;
            int itemCount = e.Item.Quantity;

            DarkSoulsItem fakeItem = new DarkSoulsItem();
            fakeItem.Category = DSItemCategory.Consumables;
            fakeItem.Id = 0x172;
            fakeItem.ApId = (int) e.Item.Id;
            fakeItem.Name = e.Item.Name;
            
            var itemToReceive = AllItems.FirstOrDefault(x => x.ApId == itemAPId, fakeItem);
            
            if (itemToReceive != fakeItem)
            {
                Log.Logger.Verbose($"Received {itemToReceive.Name} ({itemToReceive.ApId})");
                if (itemToReceive.ApId == 11120000)
                {
                    RunLagTrap();
                }
                else {
                    AddItem((int)itemToReceive.Category, itemToReceive.Id, itemCount);
                    ItemPickupDialogWithoutPickup(((int)itemToReceive.Category), itemToReceive.Id, itemCount);
                }
            }
            else
            {
                Log.Logger.Information("Couldnt find correct item");
                InjectedString injString = Helpers.SetItemPickupText(itemToReceive);
                injectedStrings.Add(injString);
                ItemPickupDialogWithoutPickup(((int)itemToReceive.Category), itemToReceive.Id, itemCount);
            }
        }

        private static async void RunLagTrap()
        {
            using (var lagTrap = new LagTrap(TimeSpan.FromSeconds(20)))
            {
                lagTrap.Start();
                await lagTrap.WaitForCompletionAsync();
            }
        }

        private static void LogItem(Item item)
        {
            var messageToLog = new LogListItem(new List<TextSpan>()
            {
                new TextSpan(){Text = $"[{item.Id.ToString()}] -", TextColor = Color.FromRgb(255, 255, 255)},
                new TextSpan(){Text = $"{item.Name}", TextColor = Color.FromRgb(200, 255, 200)},
                new TextSpan(){Text = $"x{item.Quantity.ToString()}", TextColor = Color.FromRgb(200, 255, 200)}
            });
            lock (_lockObject)
            {
                Application.Current.Dispatcher.DispatchAsync(() =>
                {
                    Context.ItemList.Add(messageToLog);
                });
            }

            
        }
        private static void LogHint(LogMessage message)
        {
            var newMessage = message.Parts.Select(x => x.Text);

            if (Context.HintList.Any(x => x.TextSpans.Select(y => y.Text) == newMessage))
            {
                return; //Hint already in list
            }
            List<TextSpan> spans = new List<TextSpan>();
            foreach (var part in message.Parts)
            {
                spans.Add(new TextSpan() { Text = part.Text, TextColor = Color.FromRgb(part.Color.R, part.Color.G, part.Color.B) });
            }
            lock (_lockObject)
            {
                Application.Current.Dispatcher.DispatchAsync(() =>
                {
                    Context.HintList.Add(new LogListItem(spans));
                });
            }
        }
        private static void OnConnected(object sender, EventArgs args)
        {
            Log.Logger.Information("Connected to Archipelago");
            Log.Logger.Information($"Playing {Client.CurrentSession.ConnectionInfo.Game} as {Client.CurrentSession.Players.GetPlayerName(Client.CurrentSession.ConnectionInfo.Slot)}");
        }

        private static void OnDisconnected(object sender, EventArgs args)
        {
            Log.Logger.Information("Disconnected from Archipelago");
        }
        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);
            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                window.Title = "DSAP - Dark Souls Archipelago Randomizer";

            }
            window.Width = 600;

            return window;
        }
    }
}

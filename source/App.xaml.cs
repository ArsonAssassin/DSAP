
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
using Windows.UI.Core;
using static DSAP.Enums;
using Location = Archipelago.Core.Models.Location;
using System.Threading.Tasks;
namespace DSAP
{
    public partial class App : Application
    {
        static MainPageViewModel Context;
        private DeathLinkService _deathlinkService;

        public static ArchipelagoClient Client { get; set; }
        public static List<DarkSoulsItem> AllItems { get; set; }
        private static Dictionary<int, ItemLot> ItemLotReplacementMap { get; set; }
        private static Dictionary<int, ItemLot> ConditionRewardMap { get; set; }
        private static readonly object _lockObject = new object();
        private bool IsHandlingDeathlink = false;
        public App()
        {
            InitializeComponent();
            //var options = new GuiDesignOptions
            //{
            //    BackgroundColor = Color.FromArgb("FF000000"),
            //    ButtonColor = Color.FromArgb("FFFF0000"),
            //    ButtonTextColor = Color.FromArgb("FF000000"),
            //    Title = "DSAP - Dark Souls Remastered Archipelago",

            //};

            Context = new MainPageViewModel();
            Context.ConnectClicked += Context_ConnectClicked;
            Context.CommandReceived += (e, a) =>
            {
                Client?.SendMessage(a.Command);
            };
            MainPage = new MainPage(Context);
            Context.ConnectButtonEnabled = true;
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

        public static void HomewardBoneCommand()
        {
            var command = Helpers.HomewardBone();

            Array.Copy(BitConverter.GetBytes(Helpers.GetBaseBOffset()), 0, command, 0x3, 4);

            var result = Memory.ExecuteCommand(command);

            Log.Logger.Information($"Forced Load Screen - Items Reset.");
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
        public static async Task MonitorLocations(List<Location> locations)
        {
            var locationBatches = locations
                .Select((location, index) => new { Location = location, Index = index })
                .GroupBy(x => x.Index / 25)
                .Select(g => g.Select(x => x.Location).ToList())
                .ToList();
            var tasks = locationBatches.Select(x => MonitorBatch(x));
            await Task.WhenAll(tasks);

        }
        private static async Task MonitorBatch(List<Location> batch)
        {
            List<Location> completed = new List<Location>();

            while (!batch.All(x => completed.Any(y => y.Id == x.Id)))
            {
                foreach (var location in batch)
                {
                    var isCompleted = location.Check();
                    if (isCompleted)
                    {
                        completed.Add(location);
                        //  Log.Logger.Information(JsonConvert.SerializeObject(location));
                    }
                }
                if (completed.Any())
                {
                    foreach (var location in completed)
                    {
                        Client.SendLocation(location);
                        //     Log.Logger.Information($"{location.Name} ({location.Id}) Completed");
                        batch.Remove(location);
                    }
                }
                completed.Clear();
                await Task.Delay(500);
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
                Client.LocationCompleted -= Client_LocationCompleted;
                if (_deathlinkService != null)
                {
                    _deathlinkService.OnDeathLinkReceived -= _deathlinkService_OnDeathLinkReceived;
                    _deathlinkService = null;
                }
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

            if (e.Host == null) e.Host = "localhost:38281";
            if (e.Slot == null) e.Slot = "Player1";
            await Client.Connect(e.Host, "Dark Souls Remastered");

            Client.ItemReceived += Client_ItemReceived;
            Client.MessageReceived += Client_MessageReceived;
            Client.LocationCompleted += Client_LocationCompleted;

            Client.IntializeOverlayService(new WindowsOverlayService());

            await Client.Login(e.Slot, !string.IsNullOrWhiteSpace(e.Password) ? e.Password : null);

            if (Client.Options.ContainsKey("enable_deathlink") && (bool)Client.Options["enable_deathlink"])
            {
                _deathlinkService = Client.EnableDeathLink();
                _deathlinkService.OnDeathLinkReceived += _deathlinkService_OnDeathLinkReceived;
                Memory.MonitorAddressForAction<int>(Helpers.GetPlayerHPAddress(), () => SendDeathlink(_deathlinkService), (health) => Helpers.GetPlayerHP() <= 0);
            }

            var bossLocations = Helpers.GetBossFlagLocations();
            var itemLocations = Helpers.GetItemLotLocations();
            var bonfireLocations = Helpers.GetBonfireFlagLocations();
            var doorLocations = Helpers.GetDoorFlagLocations();
            //var fogWallLocations = Helpers.GetFogWallFlagLocations();
            var miscLocations = Helpers.GetMiscFlagLocations();

            var goalLocation = (Location) bossLocations.First(x => x.Name.Contains("Lord of Cinder"));
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

            Context.ConnectButtonEnabled = true;


        }
        
        private void SendDeathlink(DeathLinkService _deathlinkService)
        {
            if (!IsHandlingDeathlink)
            {
                Log.Logger.Information("Sending Deathlink. RIP.");
                _deathlinkService.SendDeathLink(new DeathLink(Client.CurrentSession.Players.ActivePlayer.Name));
            }
            
            //Restart deathlink when player is alive again
            Memory.MonitorAddressForAction<int>(Helpers.GetPlayerHPAddress(), 
                () => {
                    IsHandlingDeathlink = false;
                    Memory.MonitorAddressForAction<int>(Helpers.GetPlayerHPAddress(),
                        () => SendDeathlink(_deathlinkService),
                        (health) => Helpers.GetPlayerHP() <= 0);
                    },
                (health) => Helpers.GetPlayerHP() > 0);
        }
        private void _deathlinkService_OnDeathLinkReceived(DeathLink deathLink)
        {
            Log.Logger.Information("Deathlink received. RIP.");
            IsHandlingDeathlink = true;
            Memory.Write(Helpers.GetPlayerHPAddress(), 0);
        }

        private void Client_MessageReceived(object? sender, Archipelago.Core.Models.MessageReceivedEventArgs e)
        {
            if (e.Message.Parts.Any(x => x.Text == "[Hint]: "))
            {
                LogHint(e.Message);
            }
            Log.Logger.Information(JsonConvert.SerializeObject(e.Message));
            Client.AddOverlayMessage(e.Message.ToString());
        }
        private void Client_LocationCompleted(object? sender, Archipelago.Core.Models.LocationCompletedEventArgs e)
        {
            var locid = e.CompletedLocation.Id;
            /* If the check was in non-itemlot locations, give the player items for it */
            if (ConditionRewardMap.ContainsKey(locid))
            {
                Log.Logger.Debug($"Found location in 'other' checks");
                var itemLot = ConditionRewardMap[locid];
                foreach (var  item in itemLot.Items)
                {
                    AddItem(item.LotItemCategory, item.LotItemId, 1);
                    Log.Logger.Debug($"Gave player {item.LotItemId}");
                }
            }
            Log.Logger.Debug($"Location Completed: {e.CompletedLocation.Name} at {e.CompletedLocation.Id}");
        }

        private static void ReplaceItems()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Helpers.OverwriteItemLots(ItemLotReplacementMap);
            watch.Stop();

            Log.Logger.Information($"Finished overwriting items, took {watch.ElapsedMilliseconds}ms");

            HomewardBoneCommand();
            Log.Logger.Information($"After Load screen, new item lots will be live.");
        }
        private static void Client_ItemReceived(object? sender, ItemReceivedEventArgs e)
        {
            LogItem(e.Item, 1);
            var itemId = e.Item.Id;
            var itemToReceive = AllItems.FirstOrDefault(x => x.ApId == itemId);
            if (itemToReceive != null)
            {
                Log.Logger.Verbose($"Received {itemToReceive.Name} ({itemToReceive.ApId})");
                if (itemToReceive.ApId == 11120000)
                {
                    RunLagTrap();
                }
                else AddItem((int)itemToReceive.Category, itemToReceive.Id, 1);
            }
            else
            {
                Log.Logger.Information("Couldnt find correct item");
                var filler = AllItems.First(x => x.Id == 380);
                AddItem((int)filler.Category, filler.Id, 1);
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

        private static void LogItem(Item item, int quantity)
        {
            var messageToLog = new LogListItem(new List<TextSpan>()
            {
                new TextSpan(){Text = $"[{item.Id.ToString()}] -", TextColor = Microsoft.Maui.Graphics.Color.FromRgb(255, 255, 255)},
                new TextSpan(){Text = $"{item.Name}", TextColor = Microsoft.Maui.Graphics.Color.FromRgb(200, 255, 200)},
                new TextSpan(){Text = $"x{quantity.ToString()}", TextColor = Microsoft.Maui.Graphics.Color.FromRgb(200, 255, 200)}
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
                spans.Add(new TextSpan() { Text = part.Text, TextColor = Microsoft.Maui.Graphics.Color.FromRgb(part.Color.R, part.Color.G, part.Color.B) });
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
            /* Make ready to receive items */

            /* If we haven't yet initialized the dictionary, do so. */
            if (ItemLotReplacementMap.Count == 0)
            {
                ItemLotReplacementMap = Helpers.BuildFlagToLotMap(Helpers.GetItemLotFlags().Where((x) => x.IsEnabled).Cast<EventFlag>().ToList());
                var nonItemLotFlags = Helpers.GetBossFlags().Cast<EventFlag>().ToList();
                nonItemLotFlags.AddRange(Helpers.GetBonfireFlags().Cast<EventFlag>());
                nonItemLotFlags.AddRange(Helpers.GetDoorFlags().Cast<EventFlag>());
                //nonItemLotFlags.AddRange(Helpers.GetFogWallFlags().Cast<EventFlag>());
                nonItemLotFlags.AddRange(Helpers.GetMiscFlags().Cast<EventFlag>());

                //var nonItemLotFlags = Helpers.GetDoorFlags().Cast<EventFlag>().ToList();
                Log.Logger.Debug($"nonitemlotflags count = {nonItemLotFlags.Count}");
                foreach (var item in nonItemLotFlags) Log.Logger.Information($"nonitemlotflags flag {item.Flag} id {item.Id} name {item.Name}");
                //ConditionRewardMap = Helpers.BuildIdFlagLotMap(nonItemLotFlags);
                ConditionRewardMap = Helpers.BuildIdToLotMap(nonItemLotFlags);

                foreach (var pair in ConditionRewardMap) Log.Logger.Warning($"ConditionRewardMap item {pair.Key} has {pair.Value.Items.Count} items, first is itemid {pair.Value.Items[0].LotItemId}");
                Log.Logger.Warning($"ConditionRewardMap has {ConditionRewardMap.Count} members");

            }
            /* Set to only receive remote items and starting inventory */
            Client.CurrentSession.ConnectionInfo.UpdateConnectionOptions(Client.CurrentSession.ConnectionInfo.Tags, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.IncludeStartingInventory);
            ReplaceItems();

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

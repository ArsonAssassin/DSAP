using Archipelago.Core;
using Archipelago.Core.AvaloniaGUI.Models;
using Archipelago.Core.AvaloniaGUI.ViewModels;
using Archipelago.Core.AvaloniaGUI.Views;
using Archipelago.Core.Models;
using Archipelago.Core.Traps;
using Archipelago.Core.Util;
using Archipelago.Core.Util.Overlay;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DSAP.Models;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using static DSAP.Enums;
using Color = Avalonia.Media.Color;
using Location = Archipelago.Core.Models.Location;

namespace DSAP;

public partial class App : Application
{
    public static MainWindowViewModel Context;
    private DeathLinkService _deathlinkService;
    DateTime lastDeathLinkTime = DateTime.MinValue;
    private const bool DEBUG_TXTLOG = false;
    public static ArchipelagoClient Client { get; set; }
    public static List<DarkSoulsItem> AllItems { get; set; }
    private static Dictionary<int, ItemLot> ItemLotReplacementMap = new Dictionary<int, ItemLot>();
    private static Dictionary<int, ItemLot> ConditionRewardMap = new Dictionary<int, ItemLot>();
    private static readonly object _lockObject = new object();
    private static readonly object _deathlinkLock = new object(); // lock that protects IsHandlingDeathLink and lastDeathLinkTime
    private bool IsHandlingDeathlink = false;
    TimeSpan graceperiod = new TimeSpan(0, 0, 25);
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Start();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Context
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainWindow
            {
                DataContext = Context
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    public void Start()
    {
        Context = new MainWindowViewModel("0.6.2");
        
        Context.ClientVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
        Context.ConnectClicked += Context_ConnectClicked;
        Context.CommandReceived += (e, a) =>
        {
            if (string.IsNullOrWhiteSpace(a.Command)) return;
            Client?.SendMessage(a.Command);
        };

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
    private async void Context_ConnectClicked(object? sender, ConnectClickedEventArgs e)
    {
        Context.ConnectButtonEnabled = false;

        // debugging
        if (DEBUG_TXTLOG)
        {
            Log.CloseAndFlush();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File("log.txt",
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }


        Log.Logger.Information("Connecting...");
        if (Client != null)
        {
            Client.Connected -= OnConnected;
            Client.Disconnected -= OnDisconnected;
            Client.ItemReceived -= Client_ItemReceived;
            Client.MessageReceived -= Client_MessageReceived;
            Client.LocationCompleted -= Client_LocationCompleted;
            Client.EnableLocationsCondition = null;
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

        if (!Client.IsConnected)
        {
            Log.Logger.Warning("Connect to AP Server failed");
            Context.ConnectButtonEnabled = true;
            return;

        }
        Client.ItemReceived += Client_ItemReceived;
        Client.MessageReceived += Client_MessageReceived;
        Client.LocationCompleted += Client_LocationCompleted;
        Client.EnableLocationsCondition = () => Helpers.IsInGame();

        //Client.IntializeOverlayService(new OverlayService());

        await Client.Login(e.Slot, !string.IsNullOrWhiteSpace(e.Password) ? e.Password : null, ItemsHandlingFlags.IncludeStartingInventory);

        if (!Client.IsLoggedIn)
        {
            Log.Logger.Warning("Login failed");
            Context.ConnectButtonEnabled = true;
            return;

        }
        if (Client.Options.ContainsKey("enable_deathlink") && ((JsonElement)Client.Options["enable_deathlink"]).GetUInt32() != 0)
        {
            _deathlinkService = Client.EnableDeathLink();
            _deathlinkService.OnDeathLinkReceived += _deathlinkService_OnDeathLinkReceived;
            Log.Debug($"initializing deathlink");
            Memory.MonitorAddressForAction<int>(Helpers.GetPlayerHPAddress(), () => SendDeathlink(_deathlinkService),
                (health) => _playerIsDead());
        }

        var bossLocations = Helpers.GetBossFlagLocations();
        var itemLocations = Helpers.GetItemLotLocations();
        var bonfireLocations = Helpers.GetBonfireFlagLocations();
        var doorLocations = Helpers.GetDoorFlagLocations();
        //var fogWallLocations = Helpers.GetFogWallFlagLocations();
        var miscLocations = Helpers.GetMiscFlagLocations();

        var goalLocation = (Location)bossLocations.First(x => x.Name.Contains("Lord of Cinder"));
        Memory.MonitorAddressBitForAction(goalLocation.Address, goalLocation.AddressBit, () => Client.SendGoalCompletion());

        var fullLocationsList = bossLocations.Union(itemLocations).Union(bonfireLocations).Union(doorLocations).Union(miscLocations).ToList();
        Client.MonitorLocations(fullLocationsList);


        //Helpers.MonitorLastBonfire((lastBonfire) =>
        //{
        //    Log.Logger.Debug($"Rested at bonfire: {lastBonfire.id}:{lastBonfire.name}");
        //});

        if (DEBUG_TXTLOG)
        { 
            Log.CloseAndFlush();
        }

        Context.ConnectButtonEnabled = true;


    }
    private void SendDeathlink(DeathLinkService _deathlinkService)
    {
        Log.Debug($"Attempting deathlink");

        var deathtime = System.DateTime.Now; /* get the "time of deathlink" before we wait for lock */
        lock (_deathlinkLock)
        {
            // If we are neither already handling one nor in grace period, send it to everybody else.
            if (!IsHandlingDeathlink
                && lastDeathLinkTime + graceperiod < deathtime)
            {
                Log.Logger.Information("Sending Deathlink. RIP.");
                lastDeathLinkTime = System.DateTime.Now;
                _deathlinkService.SendDeathLink(new DeathLink(Client.CurrentSession.Players.ActivePlayer.Name));
            }
            else if (IsHandlingDeathlink)
            {
                Log.Logger.Debug("Not sending Deathlink - still handling one we received.");
            }
            else 
            {
                Log.Logger.Information($"Not sending Deathlink - less than {graceperiod.TotalSeconds} seconds have passed since last Deathlink.");
            }
        }

        //Restart deathlink when player is alive again
        Memory.MonitorAddressForAction<int>(Helpers.GetPlayerHPAddress(),
            () => {                
                /* re-enable monitoring condition */
                Log.Debug($"Re-enabling deathlink");
                Memory.MonitorAddressForAction<int>(Helpers.GetPlayerHPAddress(),
                    () => SendDeathlink(_deathlinkService),
                    (health) => _playerIsDead());
                lock (_deathlinkLock)
                {
                    /* mark that we are no longer mid-deathlink once hp is positive and hook has been reset */
                    IsHandlingDeathlink = false;
                }
            },
            (health) => Helpers.IsInGame() && Helpers.GetPlayerHP() > 0); // condition to re-enable deathlink
    }
    /// <summary>
    /// Check if player is dead. Intended to be called after checking player hp == 0.
    /// </summary>
    /// <returns>True if player is both in game and has 0 hp</returns>
    private bool _playerIsDead()
    {
        /* Deathlink Check Reasoning:
            * This code is triggered once monitor finds hp <= 0.
            * Check if we're loaded into game, then check once again if player hp is still 0 (instead of using (health) var).
            * 
            * This avoids the race condition for when:
            * 1) Monitor triggers for hp=0 when not in game
            * 2) Player loads into game (hp restored)
            * 3) This condition is checked.
            * 
            * If they leave the game between the first check below and the second, it isn't a problem.
            *   That's because the only case where they got to this code while in game is when they're already dead!
            */
        if (Helpers.IsInGame())
        {
            if (Helpers.GetPlayerHP() <= 0)
            {
                return true;
            }
        }
        return false;
    }
    private void _deathlinkService_OnDeathLinkReceived(DeathLink deathLink)
    {
        Log.Logger.Information($"Deathlink received: {deathLink.Cause ?? deathLink.Source + " died."} RIP.");

        // Don't process deaths from ourself, if they come to us
        if (deathLink.Source != Client.CurrentSession.Players.ActivePlayer.Name )
        {
            var deathtime = System.DateTime.Now; /* get the "time of deathlink" before we wait for lock */
            lock (_deathlinkLock)
            {
                var playerInGame = Helpers.IsInGame();

                // If player is in game, not already handling deathlink, and not in grace period, receive it for real.
                if (playerInGame
                    && !IsHandlingDeathlink 
                    && lastDeathLinkTime + graceperiod < deathtime)
                {
                    ulong whpp = Helpers.GetPlayerWritableHPAddress();
                    Log.Logger.Debug($"whp address={whpp.ToString("X2")}");
                    /* If we got an address */
                    if (whpp != 0)
                    {
                        var whp = Memory.ReadInt(whpp);
                        Log.Logger.Debug($"whp value={whp}");
                        /* Extra guard rail: If it's not a real HP value, don't write it and instead error out */
                        if (whp < 10000)
                        {
                            Memory.Write(whpp, 0);
                            lastDeathLinkTime = System.DateTime.Now;
                            IsHandlingDeathlink = true;
                        }
                        else
                        {
                            Log.Error($"Deathlink ignored - could not resolve hp location.");
                        }
                    }
                    else
                    {
                        Log.Error($"Deathlink ignored - could not resolve hp location.");
                    }

                }
                else /* log why we aren't doing deathlink */
                {
                    if (!playerInGame)
                        Log.Information($"Deathlink ignored - player not in game");
                    else if (IsHandlingDeathlink)
                        Log.Information($"Deathlink ignored - already handling deathlink");
                    else if (lastDeathLinkTime + graceperiod >= deathtime)
                        Log.Information($"Deathlink ignored - less than {graceperiod.TotalSeconds} seconds have passed since previous Deathlink");
                }
            }
        }
    }

    private void Client_MessageReceived(object? sender, Archipelago.Core.Models.MessageReceivedEventArgs e)
    {
        if (e.Message.Parts.Any(x => x.Text == "[Hint]: "))
        {
            LogHint(e.Message);
        }
        Log.Logger.Information(JsonSerializer.Serialize(e.Message, Helpers.GetJsonOptions()));
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
            foreach (var item in itemLot.Items)
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
        Client.AddOverlayMessage($"Finished overwriting items, took {watch.ElapsedMilliseconds}ms");

        Log.Logger.Debug($"Player in game? {(Helpers.IsInGame() ? "yes" : "no")}");
        Log.Logger.Debug($"ingame time = {Helpers.getIngameTime()}");
        if (Helpers.IsInGame())
        {
            HomewardBoneCommand();
            Log.Logger.Information($"After Load screen, new item lots will be live.");
            Client.AddOverlayMessage($"After Load screen, new item lots will be live.");
        }
        else
        {
            Log.Logger.Information($"You are now safe to load your save.");
            Client.AddOverlayMessage($"You are now safe to load you save.");
        }
    }
    private static void Client_ItemReceived(object? sender, ItemReceivedEventArgs e)
    {
        LogItem(e.Item, 1);
        var itemId = e.Item.Id;
        var itemToReceive = AllItems.FirstOrDefault(x => x.ApId == itemId);
        if (itemToReceive != null)
        {
            Log.Logger.Information($"Received {itemToReceive.Name} ({itemToReceive.ApId})");
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
                new TextSpan(){Text = $"[{item.Id.ToString()}] -", TextColor = new SolidColorBrush(Color.FromRgb(255, 255, 255))},
                new TextSpan(){Text = $"{item.Name}", TextColor = new SolidColorBrush(Color.FromRgb(200, 255, 200))},
                new TextSpan(){Text = $"x{quantity.ToString()}", TextColor =new SolidColorBrush(Color.FromRgb(200, 255, 200))}
            });
        lock (_lockObject)
        {
            RxApp.MainThreadScheduler.Schedule(() =>
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
            spans.Add(new TextSpan() { Text = part.Text, TextColor = new SolidColorBrush(Color.FromRgb(part.Color.R, part.Color.G, part.Color.B) )});
        }
        lock (_lockObject)
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                Context.HintList.Add(new LogListItem(spans));
            });
        }
    }
    private static void OnConnected(object sender, ConnectionChangedEventArgs args)
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
            foreach (var item in nonItemLotFlags)
            {
                Log.Logger.Verbose($"nonitemlotflags flag {item.Flag} id {item.Id} name {item.Name}");
            }
            //ConditionRewardMap = Helpers.BuildIdFlagLotMap(nonItemLotFlags);
            ConditionRewardMap = Helpers.BuildIdToLotMap(nonItemLotFlags);

            foreach (var pair in ConditionRewardMap) Log.Logger.Verbose($"ConditionRewardMap item {pair.Key} has {pair.Value.Items.Count} items, first is itemid {pair.Value.Items[0].LotItemId}");
            Log.Logger.Debug($"ConditionRewardMap has {ConditionRewardMap.Count} members");

        }
        /* Set to only receive remote items and starting inventory */
        ReplaceItems();

    }

    private static void OnDisconnected(object sender, EventArgs args)
    {
        Log.Logger.Information("Disconnected from Archipelago");
    }
}

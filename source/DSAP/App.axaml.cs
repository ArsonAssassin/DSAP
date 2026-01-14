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
using System.Threading.Tasks;
using System.Threading;

namespace DSAP;

public partial class App : Application
{
    public static MainWindowViewModel Context;
    private DeathLinkService _deathlinkService;
    DateTime lastDeathLinkTime = DateTime.MinValue;
    private const bool DEBUG_TXTLOG = false;
    private const bool DO_NOT_CONNECT = false;
    public static ArchipelagoClient Client { get; set; }
    public static List<DarkSoulsItem> AllItems { get; set; }
    private static Dictionary<int, ItemLot> ItemLotReplacementMap = new Dictionary<int, ItemLot>();
    private static Dictionary<int, ItemLot> SpecialItemLotsMap = new Dictionary<int, ItemLot>();
    
    private static Dictionary<int, ItemLot> ConditionRewardMap = new Dictionary<int, ItemLot>();
    private static Dictionary<string, Tuple<int, string>> SlotLocToItemUpgMap = [];
    private static readonly object _lockObject = new object();
    private static readonly object _deathlinkLock = new object(); // lock that protects IsHandlingDeathLink and lastDeathLinkTime
    private bool IsHandlingDeathlink = false;
    private bool deathlink_enabled = false;
    TimeSpan graceperiod = new TimeSpan(0, 0, 25);
    public static DarkSoulsOptions DSOptions;
    private static bool _goalSent = false;
    private readonly SemaphoreSlim _goalSemaphore = new SemaphoreSlim(1, 1);
    static List<EmkController> EmkControllers = [];
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
        Context = new MainWindowViewModel("0.6.2 - 0.6.5");
        
        Context.ClientVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
        Context.ConnectClicked += Context_ConnectClicked;
        Context.UnstuckClicked += Context_UnstuckClicked;
        Context.CommandReceived += Context_CommandReceived;
        Context.OverlayEnabled = true;
        Context.AutoscrollEnabled = true;

        Context.ConnectButtonEnabled = true;

    }
    public void Context_CommandReceived(object? sender, ArchipelagoCommandEventArgs a)
    {
        if (string.IsNullOrWhiteSpace(a.Command)) return;

        string command = a.Command.Trim().ToLower();
        Log.Logger.Debug($"command received: {command}");
        if (command.StartsWith("/help"))
        {
            Log.Logger.Warning("--- DSAP commands: --- ");
            Log.Logger.Warning(" /help - Display this menu.");
            Log.Logger.Warning(" /diag - Print out some diagnostic information.");
            Log.Logger.Warning(" /deathlink [on/off/toggle] - change your deathlink status (does not persist beyond current session).");
            Log.Logger.Warning(" /goalcheck - Manually check if the goal has been completed, if for some reason it did not send [continued below].");
            Log.Logger.Warning("              Please report back with the screenshots + the resulting messages if you have to use this.");
            Log.Logger.Warning(" /lock [Locked/Unlocked/All] - Display list of all locked or unlocked lockable events, or status of all of them (default).");
            Log.Logger.Warning(" /fog [Locked/Unlocked/All] - Display list of locked or unlocked fog walls, or status of all of them (default).");
            Log.Logger.Warning(" /bossfog [Locked/Unlocked/All] - Display list of locked or unlocked boss fog walls, or status of all of them (default).");
            Log.Logger.Warning("--- End of DSAP commands. ---");
            Client?.SendMessage(a.Command); /* send original command through client for the rest of /help - maybe player will have something if they are an admin. */
        }
        else if (command.StartsWith("/deathlink"))
        {
            string[] cmdparts = command.Split(" ");
            if (cmdparts.Length == 1)
            {
                ToggleDeathlink();
            }
            else if (cmdparts.Length == 2)
            {
                if (cmdparts[1] == "on")
                    SetDeathlink(true);
                else if (cmdparts[1] == "off")
                    SetDeathlink(false);
                else if (cmdparts[1] == "toggle")
                    ToggleDeathlink();
                else
                    Log.Logger.Warning($"Invalid command: \"{a.Command}\". Second argument must be one of [on, off, toggle].");
            }
            else
            {
                Log.Logger.Warning($"Invalid command: {a.Command} - too many arguments received. Format: /deathlink [on/off/toggle]");
            }
            /* Don't send /deathlink to normal processing */
        }
        else if (command.StartsWith("/fog"))
        {
            ProcessListLocksCommand("/fog", command, "Fog Walls", x => x == DsrEventType.FOGWALL || x == DsrEventType.EARLYFOGWALL);
            /* Don't send to normal processing */
        }
        else if (command.StartsWith("/bossfog"))
        {
            ProcessListLocksCommand("/bossfog", command, "Boss Fog Walls", x => x == DsrEventType.BOSSFOGWALL);
            /* Don't send to normal processing */
        }
        else if (command.StartsWith("/lock"))
        {
            ProcessListLocksCommand("/lock", command, "Lockable Events", x => true);
            /* Don't send to normal processing */
        }
        else if (command.StartsWith("/goalcheck")) // check for goal conditions and print a bunch of diagnostics and values.
        {
            GoalCheck();
        }
        else if (command.StartsWith("/diag")) // print diagnostic info
        {
            PrintDiagnosticInfo();
        }
        else if (command.StartsWith("/cef")) // check event flag
        {
            string[] cmdparts = command.Split(" ");
            if (cmdparts.Length == 2)
            {
                int result = CheckEventFlag(Int32.Parse(cmdparts[1]));
                Log.Logger.Information($"{cmdparts[1]}={result}");
            }
        }
        else if (command.StartsWith("/mef")) // monitor event flag
        {
            string[] cmdparts = command.Split(" ");
            if (cmdparts.Length == 2)
                MonitorEventFlag(Int32.Parse(cmdparts[1]));
        }
        else /* send any not-specifically-handled message to normal processing */
        {
            Client?.SendMessage(a.Command);
        }

    }
    // Process the command which will list all of a specific type of lock that is active.
    // Based on the list of "relevant" EmkControllers (built on connect based on which of our DSR items are in the pool)
    private void ProcessListLocksCommand(string shortCmd, string fullCmd, string displayableEventType, Func<DsrEventType, bool> condition)
    {
        string[] cmdparts = fullCmd.Split(" ");
        if (cmdparts.Length == 1)
        {
            ListEventLocks(displayableEventType, 'A', condition);
        }
        else if (cmdparts.Length == 2)
        {
            if (cmdparts[1].StartsWith("l"))
                ListEventLocks(displayableEventType, 'L', condition); 
            else if (cmdparts[1].StartsWith("u"))
                ListEventLocks(displayableEventType, 'U', condition); 
            else if (cmdparts[1].StartsWith("a"))
                ListEventLocks(displayableEventType, 'A', condition);
            else
                Log.Logger.Warning($"Invalid command: \"{fullCmd}\". Second argument must be one of [Locked, Unlocked, All], or [L, U, A].");
        }
        else
        {
            Log.Logger.Warning($"Invalid command: {fullCmd} - too many arguments received. Format: {shortCmd} [Locked, Unlocked, All]");
        }
    }

    private int CheckEventFlag(int flagnum)
    {
        var baseAddress = Helpers.GetEventFlagsOffset();
        Location newloc = new Location()
        {
            Address = baseAddress + Helpers.GetEventFlagOffset(flagnum).Item1,
            AddressBit = Helpers.GetEventFlagOffset(flagnum).Item2
        };
        int result = newloc.Check() ? 1 : 0;
        return result;
    }
    private void MonitorEventFlag(int flagnum)
    {
        int result = CheckEventFlag(flagnum);
        Log.Logger.Information($"{flagnum}={result}");
        Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    int result2 = CheckEventFlag(flagnum);
                    if (result != result2)
                    {
                        result = result2;
                        Log.Logger.Information($"{flagnum}={result}");
                    }
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Exception in event watcher: {ex.Message}\n{ex.InnerException}\n{ex.Source}");
            }
        });
    }

    private void GoalCheck()
    {
        Log.Logger.Warning("Beginning /goalcheck processing");
        bool sendingGoal = false;

        // Begin by stating what the goal is.
        // Later, if there are other options, display the appropriate goal, and update the checks performed.
        Log.Logger.Warning("Your goal is to defeat Gwyn, Lord of Cinder.");

        PrintDiagnosticInfo();

        // check if goal is completed
        if (Helpers.IsInGame())
        {
            ulong baseb = Helpers.GetBaseBAddress();
            Log.Logger.Warning($"$Baseb={baseb.ToString("X")}");
            var locs = Client.CurrentSession.Locations.AllLocationsChecked.Where(x => x == 11110499 || x == 11110500);
            foreach (var loc in locs)
            {
                Log.Logger.Warning($"Lord of Cinder location ({loc}) found as completed. Completing goal.");
                sendingGoal = true;
            }
            if (baseb > 0)
            {
                
                int ngplus = Memory.ReadByte(baseb + 0x78);
                if (ngplus > 0)
                {
                    Log.Logger.Warning($"ng+{ngplus} detected. Completing goal.");
                    sendingGoal = true;
                }
            }
            else
            {
                Log.Logger.Warning("baseb could not be resolved");
            }
            var gwynloc = (Location)Helpers.GetBossFlagLocations().Where(x => x.Name == "Gwyn, Lord of Cinder").First();
            if (gwynloc != null)
            {
                Log.Logger.Warning($"{gwynloc.Name} at {gwynloc.Address.ToString("X")}_{gwynloc.AddressBit.ToString("X")} type {gwynloc.CheckType}.");

                bool result = gwynloc.Check();
                if (result)
                {
                    Log.Logger.Warning("Gwyn bit on. Completing Goal.");
                    sendingGoal = true;
                }
                bool gwynval = Memory.ReadBit(gwynloc.Address, gwynloc.AddressBit);
                if (gwynval)
                {
                    Log.Logger.Warning("Gwyn bit read on. Completing Goal.");
                    sendingGoal = true;
                }
                byte gwynbyte = Memory.ReadByte(gwynloc.Address);
                Log.Logger.Warning($"Gwyn byte={gwynbyte.ToString("X")}");
            }
            else
            {
                Log.Logger.Warning("No Gwyn location found");
            }
            
            if (Helpers.IsInGame())
            {
                if (sendingGoal)
                {
                    SendGoal();
                }
                else
                {
                    Log.Logger.Warning("Goal condition not detected. If this is unexpected, please report this to the developers.");
                }

                Log.Logger.Error("Please help us! Included in these messages are useful diagnostics.");
                Log.Logger.Error("Please post a screenshot of this log in the AP discord's 'dark-souls' channel.");
                Client.AddOverlayMessage($"Action required - see log for details.");
            }
            else
            {
                Log.Logger.Warning("Player not in game. Could not send goal.");
            }
        }
        else
        {
            Log.Logger.Warning("Player not in game. Could not check for goal conditions.");
        }
        Log.Logger.Warning("Ending /goalcheck processing");
    }

    private void PrintDiagnosticInfo()
    {
        Log.Logger.Warning("Diagnostic info:");
        Log.Logger.Warning($"isc={Client?.IsConnected}, ili={Client?.IsLoggedIn}, irtri={Client?.isReadyToReceiveItems}, ircs={Client?.itemsReceivedCurrentSession},");
        Log.Logger.Warning($"v={Client?.CurrentSession.RoomState.Version},gv={Client?.CurrentSession.RoomState.GeneratorVersion}," +
            $"rist={Client?.CurrentSession.RoomState.RoomInfoSendTime.ToShortTimeString()},ctime={DateTime.Now.ToUniversalTime().ToShortTimeString()},Slot={Client?.CurrentSession.ConnectionInfo.Slot}");
        Log.Logger.Warning($"locs={Client?.CurrentSession.Locations.AllLocationsChecked.Count}/{Client?.CurrentSession.Locations.AllLocations.Count}");
        Log.Logger.Warning($"items received={Client?.CurrentSession.Items.AllItemsReceived.Count},ilrm={ItemLotReplacementMap?.Count},crm={ConditionRewardMap?.Count}");
        Log.Logger.Warning($"version info={DSOptions?.VersionInfoString()}, cdv={Archipelago.Core.AvaloniaGUI.Utils.Helpers.GetAppVersion()}");
        if (Client != null && Helpers.IsInGame())
        {
            ulong baseb = Helpers.GetBaseBAddress();
            Log.Logger.Warning($"$Baseb={baseb.ToString("X")}");
            if (baseb > 0)
            {
                int ngplus = Memory.ReadByte(baseb + 0x78);
                Log.Logger.Warning($"ng+={ngplus}");
            }
            else
            {
                Log.Logger.Warning("diag baseb could not be resolved");
            }
        }
    }

    /* Add an abstract "item" which can be a trap, event, or normal item */
    public static void AddAbstractItem(int category, int id)
    {
        if (category == (int)DSItemCategory.Trap)
        {
            RunLagTrap();
        }
        else if (category == (int)DSItemCategory.DsrEvent)
        {
            ReceiveEventItem(id);
        }
        else
        {
            AddItem(category, id, 1);
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

    public static void HomewardBoneCommand()
    {
        var command = Helpers.HomewardBone();

        Array.Copy(BitConverter.GetBytes(Helpers.GetBaseBAddress()), 0, command, 0x3, 4);

        var result = Memory.ExecuteCommand(command);

        Log.Logger.Information($"Forced Load Screen - Items Reset.");
        App.Client.AddOverlayMessage($"Forced Load Screen - Items Reset.");
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
                deathlink_enabled = false;
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


        if (!DO_NOT_CONNECT)
        {

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

            if (Context.OverlayEnabled)
            {
                Client.IntializeOverlayService(new WindowsOverlayService(new OverlayOptions()
                {
                    YOffset = 250 // later, set this dynamically based on "UI scale" DSR option
                }));
            }
            else // otherwise set a task to poll it until it's enabled.
            {
                Task.Run(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(2000);
                        if (Context.OverlayEnabled)
                        {
                            Client.IntializeOverlayService(new WindowsOverlayService(new OverlayOptions()
                            {
                                YOffset = 250 // later, set this dynamically based on "UI scale" DSR option
                            }));
                            Log.Logger.Information("Overlay Enabled.");
                            Client.AddOverlayMessage("Overlay Enabled.");
                            break;
                        }
                    }
                });
            }

            await Client.Login(e.Slot, !string.IsNullOrWhiteSpace(e.Password) ? e.Password : null, ItemsHandlingFlags.IncludeStartingInventory);

            if (!Client.IsLoggedIn)
            {
                Log.Logger.Warning("Login failed");
                Client.AddOverlayMessage("Login failed");
                Context.ConnectButtonEnabled = true;
                return;

            }
            if (Client.Options.ContainsKey("enable_deathlink") && ((JsonElement)Client.Options["enable_deathlink"]).GetUInt32() != 0)
            {
                SetDeathlink(true);
            }

            /* Look for event unlocks in full list of received items and locations */
            DetectEventKeys();
            
            var bossLocations = Helpers.GetBossFlagLocations();
            var itemLocations = Helpers.GetItemLotLocations();
            var bonfireLocations = Helpers.GetBonfireFlagLocations();
            var doorLocations = Helpers.GetDoorFlagLocations();
            var fogWallLocations = Helpers.GetFogWallFlagLocations();
            var miscLocations = Helpers.GetMiscFlagLocations();

            var fullLocationsList = bossLocations.Union(itemLocations).Union(bonfireLocations).Union(doorLocations).Union(fogWallLocations).Union(miscLocations).ToList();
            Client.MonitorLocations(fullLocationsList);

            StartEmkWatchers(EmkControllers);
        }
        else
        {
            StartEventWatcher();
        }
        //Helpers.MonitorLastBonfire((lastBonfire) =>
        //{
        //    Log.Logger.Debug($"Rested at bonfire: {lastBonfire.id}:{lastBonfire.name}");
        //});

        if (DEBUG_TXTLOG)
        { 
            Log.CloseAndFlush();
        }

        //Client.GPSHandler = new Archipelago.Core.Util.GPS.GPSHandler(Helpers.GetPosition, 5000);
        //Client.GPSHandler.Start();

        Context.ConnectButtonEnabled = true;
        Context.UnstuckButtonEnabled = true;


    }
    private void Context_UnstuckClicked(object? sender, EventArgs e)
    {
        Context.UnstuckButtonEnabled = false;
        if (Helpers.IsInGame())
        {
            /* Get the flag for if firelink shrine is lit */
            var isFSLit = Helpers.ReadBonfireFlag("Firelink Shrine");
            if (isFSLit && Helpers.SetLastBonfireToFS())
            {
                /* Set last rested bonfire to FS */
                HomewardBoneCommand();
                Log.Logger.Information("Unstuck - player sent to Firelink Shrine.");
                Client.AddOverlayMessage("Unstuck - player sent to Firelink Shrine.");
            }
            else /* FS not lit or it failed to set */
            {
                HomewardBoneCommand();
                Log.Logger.Warning("Unstuck - Firelink Shrine not yet lit. Sending player to last bonfire.");
                Client.AddOverlayMessage("Unstuck - Firelink Shrine not yet lit. Sending player to last bonfire.");
            }
        }
        else
        {
            Log.Logger.Warning("Unstuck - Player is not in game. Unstuck will do nothing.");
            Client.AddOverlayMessage("Unstuck - Player is not in game. Unstuck will do nothing.");
        }
        Context.UnstuckButtonEnabled = true;
    }
    private void SendDeathlink(DeathLinkService _deathlinkService)
    {
        Log.Logger.Debug($"Attempting deathlink");

        if (!deathlink_enabled)
        {
            Log.Logger.Warning("Deathlink not enabled. Canceling sending deathlink. This may occur if deathlink was turned off after being on.");
            return;
        }
        var deathtime = System.DateTime.Now; /* get the "time of deathlink" before we wait for lock */
        lock (_deathlinkLock)
        {
            // If we are neither already handling one nor in grace period, send it to everybody else.
            if (!IsHandlingDeathlink
                && lastDeathLinkTime + graceperiod < deathtime)
            {
                Log.Logger.Information("Sending Deathlink. RIP.");
                Client.AddOverlayMessage("Sending Deathlink. RIP.");
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
                Client.AddOverlayMessage($"Not sending Deathlink - less than {graceperiod.TotalSeconds} seconds have passed since last Deathlink.");
            }
        }

        //Restart deathlink when player is alive again
        Memory.MonitorAddressForAction<int>(Helpers.GetPlayerHPAddress(),
            () => {                
                /* re-enable monitoring condition */
                Log.Logger.Debug($"Re-enabling deathlink");
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
        Client.AddOverlayMessage($"Deathlink received: {deathLink.Cause ?? deathLink.Source + " died."} RIP.");

        if (!deathlink_enabled)
        {
            Log.Logger.Warning("Deathlink not enabled. Canceling receiving death. This may occur if deathlink was turned off after being on.");
            return;
        }
        // Don't process deaths from ourself, if they come to us
        if (deathLink.Source == Client.CurrentSession.Players.ActivePlayer.Name) return;

        DateTime deathtime = System.DateTime.Now; /* get the "time of deathlink" before we wait for lock */
        lock (_deathlinkLock)
        {
            bool playerInGame = Helpers.IsInGame();

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
                        Log.Logger.Error($"Deathlink ignored - could not resolve hp location (bad hp value) {whp} @ {whpp}/0x{whpp.ToString("X")}.");
                        Client.AddOverlayMessage($"Deathlink ignored - could not resolve hp location (bad hp value).");
                    }
                }
                else
                {
                    Log.Logger.Error($"Deathlink ignored - could not resolve hp location.");
                    Client.AddOverlayMessage($"Deathlink ignored - could not resolve hp location.");
                }

            }
            else /* log why we aren't doing deathlink */
            {
                if (!playerInGame)
                {
                    Log.Logger.Information($"Deathlink ignored - player not in game");
                    Client.AddOverlayMessage($"Deathlink ignored - player not in game");
                }
                else if (IsHandlingDeathlink)
                {
                    Log.Logger.Information($"Deathlink ignored - already handling deathlink");
                    Client.AddOverlayMessage($"Deathlink ignored - already handling deathlink");
                }
                else if (lastDeathLinkTime + graceperiod >= deathtime)
                {
                    Log.Logger.Information($"Deathlink ignored - less than {graceperiod.TotalSeconds} seconds have passed since previous Deathlink");
                    Client.AddOverlayMessage($"Deathlink ignored - less than {graceperiod.TotalSeconds} seconds have passed since previous Deathlink");
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
        Client.AddRichOverlayMessage(e.Message);
    }
    private void Client_LocationCompleted(object? sender, Archipelago.Core.Models.LocationCompletedEventArgs e)
    {
        var locid = e.CompletedLocation.Id;
        if (e.CompletedLocation.Name.Contains("Lord of Cinder"))
        {
            Log.Logger.Information($"Sending Goal for location: {e.CompletedLocation.Name}");
            SendGoal();
        }
        else if (locid == 11110499) // hardcoded "Gwyn, Lord of Cinder" location
        {
            Log.Logger.Information($"Sending Goal for location id: {locid}");
            SendGoal();
        }

        /* If the check was in non-itemlot locations, give the player items for it */
        if (ConditionRewardMap.ContainsKey(locid))
        {
            Log.Logger.Debug($"Found location in 'other' checks");
            var itemLot = ConditionRewardMap[locid];
            foreach (var item in itemLot.Items)
            {
                AddAbstractItem(item.LotItemCategory, item.LotItemId);
                Log.Logger.Debug($"Gave player {item.LotItemId}");
            }
        }
        /* If the check was in special-itemlot locations, give the player items for it */
        if (SpecialItemLotsMap.ContainsKey(locid))
        {
            Log.Logger.Debug($"Found location in 'special' checks");
            var itemLot = SpecialItemLotsMap[locid];
            foreach (var item in itemLot.Items)
            {
                AddAbstractItem(item.LotItemCategory, item.LotItemId);
                Log.Logger.Debug($"Gave player {item.LotItemId}");
            }
        }
        Log.Logger.Debug($"Location Completed: {e.CompletedLocation.Name} at {e.CompletedLocation.Id}");
    }

    private void SendGoal()
    {
        Task.Run(async () =>
        {
            await _goalSemaphore.WaitAsync(); 
            try
            {
                if (!_goalSent)
                {
                    Client.SendGoalCompletion();
                    Log.Logger.Warning("Goal sent.");
                    Client.AddOverlayMessage($"Goal sent.");
                }
                else
                    Log.Logger.Information("Goal already sent.");
                _goalSent = true;
            }
            finally
            {
                _goalSemaphore.Release();
            }
        });
    }

    private void ToggleDeathlink()
    {
        if (deathlink_enabled)
            SetDeathlink(false);
        else
            SetDeathlink(true);
    }
    private void SetDeathlink(bool enable)
    {
        if (enable && !deathlink_enabled)
        {
            if (_deathlinkService == null)
            {
                _deathlinkService = Client.EnableDeathLink();
            }
            else
            {
                _deathlinkService.EnableDeathLink();
            }
            _deathlinkService.OnDeathLinkReceived += _deathlinkService_OnDeathLinkReceived;
            Log.Logger.Information($"Initializing deathlink.");
            deathlink_enabled = true;
            Memory.MonitorAddressForAction<int>(Helpers.GetPlayerHPAddress(), () => SendDeathlink(_deathlinkService),
                (health) => _playerIsDead());
        }
        else if (!enable && deathlink_enabled)
        {
            Log.Logger.Information($"Disabling deathlink.");
            deathlink_enabled = false;
            if (_deathlinkService != null)
            {
                _deathlinkService.DisableDeathLink();
                _deathlinkService.OnDeathLinkReceived -= _deathlinkService_OnDeathLinkReceived;
            }
        }
        else
        {
            if (deathlink_enabled)
                Log.Logger.Warning("Deathlink is already enabled.");
            else
                Log.Logger.Warning("Deathlink is already disabled.");
        }
    }
    private void ListEventLocks(string displayableEventType, char filter, Func<DsrEventType, bool> condition)
    {
        string status;
        if (filter == 'L') status = "Locked";
        else if (filter == 'U') status = "Unlocked";
        else status = "All";

        Log.Logger.Information($"-- List of {status} {displayableEventType} -- ");
        var emklist = EmkControllers.Where(x=>condition(x.Type)).OrderBy(x => x.HasKey).ThenBy(x => x.Name).ToList();
        foreach (var emk in emklist)
        {
            if (filter == 'A'
                || filter == 'U' && emk.HasKey == true
                || filter == 'L' && emk.HasKey == false)
            {
                if (emk.HasKey) status = "Unlocked";
                else status = "Locked";
                Log.Logger.Information($"{status} -> {emk.Name}");
            }
        }
        Log.Logger.Information($"-- End of List of {status} {displayableEventType} -- ");
        if (emklist.Count == 0)
        {
            Log.Logger.Information($"You do not have {displayableEventType} locking enabled");
        }
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
            Client.AddOverlayMessage($"You are now safe to load your save.");
        }
    }
    private static void Client_ItemReceived(object? sender, ItemReceivedEventArgs e)
    {
        LogItem(e.Item, 1);
        bool success = false;
        if (Helpers.IsInGame())
        {
            var itemId = e.Item.Id;
            var itemToReceive = AllItems.FirstOrDefault(x => x.ApId == itemId);
            if (itemToReceive != null)
            {
                Log.Logger.Information($"Received {itemToReceive.Name} ({itemToReceive.ApId})");
                Client.AddOverlayMessage($"Received {itemToReceive.Name} ({itemToReceive.ApId})");

                Log.Logger.Verbose($"Attempting to upgrade item: '{itemToReceive.ApId}' {itemToReceive.Name} from loc {e.LocationId}.");
                if (DSOptions.UpgradedWeaponsPercentage > 0
                    && SlotLocToItemUpgMap.TryGetValue($"{e.Player.Slot}:{e.LocationId}", out var itemupg))
                {
                    if (itemupg.Item1 == itemToReceive.ApId) // if item apid matches
                        itemToReceive = Helpers.UpgradeItem(itemToReceive, itemupg.Item2, true);
                    else
                    {
                        Log.Logger.Error($"Item upgrade error: '{itemupg.Item1}' != '{itemToReceive.ApId}', for item {itemToReceive.Name}.");
                        Client.AddOverlayMessage($"Item upgrade error: '{itemupg.Item1}' != '{itemToReceive.ApId}', for item {itemToReceive.Name}.");
                    }


                }
                AddAbstractItem((int)itemToReceive.Category, itemToReceive.Id);

                /* If after receiving item (or trap), player is still in game, then it received successfully */
                if (Helpers.IsInGame())
                {
                    success = true;
                }
            }
            else
            {
                Log.Logger.Warning($"Unable to identify received item {itemId}, receiving rubbish instead.");
                Client.AddOverlayMessage($"Unable to identify received item {itemId}, receiving rubbish instead.");
                var filler = AllItems.First(x => x.Id == 380);
                AddAbstractItem((int)filler.Category, filler.Id);
            }
        }
        e.Success = success;
        /* If receive didn't work, schedule re-receive for when we are back in game */
        if (!success)
        {
            Log.Logger.Warning($"Failed to receive item - Player not loaded into game. Will retry when player is once again in game.");
            Client.AddOverlayMessage($"Failed to receive item - Player not loaded into game. Will retry when player is once again in game.");
            
            Task.Run(async () =>
            {
                /* Check every second if player is in game again yet */
                while(!Helpers.IsInGame())
                {
                    await Task.Delay(1000);
                }

                Log.Logger.Warning($"Player once again detected as in game. Re-trying item receive.");
                Client.AddOverlayMessage($"Player once again detected as in game. Re-trying item receive.");
                /* Once finally in game, re-enable receives. */
                Client.ReceiveReady();
            });
        }
    }
    private static void DetectEventKeys()
    {
        Log.Logger.Debug("detecting event keys in all items");
        if (Client.CurrentSession.Items.AllItemsReceived.Count > 0)
        {
            var itemlistcopy = Client.CurrentSession.Items.AllItemsReceived.ToList();
            foreach (var item in itemlistcopy)
            {
                var emk = EmkControllers.Find(x => x.ApId == item.ItemId);
                if (emk != null)
                {
                    emk.Unlock();
                }
            }
        }
        if (Client.CurrentSession.Locations.AllLocationsChecked.Count > 0)
        {
            Log.Logger.Debug("detecting event keys in all locs");
            var locationlistcopy = Client.CurrentSession.Locations.AllLocationsChecked;
            foreach (var location in locationlistcopy)
            {
                /* search condition rewards map (doors, etc) */
                if (ConditionRewardMap.ContainsKey(((int)location)))
                {
                    foreach (var item in ConditionRewardMap[((int)location)].Items)
                    {
                        if (item.LotItemCategory == (int)DSItemCategory.DsrEvent)
                        {
                            var emk = EmkControllers.Find(x => x.ApId == item.LotItemId);
                            if (emk != null)
                            {
                                emk.Unlock();
                            }
                        }
                    }
                }
                
                /* then search special item lot map (floor items) */
                if (SpecialItemLotsMap.ContainsKey(((int)location)))
                {
                    Log.Logger.Debug($"found loc {((int)location)} in special lot list");
                    foreach (var item in SpecialItemLotsMap[((int)location)].Items)
                    {
                        if (item.LotItemCategory == (int)DSItemCategory.DsrEvent)
                        {
                            var emk = EmkControllers.Find(x => x.ApId == item.LotItemId);
                            if (emk != null)
                            {
                                emk.Unlock();
                            }
                        }
                    }
                }
            }
        }
    }

    private void StartEventWatcher()
    {
        // every second, check the events list.
        Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    Helpers.CheckEventsList();
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Exception in event watcher: {ex.Message}\n{ex.InnerException}\n{ex.Source}");
            }
        });
    }

    internal static void StartEmkWatchers(List<EmkController> emkControllers)
    {
        // every second, check the events list.
        Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    if (!Client.IsConnected)
                    {
                        Log.Logger.Error("Client disconnection detected - stopping event listener");
                        return;
                    }
                    Helpers.CheckEventsList();
                    Helpers.ManageEventsList(emkControllers);
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Exception in events list: {ex.Message}\n{ex.InnerException}\n{ex.Source}");
            }
        });
    }
    private static async void RunLagTrap()
    {
        using (var lagTrap = new LagTrap(TimeSpan.FromSeconds(20)))
        {
            lagTrap.Start();
            await lagTrap.WaitForCompletionAsync();
        }
    }
    private static void ReceiveEventItem(int ApId)
    {
        /* Find the event in the EmkController list,
         * and mark it as unlocked. */

        EmkController? emk = EmkControllers.Find(x => x.ApId == ApId);
        if (emk != null)
        {
            emk.Unlock();
        }
        else
        {
            Log.Logger.Error($"Error, received item {ApId}, but no emk controller found. Check that your AP world and client match.");
        }
        // On next event scan, it'll re-add it if needed
    }

    private static void LogItem(Item item, int quantity)
    {
        lock (_lockObject)
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                var messageToLog = new LogListItem(new List<TextSpan>()
                {
                    new TextSpan(){Text = $"[{item.Id.ToString()}] -", TextColor = new SolidColorBrush(Color.FromRgb(255, 255, 255))},
                    new TextSpan(){Text = $"{item.Name}", TextColor = new SolidColorBrush(Color.FromRgb(200, 255, 200))},
                    new TextSpan(){Text = $"x{quantity.ToString()}", TextColor =new SolidColorBrush(Color.FromRgb(200, 255, 200))}
                });
                Context.ItemList.Add(messageToLog);
            });
        }
    }
    private static void LogHint(LogMessage message)
    {
        var newMessage = message.Parts.Select(x => x.Text);
        lock (_lockObject)
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                if (Context.HintList.Any(x => x.TextSpans.Select(y => y.Text) == newMessage))
                {
                    return; //Hint already in list
                }
                List<TextSpan> spans = new List<TextSpan>();
                foreach (var part in message.Parts)
                {
                    spans.Add(new TextSpan() { Text = part.Text, TextColor = new SolidColorBrush(Color.FromRgb(part.Color.R, part.Color.G, part.Color.B)) });
                }

                Context.HintList.Add(new LogListItem(spans));
            });
        }
    }
    private static void OnConnected(object sender, ConnectionChangedEventArgs args)
    {
        Log.Logger.Information("Connected to Archipelago");
        Client.AddOverlayMessage("Connected to Archipelago");
        Log.Logger.Information($"Playing {Client.CurrentSession.ConnectionInfo.Game} as {Client.CurrentSession.Players.GetPlayerName(Client.CurrentSession.ConnectionInfo.Slot)}");
        Client.AddOverlayMessage($"Playing {Client.CurrentSession.ConnectionInfo.Game} as {Client.CurrentSession.Players.GetPlayerName(Client.CurrentSession.ConnectionInfo.Slot)}");
        /* Make ready to receive items */

        /* If we haven't yet initialized the dictionary, do so. */
        if (ItemLotReplacementMap.Count == 0)
        {
            /* get slot info */
            int currentSlot = App.Client.CurrentSession.ConnectionInfo.Slot;
            var slotDataTask = App.Client.CurrentSession.DataStorage.GetSlotDataAsync(currentSlot);
            slotDataTask.Wait();
            Dictionary<string, object> slotData = slotDataTask.Result;

            DSOptions = new DarkSoulsOptions(App.Client.Options, slotData);
            if (DSOptions.outofdate)
            {
                Client.AddOverlayMessage("Client or apworld out of data - instability and errors likely.");
                Client.AddOverlayMessage("See client log for details.");
            }
            Log.Logger.Debug($"{DSOptions.ToString()}");

            EmkControllers = Helpers.BuildEmkControllers(slotData);

            SlotLocToItemUpgMap = Helpers.BuildSlotLocationToItemUpgMap(slotData, currentSlot);

            var itemflags = Helpers.GetItemLotFlags().Where((x) => x.IsEnabled).Cast<EventFlag>().ToList();
            Helpers.BuildFlagToLotMap(out ItemLotReplacementMap, out SpecialItemLotsMap, itemflags, slotData, SlotLocToItemUpgMap);

            var nonItemLotFlags = Helpers.GetBossFlags().Cast<EventFlag>().ToList();
            nonItemLotFlags.AddRange(Helpers.GetBonfireFlags().Cast<EventFlag>());
            nonItemLotFlags.AddRange(Helpers.GetDoorFlags().Cast<EventFlag>());
            nonItemLotFlags.AddRange(Helpers.GetFogWallFlags().Cast<EventFlag>());
            nonItemLotFlags.AddRange(Helpers.GetMiscFlags().Cast<EventFlag>());

            //var nonItemLotFlags = Helpers.GetDoorFlags().Cast<EventFlag>().ToList();
            Log.Logger.Debug($"Special lot item count: {SpecialItemLotsMap.Count}");
            Log.Logger.Debug($"nonitemlotflags count = {nonItemLotFlags.Count}");
            foreach (var item in nonItemLotFlags)
            {
                Log.Logger.Verbose($"nonitemlotflags flag {item.Flag} id {item.Id} name {item.Name}");
            }
            //ConditionRewardMap = Helpers.BuildIdFlagLotMap(nonItemLotFlags);
            ConditionRewardMap = Helpers.BuildIdToLotMap(nonItemLotFlags, slotData, SlotLocToItemUpgMap);

            foreach (var pair in ConditionRewardMap) Log.Logger.Verbose($"ConditionRewardMap item {pair.Key} has {pair.Value.Items.Count} items, first is itemid {pair.Value.Items[0].LotItemId}");
            Log.Logger.Debug($"ConditionRewardMap has {ConditionRewardMap.Count} members");

        }
        /* Set to only receive remote items and starting inventory */
        ReplaceItems();

    }

    private static void OnDisconnected(object sender, EventArgs args)
    {
        Log.Logger.Information("Disconnected from Archipelago");
        Client.AddOverlayMessage("Disconnected from Archipelago");
    }
}

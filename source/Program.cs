using Archipelago.Core;
using Archipelago.Core.GUI;
using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Newtonsoft.Json;

namespace DSAP
{
    internal static class Program
    {

        public static ArchipelagoClient Client { get; set; }
        public static MainForm MainForm { get; set; }   
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            var options = new GuiDesignOptions
            {
                BackgroundColor = Color.Black,
                ButtonColor = Color.DarkRed,
                Title = "DSAP - Dark Souls Remastered Archipelago"
            };
            MainForm = new MainForm(options);
            MainForm.ConnectClicked += MainForm_ConnectClicked;
            Application.Run(MainForm);
        }



        public static void AddItem(int category, int id, int quantity)
        {
            var command = Helpers.GetItemCommand();
            var result = Memory.ExecuteCommand(command);
            //Set item category
            Array.Copy(BitConverter.GetBytes(category), 0, command, 0x1, 4);
            //Set item quantity
            Array.Copy(BitConverter.GetBytes(quantity), 0, command, 0x7, 4);
            //set item id
            Array.Copy(BitConverter.GetBytes(id), 0, command, 0xD, 4);

            var result2 = Memory.ExecuteCommand(command);
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
                    var isCompleted = await global::Archipelago.Core.Util.Helpers.CheckLocation(location);
                    if (isCompleted)
                    {
                        completed.Add(location);
                        MainForm.WriteLine(JsonConvert.SerializeObject(location));
                    }
                }
                if (completed.Any())
                {
                    foreach (var location in completed)
                    {
                        batch.Remove(location);
                    }
                }
                completed.Clear();
                await Task.Delay(500);
            }
        }

        private static void MainForm_ConnectClicked(object? sender, ConnectClickedEventArgs e)
        {
            if (Client != null)
            {
                Client.Connected -= OnConnected;
                Client.Disconnected -= OnDisconnected;
            }
            DarkSoulsClient client = new DarkSoulsClient();
            var connected = client.Connect();
            if (!connected)
            {
                MainForm.WriteLine("Dark Souls not running, open Dark Souls before connecting!");
                return;
            }
            Client = new ArchipelagoClient(client);

            Client.Connected += OnConnected;
            Client.Disconnected += OnDisconnected;

            // await Client.Connect(hostTextbox.Text, "Dark Souls Remastered");
            // await Client.Login(slotTextbox.Text, !string.IsNullOrWhiteSpace(passwordTextbox.Text) ? passwordTextbox.Text : null);
            Client.ItemReceived += Client_ItemReceived;
            var locations = Helpers.GetBossLocations();
            MonitorLocations(locations);
        }

        private static void Client_ItemReceived(object? sender, ItemReceivedEventArgs e)
        {
            AddItem(0x40000000, 292, 1);
        }

        private static void OnConnected(object sender, EventArgs args)
        {
            MainForm.WriteLine("Connected to Archipelago");
            MainForm.WriteLine($"Playing {Client.CurrentSession.ConnectionInfo.Game} as {Client.CurrentSession.Players.GetPlayerName(Client.CurrentSession.ConnectionInfo.Slot)}");

        }

        private static void OnDisconnected(object sender, EventArgs args)
        {
            MainForm.WriteLine("Disconnected from Archipelago");
        }
    }
}
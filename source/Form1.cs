using System.Text;
using Archipelago.Core;
using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Newtonsoft.Json;
namespace DSAP
{
    public partial class Form1 : Form
    {
        public static ArchipelagoClient Client { get; set; }
        public Form1()
        {
            InitializeComponent();
        }

        private void connectBtn_Click(object sender, EventArgs e)
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
                WriteLine("Dark Souls not running, open Dark Souls before connecting!");
                return;
            }
            Client = new ArchipelagoClient(client);

            Client.Connected += OnConnected;
            Client.Disconnected += OnDisconnected;

            // await Client.Connect(hostTextbox.Text, "Dark Souls Remastered");
            // await Client.Login(slotTextbox.Text, !string.IsNullOrWhiteSpace(passwordTextbox.Text) ? passwordTextbox.Text : null);
            
            var locations = Helpers.GetBossLocations();
            MonitorLocations(locations);

        }

        public void AddItem(int category, int id, int quantity)
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
        public async Task MonitorLocations(List<Location> locations)
        {
            var locationBatches = locations
                .Select((location, index) => new { Location = location, Index = index })
                .GroupBy(x => x.Index / 25)
                .Select(g => g.Select(x => x.Location).ToList())
                .ToList();
            var tasks = locationBatches.Select(x => MonitorBatch(x));
            await Task.WhenAll(tasks);

        }
        private async Task MonitorBatch(List<Location> batch)
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
                        WriteLine(JsonConvert.SerializeObject(location));
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
        private void OnConnected(object sender, EventArgs args)
        {
            WriteLine("Connected to Archipelago");
            WriteLine($"Playing {Client.CurrentSession.ConnectionInfo.Game} as {Client.CurrentSession.Players.GetPlayerName(Client.CurrentSession.ConnectionInfo.Slot)}");
            Invoke(() =>
            {
                connectBtn.Text = "Disconnect";
            });

        }

        private void OnDisconnected(object sender, EventArgs args)
        {
            WriteLine("Disconnected from Archipelago");
            Invoke(() =>
            {
                connectBtn.Text = "Connect";
            });
        }
        public void WriteLine(string output)
        {
            Invoke(() =>
            {
                outputTextbox.Text += output;
                outputTextbox.Text += System.Environment.NewLine;

                System.Diagnostics.Debug.WriteLine(output + System.Environment.NewLine);
            });
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            WriteLine("DSAP - Dark Souls Archipelago Randomiser");
            WriteLine("-- By ArsonAssassin --");
            WriteLine("Initialising collections...");
            WriteLine("Ready to connect!");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Memory.CurrentProcId = Memory.GetProcIdFromExe("DarkSoulsRemastered");
            AddItem(0x40000000, 292, 1);
        }
    }
}

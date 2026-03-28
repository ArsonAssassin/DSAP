using Avalonia.Controls;
using ReactiveUI;
using Serilog;
using System.ComponentModel;
using System.Reactive;


namespace DSAP.ViewModels
{
    public partial class DsrControlsWindowModel : Archipelago.Core.AvaloniaGUI.ViewModels.ViewModelBase, INotifyPropertyChanged
    {
        private string _test = "test22";
        public string Test
        {
            get => _test;
            set => this.RaiseAndSetIfChanged(ref _test, value);
        }
        private bool _foundItemProgressive = true;
        private bool _foundItemUseful = true;
        private bool _foundItemTrap = true;
        private bool _foundItemFiller = true;
        private bool _sentItemProgressive = true;
        private bool _sentItemUseful = true;
        private bool _sentItemTrap = true;
        private bool _sentItemFiller = true;
        private bool _receivedItemProgressive = true;
        private bool _receivedItemUseful = true;
        private bool _receivedItemTrap = true;
        private bool _receivedItemFiller = true;
        private bool _deathlink = false;

        public bool FoundItemProgressive
        {
            get => _foundItemProgressive;
            set => this.RaiseAndSetIfChanged(ref _foundItemProgressive, value);
        }
        public bool FoundItemUseful
        {
            get => _foundItemUseful;
            set => this.RaiseAndSetIfChanged(ref _foundItemUseful, value);
        }
        public bool FoundItemTrap
        {
            get => _foundItemTrap;
            set => this.RaiseAndSetIfChanged(ref _foundItemTrap, value);
        }
        public bool FoundItemFiller
        {
            get => _foundItemFiller;
            set => this.RaiseAndSetIfChanged(ref _foundItemFiller, value);
        }
        public bool SentItemProgressive
        {
            get => _sentItemProgressive;
            set => this.RaiseAndSetIfChanged(ref _sentItemProgressive, value);
        }
        public bool SentItemUseful
        {
            get => _sentItemUseful;
            set => this.RaiseAndSetIfChanged(ref _sentItemUseful, value);
        }
        public bool SentItemTrap
        {
            get => _sentItemTrap;
            set => this.RaiseAndSetIfChanged(ref _sentItemTrap, value);
        }
        public bool SentItemFiller
        {
            get => _sentItemFiller;
            set => this.RaiseAndSetIfChanged(ref _sentItemFiller, value);
        }
        public bool ReceivedItemProgressive
        {
            get => _receivedItemProgressive;
            set => this.RaiseAndSetIfChanged(ref _receivedItemProgressive, value);
        }
        public bool ReceivedItemUseful
        {
            get => _receivedItemUseful;
            set => this.RaiseAndSetIfChanged(ref _receivedItemUseful, value);
        }
        public bool ReceivedItemTrap
        {
            get => _receivedItemTrap;
            set => this.RaiseAndSetIfChanged(ref _receivedItemTrap, value);
        }
        public bool ReceivedItemFiller
        {
            get => _receivedItemFiller;
            set => this.RaiseAndSetIfChanged(ref _receivedItemFiller, value);
        }
        public bool Deathlink
        {
            get => _deathlink;
            set
            {
                this.RaiseAndSetIfChanged(ref _deathlink, value);
                if (App.Client?.IsConnected ?? false) // only set deathlink value if AP connection is active
                {
                    if (((App)App.Current).deathlink_enabled != value) // skip update if it's already set to value
                        ((App)App.Current).SetDeathlink(_deathlink);
                }
            }
        }
        public override void Dispose()
        {
            base.Dispose();
        }

        //public ReactiveCommand<Unit, Unit> ToggleDeathlinkClicked { get; }
        
        public DsrControlsWindowModel()
        {
            //ToggleDeathlinkClicked = ReactiveCommand.Create(ToggleDeathlink);
        }

        //private void ToggleDeathlink()
        //{
        //    Log.Logger.Information("You clicked toggle deathlink");
        //    Deathlink = !Deathlink;
        //}
    }
}

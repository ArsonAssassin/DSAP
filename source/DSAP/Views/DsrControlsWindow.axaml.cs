using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Serilog;

namespace DSAP;

public partial class DsrControlsWindow : Window
{
    public DsrControlsWindow()
    {
        InitializeComponent();
    }
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (IsVisible && !e.IsProgrammatic)
        {
            Hide();
            e.Cancel = true;
        }    
        else
            base.OnClosing(e);
    }
}

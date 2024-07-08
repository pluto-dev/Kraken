using Kraken.Desktop.Views;
using ModernWpf.Controls;

namespace Kraken.Desktop;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        RootFrame.Navigate(typeof(CoolingPage));
    }


    private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        RootFrame.Navigate(typeof(CoolingPage));
    }
}
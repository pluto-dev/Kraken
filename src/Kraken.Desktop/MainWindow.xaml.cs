using System.Windows;
using Kraken.Desktop.Views;
using ModernWpf.Controls;

namespace Kraken.Desktop;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        var navigated = RootFrame.Navigate(typeof(CoolingPage));
        if (navigated)
        {
            var navViewItem = RootNavigationView.MenuItems.OfType<NavigationViewItem>().First();
            RootNavigationView.SelectedItem = navViewItem;
        }
    }

    private void RootNavigationView_OnSelectionChanged(NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer.Tag is not string tag) return;
        RootFrame.Navigate(Type.GetType(tag));
    }
}
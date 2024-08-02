using System;
using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;

namespace Kraken.Desktop.Views;

public sealed partial class ShellPage : Page
{
    public ShellPage()
    {
        InitializeComponent();

        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
    }

    private void NavigationView_OnSelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args
    )
    {
        if (args.IsSettingsSelected) { }

        var selectedItem = args.SelectedItemContainer;
        var tag = selectedItem.Tag as string;
        Debug.Assert(!string.IsNullOrEmpty(tag), "!string.IsNullOrEmpty(tag)");
        var type = Type.GetType(tag);
        Debug.Assert(type is not null, "type is null");
        ContentFrame.Navigate(type);
    }
}

using System;
using Kraken.Desktop.Utils;
using Kraken.Desktop.Views;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace Kraken.Desktop;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        await Util.InitializeAsync();

        if (MainWindow.Content is not null)
        {
            throw new InvalidOperationException($"Content of {nameof(MainWindow)} must be null.");
        }

        MainWindow.Content = new ShellPage();
        MainWindow.Activate();
    }

    public static WindowEx MainWindow { get; } = new MainWindow();
}

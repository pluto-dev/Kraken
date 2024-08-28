using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Kraken.Desktop.Services;
using Kraken.Desktop.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Kraken.Desktop.Controls;

public sealed partial class CustomColorPicker : UserControl
{
    private readonly StorageService _storageService;

    //public event ItemClickEventHandler? Click;
    public event EventHandler<SolidColorBrush>? Click;

    public CustomColorPicker()
    {
        _storageService = StorageService.Instance;
        InitializeComponent();
    }

    public List<SolidColorBrush> FixColors
    {
        get => (List<SolidColorBrush>)GetValue(FixColorsProperty);
        set => SetValue(FixColorsProperty, value);
    }
    public ObservableCollection<SolidColorBrush> RecentColors
    {
        get => (ObservableCollection<SolidColorBrush>)GetValue(RecentColorsProperty);
        set => SetValue(RecentColorsProperty, value);
    }

    public bool IsColorSelectionEnabled
    {
        get => (bool)GetValue(IsColorSelectionEnabledProperty);
        set => SetValue(IsColorSelectionEnabledProperty, value);
    }

    public Visibility Not(bool? b) => b == true ? Visibility.Collapsed : Visibility.Visible;

    private void ColorGrid_OnItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not SolidColorBrush color)
            return;

        UpdateRecentColor(color);

        Click?.Invoke(sender, color);
    }

    private void SelectColorButton_OnClick(object sender, RoutedEventArgs e)
    {
        var color = new SolidColorBrush(ColorPicker.Color);

        UpdateRecentColor(color);

        Click?.Invoke(sender, color);
    }

    private async void UpdateRecentColor(SolidColorBrush brush)
    {
        var color = brush.Color;
        var index = RecentColors.ToList().FindIndex(p => p.Color.Equals(color));

        if (index != -1)
        {
            RecentColors.Move(index, RecentColors.Count - 1);
        }
        else
        {
            RecentColors.RemoveAt(0);
            RecentColors.Add(brush);
        }

        await SaveRecentColors();
    }

    private async Task SaveRecentColors()
    {
        var recentColors = RecentColors
            .Select(p => new int[] { p.Color.A, p.Color.R, p.Color.G, p.Color.B })
            .ToArray();

        await _storageService.SaveRecentColors(recentColors);
    }
}

public partial class CustomColorPicker
{
    public static readonly DependencyProperty FixColorsProperty = DependencyProperty.Register(
        nameof(FixColors),
        typeof(List<SolidColorBrush>),
        typeof(CustomColorPicker),
        new PropertyMetadata(default(List<SolidColorBrush>))
    );
    public static readonly DependencyProperty RecentColorsProperty = DependencyProperty.Register(
        nameof(RecentColors),
        typeof(ObservableCollection<SolidColorBrush>),
        typeof(CustomColorPicker),
        new PropertyMetadata(default(ObservableCollection<SolidColorBrush>))
    );
    public static readonly DependencyProperty IsColorSelectionEnabledProperty =
        DependencyProperty.Register(
            nameof(IsColorSelectionEnabled),
            typeof(bool),
            typeof(CustomColorPicker),
            new PropertyMetadata(true)
        );
}

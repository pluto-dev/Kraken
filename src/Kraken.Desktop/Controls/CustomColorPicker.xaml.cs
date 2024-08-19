using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Kraken.Desktop.Controls;

public sealed partial class CustomColorPicker : UserControl
{
    //public event ItemClickEventHandler? Click;
    public event EventHandler<SolidColorBrush>? Click;

    public CustomColorPicker()
    {
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

        RecentColors.RemoveAt(0);
        RecentColors.Add(color);

        Click?.Invoke(sender, color);
    }

    private void SelectColorButton_OnClick(object sender, RoutedEventArgs e)
    {
        var color = new SolidColorBrush(ColorPicker.Color);

        RecentColors.RemoveAt(0);
        RecentColors.Add(color);

        Click?.Invoke(sender, color);
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
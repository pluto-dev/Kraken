using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ScottPlot;
using ScottPlot.DataGenerators;
using ScottPlot.Palettes;
using ScottPlot.Plottables;

namespace Kraken.Desktop.Views;

public partial class CoolingPage : Page
{
    private readonly Text _markerLabel;
    private readonly Scatter _pumpDutyScatter;
    private readonly VerticalLine _temperatureLine;

    private int? _indexBeingDragged;
    private readonly int[] _xs = [20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100];
    private readonly int[] _ys = [50, 50, 50, 50, 50, 50, 50, 50, 100, 100, 100, 100, 100, 100, 100, 100, 100];

    private readonly RandomWalker _walker = new(1, 30);

    private readonly DispatcherTimer _dispatcherTimer = new()
        { Interval = TimeSpan.FromMilliseconds(100), IsEnabled = true };

    private readonly Color _gray = Color.FromHex("#404040");
    private readonly Color _white = Color.FromHex("#e5e9f0");
    private readonly Color _backgroundDark = Color.FromHex("#1f1f1f");
    private readonly Color _backgroundDark2 = Color.FromHex("#2b2b2b");
    private readonly Color _orange = Color.FromHex("#ebcb8b");
    private readonly Color _green = Color.FromHex("#a3be8c");
    private readonly Color _purple = Color.FromHex("#b48ead");
    private readonly Color _red = Color.FromHex("#bf616a");
    private readonly Color _blue = Color.FromHex("#88c0d0");
    private readonly Color _blueDark = Color.FromHex("#81a1c1");

    public CoolingPage()
    {
        InitializeComponent();
        _temperatureLine = CoolingPlot.Plot.Add.VerticalLine(x: 0);
        _pumpDutyScatter = CoolingPlot.Plot.Add.Scatter(_xs, _ys);
        _markerLabel = CoolingPlot.Plot.Add.Text("", 0, 0);

        ConfigurePlot();
    }

    private void ConfigurePlot()
    {
        CoolingPlot.Interaction.Disable();
        CoolingPlot.Plot.Grid.XAxisStyle.IsVisible = false;
        CoolingPlot.Plot.Grid.MajorLineWidth = 2;
        CoolingPlot.Plot.Axes.Top.MinimumSize = 35;
        CoolingPlot.Plot.Axes.Bottom.MinimumSize = 35;
        // marker label
        _markerLabel.OffsetX = 20;
        _markerLabel.OffsetY = 15;
        _markerLabel.LabelBold = true;
        _markerLabel.LabelFontColor = _blue;
        // scatter
        _pumpDutyScatter.LineWidth = 2;
        _pumpDutyScatter.MarkerSize = 10;
        _pumpDutyScatter.Color = _blue;
        // temperature line

        _temperatureLine.IsVisible = false;
        _temperatureLine.LinePattern = LinePattern.DenselyDashed;
        _temperatureLine.LabelOppositeAxis = true;
        _temperatureLine.LabelFontSize = 12;
        _temperatureLine.LabelBackgroundColor = Colors.Transparent;
        _temperatureLine.LineColor = _green;
        _temperatureLine.LabelFontColor = _green;

        ScottPlot.TickGenerators.NumericManual tickGenX =
            new([0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100],
                ["0˚", "10˚", "20˚", "30˚", "40˚", "50˚", "60˚", "70˚", "80˚", "90˚", "100˚"]);
        ScottPlot.TickGenerators.NumericManual tickGenY =
            new([0, 25, 50, 75, 100], ["0%", "25%", "50%", "75%", "100%"]);

        CoolingPlot.Plot.Axes.Bottom.TickGenerator = tickGenX;
        CoolingPlot.Plot.Axes.Left.TickGenerator = tickGenY;
        CoolingPlot.Plot.Axes.SetLimits(0, 110, 0, 105);

        CoolingPlot.Plot.FigureBackground.Color = _backgroundDark2;
        CoolingPlot.Plot.DataBackground.Color = _backgroundDark;
        CoolingPlot.Plot.Axes.Color(_white);
        CoolingPlot.Plot.Grid.MajorLineColor = _gray;
    }

    private void CoolingPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        CoolingPlot.MouseDown += CoolingPlotOnMouseDown;
        CoolingPlot.MouseMove += CoolingPlotOnMouseMove;
        CoolingPlot.MouseUp += CoolingPlotOnMouseUp;
        _dispatcherTimer.Tick += DispatcherTimerOnTick;
    }

    private async void DispatcherTimerOnTick(object? sender, EventArgs args)
    {
        var tempSource = "Liquid";
        double? status = _walker.Next();
        if (status is null) return;
        _temperatureLine.IsVisible = true;
        
        var color = status switch
        {
            < 35.0 => _green,
            < 50.0 => _orange,
            >= 50.0 => _red,
            _ => throw new ArgumentOutOfRangeException()
        };

        _temperatureLine.LineColor = color;
        _temperatureLine.LabelFontColor = color;
        _temperatureLine.Position = status.Value;
        _temperatureLine.LabelText = $"{tempSource}: {status.Value:N0}˚";
        CoolingPlot.Refresh();
    }

    private void CoolingPlotOnMouseUp(object sender, MouseButtonEventArgs e)
    {
        _indexBeingDragged = null;
        CoolingPlot.Refresh();
    }

    private void CoolingPlotOnMouseMove(object sender, MouseEventArgs e)
    {
        var mousePoint = e.GetPosition(CoolingPlot);
        var mousePixel = new Pixel(mousePoint.X, mousePoint.Y);
        var mouseLocation = CoolingPlot.Plot.GetCoordinates(mousePixel);
        var nearest = _pumpDutyScatter.Data.GetNearest(mouseLocation, CoolingPlot.Plot.LastRender);
        CoolingPlot.Cursor =
            nearest is { IsReal: true, X: < 60 } ? Cursors.Hand : Cursors.Arrow;

        _markerLabel.IsVisible = true;
        _markerLabel.Location = nearest.Coordinates;
        _markerLabel.LabelText = $"{nearest.Y} %";

        CoolingPlot.Refresh();

        var intYLocation = Convert.ToInt32(mouseLocation.Y);
        if (!_indexBeingDragged.HasValue) return;
        if (intYLocation is > 100 or < 20) return;
        if (_xs[_indexBeingDragged.Value] >= 60) return;

        _ys[_indexBeingDragged.Value] = intYLocation;

        for (int i = 0; i < _ys.Length; i++)
        {
            if (i < _indexBeingDragged.Value)
            {
                if (_ys[i] > intYLocation)
                {
                    _ys[i] = intYLocation;
                }
            }
            else if (i > _indexBeingDragged.Value)
            {
                if (_ys[i] < intYLocation)
                {
                    _ys[i] = intYLocation;
                }
            }

            CoolingPlot.Refresh();
        }
    }

    private void CoolingPlotOnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var mousePoint = e.GetPosition(CoolingPlot);
        var mousePixel = new Pixel(mousePoint.X, mousePoint.Y);
        var mouseLocation = CoolingPlot.Plot.GetCoordinates(mousePixel);
        var nearest = _pumpDutyScatter.Data.GetNearest(mouseLocation, CoolingPlot.Plot.LastRender);
        _indexBeingDragged = nearest.IsReal ? nearest.Index : null;
    }
}
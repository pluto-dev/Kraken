using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ScottPlot;
using ScottPlot.DataGenerators;
using ScottPlot.Plottables;

namespace Kraken.Desktop.Views;

public partial class CoolingPage : Page
{
    private readonly Marker _marker;
    private readonly Text _markerLabel;
    private readonly Scatter _pumpDutyScatter;
    private readonly VerticalLine _temperatureLine;

    private int? _indexBeingDragged;
    private readonly int[] _xs = [20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100];
    private readonly int[] _ys = [50, 50, 50, 50, 50, 50, 50, 50, 100, 100, 100, 100, 100, 100, 100, 100, 100];

    private readonly RandomWalker _walker = new();

    private readonly DispatcherTimer _dispatcherTimer = new()
        { Interval = TimeSpan.FromMilliseconds(1000), IsEnabled = true };

    public CoolingPage()
    {
        InitializeComponent();
        _temperatureLine = CoolingPlot.Plot.Add.VerticalLine(x: 0);
        _pumpDutyScatter = CoolingPlot.Plot.Add.Scatter(_xs, _ys);
        _marker = CoolingPlot.Plot.Add.Marker(0, 0);
        _markerLabel = CoolingPlot.Plot.Add.Text("", 0, 0);
    }

    private void ConfigurePlot()
    {
        CoolingPlot.Interaction.Disable();
        CoolingPlot.Plot.Grid.XAxisStyle.IsVisible = false;
        CoolingPlot.Plot.Grid.MajorLineWidth = 2;
        CoolingPlot.Plot.Axes.Top.MinimumSize = 35;
        // marker
        _marker.Shape = MarkerShape.OpenCircle;
        _marker.IsVisible = true;
        _marker.Size = 10;
        _marker.LineWidth = 2;
        _marker.Color = _pumpDutyScatter.MarkerStyle.FillColor;
        // marker label
        _markerLabel.LabelAlignment = Alignment.LowerLeft;
        _markerLabel.OffsetX = 7;
        _markerLabel.OffsetY = -7;
        _markerLabel.LabelBold = true;
        _markerLabel.LabelFontColor = _pumpDutyScatter.MarkerStyle.FillColor;
        // scatter
        _pumpDutyScatter.LineWidth = 2;
        _pumpDutyScatter.MarkerSize = 10;
        // temperature line
        _temperatureLine.LinePattern = LinePattern.DenselyDashed;
        _temperatureLine.LabelOppositeAxis = true;
        _temperatureLine.IsVisible = true;
        _temperatureLine.LabelFontSize = 12;

        ScottPlot.TickGenerators.NumericManual tickGenX =
            new([0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100],
                ["0˚", "10˚", "20˚", "30˚", "40˚", "50˚", "60˚", "70˚", "80˚", "90˚", "100˚"]);
        ScottPlot.TickGenerators.NumericManual tickGenY =
            new([0, 25, 50, 75, 100], ["0%", "25%", "50%", "75%", "100%"]);

        CoolingPlot.Plot.Axes.Bottom.TickGenerator = tickGenX;
        CoolingPlot.Plot.Axes.Left.TickGenerator = tickGenY;
        CoolingPlot.Plot.Axes.SetLimits(0, 110, 0, 105);
    }

    private void CoolingPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        ConfigurePlot();
        CoolingPlot.MouseDown += CoolingPlotOnMouseDown;
        CoolingPlot.MouseMove += CoolingPlotOnMouseMove;
        CoolingPlot.MouseUp += CoolingPlotOnMouseUp;

        _dispatcherTimer.Tick += DispatcherTimerOnTick;
    }

    private async void DispatcherTimerOnTick(object? sender, EventArgs args)
    {
        var tempSource = "Liquid";
        var status = _walker.Next() * 10;
        _temperatureLine.Position = status;
        _temperatureLine.LabelText = $"{tempSource}: {status:N0}˚";

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
            nearest.IsReal ? Cursors.Hand : Cursors.Arrow;

        _marker.IsVisible = true;
        _marker.Location = nearest.Coordinates;

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
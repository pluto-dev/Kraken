using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Kraken.Desktop.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using Windows.System;

namespace Kraken.Desktop.Views;

public sealed partial class CoolingPage : Page
{
    #region members

    private readonly KrakenService _krakenService;
    private KrakenDevice? _krakenDevice;
    private ChartPoint? _currentChartPoint;

    private readonly DispatcherQueueTimer _timer = Windows
        .System.DispatcherQueue.GetForCurrentThread()
        .CreateTimer();

    private readonly LineSeries<ObservablePoint> _lineSeries =
        new()
        {
            Values = new List<ObservablePoint>()
            {
                new(20, 60),
                new(25, 60),
                new(30, 60),
                new(35, 60),
                new(40, 60),
                new(45, 60),
                new(50, 60),
                new(55, 60),
                new(60, 60),
            },
            Fill = null,
            LineSmoothness = 0,
            GeometrySize = 20,
            GeometryFill = new SolidColorPaint(new SKColor(36, 151, 104)),
            Stroke = new SolidColorPaint(new SKColor(36, 151, 104)) { StrokeThickness = 4, },
            GeometryStroke = new SolidColorPaint(new SKColor(36, 151, 104)) { StrokeThickness = 4 },
            YToolTipLabelFormatter = point => $"{point.Coordinate.PrimaryValue} %",
            XToolTipLabelFormatter = _ => "",
        };

    private readonly RectangularSection _liquidSection =
        new()
        {
            Xi = 34,
            Xj = 34,
            LabelPaint = new SolidColorPaint(new SKColor(220, 223, 228).WithAlpha(225)),
            LabelSize = 16,
            ZIndex = 99,
            Stroke = new SolidColorPaint
            {
                Color = new SKColor(97, 175, 240).WithAlpha(200),
                StrokeThickness = 1.5f,
                PathEffect = new DashEffect([3.0f, 3.0f]),
            }
        };

    private readonly RectangularSection _rpmSection =
        new()
        {
            Yi = 30,
            Yj = 30,
            LabelPaint = new SolidColorPaint(new SKColor(220, 223, 228).WithAlpha(225)),
            LabelSize = 16,
            ZIndex = 99,
            Stroke = new SolidColorPaint
            {
                Color = new SKColor(229, 192, 123).WithAlpha(200),
                StrokeThickness = 1.5f,
                PathEffect = new DashEffect([3.0f, 3.0f]),
            }
        };

    #endregion

    public CoolingPage()
    {
        InitializeComponent();
        DataContext = this;
        _krakenService = new();
        Series.Add(_lineSeries);
        Sections.Add(_liquidSection);
        Sections.Add(_rpmSection);
        _timer.Tick += TimerOnTick;
        _timer.Interval = TimeSpan.FromMilliseconds(1000);
        _timer.Start();
    }

    #region properties
    private SolidColorPaint TooltipTextPaint { get; } =
        new() { Color = new SKColor(220, 223, 228), };
    private SolidColorPaint TooltipBackgroundPaint { get; } = new(SKColors.Transparent);
    private List<ISeries> Series { get; } = [];
    private List<Section<SkiaSharpDrawingContext>> Sections { get; } = [];
    private IEnumerable<ICartesianAxis> XAxis { get; } =
        [
            new Axis
            {
                ForceStepToMin = true,
                Labeler = (value) => $"{Math.Ceiling(value)} °C",
                ShowSeparatorLines = true,
                MinLimit = 15,
                MinStep = 10,
                MaxLimit = 65,
                LabelsPaint = new SolidColorPaint(new SKColor(220, 223, 228)),
            }
        ];

    private IEnumerable<ICartesianAxis> YAxis { get; } =
        [
            new Axis
            {
                ForceStepToMin = true,
                MaxLimit = 110,
                Labeler = (value) => $"{Math.Ceiling(value)} %",
                ShowSeparatorLines = true,
                MinLimit = 0,
                MinStep = 25,
                LabelsPaint = new SolidColorPaint(new SKColor(220, 223, 228)),
                SeparatorsPaint = new SolidColorPaint
                {
                    Color = new SKColor(220, 223, 228).WithAlpha(125),
                    StrokeThickness = 0.5f,
                }
            }
        ];

    #endregion

    private async void TimerOnTick(DispatcherQueueTimer sender, object args)
    {
        var status = await _krakenService.GetKrakenStatus(_krakenDevice?.Id);
        if (status is null)
        {
            return;
        }

        var temp = status.Temp.Value;
        var duty = status.Duty.Value;

        _liquidSection.Xi = temp;
        _liquidSection.Xj = temp;
        _liquidSection.Label = $"Liquid {temp}˚";

        _rpmSection.Yi = duty;
        _rpmSection.Yj = duty;
        _rpmSection.Label = $"{status.Speed.Value} RPM";
    }

    private void Chart_OnChartPointPointerDown(IChartView chart, ChartPoint? point)
    {
        _currentChartPoint = point;
    }

    private void Chart_OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _currentChartPoint = null;
    }

    private void Chart_OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_currentChartPoint is null)
            return;

        var currentPoint = e.GetCurrentPoint(Chart);
        var scaledPointY = (int)
            Chart
                .ScalePixelsToData(new LvcPointD(currentPoint.Position.X, currentPoint.Position.Y))
                .Y;

        if (scaledPointY is > 100 or < 20)
            return;

        var values = _lineSeries.Values as List<ObservablePoint>;
        Debug.Assert(values is not null, "Line Series Values can't be null!");
        var currentIndex = _currentChartPoint.Index;

        values[currentIndex] = new ObservablePoint(
            _currentChartPoint.Coordinate.SecondaryValue,
            scaledPointY
        );

        for (var i = 0; i < values.Count; i++)
        {
            if (
                (i < currentIndex && values[i].Y > scaledPointY)
                || (i > currentIndex && values[i].Y < scaledPointY)
            )
            {
                values[i].Y = scaledPointY;
            }
        }

        Chart.CoreChart.Update();
    }

    private void Chart_OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        _currentChartPoint = null;
    }

    private async void CoolingPage_OnLoaded(object sender, RoutedEventArgs e) { }

    private async void CoolingPage_OnLoading(FrameworkElement sender, object args)
    {
        var krakenDevices = await _krakenService.GetDevices();
        _krakenDevice = krakenDevices?.FirstOrDefault(x =>
            x is { ProductId: 8199, VendorId: 7793 }
        );
        _ = await _krakenService.InitializeKraken(_krakenDevice?.Id);
    }
}

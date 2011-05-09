using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
#if SILVERLIGHT
using Schementi.Controls.Extensions.Silverlight;
#endif

namespace Schementi.Controls {

    public class TimeValue {
        public DateTime Time;
        public double Value;
    }

    public class TimeSeries : ObservableCollection<TimeValue> {
        public void AddTimeValue(double value, DateTime? dateTime = null) {
            if (dateTime == null) dateTime = DateTime.Now;
            Add(new TimeValue { Time = dateTime.Value, Value = value });
        }
    }
    
    /// <summary>
    /// Interaction logic for Sparkline.xaml
    /// </summary>
    public partial class Sparkline {
        #region Dependency Properties
        #region Points
        public static DependencyProperty TimeSeriesProperty = DependencyProperty.Register(
            "TimeSeries",
            typeof(TimeSeries),
            typeof(Sparkline),
            new PropertyMetadata(new TimeSeries(), OnTimeSeriesPropertyChanged));

        public TimeSeries TimeSeries {
            get { return (TimeSeries)GetValue(TimeSeriesProperty); }
            set { SetValue(TimeSeriesProperty, value); }
        }
        #endregion

        #region StrokeThickness
        public static DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            "StrokeThickness",
            typeof(double),
            typeof(Sparkline),
            new PropertyMetadata(0.5));

        public double StrokeThickness {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        #endregion

        #region PointFill
        public static DependencyProperty PointFillProperty = DependencyProperty.Register(
            "PointFill",
            typeof(Brush),
            typeof(Sparkline),
            new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public Brush PointFill {
            get { return (Brush)GetValue(PointFillProperty); }
            set { SetValue(PointFillProperty, value); }
        }
        #endregion

        #region PointRadius
        public static DependencyProperty PointRadiusProperty = DependencyProperty.Register(
            "PointRadius",
            typeof(double),
            typeof(Sparkline),
            new PropertyMetadata(0.0));

        public double PointRadius {
            get { return (double)GetValue(PointRadiusProperty); }
            set { SetValue(PointRadiusProperty, value); }
        }
        #endregion

        #region HighWaterMark
        public static DependencyProperty HighWaterMarkProperty = DependencyProperty.Register(
            "HighWaterMark",
            typeof(double?),
            typeof(Sparkline),
            new PropertyMetadata(null));

        public double? HighWaterMark {
            get { return (double?)GetValue(HighWaterMarkProperty); }
            set { SetValue(HighWaterMarkProperty, value); }
        }
        #endregion

        #region LowWaterMark
        public static DependencyProperty LowWaterMarkProperty = DependencyProperty.Register(
            "LowWaterMark",
            typeof(double?),
            typeof(Sparkline),
            new PropertyMetadata(null));

        public double? LowWaterMark {
            get { return (double?)GetValue(LowWaterMarkProperty); }
            set { SetValue(LowWaterMarkProperty, value); }
        }
        #endregion

        #region LatestLevel
        public static DependencyProperty LatestLevelProperty = DependencyProperty.Register(
            "LatestLevel",
            typeof(double?),
            typeof(Sparkline),
            new PropertyMetadata(null));

        public double? LatestLevel {
            get { return (double?)GetValue(LatestLevelProperty); }
            set { SetValue(LatestLevelProperty, value); }
        }
        #endregion

        #region ShowWatermarks
        public static DependencyProperty ShowWatermarksProperty = DependencyProperty.Register(
            "ShowWatermarks",
            typeof(bool),
            typeof(Sparkline),
            new PropertyMetadata(false, OnShowWatermarksPropertyChanged));

        private static void OnShowWatermarksPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((Sparkline)d).OnShowWatermarksPropertyChanged();
        }

        private void OnShowWatermarksPropertyChanged()
        {
            if (ShowWatermarks) {
                _lowwatermark = new Rectangle {
                    Fill = new SolidColorBrush(Colors.Red),
                    Opacity = 0.5,
                    Height = StrokeThickness,
                    VerticalAlignment = VerticalAlignment.Top
                };
                BindingOperations.SetBinding(_lowwatermark, MarginProperty,
                                             new Binding("LowWaterMark") { Source = this, Converter = new YCoordinateToThicknessConverter() });
                Canvas.Children.Insert(0, _lowwatermark);

                _highwatermark = new Rectangle {
                    Fill = new SolidColorBrush(Colors.Green),
                    Opacity = 0.5,
                    Height = StrokeThickness,
                    VerticalAlignment = VerticalAlignment.Top
                };
                BindingOperations.SetBinding(_highwatermark, MarginProperty,
                                             new Binding("HighWaterMark") { Source = this, Converter = new YCoordinateToThicknessConverter() });
                Canvas.Children.Insert(0, _highwatermark);
            } else {
                if (_lowwatermark != null) {
                    Canvas.Children.Remove(_lowwatermark);
                    _lowwatermark = null;
                }

                if (_highwatermark != null) {
                    Canvas.Children.Remove(_highwatermark);
                    _highwatermark = null;
                }
            }
        }

        public bool ShowWatermarks {
            get { return (bool)GetValue(ShowWatermarksProperty); }
            set { SetValue(ShowWatermarksProperty, value); }
        }
        #endregion

        #region ShowLatestLevel
        public static DependencyProperty ShowLatestLevelProperty = DependencyProperty.Register(
            "ShowLatestLevel",
            typeof(bool),
            typeof(Sparkline),
            new PropertyMetadata(false, OnShowLatestLevelPropertyChanged));

        private static void OnShowLatestLevelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((Sparkline)d).OnShowLatestLevelPropertyChanged();
        }

        private void OnShowLatestLevelPropertyChanged() {
            if (ShowLatestLevel) {
                _latestLevel = new Rectangle {
                    Fill = new SolidColorBrush(Colors.White),
                    Opacity = 0.5,
                    Height = StrokeThickness,
                    VerticalAlignment = VerticalAlignment.Top
                };
                BindingOperations.SetBinding(_latestLevel, MarginProperty,
                                             new Binding("LatestLevel") { Source = this, Converter = new YCoordinateToThicknessConverter() });
                Canvas.Children.Insert(0, _latestLevel);
            } else {
                if (_latestLevel != null) {
                    Canvas.Children.Remove(_latestLevel);
                    _latestLevel = null;
                }
            }
        }

        public bool ShowLatestLevel {
            get { return (bool)GetValue(ShowLatestLevelProperty); }
            set { SetValue(ShowLatestLevelProperty, value); }
        }
        #endregion

        #region MinYRange
        public static DependencyProperty MinYRangeProperty = DependencyProperty.Register(
            "MinYRange",
            typeof(double),
            typeof(Sparkline),
            new PropertyMetadata(25.0));

        public double MinYRange {
            get { return (double)GetValue(MinYRangeProperty); }
            set { SetValue(MinYRangeProperty, value); }
        }
        #endregion
        #endregion

        #region Fields
        private int _nextXValue;

        private const int XWidth = 10;

        private Polyline _polyline;

        private Rectangle _highwatermark;
        private Rectangle _lowwatermark;
        private Rectangle _latestLevel;
        #endregion

        #region Public API
        public Sparkline() {
            MyInitializeComponent();
            TimeSeries = new TimeSeries();
            Loaded += (s, e) => InitializePolyline();
        }

        public void AddTimeValue(double value, DateTime? time = null) {
            TimeSeries.AddTimeValue(value, time);
        }
        #endregion

        #region Implementation

        public void MyInitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            Application.LoadComponent(this, new Uri("/Schementi.Controls.Sparkline;component/Sparkline.xaml", UriKind.Relative));
            Root = ((Grid)(FindName("Root")));
            ScrollViewer = ((ScrollViewer)(FindName("ScrollViewer")));
            Canvas = ((Grid)(FindName("Canvas")));
        }

        private void InitializePolyline() {
            _polyline = new Polyline();
            if (Foreground == null) Foreground = new SolidColorBrush(Colors.Black);
            BindingOperations.SetBinding(_polyline, Shape.StrokeProperty,
                                         new Binding("Foreground") { Mode = BindingMode.TwoWay, Source = this });
            BindingOperations.SetBinding(_polyline, Shape.StrokeThicknessProperty,
                                         new Binding("StrokeThickness") { Mode = BindingMode.TwoWay, Source = this });
            Canvas.Children.Add(_polyline);
        }

        private static void OnTimeSeriesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((Sparkline)d).OnTimeSeriesPropertyChanged(e);
        }

        private void OnTimeSeriesPropertyChanged(DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != null) ((TimeSeries)e.OldValue).CollectionChanged -= TimeSeriesCollectionChanged;
            if (e.NewValue != null) ((TimeSeries)e.NewValue).CollectionChanged += TimeSeriesCollectionChanged;
        }

        private void TimeSeriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (var timeValue in e.NewItems.OfType<TimeValue>()) DrawTimeValue(timeValue);
                    break;
                default:
                    ResetTimeSeries();
                    break;
            }
        }

        private void ResetTimeSeries() {
            _both = _lower = _higher = false;
            Canvas.Children.Clear();
            Canvas.Children.Add(_polyline);
            if (ShowWatermarks) {
                Canvas.Children.Add(_lowwatermark);
                Canvas.Children.Add(_highwatermark);
            }
            if (ShowLatestLevel) {
                Canvas.Children.Add(_latestLevel);
            }
            Canvas.Height = double.NaN;
            _nextXValue = 0;
            foreach (var timeValue in TimeSeries) {
                DrawTimeValue(timeValue);
            }
        }

        private void DrawTimeValue(TimeValue newTimeValue) {
            var point = GetPoint(newTimeValue);
            AddPoint(point);
            ScrollViewer.ScrollToRightEnd();
        }

        private void AddPoint(Point point) {
            _polyline.Points.Add(point);
            SetWatermarks(point.Y);
            SetLatestLevel(point.Y);
            SetCanvasHeight(point.Y);
            if (PointRadius > 0.0)
                Canvas.Children.Add(DrawDot(point));
//#if DEBUG
//            var textbox = new TextBlock {
//                Text = Convert.ToString(Math.Round(point.Y, 2)),
//                FontSize = 3,
//                Foreground = this.Foreground,
//                Opacity = 0.25,
//                Padding = new Thickness(2.0, 0, 0, 0),
//                Margin = new Thickness(point.X, point.Y + 1, 0, 0),
//                RenderTransform = new ScaleTransform { ScaleY = -1.0 },
//                RenderTransformOrigin = new Point(0.0, 0.0),
//            };
//            Canvas.Children.Add(textbox);
//#endif
            _nextXValue++;
        }

        private Path DrawDot(Point center) {
            var path = new Path();
            var circle = new EllipseGeometry {Center = center, RadiusX = PointRadius, RadiusY = PointRadius};
            path.Fill = PointFill;
            path.Data = circle;
            return path;
        }

        private Point GetPoint(TimeValue timeValue) {
            return new Point(_nextXValue * XWidth, timeValue.Value);
        }

        private void SetWatermarks(double y) {
            if (LowWaterMark == null)
                LowWaterMark = y;
            if (HighWaterMark == null)
                HighWaterMark = y;

            if (y > HighWaterMark) HighWaterMark = y;
            else if (y < LowWaterMark) LowWaterMark = y;
        }

        private void SetLatestLevel(double y) {
            LatestLevel = y;
        }

        private double _lowMargin;
        private double _height;
        private bool _lower;
        private bool _higher;
        private bool _both;

        private void SetCanvasHeight(double y) {
            if (TimeSeries.Count < 2) {
                Canvas.Height = _height = y + MinYRange;
                _lowMargin = -y + MinYRange;
                Canvas.Margin = new Thickness(0, 0, 0, _lowMargin);
                return;
            }
            if (!_both && LowWaterMark != null && LowWaterMark < _lowMargin * -1) {
                _lower = true;
                _lowMargin = -LowWaterMark.Value;
                Canvas.Margin = new Thickness(0, 0, 0, _lowMargin);
                _both = _lower && _higher;
            }
            if (!_both && HighWaterMark != null && HighWaterMark > _height) {
                _higher = true;
                Canvas.Height = HighWaterMark.Value;
                _both = _lower && _higher;    
            }
            if (!_both) return;
            Canvas.Height = double.NaN;
            Canvas.Margin = new Thickness(0, 0, 0, -(LowWaterMark ?? 0));
        }

        #endregion
    }

    public class YCoordinateToThicknessConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return new Thickness(0, (double?)value ?? 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}

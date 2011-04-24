using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

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
            new PropertyMetadata(Brushes.Black));

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
                    Fill = Brushes.Red,
                    Opacity = 0.5,
                    Height = 1,
                    VerticalAlignment = VerticalAlignment.Top
                };
                BindingOperations.SetBinding(_lowwatermark, MarginProperty,
                                             new Binding("LowWaterMark") { Source = this, Converter = new YCoordinateToThicknessConverter() });
                Canvas.Children.Add(_lowwatermark);

                _highwatermark = new Rectangle {
                    Fill = Brushes.Green,
                    Opacity = 0.5,
                    Height = 1,
                    VerticalAlignment = VerticalAlignment.Top
                };
                BindingOperations.SetBinding(_highwatermark, MarginProperty,
                                             new Binding("HighWaterMark") { Source = this, Converter = new YCoordinateToThicknessConverter() });
                Canvas.Children.Add(_highwatermark);
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
        #endregion

        #region Fields
        private int _nextXValue;

        private const int XWidth = 10;

        private Polyline _polyline;

        private Rectangle _highwatermark;
        private Rectangle _lowwatermark;
        #endregion

        #region Public API
        public Sparkline() {
            InitializeComponent();
            TimeSeries = new TimeSeries();
            InitializePolyline();
        }

        private void InitializePolyline() {
            _polyline = new Polyline();
            BindingOperations.SetBinding(_polyline, Shape.StrokeProperty,
                                         new Binding("Foreground") { Mode = BindingMode.TwoWay, Source = this });
            BindingOperations.SetBinding(_polyline, Shape.StrokeThicknessProperty,
                                         new Binding("StrokeThickness") { Mode = BindingMode.TwoWay, Source = this });
            Canvas.Children.Add(_polyline);
        }

        public void AddTimeValue(double value, DateTime? time = null) {
            TimeSeries.AddTimeValue(value, time);
        }
        #endregion

        #region Implementation

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
                case NotifyCollectionChangedAction.Move:
                    ResetTimeSeries();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ResetTimeSeries();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    ResetTimeSeries();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ResetTimeSeries();
                    break;
                default:
                    break;
            }
        }

        private void ResetTimeSeries() {
            Canvas.Children.Clear();
            Canvas.Children.Add(_polyline);
            Canvas.Children.Add(_lowwatermark);
            Canvas.Children.Add(_highwatermark);
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
            SetCanvasHeight();
            if (PointRadius > 0.0)
                Canvas.Children.Add(DrawDot(point));
            _nextXValue++;
        }

        private Path DrawDot(Point center) {
            var path = new Path();
            var circle = new EllipseGeometry(center, StrokeThickness, StrokeThickness);
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

        private void SetCanvasHeight() {
            if (HighWaterMark != null && HighWaterMark > Canvas.ActualHeight)
                Canvas.Height = HighWaterMark.Value;
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

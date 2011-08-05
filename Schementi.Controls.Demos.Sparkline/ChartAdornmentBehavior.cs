// Copyright 2011 Jimmy Schementi
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Shapes;

using SubscriptionToken = System.Nullable<System.Guid>;

namespace Schementi.Controls.Demos.Sparkline {
    public class Flag {
        public string Id;
        public string FlagDirection;
        public double Value;
    }

    public enum FlagDirection {
        Up,
        Down
    }

    public class ChartAdornmentBehavior : Behavior<Controls.Sparkline> {

        private static readonly object PointsLock = new object();

        private static readonly object PendingFlagsLock = new object();

        private readonly IList<Controls.Sparkline.TimeValueAddedEventArgs> _points = new List<Controls.Sparkline.TimeValueAddedEventArgs>();

        private readonly IList<Flag> _pendingFlags = new List<Flag>();

        private readonly IList<UIElement> _flags = new List<UIElement>();

        private Panel _panel;

        private SubscriptionToken _flagSubscription;

        public delegate SubscriptionToken EventSubscribe(Action<object> handler);
        public delegate void EventUnsubscribe(SubscriptionToken handlerToken);

        public static DependencyProperty SubscribeToFlagsProperty = DependencyProperty.Register(
            "SubscribeToFlags",
            typeof (EventSubscribe),
            typeof (ChartAdornmentBehavior),
            new PropertyMetadata(null, OnSubscribeToFlagsPropertyChanged));

        private static void OnSubscribeToFlagsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((ChartAdornmentBehavior) d).OnSubscribeToFlagsPropertyChanged();
        }

        private void OnSubscribeToFlagsPropertyChanged() {
            if (SubscribeToFlags == null) return;

            if (_flagSubscription != null && UnsubscribeFromFlags != null) {
                UnsubscribeFromFlags(_flagSubscription);
                _flagSubscription = null;
            }

            _flagSubscription = SubscribeToFlags(ProcessFlag);
        }

        public EventSubscribe SubscribeToFlags { 
            get { return (EventSubscribe) GetValue(SubscribeToFlagsProperty); } 
            set { SetValue(SubscribeToFlagsProperty, value); }
        }

        public static DependencyProperty UnsubscribeFromFlagsProperty = DependencyProperty.Register(
            "UnsubscribeFromFlags",
            typeof (EventUnsubscribe),
            typeof (ChartAdornmentBehavior),
            new PropertyMetadata(null));

        public EventUnsubscribe UnsubscribeFromFlags {
            get { return (EventUnsubscribe)GetValue(UnsubscribeFromFlagsProperty); }
            set { SetValue(UnsubscribeFromFlagsProperty, value); }
        }

        public string FlagUpLabel { get; set; }

        public string FlagDownLabel { get; set; }

        protected override void OnAttached() {
            AssociatedObject.TimeValueAdded += AssociatedObjectTimeValueAdded;
            base.OnAttached();
        }

        protected override void OnDetaching() {
            _flags.Clear();
            AssociatedObject.TimeValueAdded -= AssociatedObjectTimeValueAdded;
            base.OnDetaching();
        }

        private void AssociatedObjectTimeValueAdded(Controls.Sparkline obj, Controls.Sparkline.TimeValueAddedEventArgs eventArgs) {
            _panel = eventArgs.Panel;
            lock (PointsLock) {
                _points.Add(eventArgs);
            }

            lock (PendingFlagsLock) {
                var pendingFlagsStatus = _pendingFlags.ToList().Select(ProcessFlag).ToList();
                for (var i = 0; i < pendingFlagsStatus.Count; i++) {
                    if (pendingFlagsStatus[i] && _pendingFlags.Count < i) {
                        _pendingFlags.RemoveAt(i);
                    }
                }
            }
        }

        private void ProcessFlag(object obj) {
            var flag = (Flag) obj;
            if (!ProcessFlag(flag)) {
                lock (PendingFlagsLock)
                    _pendingFlags.Add(flag);
            }
        }

        private bool ProcessFlag(Flag flag) {
            Controls.Sparkline.TimeValueAddedEventArgs arg;
            lock (PointsLock) {
                arg = _points.FirstOrDefault(a => a.TimeValue.Id == flag.Id);
                if (arg == null) {
                    return false;
                }
                _points.Remove(arg);
            }

            FlagDirection flagDirection;
            Enum.TryParse(flag.FlagDirection, true, out flagDirection);
            Action drawFlag = () => DrawFlag(arg.Point, flagDirection);
            Dispatcher.BeginInvoke(drawFlag);

            return true;
        }

        private void DrawFlag(Point point, FlagDirection flagDirection) {
            var flagUp = false;
            if (flagDirection == FlagDirection.Up) {
                flagUp = true;
            }

            // add "flag"
            var flag = new Grid {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = flagUp ? VerticalAlignment.Top : VerticalAlignment.Bottom,
                Margin = new Thickness(point.X - 0.3, point.Y, 0, 0),
                Height = 7,
                Children = {
                    
                    // flagpole
                    new Border {
                        Background = AssociatedObject.Foreground,
                        Width = 0.6,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        UseLayoutRounding = false
                    },

                    // Actual flag
                    new Border {
                        Margin = new Thickness(0, 0, 0, -0.2),
                        VerticalAlignment = flagUp ? VerticalAlignment.Bottom : VerticalAlignment.Top,
                        RenderTransform = new ScaleTransform {ScaleY = -1.0},
                        RenderTransformOrigin = new Point(0.5, 0.5),
                        Background = AssociatedObject.Foreground,
                        Child = new StackPanel {
                            Orientation = Orientation.Vertical,
                            Children = {
                                // flag content
                                new TextBlock {
                                    Text = Convert.ToString(Math.Round(point.Y, 2)),
                                    FontSize = 2.5,
                                    Foreground = new SolidColorBrush(Colors.White),
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                    VerticalAlignment = VerticalAlignment.Center,
                                },
                                // flag label
                                new TextBlock {
                                    Text = flagDirection == FlagDirection.Up ? FlagUpLabel : FlagDownLabel,
                                    FontSize = 1.5,
                                    Foreground = new SolidColorBrush(Colors.White),
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                    VerticalAlignment = VerticalAlignment.Center,
                                },
                            }
                        },
                    },
                }
            };

            // add red point to highlight sparkline
            var flagPoint = new Path {
                Cursor = Cursors.Hand,
                Fill = new SolidColorBrush(Colors.Red),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 0.2,
                Data = new EllipseGeometry { Center = point, RadiusX = 0.5, RadiusY = 0.5 }
            };

            // Fix up flag for pointing down ...
            var border = (Border)flag.Children[1];
            var stackpanel = (StackPanel)border.Child;
            if (!flagUp) {
                flag.Margin = new Thickness(point.X - 0.2, 0, 0, _panel.Height - point.Y);
                border.Margin = new Thickness(0, -0.2, 0, 0);
                var children = stackpanel.Children;
                var label = children[1];
                children.RemoveAt(1);
                children.Insert(0, label);
            }

            if (_flags.Count > 0) _flags.Last().Visibility = Visibility.Collapsed;

            _panel.Children.Add(flag);
            _panel.Children.Add(flagPoint);
            _flags.Add(flag);

            flagPoint.MouseLeftButtonDown += (s, e) => {
                foreach (var f in _flags.Where((ff,i) => i != _flags.Count - 1 && ff != flag)) // all flags but the last one and this one ...
                    f.Visibility = Visibility.Collapsed;
                flag.Visibility = flag.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                if (flag.Visibility == Visibility.Visible) {
#if SILVERLIGHT
                    flag.SetValue(Canvas.ZIndexProperty, _panel.Children.Count);
                    flagPoint.SetValue(Canvas.ZIndexProperty, _panel.Children.Count + 1);
#else
                    Panel.SetZIndex(flag, _panel.Children.Count);
                    Panel.SetZIndex(flagPoint, _panel.Children.Count + 1);
#endif
                }
                e.Handled = true;
            };
        }
    }
}

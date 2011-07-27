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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TestSimpleRNG;

namespace Schementi.Controls.Demos.Sparkline {

    public partial class MainPage {

        private const bool IsSilverlight =
#if SILVERLIGHT
                true
#else
                false
#endif
                ;

        static readonly object StopLock = new object();
        public MainPage() {
            InitializeComponent();
            StartSparkline();
        }

        private Controls.Sparkline[] _sparklines;
        private IEnumerable<Controls.Sparkline> Sparklines {
            get {
                return _sparklines ?? (_sparklines = FindVisualChildren<Controls.Sparkline>(LayoutRoot).ToArray());
            }
        }

        private void StartSparkline() {
            Func<int> tickTime = () => {
                // Convert.ToInt32(Math.Floor(SimpleRNG.GetUniform()*2000));
                return 1000/2;
            };
            var stop = false;
            int lineCount = 0;

            Func<Controls.Sparkline, Action> tickGenerator = s => {
                var x = 100.0;
                var currentLine = ++lineCount - 1;
                var decoratedTickCount = 0;
                Action<Point, Panel> drawFlag = (point, panel) => {
                    if (SimpleRNG.GetUniform() < 0.8) return;
                    var flagUp = SimpleRNG.GetUniform() < 0.5;
                    decoratedTickCount += DrawFlag(flagUp, s, point, panel, decoratedTickCount);
                };
                if (currentLine == 0) {
                    Controls.Sparkline.TimeValueAddedHandler onTimeValueAdded = (sender, eventArgs) =>
                        drawFlag(eventArgs.Point, eventArgs.Panel);
                    s.TimeValueAdded += onTimeValueAdded;
                    s.Unloaded += (sender, e) => s.TimeValueAdded -= onTimeValueAdded;
                }
                return () => {
                    while (true) {
                        x = x + SimpleRNG.GetNormal()*2;
                        Console.WriteLine(x);

                        Dispatcher.BeginInvoke((Action)(() => s.AddTimeValue(x)));

                        Thread.Sleep(tickTime());
                        lock (StopLock) if (stop) return;
                    }
                };
            };

            var tickGenerators = Sparklines.Select(tickGenerator).ToArray();

            Func<Action, WaitCallback> toWaitCallback = gen => wc => gen();
            Action start = () => {
                foreach (var generateTicks in tickGenerators)
                    ThreadPool.QueueUserWorkItem(toWaitCallback(generateTicks));
            };

            Loaded += (s, e) => start();
            Unloaded += (s, e) => {
                lock (StopLock) {
                    stop = true;
                }
            };
        }

        public int DrawFlag(bool flagUp, Controls.Sparkline s, Point point, Panel panel, int count) {

            // add red point to highlight sparkline
            var path = new Path {
                Fill = new SolidColorBrush(Colors.Red),
                Data = new EllipseGeometry {Center = point, RadiusX = 0.5, RadiusY = 0.5}
            };
            panel.Children.Add(path);

            // add "flag"
            var flag = new Grid {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = flagUp ? VerticalAlignment.Top : VerticalAlignment.Bottom,
                Margin = new Thickness(point.X - 0.3, point.Y, 0, 0).Align(s.LineMargin),
                Height = 10,
                Children = {
                    
                    // flagpole
                    new Border {
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x55, 0x55, 0x55)),
                        Width = 0.6,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        UseLayoutRounding = false
                    },

                    // Actual flag
                    new Border {
                        Margin = new Thickness(0, 0, 0, -0.2),
                        //Padding = new Thickness(0.2, 0.0, 0.4, 0.0),
                        VerticalAlignment = flagUp ? VerticalAlignment.Bottom : VerticalAlignment.Top,
                        RenderTransform = new ScaleTransform {ScaleY = -1.0},
                        RenderTransformOrigin = new Point(0.5, 0.5),
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x55, 0x55, 0x55)),
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
                                    Text = flagUp ? "Buy" : "Sell",
                                    FontSize = 1.5,
                                    //Padding = new Thickness(0,0.2,0,0),
                                    Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xaa, 0xaa, 0xaa)),
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                    VerticalAlignment = VerticalAlignment.Center,
                                },
                            }
                        },
                    },
                }
            };

            var border = (Border)flag.Children[1];
            var stackpanel = (StackPanel)border.Child;
            if (!flagUp) {
                flag.Margin = new Thickness(point.X - 0.2, 0, 0, panel.Height - point.Y).Align(s.LineMargin);
                border.Margin = new Thickness(0, -0.2, 0, 0);
                var children = stackpanel.Children;
                var label = children[1];
                children.RemoveAt(1);
                children.Insert(0, label);
            }

            panel.Children.Insert(count, flag);
            //panel.Children.Insert(count, new Border { Background = new SolidColorBrush(Color.FromArgb(0x22, 0x00, 0x00, 0x00))});

            return 1;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject {
            if (depObj == null) yield break;
            
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T) 
                    yield return (T) child;

                foreach (var childOfChild in FindVisualChildren<T>(child)) 
                    yield return childOfChild;
            }
        }
    }

    public static class ThicknessExtensions {
        public static Thickness Align(this Thickness lhs, Thickness rhs) {
            return new Thickness(
                lhs.Left + rhs.Left,
                lhs.Top - rhs.Top,
                lhs.Right + rhs.Right,
                lhs.Bottom - rhs.Bottom);
        }
    }
}

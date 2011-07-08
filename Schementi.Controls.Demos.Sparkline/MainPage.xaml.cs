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
        static readonly object StopLock = new object();
        public MainPage() {
            InitializeComponent();
            StartSparkline();
        }

        private Controls.Sparkline[] _sparklines;
        private Controls.Sparkline[] Sparklines {
            get {
                return _sparklines ?? (_sparklines = FindVisualChildren<Controls.Sparkline>(LayoutRoot).ToArray());
            }
        }

        private void StartSparkline() {
            const int tickTime = 1000 / 2;
            var stop = false;
            int lineCount = 0;
            Func<Controls.Sparkline, Action> nextTick = s => {
                var currentLine = lineCount++;
                var x = 100.0;
                return () => {
                    var count = 0;
                    while (true) {
                        x = x + SimpleRNG.GetNormal()*2;
                        if (x < 1) x = Math.Abs(x) + 1;
                        Console.WriteLine(x);

                        Action eachTick = () =>
                            s.AddTimeValue(x, null, (point, panel) =>
                                DrawFlag(currentLine, s, point, panel, ref count));

                        Dispatcher.BeginInvoke(eachTick);

                        Thread.Sleep(tickTime);
                        lock (StopLock) if (stop) return;
                    }
                };
            };

            var tickGenerators = Sparklines.Select(nextTick).ToArray();

            Func<Action, WaitCallback> tickLoop = gen => wc => gen();
            Action start = () => { foreach (var gen in tickGenerators) ThreadPool.QueueUserWorkItem(tickLoop(gen)); };

            Loaded += (s, e) => start();
            Unloaded += (s, e) => {
                lock (StopLock) {
                    stop = true;
                }
            };
        }

        public void DrawFlag(int currentLine, Controls.Sparkline s, Point point, Panel panel, ref int count) {
            if (currentLine > 0) return;
            if (SimpleRNG.GetUniform() < 0.75) return;

            var path = new Path();
            var circle = new EllipseGeometry { Center = point, RadiusX = 0.5, RadiusY = 0.5 };
            path.Fill = new SolidColorBrush(Colors.Red);
            path.Data = circle;
            panel.Children.Add(path);

            var rect = new Grid {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(point.X - 0.25, point.Y, 0, 0).Align(s.LineMargin),
            };
            var line = new Border { Background = new SolidColorBrush(Colors.Gray), Width = 0.4, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, UseLayoutRounding = false };
            var box = new Border {
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x11, 0x11, 0x11)),
                BorderBrush = new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(0.4),
                Padding = new Thickness(0.5, 0.5, 0.5, 0.5),
                VerticalAlignment = VerticalAlignment.Bottom,
                RenderTransform = new ScaleTransform { ScaleY = -1.0 },
                RenderTransformOrigin = new Point(0.0, 0.0),
            };
            var text = new TextBlock {
                Text = Convert.ToString(Math.Round(point.Y, 2)),
                FontSize = 2,
                Foreground = new SolidColorBrush(Colors.White),
            };
            box.Child = text;
            rect.Children.Add(line);
            rect.Children.Add(box);

            panel.Children.Insert(count++, rect);
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

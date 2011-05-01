using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Schementi.Controls.Demos.Sparkline {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();

            var sparklines = Grid.Children.OfType<Controls.Sparkline>();

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.5) };
            var random = new Random();
            var x = 20.0;
            timer.Tick += (s, e) => {
                foreach (var sparkline in sparklines) {
                    x = x + (random.NextDouble() * 10 * random.NextDouble() * (random.NextDouble() < 0.5 ? -1 : 1));
                    if (x < 1) x = Math.Abs(x) + 1;
                    Console.WriteLine(x);
                    sparkline.AddTimeValue(x);
                }
            };

            Loaded += (s, e) => timer.Start();
            Unloaded += (s, e) => timer.Stop();
        }
    }
}

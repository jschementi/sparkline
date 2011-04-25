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
            timer.Tick += (s, e) => {
                foreach (var sparkline in sparklines) {
                    var x = random.Next(10, 61);
                    Console.WriteLine(x);
                    sparkline.AddTimeValue(x);
                }
            };

            Loaded += (s, e) => timer.Start();
            Unloaded += (s, e) => timer.Stop();
        }
    }
}

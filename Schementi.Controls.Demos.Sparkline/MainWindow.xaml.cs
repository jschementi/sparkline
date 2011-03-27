using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Schementi.Controls;

namespace Schementi.Controls.Demos.Sparkline {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            var sparklines = Grid.Children.OfType<Controls.Sparkline>();

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.1) };
            var random = new Random();
            timer.Tick += (s, e) => {
                              foreach(var sparkline in sparklines) 
                                  sparkline.AddTimeValue((random.NextDouble()*60) + 20);
                          };

            Loaded += (s, e) => timer.Start();
            Unloaded += (s, e) => timer.Stop();
        }
    }
}

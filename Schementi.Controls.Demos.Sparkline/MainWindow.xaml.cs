using System;
using System.Linq;
using System.Threading;

namespace Schementi.Controls.Demos.Sparkline {
    public partial class MainWindow {
        static readonly object StopLock = new object();
        public MainWindow() {
            InitializeComponent();

            var sparklines = Grid.Children.OfType<Controls.Sparkline>().ToArray();
            var sparklinesCount = sparklines.Length;
            const int tickTime = 1000/2;
            var stop = false;
            WaitCallback tickLoop = wc => {
                while (true) {
                    var x = 100.0;
                    for (var i = 0; i < sparklinesCount; i++) {
                        var random = new Random();
                        //x = random.Next(100, 200);
                        x = x + (random.NextDouble() * 10 * random.NextDouble() * (random.NextDouble() < 0.5 ? -1 : 1));
                        if (x < 1) x = Math.Abs(x) + 1;
                        Console.WriteLine(x);

                        var i1 = i;
                        var x1 = x;
                        Dispatcher.BeginInvoke(new Action(() => sparklines[i1].AddTimeValue(x1)));
                    }
                    Thread.Sleep(tickTime);
                    lock (StopLock) if (stop) return;
                }
            };
            Action start = () => ThreadPool.QueueUserWorkItem(tickLoop);

            Loaded += (s, e) => start();
            Unloaded += (s, e) => {
                            lock (StopLock) {
                                stop = true;
                            }
                        };
        }
    }
}

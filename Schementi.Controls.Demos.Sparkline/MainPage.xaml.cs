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
using System.Windows.Threading;
using Schementi.Controls.Utilities;
using TestSimpleRNG;

namespace Schementi.Controls.Demos.Sparkline {

    public delegate string AddTimeValue(double value, DateTime? time = null);

    public class MainPageViewModel {
        private readonly Dispatcher _dispatcher;

        #region ChartAdornmentBehavior events
        private readonly IDictionary<Guid, Action<object>> _handlers = new Dictionary<Guid, Action<object>>();
        private readonly ChartAdornmentBehavior.EventSubscribe _subscribeToFlags;
        public ChartAdornmentBehavior.EventSubscribe SubscribeToFlags {
            get { return _subscribeToFlags; }
        }
        private readonly ChartAdornmentBehavior.EventUnsubscribe _unsubscribeFromFlags;
        

        public ChartAdornmentBehavior.EventUnsubscribe UnsubscribeFromFlags {
            get { return _unsubscribeFromFlags; }
        }
        #endregion

        public MainPageViewModel(Dispatcher dispatcher) {
            _dispatcher = dispatcher;
            _subscribeToFlags = handler => {
                var guid = Guid.NewGuid();
                _handlers[guid] = handler;
                return guid;
            };
            _unsubscribeFromFlags = token => {
                if (token != null) 
                    _handlers.Remove(token.Value);
            };
        }

        public Action StartSparkline(IEnumerable<AddTimeValue> addTimeValues) {
            Func<int> tickTime = () => {
                // Convert.ToInt32(Math.Floor(SimpleRNG.GetUniform()*2000));
                return 1000 / 2;
            };

            Func<AddTimeValue, Action> tickGenerator = addTimeValue => {
                var x = 100.0;
                return () => {
                    while (true) {
                        var newx = x + SimpleRNG.GetNormal() * 2;
                        if (newx < 0.0) newx = 0.0;
                        _dispatcher.BeginInvoke((Action)(() => {
                            var guid = addTimeValue(newx);
                            if (SimpleRNG.GetUniform() > 0.75)
                                ShowFlag(guid, newx);
                        }));
                        x = newx;
                        Thread.Sleep(tickTime());
                    }
                };
            };

            Func<Action, WaitCallback> toWaitCallback = gen => wc => gen();
            Action start = () => {
                var tickGenerators = addTimeValues.Select(tickGenerator).ToArray();
                foreach (var generateTicks in tickGenerators)
                    ThreadPool.QueueUserWorkItem(toWaitCallback(generateTicks));
            };
            return start;
        }

        private void ShowFlag(string guid, double value) {
            var direction = SimpleRNG.GetUniform() > 0.5 ? FlagDirection.Up : FlagDirection.Down;
            var flag = new Flag { Id = guid, Value = value, FlagDirection = direction.ToString() };
            foreach (var h in _handlers)
                h.Value(flag);
        }
    }


    public partial class MainPage {
        private readonly MainPageViewModel _viewModel;

        public MainPage() {
            _viewModel = new MainPageViewModel(Dispatcher);
            DataContext = _viewModel;
            
            InitializeComponent();

            Loaded += (s, e) => {
                var sparklines = LayoutRoot.FindLogicalChildren<Controls.Sparkline>();
                var addTimeValues = sparklines.Select(sl => new AddTimeValue(sl.AddTimeValue));
                _viewModel.StartSparkline(addTimeValues)();
            };
        }
    }
}

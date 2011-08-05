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

using System.Linq;
using System.Windows.Controls;
#if !SILVERLIGHT
using System.Windows;
#endif
using Schementi.Controls.Utilities;
#if SILVERLIGHT
using Schementi.Controls.Extensions.Silverlight;
#endif

namespace Schementi.Controls {
    public class LineChart : ItemsControl {
        private ScrollViewer _scrollViewer;
#if !SILVERLIGHT
        static LineChart() {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(LineChart),
                new FrameworkPropertyMetadata(typeof(LineChart)));
        }
#endif

        public LineChart() {
            DefaultStyleKey = typeof (LineChart);
        }

        public override void OnApplyTemplate() {
            _scrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
            if (_scrollViewer == null) return;
            foreach (var sl in this.FindLogicalChildren<Sparkline>())
                sl.ScrollToRightEnd = _scrollViewer.ScrollToRightEnd;
            base.OnApplyTemplate();
        }
    }
}
